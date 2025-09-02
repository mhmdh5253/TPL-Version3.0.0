using System.Security.Claims;
using BLL.Calendar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TPLWeb.Models.Calendar;
using BE;
using BE.Calendar;
using Newtonsoft.Json;
using NPOI.Util;
using NuGet.Protocol;

namespace TPLWeb.Controllers
{
    [Authorize]
    public class CalendarController : Controller
    {
        private readonly ICalendarService _calendarService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly object _dateConverter;
        private readonly BLL.Ticketing.BlNotification _notificationService;

        public CalendarController(
            ICalendarService calendarService,
            UserManager<ApplicationUser> userManager,
            BLL.Ticketing.BlNotification notificationService)
        {
            _calendarService = calendarService;
            _userManager = userManager;
            _dateConverter = new object();
            _notificationService = notificationService;
        }

        public async Task<IActionResult> AppCalendar()
        {
            var userId = _userManager.GetUserId(User);
            var dashboardData = await _calendarService.GetDashboardDataAsync(userId!);
            return View(dashboardData);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<object>());

            try
            {
                var userId = _userManager.GetUserId(User);
                var events = await _calendarService.SearchEventsAsync(term, userId);
                var results = events.Select(e => new
                {
                    id = e.Id,
                    title = e.Title,
                    start = e.StartDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = e.EndDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                    color = e.Color,
                    type = e.EventType.ToString(),
                    isManagerAnnouncement = e.IsManagerAnnouncement
                });
                
                return Json(results);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,SuperAdmin")]
        public async Task<IActionResult> Categories()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var categories = await _calendarService.GetCategoriesAsync(userId);
                return View(categories);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"خطا در بارگذاری دسته‌بندی‌ها: {ex.Message}";
                return RedirectToAction(nameof(AppCalendar));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager,SuperAdmin")]
        public async Task<IActionResult> CreateCategory(CalendarEventCategory model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    TempData["ErrorMessage"] = $"خطاهای اعتبارسنجی: {errors}";
                    return RedirectToAction(nameof(Categories));
                }

                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "شناسه کاربر یافت نشد";
                    return RedirectToAction(nameof(Categories));
                }

                var category = await _calendarService.CreateCategoryAsync(model, userId);
                TempData["SuccessMessage"] = "دسته‌بندی با موفقیت ایجاد شد";
                return RedirectToAction(nameof(Categories));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"خطا در ایجاد دسته‌بندی: {ex.Message}";
                return RedirectToAction(nameof(Categories));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager,SuperAdmin")]
        public async Task<IActionResult> UpdateCategory(int id, CalendarEventCategory model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    TempData["ErrorMessage"] = $"خطاهای اعتبارسنجی: {errors}";
                    return RedirectToAction(nameof(Categories));
                }

                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "شناسه کاربر یافت نشد";
                    return RedirectToAction(nameof(Categories));
                }

