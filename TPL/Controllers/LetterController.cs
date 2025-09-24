using BE;
using BE.LetterAutomation;
using BLL.LetterAutomation;
using BLL.Ticketing;
using DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text.Json;
using TPLWeb.Tools;
using SixImage = SixLabors.ImageSharp.Image;
using System.Runtime.Versioning;
using Newtonsoft.Json;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;

namespace TPLWeb.Controllers
{
    [Authorize]
    [Route("Letter")]
    [SupportedOSPlatform("windows")] // این کنترلر وابسته به API های فقط-ویندوز است
    public class LetterController : Controller
    {
        #region Fields
        private readonly ILetterService _letterService;
        private readonly Db _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<LetterController> _logger;
        private readonly BlNotification _notificationService;
        private readonly BlRecivers _recivers;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        #endregion

        #region Ctor
        public LetterController(ILetterService letterService, Db context, UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment, ILogger<LetterController> logger, BlRecivers recivers, BlNotification notificationService,
            IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _letterService = letterService;
            _context = context;
            _userManager = userManager;
            _environment = environment;
            _logger = logger;
            _recivers = recivers;
            _notificationService = notificationService;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }
        #endregion

        /// <summary>
        /// Populates the IsRead property for letters based on LetterAction table
        /// </summary>
        private async Task PopulateLetterReadStatus(List<Letter> letters, string userId)
        {
            if (string.IsNullOrEmpty(userId) || letters == null || !letters.Any())
                return;

            var readLetterIds = await _context.LetterActions
                .Where(a => a.UserId == userId && a.ActionDescription == "Viewed")
                .Select(a => a.LetterId)
                .ToListAsync();

            foreach (var letter in letters)
            {
                letter.IsRead = readLetterIds.Contains(letter.Id);
            }
        }

    [HttpGet("letterviewpdf")]
    [SupportedOSPlatform("windows")] // تولید فایل Word/PDF فقط در ویندوز پشتیبانی می‌شود
        public async Task<IActionResult> ViewLetterPdf(int id)
        {
            string mainpath = Path.Combine(_environment.WebRootPath, "wordfiles");
            if (!Directory.Exists(mainpath))
            {
                Directory.CreateDirectory(mainpath);
            }
            await LetterGenerate(id);
            //Validate the file name/ path here for security
            var WordfilePath = Path.Combine(_environment.WebRootPath, "wordfiles", $"{id}.docx");
            var PdffilePath = Path.Combine(_environment.WebRootPath, "wordfiles", $"{id}.pdf");

            if (!global::System.IO.File.Exists(WordfilePath) || !global::System.IO.File.Exists(PdffilePath))
            {
                await LetterGenerate(id);
            }

            ViewBag.LetterNumber = id;

            return View();
        }

    [HttpGet("lettergenerate")]
    [SupportedOSPlatform("windows")] // استفاده از موتور قالب و System.Drawing فقط ویندوز
        public async Task<IActionResult> LetterGenerate(int id)
        {
            var model = await _letterService.GetLetterByIdAsync(id);
            var res = (Letter)model.Data!;
            #pragma warning disable CA1416 // استفاده از APIهای فقط-ویندوز
            var engine = new LetterTemplateEngine(); // کلاس ویندوز-محور
            // 1. آماده‌سازی داده‌های جایگزین
            var textReplacements = new Dictionary<string, string>
            {
                { "DATE", res!.RegistrationDate.ToString("yyyy/MM/dd") },
                { "SENDER",await _recivers.GetReceiverFullPath(res.Sender!) },
                { "RECIVER", await _recivers.GetReceiverFullPath(res.Receiver!) },
                { "SUBJECT", res.Subject! },
                { "PRIORITY", res.Priority.ToString() },
                { "CLASSIFICATION", res.Classification.ToString() },
                { "LETTERNUMBER", res?.LetterNumber ?? " " },
                { "SEMAT", res?.SignerPosition ?? " " },
                { "SIGNER", res?.SignerName ?? " " },
                { "CONTENT", res!.Content! },
                { "Evaluation Warning: The document was created with Spire.Doc for", "" },
            };
            var signerr = _userManager.Users.FirstOrDefault(x => x.UserName == res.SignerUserName);

            // 2. آماده‌سازی تصاویر (اختیاری)
            var imageReplacements = new Dictionary<string, global::System.Drawing.Image>
            {

                {
                    "SIGNIMAGE",
                    global::System.Drawing.Image.FromFile(Path.Combine(_environment.WebRootPath,"CompanyVariables", "signatures",
                        signerr!.Emza!))
                }

            };
            // 3. آماده‌سازی جداول
            var tableReplacements = new Dictionary<string, TableData>();

            // جدول اطلاعات نامه
            var letterInfoTable = new TableData(4, 2);
            letterInfoTable.FillData(new string[,]
            {
                { "تاریخ:", "{DATE}" },
                { "اولویت:", "{PRIORITY}" },
                { "محرمانگی:", "{CLASSIFICATION}" },
                { "شماره نامه:", "{LETTERNUMBER}" }
            });
            tableReplacements.Add("{LETTERINFO}", letterInfoTable);
            var letterpath = res.LetterNumber?.Replace("-", "")?.ToString()! ?? res.Id.ToString();
            string pathtemplatePath = Path.Combine(_environment.WebRootPath, "CompanyVariables", "WordTemplate", "Doc1.docx");
            string mainpath = Path.Combine(_environment.WebRootPath, "wordfiles");
            if (!Directory.Exists(mainpath))
            {
                Directory.CreateDirectory(mainpath);
            }
            string pathfinalCombine = Path.Combine(_environment.WebRootPath, "wordfiles", letterpath);
            // 4. تولید سند نهایی
            engine.GenerateLetterFromTemplate(
                templatePath: pathtemplatePath,
                outputPath: pathfinalCombine,
                textReplacements: textReplacements,
                tableReplacements: tableReplacements,
                imageReplacements: imageReplacements

            );
            #pragma warning restore CA1416

            Console.WriteLine($"doc is created {letterpath}!");
            TempData["LetterDownloadLink"] = letterpath + ".docx";
            return Json(new { fileId = letterpath });
        }

        [HttpGet("letterview")]
        public async Task<IActionResult> LetterView(int id)
        {
            var model = await _letterService.GetLetterByIdAsync(id);
            try
            {
                var currentUserId = _userManager.GetUserId(User);
                if (!string.IsNullOrEmpty(currentUserId))
                {
                    var already = await _context.LetterActions
                        .AnyAsync(a => a.LetterId == id && a.UserId == currentUserId && a.ActionDescription == "Viewed");
                    if (!already)
                    {
                        _context.LetterActions.Add(new LetterAction
                        {
                            LetterId = id,
                            UserId = currentUserId,
                            ActionDescription = "Viewed",
                            ActionDate = DateTime.Now
                        });
                        await _context.SaveChangesAsync();
                    }
                }
            }
            catch { }
            return View((Letter)model.Data!);
        }

