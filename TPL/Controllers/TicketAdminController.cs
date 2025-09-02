using BE;
using BE.Ticketing.SupportTicketSystem.Models;
using BLL.Ticketing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TPLWeb.Models.Ticketing;

namespace TPLWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("AdminTicketManagement")]
    public class AdminTicketController : Controller
    {
        private readonly BlTicket _ticketService;
        private readonly BlComment _commentService;
        private readonly BlAttachment _attachmentService;
        private readonly BlNotification _notificationService;
        private readonly BlCategory _category;
        private readonly BlTicketReport _ticketReport;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AdminTicketController> _logger;
        public AdminTicketController(BlTicket ticketService, BlComment commentService, BlAttachment attachmentService, BlNotification notificationService, BlCategory category, BlTicketReport ticketReport, UserManager<ApplicationUser> userManager, ILogger<AdminTicketController> logger)
        {
            _ticketService = ticketService;
            _commentService = commentService;
            _attachmentService = attachmentService;
            _notificationService = notificationService;
            _category = category;
            _ticketReport = ticketReport;
            _userManager = userManager;
            _logger = logger;
        }

        // داشبورد ادمین
        [HttpGet("AdminTicketDashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var model = new AdminDashboardViewModel
            {
                TotalTickets = await _ticketService.GetAllTickets(),
                Categories = await _category.GetAllCategories(),
                RecentTickets = (await _ticketService.GetAllTickets())
                    .OrderByDescending(t => t.CreatedDate)
                    .Take(10)
                    .ToList()
            };

            return View(model);
        }
        // مشاهده تیکت
        [HttpGet("viewticket")]
        public async Task<IActionResult> ViewTicket(int id)
        {
            var ticket = (await _ticketService.GetAllTickets()).FirstOrDefault(t => t.Id == id);
            if (ticket == null)
                return NotFound();

            var comments = await _commentService.GetCommentsByTicket(id.ToString());
            var attachments = await _attachmentService.GetAttachmentsByTicket(id.ToString());

            var model = new TicketViewModel
            {
                Ticket = ticket,
                Comments = comments,
                Attachments = attachments
            };

            return View(model);
        }
        // مدیریت تیکت‌ها
        [HttpGet("AdminTicketManage")]
        public async Task<IActionResult> ManageTickets()
        {
            var tickets = await _ticketService.GetAllTickets();
            ViewBag.Categories = await _category.GetAllCategories();
            return View(tickets);
        }

        // تغییر وضعیت تیکت
        [HttpPost("Changeticketstatus")]
        public async Task<IActionResult> ChangeTicketStatus(int ticketId, TicketStatus status)
        {
            var result = await _ticketService.ChangeStatus(ticketId.ToString(), status);
            if (result)
            {
                TempData["Success"] = "وضعیت تیکت با موفقیت تغییر کرد";
                // اعلان برای صاحب تیکت
                var ticket = (await _ticketService.GetAllTickets()).FirstOrDefault(t => t.Id == ticketId);
                if (ticket != null && !string.IsNullOrEmpty(ticket.CreatedByUserId))
                {
                    await _notificationService.SendNotification(ticket.CreatedByUserId!, $"وضعیت تیکت '{ticket.Title}' به '{status}' تغییر کرد", Url.Action("ViewTicket", "TicketUser", new { id = ticket.Id })!);
                }
            }
            else
                TempData["Error"] = "خطا در تغییر وضعیت تیکت";

            return RedirectToAction("ManageTickets");
        }

        // مدیریت دسته‌بندی‌ها
        [HttpGet("Managecategories")]
        public async Task<IActionResult> ManageCategories()
        {
            var categories = await _category.GetAllCategories();
            return View(categories);
        }

        // در AdminController.cs
        [HttpGet("Editticket")]
        public async Task<IActionResult> EditTicket(int id)
        {
            var ticket = (await _ticketService.GetAllTickets()).FirstOrDefault(t => t.Id == id);
            if (ticket == null)
            {
                return View();
            }

            ViewBag.Categories = new SelectList(await _category.GetAllCategories(), "Id", "Name", ticket.CategoryId);
            ViewBag.StatusList = new SelectList(Enum.GetValues(typeof(TicketStatus)), ticket.Status);
            ViewBag.PriorityList = new SelectList(Enum.GetValues(typeof(Priority)), ticket.Priority);

            return View(ticket);
        }

        [HttpPost("Editticket")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTicket(Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                var result = await _ticketService.Update(ticket);
                if (result)
                {
                    TempData["Success"] = "تیکت با موفقیت ویرایش شد";
                    // اعلان به کاربر ثبت‌کننده
                    if (!string.IsNullOrEmpty(ticket.CreatedByUserId))
                    {
                        await _notificationService.SendNotification(ticket.CreatedByUserId!, $"تیکت '{ticket.Title}' توسط ادمین به‌روزرسانی شد", Url.Action("ViewTicket", "TicketUser", new { id = ticket.Id })!);
                    }
                    return RedirectToAction("ManageTickets");
                }
                TempData["Error"] = "خطا در ویرایش تیکت";
            }

            ViewBag.Categories = new SelectList(await _category.GetAllCategories(), "Id", "Name", ticket.CategoryId);
            ViewBag.StatusList = new SelectList(Enum.GetValues(typeof(TicketStatus)), ticket.Status);
            ViewBag.PriorityList = new SelectList(Enum.GetValues(typeof(Priority)), ticket.Priority);

            return View(ticket);
        }


        // ایجاد دسته‌بندی جدید
        [HttpPost("Createcategory")]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            var result = await _category.CreateCategory(category);
            if (result)
                TempData["Success"] = "دسته‌بندی با موفقیت ایجاد شد";
            else
                TempData["Error"] = "خطا در ایجاد دسته‌بندی";

            return RedirectToAction("ManageCategories");
        }

        // گزارش‌گیری
        [HttpGet("Ticketrepors")]
        public async Task<IActionResult> Reports()
        {
            ViewBag.RecentReports = await _ticketReport.GetReportHistory();
            return View();
        }

        [HttpPost("Ticketrepors")]
        public async Task<IActionResult> GenerateStatusReport(DateTime startDate, DateTime endDate)
        {
            endDate = endDate.Date.AddDays(1).AddSeconds(-1);
            var userid = _userManager.Users.FirstOrDefault(x => x.UserName == User.Identity!.Name)!.Id;
            var report = await _ticketReport.GenerateStatusReport(startDate, endDate, userid);
            if (report != null)
            {
                ViewBag.StartDate = startDate;
                ViewBag.EndDate = endDate;
                return View("ReportResult", report);

            }
            return View("Reports");

        }

        [HttpGet("ReportResult")]
        public async Task<IActionResult> ReportResult(int id)
        {
            var model = await _ticketReport.GetReportById(id);
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            try
            {
                var result = await _category.DeleteCategory(categoryId);

                if (result)
                {
                    TempData["Success"] = "دسته‌بندی با موفقیت حذف شد";
                    _logger.LogInformation($"Category {categoryId} deleted by {User.Identity!.Name}");
                }
                else
                {
                    TempData["Error"] = "خطا در حذف دسته‌بندی. ممکن است تیکت‌های مرتبط وجود داشته باشند";
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error deleting category {CategoryId}", categoryId);
                TempData["Error"] = "این دسته‌بندی دارای تیکت‌های مرتبط است و نمی‌توان آن را حذف کرد";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category {CategoryId}", categoryId);
                TempData["Error"] = "خطای سیستمی در حذف دسته‌بندی";
            }

            return RedirectToAction("ManageCategories");
        }

        [HttpPost("updatecategory")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCategory(Category category)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "اطلاعات وارد شده معتبر نیست";
                return RedirectToAction("ManageCategories");
            }

            try
            {
                var result = await _category.UpdateCategory(category);

                if (result)
                {
                    TempData["Success"] = "دسته‌بندی با موفقیت بروزرسانی شد";
                    _logger.LogInformation($"Category {category.Id} updated by {User.Identity!.Name}");
                }
                else
                {
                    TempData["Error"] = "دسته‌بندی مورد نظر یافت نشد";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category {CategoryId}", category.Id);
                TempData["Error"] = "خطا در بروزرسانی دسته‌بندی";
            }

            return RedirectToAction("ManageCategories");
        }

        [HttpPost("changeticketcategory")]
        public async Task<IActionResult> ChangeTicketCategory(int ticketId, int categoryId)
        {
            var ticket = await _ticketService.GetTicketById(ticketId);
            if (ticket == false)
            {
                return NotFound();
            }

            var result = await _ticketService.UpdateTicketCategory(ticketId, categoryId);
            if (result)
            {
                TempData["Success"] = "دسته‌بندی تیکت با موفقیت تغییر کرد";
            }
            else
            {
                TempData["Error"] = "خطا در تغییر دسته‌بندی تیکت";
            }

            return RedirectToAction("ManageTickets");
        }
    }
}