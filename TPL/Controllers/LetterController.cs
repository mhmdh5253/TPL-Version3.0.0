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

namespace TPLWeb.Controllers
{
    [Authorize]
    [Route("Letter")]
    [SupportedOSPlatform("windows")]
    public class LetterController : Controller
    {
        private readonly ILetterService _letterService;
        private readonly Db _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<LetterController> _logger;
        private readonly BlNotification _notificationService;
        private readonly BlRecivers _recivers;

        public LetterController(ILetterService letterService, Db context, UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment, ILogger<LetterController> logger, BlRecivers recivers, BlNotification notificationService)
        {
            _letterService = letterService;
            _context = context;
            _userManager = userManager;
            _environment = environment;
            _logger = logger;
            _recivers = recivers;
            _notificationService = notificationService;
        }

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
        public async Task<IActionResult> ViewLetterPdf(int id)
        {
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
        public async Task<IActionResult> LetterGenerate(int id)
        {
            var model = await _letterService.GetLetterByIdAsync(id);
            var res = (Letter)model.Data!;
            var engine = new LetterTemplateEngine();
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
            string pathfinalCombine = Path.Combine(_environment.WebRootPath, "wordfiles", letterpath);
            // 4. تولید سند نهایی
            engine.GenerateLetterFromTemplate(
                templatePath: pathtemplatePath,
                outputPath: pathfinalCombine,
                textReplacements: textReplacements,
                tableReplacements: tableReplacements,
                imageReplacements: imageReplacements

            );

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
        public  IActionResult Index()
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
                                       x.Status==LetterStatus.Rejected || x.Status == LetterStatus.InReview ||
                                       x.Status == LetterStatus.Returned ).ToList();
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
                .FirstOrDefaultAsync(x => x.UserId == currentUser.Id && x.IsActive==true);

            if (userOrganization?.Organization == null || userOrganization.IsActive==false)
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
                !x.IsDeleted&&
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
            var let =await _context.Letters.Where(x => x.Id == letterid).FirstOrDefaultAsync();
            ViewBag.ParentOrganizations = _context.Organizations
                .Where(o => o.IsActive)
                .ToList();
            ViewBag.relatedtext  = $"{let!.LetterNumber}-{let.Subject}";
            return View(nameof(Create),new Letter()
            {
                LetterRelationType=LetterRelationType.پاسخ,
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
                        return RedirectToAction(nameof(KartableVaredeh));
                    }

                    TempData["ErrorMessage"] = result.Message;
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
            if (!letter.CopyReceivers!.FirstOrDefault().IsNullOrEmpty())
            {
                letter.CopyReceiversList.Clear();
                List<string> recivers = letter?.CopyReceivers?.FirstOrDefault()!.Split(',', StringSplitOptions.None).ToList()!;
                foreach (var item in recivers)
                {
                    letter!.CopyReceiversList.Add((await _context.Organizations.FirstOrDefaultAsync(x => x.Id == int.Parse(item)))!);
                }
            }
           
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
                    // مدیریت پیوست
                    if (!string.IsNullOrEmpty(letter.AttachmentName))
                    {
                        existingLetter.AttachmentName = letter.AttachmentName;
                    }

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
                        return RedirectToAction("KartableVaredeh");
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

        [HttpPost("RemoveAttachment")]
        public async Task<IActionResult> RemoveAttachment(int id)
        {
            try
            {
                var letter = await _context.Letters.FindAsync(id);
                if (letter == null)
                {
                    return Json(new { success = false, message = "نامه مورد نظر یافت نشد" });
                }

                // حذف فایل فیزیکی
                if (!string.IsNullOrEmpty(letter.AttachmentName))
                {
                    var filePath = Path.Combine(_environment.WebRootPath, "PeyvastNameha", letter.AttachmentName);
                    if (global::System.IO.File.Exists(filePath))
                    {
                        global::System.IO.File.Delete(filePath);
                    }
                }

                letter.AttachmentName = null;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "پیوست با موفقیت حذف شد" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing attachment");
                return Json(new { success = false, message = "خطا در حذف پیوست" });
            }
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
                    item.Receiver =await _recivers.GetReceiverFullPath(item.Receiver!);
                    item.Sender =await _recivers.GetReceiverFullPath(item.Sender!);
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


    }
}