        [HttpPost("MarkDashboardRead/{id}")]
        public async Task<IActionResult> MarkDashboardRead(int id)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(currentUserId)) return Json(new { success = false });
                var exists = await _context.LetterActions
                    .AnyAsync(a => a.LetterId == id && a.UserId == currentUserId && a.ActionDescription == "DashboardRead");
                if (!exists)
                {
                    _context.LetterActions.Add(new LetterAction
                    {
                        LetterId = id,
                        UserId = currentUserId,
                        ActionDescription = "DashboardRead",
                        ActionDate = DateTime.Now
                    });
                    await _context.SaveChangesAsync();
                }
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }



        [HttpGet("Letters")]
        public IActionResult Index()
        {
            ViewData["Title"] = "کارتابل نامه های وارده";
            return RedirectToAction(nameof(KartableVaredeh));
        }


        [HttpGet("Elanat")]
        public async Task<IActionResult> TabloElanat()
        {
            var result = await _letterService.GetAllLettersAsync();
            var res = (List<Letter>)result.Data!;
            var model = res.Where(x => x.IsDeleted != true && x.ShowOnDashboard && x.Status == LetterStatus.Approved).ToList();
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
            }

            // Exclude letters already marked as read on dashboard by current user
            var currentUserId = _userManager.GetUserId(User);
            if (!string.IsNullOrEmpty(currentUserId))
            {
                var readIds = await _context.LetterActions
                    .Where(a => a.UserId == currentUserId && a.ActionDescription == "DashboardRead")
                    .Select(a => a.LetterId)
                    .ToListAsync();
                model = model.Where(l => !readIds.Contains(l.Id)).ToList();

                // Populate IsRead property for remaining letters
                await PopulateLetterReadStatus(model, currentUserId);
            }

            ViewData["Title"] = "تابلو اعلانات";
            TempData["tablo"] = "elanat";
            TempData["vaziat"] = null;
            return View(nameof(Index), model);
        }


        [HttpGet("NamehhayeSadereh")]
        public async Task<IActionResult> KartableSadereh()
        {
            var result = await _letterService.GetAllLettersAsync();
            var res = (List<Letter>)result.Data!;
            var model = res.Where(x => x.IsDeleted != true && x.Username == User.Identity!.Name && x.Status == LetterStatus.Pending ||
                                       x.Status == LetterStatus.Rejected || x.Status == LetterStatus.InReview ||
                                       x.Status == LetterStatus.Returned).ToList();
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
            }

            // Populate IsRead property for letters
            var currentUserId = _userManager.GetUserId(User);
            if (!string.IsNullOrEmpty(currentUserId))
            {
                await PopulateLetterReadStatus(model, currentUserId);
            }

            ViewData["Title"] = "کارتابل نامه های صادره";
            TempData["vaziat"] = "kartable";
            return View(nameof(Index), model);
        }


        [HttpGet("NamehhayeVaredeh")]
        public async Task<IActionResult> KartableVaredeh()
        {
            var currentUser = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity!.Name);
            if (currentUser == null)
            {
                TempData["ErrorMessage"] = "کاربر یافت نشد.";
                return RedirectToAction(nameof(KartableSadereh));
            }

            var userOrganization = await _context.UserOrganizations
                .Include(c => c.Organization)
                .FirstOrDefaultAsync(x => x.UserId == currentUser.Id && x.IsActive == true);

            if (userOrganization?.Organization == null || userOrganization.IsActive == false)
            {
                TempData["ErrorMessage"] = "سازمان کاربر یافت نشد.";
                return RedirectToAction(nameof(KartableSadereh));
            }

            var result = await _letterService.GetAllLettersAsync();
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(KartableSadereh));
            }

            var userorgid = userOrganization.Organization.Id;
            var letters = (List<Letter>)result.Data!;
            var model = letters.Where(x =>
                !x.IsDeleted &&
                x.Status != LetterStatus.Deleted &&
                x.Status != LetterStatus.Pending &&
                x.Status != LetterStatus.Archived &&
                x.Status != LetterStatus.InReview &&
                x.Status != LetterStatus.Returned &&
                x.Status != LetterStatus.Rejected &&
                (x.Receiver == userorgid.ToString() ||  // یا گیرنده مستقیم است
                 (x.CopyReceivers != null &&  // یا در رونوشت‌ها وجود دارد
                  !string.IsNullOrEmpty(x.CopyReceivers.FirstOrDefault()) &&
                  x.CopyReceivers.FirstOrDefault()!
                      .Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Any(c => c.Equals(userorgid.ToString()))))
            ).ToList();

            // Populate IsRead property for letters
            if (!string.IsNullOrEmpty(currentUser.Id))
            {
                await PopulateLetterReadStatus(model, currentUser.Id);
            }

            ViewData["Title"] = "کارتابل نامه های وارده";
            TempData["vaziat"] = null;
            return View(nameof(Index), model);
        }

        [HttpGet("Answerletter")]
        public async Task<IActionResult> AnswerLetter(int letterid)
        {
            var let = await _context.Letters.Where(x => x.Id == letterid).FirstOrDefaultAsync();
            ViewBag.ParentOrganizations = _context.Organizations
                .Where(o => o.IsActive)
                .ToList();
            ViewBag.relatedtext = $"{let!.LetterNumber}-{let.Subject}";
            return View(nameof(Create), new Letter()
            {
                LetterRelationType = LetterRelationType.پاسخ,
                RelatedLetterId = letterid,
                Content = $"با احترام در پاسخ به نامه شماره {let.LetterNumber} مورخ {let.RegistrationDate.ToString("yyyy/MM/dd")} :"
            });
        }
        [HttpGet("Followup")]
        public async Task<IActionResult> Followup(int letterid)
        {
            var let = await _context.Letters.Where(x => x.Id == letterid).FirstOrDefaultAsync();
            ViewBag.ParentOrganizations = _context.Organizations
                .Where(o => o.IsActive)
                .ToList();
            ViewBag.relatedtext = $"{let!.LetterNumber}-{let.Subject}";
            return View(nameof(Create), new Letter()
            {
                LetterRelationType = LetterRelationType.پیرو,
                RelatedLetterId = letterid,
                Content = $"با احترام پیرو نامه شماره {let.LetterNumber} مورخ {let.RegistrationDate.ToString("yyyy/MM/dd")} :"
            });
        }
        [HttpGet("createnewletter")]
        public IActionResult Create()
        {
            ViewBag.ParentOrganizations = _context.Organizations
                .Where(o => o.IsActive)
                .ToList();
            return View(new Letter());
        }



        [HttpPost("createnewletter")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Letter letter)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var currentUserIdForActions = _userManager.GetUserId(User);
                    // آماده‌سازی نگاشت یادداشت برای گیرندگان رونوشت (orgId -> note)
                    var ccNotes = new Dictionary<int, string>();
                    try
                    {
                        if (letter.CopyReceivers != null && letter.CopyReceivers.Count > 1 && !string.IsNullOrWhiteSpace(letter.CopyReceivers[1]))
                        {
                            using var doc = JsonDocument.Parse(letter.CopyReceivers[1]);
                            foreach (var el in doc.RootElement.EnumerateArray())
                            {
                                string? idStr = null;
                                if (el.TryGetProperty("id", out var idProp))
                                {
                                    idStr = idProp.ValueKind == JsonValueKind.String ? idProp.GetString() : idProp.GetRawText();
                                }
                                var noteStr = el.TryGetProperty("note", out var noteProp) ? noteProp.GetString() : null;
                                if (!string.IsNullOrWhiteSpace(idStr) && int.TryParse(idStr, out var parsedId) && !string.IsNullOrWhiteSpace(noteStr))
                                {
                                    ccNotes[parsedId] = noteStr!;
                                }
                            }
                        }
                    }
                    catch { /* نادیده گرفتن خطاهای پارس */ }

                    // تنظیم اطلاعات کاربر امضا کننده
                    var user = await _userManager.FindByIdAsync(letter.SignerName!);
                    if (user != null)
                    {
                        letter.SignerPosition = user.Semat;
                        letter.SignerName = $"{user.FirstName} {user.LastName}";
                        letter.SignerUserName = user.UserName;
                        letter.Username = User.Identity!.Name;
                    }

                    // مدیریت نامه مرتبط
                    if (letter.RelatedLetterId.HasValue)
                    {
                        letter.RelatedLetter = await _context.Letters
                            .FirstOrDefaultAsync(x => x.Id == letter.RelatedLetterId.Value);
                    }

                    // تنظیم توضیح کلاسه
                    if (!string.IsNullOrEmpty(letter.FileCode) && int.TryParse(letter.FileCode, out int fileCode))
                    {
                        var kelaseh = await _context.Kelasehnamehha
                            .FirstOrDefaultAsync(x => x.CodeKelaseh == fileCode);

                        if (kelaseh != null)
                        {
                            letter.FileDescription = kelaseh.NameKelaseh;
                        }
                    }

                    // پردازش کلیدواژه‌ها
                    if (!string.IsNullOrEmpty(letter.Keywords))
                    {
                        try
                        {
                            letter.KeywordsList = JsonDocument.Parse(letter.Keywords)
                                .RootElement
                                .EnumerateArray()
                                .Select(e => e.GetProperty("value").GetString())
                                .Where(x => !string.IsNullOrEmpty(x))
                                .ToList()!;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error parsing keywords");
                            letter.KeywordsList = new List<string>();
                        }
                    }

                    // پردازش گیرندگان رونوشت
                    if (letter.CopyReceivers != null && letter.CopyReceivers.Count >= 1)
                    {
                        var receiverIds = letter.CopyReceivers.FirstOrDefault()?
                            .Split(',', StringSplitOptions.RemoveEmptyEntries);

                        if (receiverIds != null)
                        {
                            foreach (string id in receiverIds)
                            {
                                if (int.TryParse(id.Trim(), out int orgId))
                                {
                                    var org = await _context.Organizations
                                        .FirstOrDefaultAsync(x => x.Id == orgId);

                                    if (org != null && !letter.CopyReceiversList.Any(x => x.Id == orgId))
                                    {
                                        letter.CopyReceiversList.Add(org);
                                    }
                                }
                            }
                        }
                    }

                    // تنظیم وضعیت اولیه
                    letter.Status = LetterStatus.Pending;
                    letter.RegistrationDate = DateTime.Now;
                    letter.LastModifiedDate = DateTime.Now;
                    letter.LastModifiedBy = User.Identity!.Name;

                    // ذخیره نامه
                    var result = await _letterService.CreateLetterAsync(letter);
                    if (result.Success)
                    {
                        // اعلان برای گیرندگان سازمانی و رونوشت‌ها
                        try
                        {
                            // گیرنده اصلی
                            if (!string.IsNullOrWhiteSpace(letter.Receiver) && int.TryParse(letter.Receiver, out int recvOrgId))
                            {
                                var recvUsers = await _context.UserOrganizations
                                    .Include(uo => uo.User)
                                    .Where(uo => uo.OrganizationId == recvOrgId && uo.IsActive && uo.User != null)
                                    .Select(uo => uo.UserId)
                                    .ToListAsync();
                                foreach (var uid in recvUsers.Distinct())
                                {
                                    await _notificationService.SendNotification(uid!, $"نامه جدید با موضوع '{letter.Subject}' به کارتابل شما رسیده است", Url.Action("KartableVaredeh", "Letter") ?? string.Empty);
                                }
                            }

                            // گیرندگان رونوشت
                            if (letter.CopyReceivers != null && letter.CopyReceivers.Count >= 1)
                            {
                                var receiverIds = letter.CopyReceivers.FirstOrDefault()?
                                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                    .Select(s => s.Trim())
                                    .Where(s => !string.IsNullOrWhiteSpace(s))
                                    .ToList();
                                if (receiverIds != null)
                                {
                                    foreach (var idStr in receiverIds)
                                    {
                                        if (int.TryParse(idStr, out int orgId))
                                        {
                                            var ccUsers = await _context.UserOrganizations
                                                .Include(uo => uo.User)
                                                .Where(uo => uo.OrganizationId == orgId && uo.IsActive && uo.User != null)
                                                .Select(uo => uo.UserId)
                                                .ToListAsync();
                                            var extraNote = ccNotes.ContainsKey(orgId) ? ccNotes[orgId] : null;
                                            var notifMsg = string.IsNullOrWhiteSpace(extraNote)
                                                ? $"رونوشت نامه با موضوع '{letter.Subject}' به کارتابل شما افزوده شد"
                                                : $"رونوشت نامه با موضوع '{letter.Subject}' به کارتابل شما افزوده شد - پیام: {extraNote}";

                                            foreach (var uid in ccUsers.Distinct())
                                            {
                                                await _notificationService.SendNotification(uid!, notifMsg, Url.Action("KartableVaredeh", "Letter") ?? string.Empty);
                                            }
                                        }
                                    }
                                }
                            }

                            // ثبت یادداشت‌های رونوشت‌ها به عنوان اکشن‌های نامه برای ردیابی
                            if (ccNotes.Any())
                            {
                                var actions = new List<LetterAction>();
                                foreach (var kv in ccNotes)
                                {
                                    actions.Add(new LetterAction
                                    {
                                        LetterId = ((Letter)result.Data!).Id,
                                        UserId = currentUserIdForActions ?? string.Empty,
                                        ActionDescription = $"CCNote|OrgId={kv.Key}|{kv.Value}",
                                        ActionDate = DateTime.Now
                                    });
                                }
                                if (actions.Count > 0)
                                {
                                    _context.LetterActions.AddRange(actions);
                                    await _context.SaveChangesAsync();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error sending notifications for new letter");
                        }

                        TempData["SuccessMessage"] = result.Message;
                        return RedirectToAction(nameof(KartableSadereh));
                    }

                    TempData["ErrorMessage"] = result?.Message;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating new letter");
                    TempData["ErrorMessage"] = "خطا در ایجاد نامه. لطفاً دوباره تلاش کنید.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "اطلاعات وارد شده معتبر نیستند.";
            }

            ViewBag.ParentOrganizations = _context.Organizations
                .Where(o => o.IsActive)
                .ToList();
            return View(letter);
        }

        [HttpGet("EditLetter")]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _letterService.GetLetterByIdAsync(id);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(KartableVaredeh));
            }

            var letter = (Letter)result.Data!;
            
            if (!string.IsNullOrEmpty(letter.AttachmentName))
            {
                // تبدیل رشته پیوست‌ها به لیست صحیح (جداکننده اصلی "," و برای هر مورد ممکن است "|" بین id و نام باشد)
                var attachments = letter.AttachmentName
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(token => token.Trim())
                    .Where(token => !string.IsNullOrWhiteSpace(token))
                    .Select(token =>
                    {
                        var parts = token.Split('|');
                        var id = parts[0];
                        var name = parts.Length > 1 ? parts[1] : parts[0];
                        return new { FileName = name, FilePath = id };
                    })
                    .ToList();

                ViewBag.ExistingAttachments = attachments;
            }

            var firstCopyReceivers = letter.CopyReceivers?.FirstOrDefault();
            if (!string.IsNullOrEmpty(firstCopyReceivers))
            {
                letter.CopyReceiversList.Clear();
                var recivers = firstCopyReceivers.Split(',', StringSplitOptions.None).ToList();
                foreach (var item in recivers)
                {
                    var org = await _context.Organizations.FirstOrDefaultAsync(x => x.Id == int.Parse(item));
                    if (org != null)
                    {
                        letter.CopyReceiversList.Add(org);
                    }
                }
            }

            // دیگر نیاز به ViewBag.CCNotes نیست؛ از Model.CopyReceivers[1] استفاده می‌کنیم

            ViewBag.ParentOrganizations = await _context.Organizations
                .Where(o => o.IsActive)
                .ToListAsync();

            return View(letter);
        }

        [HttpPost("EditLetter")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Letter letter)
        {
            if (id != letter.Id)
            {
                TempData["ErrorMessage"] = "نامه پیدا نشد و یا حذف شده است";

                return RedirectToAction(nameof(KartableVaredeh));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // دریافت نامه فعلی از دیتابیس
                    var existingLetter = await _context.Letters
                        .Include(l => l.CopyReceiversList)
                        .FirstOrDefaultAsync(l => l.Id == id);

                    if (existingLetter == null)
                    {
                        return NotFound();
                    }
                    var user = await _userManager.FindByIdAsync(letter.SignerName!);
                    // به روزرسانی فیلدها
                    existingLetter.CorrespondenceType = letter.CorrespondenceType;
                    existingLetter.Classification = letter.Classification;
                    existingLetter.Priority = letter.Priority;
                    existingLetter.LetterRelationType = letter.LetterRelationType;
                    existingLetter.RelatedLetterId = letter.RelatedLetterId;
                    existingLetter.FollowUpDate = letter.FollowUpDate;
                    existingLetter.ShowOnDashboard = letter.ShowOnDashboard;
                    existingLetter.FileCode = letter.FileCode;
                    existingLetter.FileDescription = letter.FileDescription;
                    existingLetter.Subject = letter.Subject;
                    existingLetter.Content = letter.Content;
                    existingLetter.SignerName = $"{user!.FirstName} {user.LastName}";
                    existingLetter.SignerPosition = letter.SignerPosition;
                    existingLetter.Receiver = letter.Receiver;
                    existingLetter.Keywords = letter.Keywords;
                    existingLetter.CopyReceivers = letter.CopyReceivers;
                    
                    // مدیریت پیوست‌ها: فقط تغییرات اعمال شود؛ از JSON AttachmentsData در فرم استفاده می‌کنیم
                    var attachmentsJson = Request.Form["AttachmentsData"].FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(attachmentsJson))
                    {
                        try
                        {
                            var jArray = Newtonsoft.Json.Linq.JArray.Parse(attachmentsJson);
                            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                            var ordered = new List<string>();
                            foreach (var att in jArray)
                            {
                                var attId = att["id"]?.ToString()?.Trim();
                                if (string.IsNullOrWhiteSpace(attId)) continue;
                                if (!seen.Add(attId)) continue; // جلوگیری از تکرار
                                var name = att["name"]?.ToString()?.Trim();
                                ordered.Add(!string.IsNullOrWhiteSpace(name) ? $"{attId}|{name}" : attId);
                            }
                            existingLetter.AttachmentName = string.Join(',', ordered);
                        }
                        catch
                        {
                            // اگر JSON معتبر نبود، به مقدار رشته‌ای ارسالی بسنده می‌کنیم ولی آن را نرمال‌سازی می‌کنیم
                            if (!string.IsNullOrWhiteSpace(letter.AttachmentName))
                            {
                                var normalized = letter.AttachmentName
                                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                    .Select(t => t.Trim())
                                    .Where(t => !string.IsNullOrWhiteSpace(t))
                                    .Select(t =>
                                    {
                                        var parts = t.Split('|');
                                        var fileId = parts[0];
                                        var name = parts.Length > 1 ? parts[1] : null;
                                        return !string.IsNullOrWhiteSpace(name) ? $"{fileId}|{name}" : fileId;
                                    })
                                    .Distinct(StringComparer.OrdinalIgnoreCase);
                                existingLetter.AttachmentName = string.Join(',', normalized);
                            }
                            // در غیر این صورت پیوست‌های قبلی را حفظ کن
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(letter.AttachmentName))
                    {
                        // بدون JSON، از رشته ارسالی استفاده و نرمال‌سازی می‌کنیم
                        var normalized = letter.AttachmentName
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(t => t.Trim())
                            .Where(t => !string.IsNullOrWhiteSpace(t))
                            .Select(t =>
                            {
                                var parts = t.Split('|');
                                var fileId = parts[0];
                                var name = parts.Length > 1 ? parts[1] : null;
                                return !string.IsNullOrWhiteSpace(name) ? $"{fileId}|{name}" : fileId;
                            })
                            .Distinct(StringComparer.OrdinalIgnoreCase);
                        existingLetter.AttachmentName = string.Join(',', normalized);
                    }
                    // اگر هیچ کدام نبود، پیوست‌های قبلی بدون تغییر باقی می‌مانند

                    // مدیریت گیرندگان رونوشت
                    if (!letter.CopyReceivers!.FirstOrDefault().IsNullOrEmpty())
                    {
                        existingLetter.CopyReceiversList.Clear();

                        var receiverIds = letter.CopyReceivers!.FirstOrDefault()!.Split(',', StringSplitOptions.None);
                        foreach (string idStr in receiverIds)
                        {
                            if (int.TryParse(idStr.Trim(), out int orgId))
                            {
                                var org = await _context.Organizations.FirstOrDefaultAsync(x => x.Id == orgId);
                                if (org != null)
                                {
                                    existingLetter.CopyReceiversList.Add(org);
                                }
                            }
                        }
                    }

                    // به روزرسانی اطلاعات ویرایش
                    existingLetter.LastModifiedDate = DateTime.Now;
                    existingLetter.LastModifiedBy = User.Identity?.Name;

                    var result = await _letterService.UpdateLetterAsync(existingLetter);
                    if (result.Success)
                    {
                        TempData["SuccessMessage"] = result.Message;
                        return RedirectToAction("KartableSadereh");
                    }

                    TempData["ErrorMessage"] = result.Message;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error editing letter");
                    TempData["ErrorMessage"] = "خطا در ویرایش نامه. لطفاً دوباره تلاش کنید.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "اطلاعات وارد شده معتبر نیستند.";
            }

            ViewBag.ParentOrganizations = await _context.Organizations
                .Where(o => o.IsActive)
                .ToListAsync();
            return View(letter);
        }


        [HttpGet("DeleteLetter")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _letterService.GetLetterByIdAsync(id);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            return View(result.Data);
        }

        [HttpPost("DeleteLetter")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _letterService.DeleteLetterAsync(id);
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(KartableVaredeh));
        }

        [HttpPost("Search")]
        public async Task<IActionResult> SearchLetters([FromBody] LetterSearchModel model)
        {
            try
            {
                // اعتبارسنجی مدل
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "داده‌های ارسالی معتبر نیستند",
                        errors = ModelState.Values.SelectMany(v => v.Errors)
                    });
                }

                // استخراج شناسه سازمان کاربر جاری (در صورت وجود)
                int? userOrgId = null;
                var currentUserId = _userManager.GetUserId(User);
                if (!string.IsNullOrEmpty(currentUserId))
                {
                    var userOrg = await _context.UserOrganizations
                        .Include(uo => uo.Organization)
                        .FirstOrDefaultAsync(uo => uo.UserId == currentUserId && uo.IsActive);
                    if (userOrg?.Organization != null)
                    {
                        userOrgId = userOrg.Organization.Id;
                    }
                }

                var results = await _letterService.SearchLettersAsync(new LetterSearchParams
                {
                    Subject = model.Subject,
                    LetterNumber = model.LetterNumber,
                    FromDate = model.FromDate,
                    ToDate = model.ToDate,
                    CurrentUsername = User.Identity!.Name,
                    UserOrganizationId = userOrgId
                });


                return Ok(new
                {
                    Success = true,
                    Message = "جستجو با موفقیت انجام شد",
                    Data = results // ارسال مستقیم نتایج بدون لایه اضافی
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در انجام جستجو",
                    error = ex.Message
                });
            }
        }


        [HttpPost("UploadFile")]
        [RequestSizeLimit(500 * 1024 * 1024)] // 500 MB
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "فایلی انتخاب نشده است." });
            }

            if (file.Length > 500 * 1024 * 1024) // 500 MB
            {
                return Json(new { success = false, message = "حجم فایل نباید بیش از 500 مگابایت باشد." });
            }

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "PeyvastNameha");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return Json(new { success = true, filePath = uniqueFileName });
        }


        public class LetterSearchModel
        {
            public string? Subject { get; set; }
            public string? LetterNumber { get; set; }
            public string? FromDate { get; set; }
            public string? ToDate { get; set; }
        }

        public class KeywordItem
        {
            public string? Key { get; set; }
            public string? Value { get; set; }
        }



        [HttpGet("AdvancedSearch")]
        public IActionResult AdvancedSearch()
        {
            // پر کردن لیست‌های dropdown
            ViewBag.Priorities = Enum.GetValues(typeof(PriorityType)).Cast<PriorityType>();
            ViewBag.Classifications = Enum.GetValues(typeof(ClassificationType)).Cast<ClassificationType>();
            ViewBag.Statuses = Enum.GetValues(typeof(LetterStatus)).Cast<LetterStatus>();
            ViewBag.CorrespondenceTypes = Enum.GetValues(typeof(CorrespondenceType)).Cast<CorrespondenceType>();

            return View();
        }

        [HttpPost("AdvancedSearch")]
        public async Task<IActionResult> AdvancedSearch(AdvancedLetterSearchParams searchParams)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "حداقل باید یکی از شروط جستجو پر باشد.",
                    });
                }
                // استخراج شناسه سازمان کاربر جاری (در صورت وجود)
                int? userOrgId = null;
                var currentUserId = _userManager.GetUserId(User);
                if (!string.IsNullOrEmpty(currentUserId))
                {
                    var userOrg = await _context.UserOrganizations
                        .Include(uo => uo.Organization)
                        .FirstOrDefaultAsync(uo => uo.UserId == currentUserId && uo.IsActive);
                    if (userOrg?.Organization != null)
                    {
                        userOrgId = userOrg.Organization.Id;
                    }
                }

                // محدودسازی نتایج بر اساس کاربر جاری
                searchParams.CurrentUsername = User.Identity!.Name;
                searchParams.UserOrganizationId = userOrgId;

                var result = await _letterService.AdvancedSearchLettersAsync(searchParams);

                if (!result.Success)
                    return BadRequest(result.Message);

                var res = (List<AdvancedLetterSearchResult>)result.Data!;
                foreach (AdvancedLetterSearchResult item in res)
                {
                    item.Receiver = await _recivers.GetReceiverFullPath(item.Receiver!);
                    item.Sender = await _recivers.GetReceiverFullPath(item.Sender!);
                }
                return Ok(new
                {
                    success = result.Success,
                    message = result.Message,
                    data = res // اگر وجود دارد
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in advanced search");
                return StatusCode(500, "خطا در انجام جستجو");
            }
        }
        [HttpPost]
        public JsonResult DeleteFile([FromBody] DeleteFileRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.FileId) || string.IsNullOrEmpty(request.FileName))
            {
                return Json(new { success = false, message = "اطلاعات فایل معتبر نیست." });
            }

            // مسیر فایل را تشکیل دهید
            var uploadsPath = Path.Combine(_environment.WebRootPath, "PeyvastNameha");
            var filePath = Path.Combine(uploadsPath, request.FileId);

            // بررسی وجود فایل
            if (System.IO.File.Exists(filePath))
            {
                // حذف فایل از دیسک
                System.IO.File.Delete(filePath);
            }

            // حذف فایل از AttachmentName نامه
            var letters = _context.Letters.Where(l => l.AttachmentName != null && l.AttachmentName.Contains(request.FileName)).ToList();
            foreach (var letter in letters)
            {
                if (string.IsNullOrEmpty(letter.AttachmentName))
                {
                    continue;
                }
                var attachments = letter.AttachmentName.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
                attachments.RemoveAll(a => a.Contains(request.FileName));
                letter.AttachmentName = string.Join(",", attachments);
            }
            _context.SaveChanges();

            return Json(new { success = true, message = "فایل با موفقیت حذف شد." });
        }

    [HttpGet("Attachment/Download")]
        public async Task<IActionResult> DownloadAttachment(int letterId, string fileId, string? downloadName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileId))
                {
                    return Ok(new { success = false, exists = false, message = "شناسه فایل معتبر نیست." });
                }

                var letter = await _context.Letters.FindAsync(letterId);
                if (letter == null)
                {
                    return Ok(new { success = false, exists = false, message = "نامه یافت نشد." });
                }

                // بررسی وجود توکن فایل در پیوست‌های نامه (پشتیبانی از هر دو قالب: fileId|displayName یا displayName|fileId)
                var hasToken = !string.IsNullOrEmpty(letter.AttachmentName) &&
                               letter.AttachmentName
                                   .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                   .Select(t => t.Trim())
                                   .Any(t =>
                                   {
                                       var parts = t.Split('|');
                                       if (parts.Length == 1)
                                       {
                                           return parts[0].Equals(fileId, StringComparison.OrdinalIgnoreCase) ||
                                                  parts[0].StartsWith(fileId, StringComparison.OrdinalIgnoreCase);
                                       }
                                       var p0 = parts[0].Trim();
                                       var p1 = parts[1].Trim();
                                       return p0.Equals(fileId, StringComparison.OrdinalIgnoreCase) ||
                                              p1.Equals(fileId, StringComparison.OrdinalIgnoreCase);
                                   });
                if (!hasToken)
                {
                    return Ok(new { success = false, exists = false, message = "فایل در پیوست‌های این نامه یافت نشد یا حذف شده است." });
                }

                // بررسی وجود فیزیکی فایل
                var filePath = Path.Combine(_environment.WebRootPath, "PeyvastNameha", fileId);
                if (!System.IO.File.Exists(filePath))
                {
                    return Ok(new { success = false, exists = false, message = "این فایل در سرور موجود نیست یا حذف شده است." });
                }

                // موفق: لینک دانلود استریم را برگردان
                var url = Url.Action(nameof(DownloadAttachmentFile), new { letterId, fileId, downloadName });
                return Ok(new { success = true, exists = true, message = "فایل آماده دانلود است.", downloadUrl = url });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preparing attachment download {FileId} for letter {LetterId}", fileId, letterId);
                return Ok(new { success = false, exists = false, message = "خطا در آماده‌سازی دانلود فایل." });
            }
        }

        [HttpGet("Attachment/File")]
        public async Task<IActionResult> DownloadAttachmentFile(int letterId, string fileId, string? downloadName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileId))
                {
                    return BadRequest("شناسه فایل نامعتبر است.");
                }

                var letter = await _context.Letters.FindAsync(letterId);
                if (letter == null)
                {
                    return NotFound("نامه یافت نشد.");
                }

                var hasToken = !string.IsNullOrEmpty(letter.AttachmentName) &&
                               letter.AttachmentName
                                   .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                   .Select(t => t.Trim())
                                   .Any(t =>
                                   {
                                       var parts = t.Split('|');
                                       if (parts.Length == 1)
                                       {
                                           return parts[0].Equals(fileId, StringComparison.OrdinalIgnoreCase) ||
                                                  parts[0].StartsWith(fileId, StringComparison.OrdinalIgnoreCase);
                                       }
                                       var p0 = parts[0].Trim();
                                       var p1 = parts[1].Trim();
                                       return p0.Equals(fileId, StringComparison.OrdinalIgnoreCase) ||
                                              p1.Equals(fileId, StringComparison.OrdinalIgnoreCase);
                                   });
                if (!hasToken)
                {
                    return NotFound("فایل در پیوست‌های این نامه یافت نشد یا حذف شده است.");
                }

                var filePath = Path.Combine(_environment.WebRootPath, "PeyvastNameha", fileId);
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("این فایل در سرور موجود نیست یا حذف شده است.");
                }

                var contentType = GetContentType(fileId);
                var memory = new MemoryStream();
                await using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;
                var downloadFileName = string.IsNullOrWhiteSpace(downloadName) ? fileId : downloadName;
                return File(memory, contentType, downloadFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error streaming attachment {FileId} for letter {LetterId}", fileId, letterId);
                return StatusCode(500, "خطای داخلی هنگام دانلود فایل");
            }
        }

        // دانلود پیوست در حالت ایجاد نامه (بدون وابستگی به LetterId)
        [HttpGet("Attachment/DownloadTemp")]
        public IActionResult DownloadAttachmentTemp(string fileId, string? downloadName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileId))
                {
                    return Ok(new { success = false, exists = false, message = "شناسه فایل معتبر نیست." });
                }

                var filePath = Path.Combine(_environment.WebRootPath, "PeyvastNameha", fileId);
                if (!System.IO.File.Exists(filePath))
                {
                    return Ok(new { success = false, exists = false, message = "این فایل در سرور موجود نیست یا حذف شده است." });
                }

                var url = Url.Action(nameof(DownloadAttachmentFileTemp), new { fileId, downloadName });
                return Ok(new { success = true, exists = true, message = "فایل آماده دانلود است.", downloadUrl = url });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preparing temp attachment download {FileId}", fileId);
                return Ok(new { success = false, exists = false, message = "خطا در آماده‌سازی دانلود فایل." });
            }
        }

        [HttpGet("Attachment/FileTemp")]
        public IActionResult DownloadAttachmentFileTemp(string fileId, string? downloadName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileId))
                {
                    return BadRequest("شناسه فایل نامعتبر است.");
                }

                var filePath = Path.Combine(_environment.WebRootPath, "PeyvastNameha", fileId);
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("این فایل در سرور موجود نیست یا حذف شده است.");
                }

                var contentType = GetContentType(fileId);
                var memory = new MemoryStream();
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    stream.CopyTo(memory);
                }
                memory.Position = 0;
                var downloadFileName = string.IsNullOrWhiteSpace(downloadName) ? fileId : downloadName;
                return File(memory, contentType, downloadFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error streaming temp attachment {FileId}", fileId);
                return StatusCode(500, "خطای داخلی هنگام دانلود فایل");
            }
        }

        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".txt" => "text/plain",
                ".zip" => "application/zip",
                ".rar" => "application/x-rar-compressed",
                _ => "application/octet-stream"
            };
        }

    [HttpGet("Attachment/List")]
    public async Task<IActionResult> GetLetterAttachments(int letterId)
        {
            try
            {
                var letter = await _context.Letters.FindAsync(letterId);
                if (letter == null)
                {
                    return Ok(new { success = false, message = "نامه یافت نشد." });
                }

                if (string.IsNullOrWhiteSpace(letter.AttachmentName))
                {
                    return Ok(new { success = true, data = Array.Empty<object>() });
                }

                var uploadsPath = Path.Combine(_environment.WebRootPath, "PeyvastNameha");
                var items = new List<object>();

                var tokens = letter.AttachmentName
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrWhiteSpace(t));

                foreach (var token in tokens)
                {
                    var parts = token.Split('|');
                    var fileId = parts[0];
                    var displayName = parts.Length > 1 ? parts[1] : parts[0];

                    var physicalPath = Path.Combine(uploadsPath, fileId);
                    long fileSize = 0;
                    if (System.IO.File.Exists(physicalPath))
                    {
                        var fi = new FileInfo(physicalPath);
                        fileSize = fi.Length;
                    }

                    var webPath = Url.Content($"~/PeyvastNameha/{fileId}");
                    items.Add(new { fileId = fileId, fileName = displayName, filePath = webPath, fileSize });
                }

                return Ok(new { success = true, data = items });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting letter attachments for {LetterId}", letterId);
                return StatusCode(500, new { success = false, message = "خطا در دریافت پیوست‌ها." });
            }
        }

        // ==================== DeepSeek AI Integration ====================
        public class GenerateContentRequest
        {
            public string? Prompt { get; set; }
            public string? CurrentContent { get; set; }
            public string? Subject { get; set; }
            public string? Tone { get; set; }
            public bool Refine { get; set; } = false;
            public string? Provider { get; set; } // deepseek | openrouter | openai | xai
            public string? Model { get; set; } // specific model name
        }

        public class GenerateContentResponse
        {
            public bool Success { get; set; }
            public string? Content { get; set; }
            public string? Message { get; set; }
        }

        [HttpPost("GenerateContent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateContent([FromBody] GenerateContentRequest request)
        {
            if (request == null || (string.IsNullOrWhiteSpace(request.Prompt) && string.IsNullOrWhiteSpace(request.CurrentContent)))
            {
                return BadRequest(new GenerateContentResponse { Success = false, Message = "Prompt یا متن فعلی الزامی است." });
            }

            // Select provider (default to OpenRouter per request)
            var provider = (request.Provider ?? "openrouter").ToLowerInvariant();
            string? apiKey;
            string baseUrl;
            string model;
            string path;

            switch (provider)
            {
                case "openrouter":
                    apiKey = _configuration["OpenRouter:ApiKey"]; // Uses HTTP header: Authorization: Bearer
                    baseUrl = _configuration["OpenRouter:BaseUrl"] ?? "https://openrouter.ai/api";
                    if (!baseUrl.Contains("/api"))
                    {
                        baseUrl = baseUrl.TrimEnd('/') + "/api";
                    }
                    model = string.IsNullOrWhiteSpace(request.Model) ? (_configuration.GetSection("OpenRouter:AllowedModels").Get<string[]>()?.FirstOrDefault() ?? "openrouter/anthropic/claude-3.5-sonnet") : request.Model;
                    var endsWithV1 = baseUrl.TrimEnd('/').EndsWith("/v1", StringComparison.OrdinalIgnoreCase);
                    path = endsWithV1 ? "chat/completions" : "v1/chat/completions";
                    break;
                case "openai":
                    apiKey = _configuration["OpenAI:ApiKey"];
                    baseUrl = _configuration["OpenAI:BaseUrl"] ?? "https://api.openai.com";
                    model = string.IsNullOrWhiteSpace(request.Model) ? (_configuration.GetSection("OpenAI:AllowedModels").Get<string[]>()?.FirstOrDefault() ?? "gpt-4o-mini") : request.Model;
                    path = "v1/chat/completions";
                    break;
                case "xai":
                    apiKey = _configuration["XAI:ApiKey"];
                    baseUrl = _configuration["XAI:BaseUrl"] ?? "https://api.x.ai";
                    model = string.IsNullOrWhiteSpace(request.Model) ? (_configuration.GetSection("XAI:AllowedModels").Get<string[]>()?.FirstOrDefault() ?? "grok-2") : request.Model;
                    path = "v1/chat/completions";
                    break;
                default:
                    apiKey = _configuration["DeepSeek:ApiKey"];
                    baseUrl = _configuration["DeepSeek:BaseUrl"] ?? "https://api.deepseek.com";
                    model = string.IsNullOrWhiteSpace(request.Model) ? (_configuration["DeepSeek:Model"] ?? "deepseek-chat") : request.Model;
                    path = "v1/chat/completions";
                    break;
            }

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return StatusCode(500, new GenerateContentResponse { Success = false, Message = "کلید API برای ارائه‌دهنده انتخاب‌شده تنظیم نشده است." });
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                var baseAddr = baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/";
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (provider == "openrouter")
                {
                    // OpenRouter requires additional headers
                    var confReferer = _configuration["OpenRouter:Referer"];
                    var refererVal = string.IsNullOrWhiteSpace(confReferer)
                        ? (Request?.Headers["Origin"].FirstOrDefault() ?? Request?.Host.Value ?? "https://your-app")
                        : confReferer;
                    var confTitle = _configuration["OpenRouter:Title"];
                    var titleVal = string.IsNullOrWhiteSpace(confTitle) ? "TPL Letter AI" : confTitle;
                    if (!client.DefaultRequestHeaders.Contains("HTTP-Referer"))
                        client.DefaultRequestHeaders.Add("HTTP-Referer", refererVal);
                    if (!client.DefaultRequestHeaders.Contains("Referer"))
                        client.DefaultRequestHeaders.Add("Referer", refererVal);
                    if (!client.DefaultRequestHeaders.Contains("X-Title"))
                        client.DefaultRequestHeaders.Add("X-Title", titleVal);
                }

                var systemContent =
                    "شما یک دستیار نگارنده نامه اداری فارسی هستید. متن‌هایی فاخر، رسمی، روان و ادبی تولید کنید. از هرگونه مقدمه یا برچسب اضافه (مثل نام مدل یا توضیح سیستم) خودداری کنید و فقط بدنه نهایی نامه را برگردانید. از تیترهای غیرضروری پرهیز کنید. خروجی با پاراگراف‌های منظم و علائم نگارشی مناسب باشد.";

                var userPrompt = new System.Text.StringBuilder();
                if (request.Refine && !string.IsNullOrWhiteSpace(request.CurrentContent))
                {
                    userPrompt.AppendLine("لطفاً متن زیر را بازنویسی و بهبود بده به سبکی رسمی، فاخر و مؤدبانه. فقط متن نهایی را برگردان:");
                    userPrompt.AppendLine("--- متن فعلی ---");
                    userPrompt.AppendLine(request.CurrentContent);
                    userPrompt.AppendLine("-----------------");
                }
                else if (!string.IsNullOrWhiteSpace(request.Prompt))
                {
                    userPrompt.AppendLine("لطفاً بر اساس شرح زیر یک متن نامه رسمی و فاخر بنویس و فقط متن نهایی را برگردان:");
                    if (!string.IsNullOrWhiteSpace(request.Subject))
                    {
                        userPrompt.AppendLine($"موضوع: {request.Subject}");
                    }
                    userPrompt.AppendLine($"شرح درخواست: {request.Prompt}");
                }

                if (!string.IsNullOrWhiteSpace(request.Tone))
                {
                    userPrompt.AppendLine($"لحن پیشنهادی: {request.Tone}");
                }

                object systemMsgContent;
                object userMsgContent;
                if (provider == "openrouter")
                {
                    systemMsgContent = new object[] { new { type = "text", text = systemContent } };
                    userMsgContent = new object[] { new { type = "text", text = userPrompt.ToString() } };
                }
                else
                {
                    systemMsgContent = systemContent;
                    userMsgContent = userPrompt.ToString();
                }

                object payload;
                if (provider == "openrouter" || provider == "openai")
                {
                    payload = new
                    {
                        model = model,
                        messages = new object[]
                        {
                            new { role = "system", content = systemMsgContent },
                            new { role = "user", content = userMsgContent }
                        },
                        temperature = 0.7,
                        max_tokens = 1024,
                        response_format = new { type = "text" }
                    };
                }
                else
                {
                    payload = new
                    {
                        model = model,
                        messages = new object[]
                        {
                            new { role = "system", content = systemMsgContent },
                            new { role = "user", content = userMsgContent }
                        },
                        temperature = 0.7,
                        max_tokens = 1024
                    };
                }

                var httpContent = new StringContent(JsonConvert.SerializeObject(payload), System.Text.Encoding.UTF8, "application/json");
                var endpoint = baseAddr.TrimEnd('/') + "/" + path.TrimStart('/');
                var response = await client.PostAsync(endpoint, httpContent);

                var contentType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;
                var rawText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    var snippet = rawText?.Length > 400 ? rawText.Substring(0, 400) + "…" : rawText;
                    _logger.LogError("AI provider '{Provider}' error {StatusCode}. Body: {Body}", provider, response.StatusCode, snippet);
                    return StatusCode((int)response.StatusCode, new GenerateContentResponse
                    {
                        Success = false,
                        Message = $"خطا از سرویس {provider}. کد {(int)response.StatusCode}. {snippet}"
                    });
                }

                var trimmed = rawText?.TrimStart();
                if (string.IsNullOrWhiteSpace(trimmed) || contentType.Contains("json", StringComparison.OrdinalIgnoreCase) == false || trimmed.StartsWith("<"))
                {
                    var snippet = rawText?.Length > 400 ? rawText.Substring(0, 400) + "…" : rawText;
                    _logger.LogError("AI provider '{Provider}' returned non-JSON content-type '{ContentType}'. Body: {Body}", provider, contentType, snippet);
                    return Ok(new GenerateContentResponse { Success = false, Message = "پاسخ نامعتبر از سرویس هوش مصنوعی دریافت شد." });
                }

                string? content = null;
                try
                {
                    dynamic? parsed = JsonConvert.DeserializeObject(rawText!);
                    content = parsed?.choices?[0]?.message?.content?.ToString();
                }
                catch (JsonReaderException jex)
                {
                    _logger.LogError(jex, "JSON parse failed for provider '{Provider}'. Body starts with: {Start}", provider, trimmed?.Substring(0, Math.Min(120, trimmed.Length)));
                    return Ok(new GenerateContentResponse { Success = false, Message = "پاسخ سرویس قابل پردازش نبود." });
                }

                if (string.IsNullOrWhiteSpace(content))
                {
                    return Ok(new GenerateContentResponse { Success = false, Message = "پاسخی دریافت نشد." });
                }

                return Ok(new GenerateContentResponse { Success = true, Content = content.Trim() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeepSeek GenerateContent failed");
                return StatusCode(500, new GenerateContentResponse { Success = false, Message = "بروز خطا در تولید متن." });
            }
        }

        // List providers and models with available API keys
        [HttpGet("Models")] 
        public IActionResult GetModels()
        {
            var items = new List<object>();

            // DeepSeek
            if (!string.IsNullOrWhiteSpace(_configuration["DeepSeek:ApiKey"]))
            {
                items.Add(new { provider = "deepseek", display = "DeepSeek", models = new[] { _configuration["DeepSeek:Model"] ?? "deepseek-chat" } });
            }

            // OpenRouter
            if (!string.IsNullOrWhiteSpace(_configuration["OpenRouter:ApiKey"]))
            {
                var models = _configuration.GetSection("OpenRouter:AllowedModels").Get<string[]>() ?? Array.Empty<string>();
                items.Add(new { provider = "openrouter", display = "OpenRouter", models });
            }

            // OpenAI
            if (!string.IsNullOrWhiteSpace(_configuration["OpenAI:ApiKey"]))
            {
                var models = _configuration.GetSection("OpenAI:AllowedModels").Get<string[]>() ?? Array.Empty<string>();
                items.Add(new { provider = "openai", display = "OpenAI", models });
            }

            // XAI (Grok)
            if (!string.IsNullOrWhiteSpace(_configuration["XAI:ApiKey"]))
            {
                var models = _configuration.GetSection("XAI:AllowedModels").Get<string[]>() ?? Array.Empty<string>();
                items.Add(new { provider = "xai", display = "xAI (Grok)", models });
            }

            return Ok(new { success = true, data = items });
        }
    }

    public class DeleteFileRequest
    {
        public string? FileId { get; set; }
        public string? FileName { get; set; }
    }
}