                var category = await _calendarService.UpdateCategoryAsync(id, model, userId);
                TempData["SuccessMessage"] = "دسته‌بندی با موفقیت بروزرسانی شد";
                return RedirectToAction(nameof(Categories));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"خطا در بروزرسانی دسته‌بندی: {ex.Message}";
                return RedirectToAction(nameof(Categories));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager,SuperAdmin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var result = await _calendarService.DeleteCategoryAsync(id, userId!);
                if (result)
                {
                    TempData["SuccessMessage"] = "دسته‌بندی با موفقیت حذف شد";
                }
                else
                {
                    TempData["ErrorMessage"] = "خطا در حذف دسته‌بندی";
                }
                return RedirectToAction(nameof(Categories));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"خطا در حذف دسته‌بندی: {ex.Message}";
                return RedirectToAction(nameof(Categories));
            }
        }

        // API Methods for Calendar Integration
        [HttpGet]
        public async Task<IActionResult> GetEvents(DateTime? start, DateTime? end)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                
                // Ensure management announcement category exists
                await EnsureManagementAnnouncementCategoryExists();
                
                var events = await _calendarService.GetEventsForCalendarAsync(userId, start, end);

                // Map using reflection but also log if something is missing to aid debugging
                var calendarEvents = new List<object>();
                foreach (var e in events)
                {
                    var t = e.GetType();
                    var id = t.GetProperty("Id")?.GetValue(e);
                    var title = t.GetProperty("Title")?.GetValue(e);
                    var evStart = t.GetProperty("Start")?.GetValue(e);
                    var evEnd = t.GetProperty("End")?.GetValue(e);
                    var allDay = t.GetProperty("AllDay")?.GetValue(e) ?? false;
                    var categoryName = t.GetProperty("CategoryName")?.GetValue(e) ?? t.GetProperty("Category")?.GetValue(e);
                    var location = t.GetProperty("Location")?.GetValue(e);
                    var description = t.GetProperty("Description")?.GetValue(e);
                    var isManagerAnnouncement = (t.GetProperty("IsManagerAnnouncement")?.GetValue(e) as bool?) ?? false;

                    // Set category for management announcements
                    if (isManagerAnnouncement)
                    {
                        // If no category is set, use the default management announcement category
                        if (string.IsNullOrEmpty(categoryName?.ToString()))
                        {
                            categoryName = "اعلان مدیریتی";
                        }
                    }

                    calendarEvents.Add(new
                    {
                        id,
                        title,
                        start = evStart,
                        end = evEnd,
                        allDay,
                        extendedProps = new
                        {
                            calendar = categoryName?.ToString() ?? "Business",
                            location = location?.ToString() ?? string.Empty,
                            description = description?.ToString() ?? string.Empty,
                            isCompleted = (t.GetProperty("IsCompleted")?.GetValue(e) as bool?) ?? false,
                            isManagerAnnouncement = isManagerAnnouncement,
                            guests = Array.Empty<string>()
                        }
                    });
                }

                return Json(new { success = true, events = calendarEvents });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var categories = await _calendarService.GetCategoriesAsync(userId);
                
                var calendarCategories = categories.Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    color = c.Color ?? "#3788d8"
                }).ToList();

                return Json(new { success = true, categories = calendarCategories });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddEvent([FromBody] CalendarEventCreateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    return Json(new { success = false, message = errors });
                }
                var userId = _userManager.GetUserId(User);
                
                // Check if user is admin or super admin for management notifications
                var user = await _userManager.GetUserAsync(User);
                var isAdmin = await _userManager.IsInRoleAsync(user!, "Admin") || await _userManager.IsInRoleAsync(user!, "SuperAdmin");
                
                // Normalize minimal payloads coming from UI when select values are empty
                if (model.StartDate == default) model.StartDate = DateTime.Now;
                if (model.EndDate == default) model.EndDate = model.StartDate.AddHours(1);
                if (string.IsNullOrWhiteSpace(model.Title)) model.Title = "رویداد";
                if ((int)model.EventType == 0) model.EventType = EventType.Personal;
                if ((int)model.Visibility == 0) model.Visibility = EventVisibility.Private;

                var createdEvent = await _calendarService.CreateEventAsync(model, userId!);
                
                // Send notifications to selected participants
                if (model.ParticipantUserIds != null && model.ParticipantUserIds.Any())
                {
                    foreach (var participantId in model.ParticipantUserIds)
                    {
                        if (participantId != userId) // Don't notify the creator
                        {
                            await _notificationService.SendNotification(
                                participantId, 
                                $"شما به رویداد '{model.Title}' دعوت شده‌اید", 
                                Url.Action("AppCalendar", "Calendar")!);
                        }
                    }
                }
                
                // If this is a management notification and user is admin/super admin, send notifications to all users
                if (model.IsManagerAnnouncement && isAdmin)
                {
                    var allUsers = await _userManager.Users.ToListAsync();
                    foreach (var userToNotify in allUsers)
                    {
                        if (userToNotify.Id != userId) // Don't notify the creator
                        {
                            await _notificationService.SendNotification(
                                userToNotify.Id, 
                                $"اعلان مدیریتی جدید: '{model.Title}'", 
                                Url.Action("AppCalendar", "Calendar")!);
                        }
                    }
                }
                
                return Json(new { success = true, message = "رویداد با موفقیت ثبت شد", eventId = createdEvent.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateEvent([FromBody] CalendarEventCreateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    return Json(new { success = false, message = errors });
                }

                var userId = _userManager.GetUserId(User);

                if (model.StartDate == default) model.StartDate = DateTime.Now;
                if (model.EndDate == default) model.EndDate = model.StartDate.AddHours(1);
                if (string.IsNullOrWhiteSpace(model.Title)) model.Title = "رویداد";
                if ((int)model.EventType == 0) model.EventType = EventType.Personal;
                if ((int)model.Visibility == 0) model.Visibility = EventVisibility.Private;

                await _calendarService.UpdateEventAsync(model.Id, model, userId!);
                
                return Json(new { success = true, message = "رویداد با موفقیت به‌روزرسانی شد" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var result = await _calendarService.DeleteEventAsync(id, userId!);
                
                if (result)
                {
                    return Json(new { success = true, message = "رویداد با موفقیت حذف شد" });
                }
                else
                {
                    return Json(new { success = false, message = "خطا در حذف رویداد" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _userManager.Users
                    .Select(u => new
                    {
                        u.Id,
                        u.UserName,
                        u.Avatar,
                        u.Emza
                    })
                    .ToListAsync();

                var projected = users.Select(u =>
                {
                    string avatarUrl;
                    if (!string.IsNullOrWhiteSpace(u.Avatar))
                    {
                        // If Avatar already looks like a URL/path, keep it; otherwise point to default avatars folder
                        avatarUrl = u.Avatar.StartsWith("/") || u.Avatar.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                            ? u.Avatar
                            : $"/CompanyVariables/avatars/{u.Avatar}";
                    }
                    else if (!string.IsNullOrWhiteSpace(u.Emza))
                    {
                        // Emza (signature) stored under /signatures
                        avatarUrl = u.Emza.StartsWith("/") || u.Emza.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                            ? u.Emza
                            : $"/CompanyVariables/signatures/{u.Emza}";
                    }
                    else
                    {
                        avatarUrl = "/CompanyVariables/avatars/1.png";
                    }

                    return new
                    {
                        id = u.Id,
                        text = (string.IsNullOrWhiteSpace(u.UserName) ? "کاربر" : u.UserName),
                        avatar = avatarUrl
                    };
                }).ToList();

                return Json(new { success = true, users = projected });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCurrentUserRole()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "کاربر یافت نشد" });
                }

                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                var isSuperAdmin = await _userManager.IsInRoleAsync(user, "SuperAdmin");
                var isManager = await _userManager.IsInRoleAsync(user, "Manager");

                return Json(new { 
                    success = true, 
                    isAdmin = isAdmin || isSuperAdmin || isManager,
                    isSuperAdmin = isSuperAdmin,
                    isManager = isManager,
                    userId = user.Id
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CanEditEvent(int eventId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var canEdit = await _calendarService.CanUserManageEventAsync(eventId, userId!);
                
                return Json(new { 
                    success = true, 
                    canEdit = canEdit
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkNotificationAsRead(string notificationId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var result = await _notificationService.MarkAsRead(notificationId);
                
                return Json(new { 
                    success = result, 
                    message = result ? "اعلان به عنوان خوانده شده علامت‌گذاری شد" : "خطا در علامت‌گذاری اعلان"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Private method to ensure management announcement category exists
        private async Task EnsureManagementAnnouncementCategoryExists()
        {
            try
            {
                var categories = await _calendarService.GetCategoriesAsync();
                var managementCategory = categories.FirstOrDefault(c => c.Name == "اعلان مدیریتی");
                
                if (managementCategory == null)
                {
                    var newCategory = new CalendarEventCategory
                    {
                        Name = "اعلان مدیریتی",
                        Color = "#e74c3c", // Red color for management announcements
                        Description = "دسته‌بندی پیش‌فرض برای اعلان‌های مدیریتی",
                        IsActive = true,
                        IsDeleted = false,
                        CreatedAt = DateTime.Now
                    };
                    
                    // Use system user ID for category creation if no admin user is available
                    var systemUserId = "system";
                    var adminUser = await _userManager.GetUsersInRoleAsync("Admin");
                    if (adminUser.Any())
                    {
                        systemUserId = adminUser.First().Id;
                    }
                    
                    await _calendarService.CreateCategoryAsync(newCategory, systemUserId);
                }
            }
            catch (Exception)
            {
                // Log the error but don't throw to prevent breaking the main functionality
                // The error can be logged here or handled silently
               // System.Diagnostics.Debug.WriteLine($"Error creating management announcement category: {ex.Message}");
            }
        }
    }
}
