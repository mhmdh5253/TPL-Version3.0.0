using BE;
using BE.LetterAutomation;
using BLL.LetterAutomation;
using BLL.Ticketing;
using DAL;
using DAL.LetterAutomation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace TPLWeb.Controllers
{

    [Authorize]
    public class LetterApprovalController : Controller
    {
        private readonly Db _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILetterApprovalService _approvalService;
        private readonly BlNotification _notificationService;
        private readonly BlLetter _letter;
        public LetterApprovalController(
            Db context,
            UserManager<ApplicationUser> userManager,
            ILetterApprovalService approvalService, BlLetter letter, BlNotification notificationService)
        {
            _context = context;
            _userManager = userManager;
            _approvalService = approvalService;
            _letter = letter;
            _notificationService = notificationService;
        }

        // GET: LetterApproval
        public async Task<IActionResult> Index(ApprovalStatus? status)
        {
            var currentUserId = _userManager.GetUserId(User);
            var query = _context.LetterApprovals
                .Include(la => la.Letter)
                .Include(la => la.Requester)
                .Include(la => la.Approver)
                .Include(la => la.Organization)
                .AsQueryable();

            // برای کاربران عادی فقط درخواست‌های مربوط به خودشان را ببینند
            if (!User.IsInRole("Admin"))
            {
                query = query.Where(la => la.ApproverUserId == currentUserId ||
                                          la.RequesterUserId == currentUserId);
            }

            if (status.HasValue)
            {
                query = query.Where(la => la.Status == status.Value);
            }
                                    
            var model = await query.Where(x => x.Status != ApprovalStatus.Approved)
                .OrderByDescending(la => la.RequestDate)
                .ToListAsync();

            return View(model);
        }

        // GET: LetterApproval/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var letterApproval = await _context.LetterApprovals
                .Include(la => la.Letter)
                .Include(la => la.Requester)
                .Include(la => la.Approver)
                .Include(la => la.Organization)
                .FirstOrDefaultAsync(m => m.Id == id);

            // اگر رکورد پیدا نشد، خطا برنگرداند و به صفحه NotFound هدایت شود
            if (letterApproval == null)
            {
                return NotFound();
            }

            // اگر شناسه کاربر درخواست‌کننده یا تاییدکننده نال بود، مقداردهی نشود تا خطا رخ ندهد
            if (!string.IsNullOrEmpty(letterApproval.RequesterUserId))
                letterApproval.Requester = await _userManager.FindByIdAsync(letterApproval.RequesterUserId);

            if (!string.IsNullOrEmpty(letterApproval.ApproverUserId))
                letterApproval.Approver = await _userManager.FindByIdAsync(letterApproval.ApproverUserId);

            // بررسی دسترسی کاربر
            var currentUserId = _userManager.GetUserId(User);
            if ((letterApproval.ApproverUserId != currentUserId &&
                letterApproval.RequesterUserId != currentUserId &&
                !User.IsInRole("Admin")) || currentUserId == null)
            {
                return Forbid();
            }

            return View(letterApproval);
        }

        // GET: LetterApproval/Process/5
        public async Task<IActionResult> Process(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var letterApproval = await _context.LetterApprovals
                .Include(la => la.Letter)
                .Include(la => la.Requester)
                .Include(la => la.Approver)
                .Include(la => la.Organization)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (letterApproval == null)
            {
                return NotFound();
            }

            // اگر تاییدکننده نال بود خطا نده
            var currentUserId = _userManager.GetUserId(User);
            if (letterApproval.ApproverUserId == null || currentUserId == null || letterApproval.ApproverUserId != currentUserId)
            {
                return Forbid();
            }

            if (letterApproval.Status != ApprovalStatus.Pending)
            {
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(letterApproval);
        }

        // POST: LetterApproval/Process/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Process(int id, string action, string comment)
        {
            var letterApproval = await _context.LetterApprovals
                .Include(la => la.Letter)
                .FirstOrDefaultAsync(la => la.Id == id);

            if (letterApproval == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            if (letterApproval.ApproverUserId == null || currentUserId == null || letterApproval.ApproverUserId != currentUserId)
            {
                return Forbid();
            }

            if (letterApproval.Status != ApprovalStatus.Pending)
            {
                return RedirectToAction(nameof(Details), new { id });
            }

            // پردازش اقدام کاربر
            switch (action)
            {
                case "approve":
                    letterApproval.Status = ApprovalStatus.Approved;
                    letterApproval.Comment = comment;
                    letterApproval.ActionDate = DateTime.Now;
                    // اگر Letter یا LetterNumber نال بود خطا نده
                    if (letterApproval.Letter?.LetterNumber.IsNullOrEmpty() == true)
                    {
                        letterApproval.Letter.LetterNumber = _letter.GenerateLetterNumber();
                    }
                    if (!await IsFinalApproval(letterApproval))
                    {
                        var nextApprover = await _approvalService.GetNextApprover(letterApproval.OrganizationId, letterApproval.ApproverUserId!);
                        if (nextApprover != null && !string.IsNullOrEmpty(nextApprover.UserId))
                        {
                            await _notificationService.SendNotification(nextApprover.UserId, "نامه جدید برای بررسی و تایید دارید", Url.Action("Details", "LetterApproval", new { id = letterApproval.Id })!);
                        }
                        await CreateNextLevelApproval(letterApproval);
                    }
                    else
                    {
                        if (letterApproval.Letter != null)
                            letterApproval.Letter.Status = LetterStatus.Completed;
                        if (!string.IsNullOrEmpty(letterApproval.RequesterUserId))
                        {
                            await _notificationService.SendNotification(letterApproval.RequesterUserId, "نامه شما تایید نهایی شد", Url.Action("Details", "LetterApproval", new { id = letterApproval.Id })!);
                        }
                    }
                    break;

                case "reject":
                    letterApproval.Status = ApprovalStatus.Rejected;
                    letterApproval.Comment = comment;
                    letterApproval.ActionDate = DateTime.Now;
                    if (letterApproval.Letter != null)
                        letterApproval.Letter.Status = LetterStatus.Rejected;
                    if (!string.IsNullOrEmpty(letterApproval.RequesterUserId))
                    {
                        await _notificationService.SendNotification(letterApproval.RequesterUserId, "نامه شما رد شد", Url.Action("Details", "LetterApproval", new { id = letterApproval.Id })!);
                    }
                    break;

                case "return":
                    letterApproval.Status = ApprovalStatus.Returned;
                    letterApproval.Comment = comment;
                    letterApproval.ActionDate = DateTime.Now;
                    await ReturnToPreviousLevel(letterApproval);
                    if (!string.IsNullOrEmpty(letterApproval.RequesterUserId))
                    {
                        await _notificationService.SendNotification(letterApproval.RequesterUserId, "نامه شما عودت داده شد", Url.Action("Details", "LetterApproval", new { id = letterApproval.Id })!);
                    }
                    break;

                default:
                    ModelState.AddModelError("", "عملیات نامعتبر است");
                    return View(letterApproval);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: LetterApproval/ApprovalPath/5
        public async Task<IActionResult> ApprovalPath(int letterId)
        {
            var approvals = await _context.LetterApprovals
                .Include(la => la.Approver)
                .Include(la => la.Organization)
                .Where(la => la.LetterId == letterId)
                .OrderBy(la => la.ApprovalLevel)
                .ToListAsync();

            if (approvals == null || !approvals.Any())
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            // اگر هیچکدام از کاربران مرتبط نبودند یا شناسه کاربر نال بود، خطا نده
            if (currentUserId == null || (!approvals.Any(la => la.RequesterUserId == currentUserId ||
                                     la.ApproverUserId == currentUserId) &&
                !User.IsInRole("Admin")))
            {
                return Forbid();
            }

            return View(approvals);
        }

        private async Task<bool> IsFinalApproval(LetterApproval approval)
        {
            // اگر approval یا LetterId نال بود، خطا نده و false برگردان
            if (approval == null)
                return false;

            var maxLevel = await _context.LetterApprovals
                .Where(la => la.LetterId == approval.LetterId)
                .MaxAsync(la => (int?)la.ApprovalLevel) ?? 0;

            return approval.ApprovalLevel >= maxLevel;
        }
            
        private async Task CreateNextLevelApproval(LetterApproval currentApproval)
        {
            if (currentApproval == null)
                return;

            var nextLevel = currentApproval.ApprovalLevel + 1;
            var nextApproval = await _context.LetterApprovals
                .FirstOrDefaultAsync(la => la.LetterId == currentApproval.LetterId &&
                                           la.ApprovalLevel == nextLevel);

            if (nextApproval == null)
            {
                var nextApprover = await _approvalService.GetNextApprover(
                    currentApproval.OrganizationId,
                    currentApproval.ApproverUserId!);

                if (nextApprover != null)
                {
                    var newApproval = new LetterApproval
                    {
                        LetterId = currentApproval.LetterId,
                        RequesterUserId = currentApproval.RequesterUserId,
                        ApproverUserId = nextApprover.UserId,
                        OrganizationId = nextApprover.OrganizationId,
                        ApprovalLevel = nextLevel,
                        Status = ApprovalStatus.Pending,
                        RequestDate = DateTime.Now
                    };

                    _context.LetterApprovals.Add(newApproval);
                }
            }
        }

        private async Task ReturnToPreviousLevel(LetterApproval currentApproval)
        {
            if (currentApproval == null)
                return;

            var prevLevel = currentApproval.ApprovalLevel - 1;
            if (prevLevel > 0)
            {
                var prevApproval = await _context.LetterApprovals
                    .FirstOrDefaultAsync(la => la.LetterId == currentApproval.LetterId &&
                                               la.ApprovalLevel == prevLevel);

                if (prevApproval != null)
                {
                    prevApproval.Status = ApprovalStatus.Pending;
                    prevApproval.ActionDate = null;
                    // اگر Approver یا UserName نال بود خطا نده
                    var approverName = currentApproval.Approver?.UserName ?? "نامشخص";
                    prevApproval.Comment += $"\nبازگشت داده شده توسط: {approverName}";
                }
            }
        }
    }
}