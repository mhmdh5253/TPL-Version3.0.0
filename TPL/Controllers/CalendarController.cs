using BLL.Calendar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TPLWeb.Models.Calendar;
using BE;
using BE.Calendar;
using TPLWeb.Tools;
using DAL;

namespace TPLWeb.Controllers
{
    [Authorize]
    public class CalendarController : Controller
    {
        private readonly ICalendarService _calendarService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly object _dateConverter;
    private readonly BLL.Ticketing.BlNotification _notificationService;
    private readonly ISmsSender _smsSender;
    private readonly Db _context;

        public CalendarController(
            ICalendarService calendarService,
            UserManager<ApplicationUser> userManager,
            BLL.Ticketing.BlNotification notificationService,
            ISmsSender smsSender,
            Db context)
        {
            _calendarService = calendarService;
            _userManager = userManager;
            _dateConverter = new object();
            _notificationService = notificationService;
            _smsSender = smsSender;
            _context = context;
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

                // Determine recipients: all users if "select all" chosen, otherwise selected participants; fallback to all if IsManagerAnnouncement by admin
                var allUsers = await _userManager.Users.ToListAsync();
                var selectedIds = (model.ParticipantUserIds ?? new List<string>()).Distinct().ToList();
                bool allSelected = selectedIds.Any() && selectedIds.Count >= Math.Max(1, allUsers.Count - 1);
                var recipients = new List<ApplicationUser>();

                if (selectedIds.Any())
                {
                    if (allSelected)
                        recipients = allUsers.Where(u => u.Id != userId).ToList();
                    else
                        recipients = allUsers.Where(u => selectedIds.Contains(u.Id) && u.Id != userId).ToList();
                }
                else if (model.IsManagerAnnouncement && isAdmin)
                {
                    recipients = allUsers.Where(u => u.Id != userId).ToList();
                }

                // Send in-app notifications to recipients
                foreach (var r in recipients)
                {
                    var text = model.IsManagerAnnouncement && (allSelected || !selectedIds.Any())
                        ? $"اعلان مدیریتی جدید: '{model.Title}'"
                        : $"شما به رویداد '{model.Title}' دعوت شده‌اید";
                    await _notificationService.SendNotification(
                        r.Id,
                        text,
                        Url.Action("AppCalendar", "Calendar")!);
                }

                // Send SMS to recipients with full details
                var phoneNumbers = recipients
                    .Select(u => u.PhoneNumber)
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .Select(p => p!)
                    .ToList();
                if (phoneNumbers.Any())
                {
                    var startText = createdEvent.StartDate.ToString("yyyy/MM/dd HH:mm");
                    var endText = createdEvent.EndDate.ToString("yyyy/MM/dd HH:mm");
                    var desc = string.IsNullOrWhiteSpace(createdEvent.Description) ? "-" : createdEvent.Description;
                    var loc = string.IsNullOrWhiteSpace(createdEvent.Location) ? "-" : createdEvent.Location;
                    var smsText = model.IsManagerAnnouncement && (allSelected || !selectedIds.Any())
                        ? $"اعلان مدیریتی جدید: {createdEvent.Title}\nتاریخ: {startText}\nمکان: {loc}\nتوضیحات: {desc}"
                        : $"دعوت به رویداد: {createdEvent.Title}\nتاریخ: {startText} تا {endText}\nمکان: {loc}\nتوضیحات: {desc}";
                    await _smsSender.SendBulkSmsAsync(smsText, phoneNumbers);
                }

                // Build reminder recipients: owner's personal items → only owner
                var reminderTime = createdEvent.StartDate.AddMinutes(-30);
                var reminderRecipients = new List<ApplicationUser>();
                var owner = allUsers.FirstOrDefault(u => u.Id == userId);
                bool isPersonalOrPrivate = (!model.IsManagerAnnouncement) && 
                    (!selectedIds.Any() || createdEvent.EventType == EventType.Personal || createdEvent.Visibility == EventVisibility.Private);
                if (isPersonalOrPrivate && owner != null)
                    reminderRecipients.Add(owner);
                else
                    reminderRecipients = recipients;

                if (reminderRecipients.Any() && reminderTime > DateTime.Now.AddMinutes(-1))
                {
                    var startText = createdEvent.StartDate.ToString("yyyy/MM/dd HH:mm");
                    var desc = string.IsNullOrWhiteSpace(createdEvent.Description) ? "-" : createdEvent.Description;
                    var loc = string.IsNullOrWhiteSpace(createdEvent.Location) ? "-" : createdEvent.Location;
                    var reminders = new List<CalendarReminder>();
                    foreach (var r in reminderRecipients)
                    {
                        var baseMsg = $"یادآوری: {createdEvent.Title}\nتاریخ: {startText}\nمکان: {loc}\nتوضیحات: {desc}";
                        if (owner != null && r.Id == owner.Id)
                            baseMsg += "\nحضور الزامی می باشد";
                        reminders.Add(new CalendarReminder
                        {
                            CalendarEventId = createdEvent.Id,
                            UserId = r.Id,
                            ReminderTime = reminderTime,
                            ReminderType = ReminderType.ThirtyMinutesBefore,
                            IsSent = false,
                            NotificationMethod = "SMS",
                            Message = baseMsg
                        });
                    }
                    _context.CalendarReminders.AddRange(reminders);
                    await _context.SaveChangesAsync();
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

                var updated = await _calendarService.UpdateEventAsync(model.Id, model, userId!);

                // Fetch event with participants to determine recipients
                var ev = await _calendarService.GetEventByIdAsync(model.Id);
                if (ev != null)
                {
                    var allUsers2 = await _userManager.Users.ToListAsync();
                    var participantIds = ev.Participants?.Select(p => p.UserId).Distinct().ToList() ?? new List<string>();
                    bool treatAsAll = ev.IsManagerAnnouncement || (participantIds.Any() && participantIds.Count >= Math.Max(1, allUsers2.Count - 1));
                    var recipients2 = treatAsAll
                        ? allUsers2.Where(u => u.Id != userId).ToList()
                        : allUsers2.Where(u => participantIds.Contains(u.Id) && u.Id != userId).ToList();

                    // In-app notification about update
                    foreach (var r in recipients2)
                    {
                        await _notificationService.SendNotification(
                            r.Id,
                            $"رویداد '{ev.Title}' ویرایش شد",
                            Url.Action("AppCalendar", "Calendar")!);
                    }

                    // SMS about update, include full details
                    var phones2 = recipients2
                        .Select(u => u.PhoneNumber)
                        .Where(p => !string.IsNullOrWhiteSpace(p))
                        .Select(p => p!)
                        .ToList();
                    if (phones2.Any())
                    {
                        var s = updated.StartDate.ToString("yyyy/MM/dd HH:mm");
                        var e = updated.EndDate.ToString("yyyy/MM/dd HH:mm");
                        var d = string.IsNullOrWhiteSpace(updated.Description) ? "-" : updated.Description;
                        var l = string.IsNullOrWhiteSpace(updated.Location) ? "-" : updated.Location;
                        await _smsSender.SendBulkSmsAsync($"رویداد به‌روزرسانی شد: {updated.Title}\nتاریخ: {s} تا {e}\nمکان: {l}\nتوضیحات: {d}", phones2);
                    }

                    // Update or create reminders to be 30 minutes before new start; personal/private → only owner
                    var newReminderTime = updated.StartDate.AddMinutes(-30);
                    var reminderRecipients2 = recipients2;
                    if (!updated.IsManagerAnnouncement && (!(participantIds?.Any() ?? false) || updated.EventType == EventType.Personal || updated.Visibility == EventVisibility.Private))
                    {
                        var owner2 = allUsers2.FirstOrDefault(u => u.Id == updated.UserId);
                        reminderRecipients2 = owner2 != null ? new List<ApplicationUser> { owner2 } : new List<ApplicationUser>();
                    }

                    if (newReminderTime > DateTime.Now.AddMinutes(-1) && reminderRecipients2.Any())
                    {
                        var st = updated.StartDate.ToString("yyyy/MM/dd HH:mm");
                        var dd = string.IsNullOrWhiteSpace(updated.Description) ? "-" : updated.Description;
                        var ll = string.IsNullOrWhiteSpace(updated.Location) ? "-" : updated.Location;
                        foreach (var r in reminderRecipients2)
                        {
                            var msg = $"یادآوری: {updated.Title}\nتاریخ: {st}\nمکان: {ll}\nتوضیحات: {dd}";
                            var isOwner = r.Id == updated.UserId;
                            if (isOwner) msg += "\nحضور الزامی می باشد";

                            var existingReminder = await _context.CalendarReminders
                                .FirstOrDefaultAsync(cr => cr.CalendarEventId == updated.Id && cr.UserId == r.Id && cr.ReminderType == ReminderType.ThirtyMinutesBefore);
                            if (existingReminder == null)
                            {
                                _context.CalendarReminders.Add(new CalendarReminder
                                {
                                    CalendarEventId = updated.Id,
                                    UserId = r.Id,
                                    ReminderTime = newReminderTime,
                                    ReminderType = ReminderType.ThirtyMinutesBefore,
                                    IsSent = false,
                                    NotificationMethod = "SMS",
                                    Message = msg
                                });
                            }
                            else
                            {
                                existingReminder.ReminderTime = newReminderTime;
                                existingReminder.IsSent = false;
                                existingReminder.Message = msg;
                            }
                        }
                        await _context.SaveChangesAsync();
                    }
                }

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
