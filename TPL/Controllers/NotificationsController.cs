using BE;
using BLL.Calendar;
using BLL.LetterAutomation;
using BLL.Ticketing;
using DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TPLWeb.Tools;

namespace TPLWeb.Controllers
{
    [Authorize]
    [Route("Notifications")] 
    public class NotificationsController : Controller
    {
        private readonly BlNotification _notificationService;
        private readonly ICalendarService _calendarService;
        private readonly ILetterService _letterService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Db _context;
    private readonly ISmsSender _smsSender;

        public NotificationsController(
            BlNotification notificationService,
            ICalendarService calendarService,
            ILetterService letterService,
            UserManager<ApplicationUser> userManager,
            Db context,
            ISmsSender smsSender)
        {
            _notificationService = notificationService;
            _calendarService = calendarService;
            _letterService = letterService;
            _userManager = userManager;
            _context = context;
            _smsSender = smsSender;
        }

        public class UnifiedNotificationItem
        {
            public string Id { get; set; } = string.Empty; // Use string to support ephemeral ids
            public string? Message { get; set; }
            public DateTime CreatedDate { get; set; }
            public bool IsRead { get; set; }
            public string? Link { get; set; }
            public bool Ephemeral { get; set; } = false; // Not persisted in DB
            public string? Type { get; set; } // letter | calendar | اعلان | user
            public string? SourceTitle { get; set; } // e.g., عنوان اعلان/نامه
        }

        [HttpGet("")] 
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return View(new List<UnifiedNotificationItem>());
            }
            var data = await GetUnifiedItems(currentUser.Id);
            return View(data.items);
        }

        [HttpGet("List")] 
        public async Task<IActionResult> List()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Json(new { totalCount = 0, items = new List<UnifiedNotificationItem>() });
            }

            var result = await GetUnifiedItems(currentUser.Id);
            return Json(new { totalCount = result.totalCount, items = result.items });
        }

        [HttpGet("LastMonthTicker")] 
        public async Task<IActionResult> LastMonthTicker()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Json(Array.Empty<string>());
            }

            var result = await GetUnifiedItems(currentUser.Id);
            var since = DateTime.Now.AddMonths(-1);
            var messages = result.items
                .Where(x => x.CreatedDate >= since)
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => x.Message)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .Take(100)
                .ToList();

            if (messages.Count == 0)
                messages.Add("اعلانی در یک ماه گذشته وجود ندارد");

            return Json(messages);
        }

        [HttpGet("LastMonth")] 
        public async Task<IActionResult> LastMonth()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Json(new { totalCount = 0, items = new List<UnifiedNotificationItem>() });
            }

            var result = await GetUnifiedItems(currentUser.Id);
            var since = DateTime.Now.AddMonths(-1);
            var items = result.items
                .Where(x => x.CreatedDate >= since)
                .OrderByDescending(x => x.CreatedDate)
                .ToList();

            return Json(new { totalCount = items.Count, items });
        }

        private async Task<(int totalCount, List<UnifiedNotificationItem> items)> GetUnifiedItems(string userId)
        {
            var stored = await _notificationService.GetUserNotifications(userId);
            var unreadStored = stored.Where(n => !n.IsRead)
                .OrderByDescending(n => n.CreatedDate)
                .Select(n => new UnifiedNotificationItem
                {
                    Id = n.Id.ToString(),
                    Message = n.Message,
                    CreatedDate = n.CreatedDate,
                    IsRead = n.IsRead,
                    Link = n.Link,
                    Ephemeral = false,
                    Type = "اعلان",
                    SourceTitle = "اعلان"
                })
                .ToList();

            // Ephemeral computed notifications
            var ephemeral = new List<UnifiedNotificationItem>();

            // 1) Letters cartables
            try
            {
                var allLettersRes = await _letterService.GetAllLettersAsync();
                if (allLettersRes.Success && allLettersRes.Data is IEnumerable<BE.LetterAutomation.Letter> letters)
                {
                    // صادره (outbox)
                    var outboxCount = letters.Count(x => !x.IsDeleted && x.Username == User.Identity!.Name &&
                                                         (x.Status == BE.LetterAutomation.LetterStatus.Pending ||
                                                          x.Status == BE.LetterAutomation.LetterStatus.Rejected ||
                                                          x.Status == BE.LetterAutomation.LetterStatus.InReview ||
                                                          x.Status == BE.LetterAutomation.LetterStatus.Returned));
                    if (outboxCount > 0)
                    {
                        ephemeral.Add(new UnifiedNotificationItem
                        {
                            Id = $"e-outbox-{DateTime.UtcNow.Ticks}",
                            Message = $"شما {outboxCount} نامه در کارتابل صادره دارید",
                            CreatedDate = DateTime.Now,
                            IsRead = false,
                            Link = Url.Action("KartableSadereh", "Letter"),
                            Ephemeral = true,
                            Type = "letter",
                            SourceTitle = "نامه"
                        });
                    }

                    // وارده (inbox) — حذف از زنگوله: فقط در شمارنده خوانده‌نشده‌ها نمایش داده می‌شود
                }
            }
            catch { }

            // 2) Calendar reminders upcoming
            try
            {
                var settings = await _calendarService.GetUserSettingsAsync(userId);
                if (settings != null)
                {
                    var advanceMinutes = settings.NotificationAdvanceMinutes;
                    var now = DateTime.Now;
                    var upcoming = await _calendarService.GetUpcomingEventsAsync(userId, days: 2);
                    if (upcoming != null)
                    {
                        foreach (var ev in upcoming.Where(e => e.StartDate <= now.AddMinutes(advanceMinutes)))
                        {
                            ephemeral.Add(new UnifiedNotificationItem
                            {
                                Id = $"e-cal-{ev.Id}-{ev.StartDate.Ticks}",
                                Message = $"یادآوری: در {ev.StartDate:yyyy/MM/dd HH:mm} باید '{ev.Title}' را انجام دهید",
                                CreatedDate = now,
                                IsRead = false,
                                Link = Url.Action("AppCalendar", "Calendar"),
                                Ephemeral = true,
                                Type = "calendar",
                                SourceTitle = ev.Title
                            });
                        }
                    }
                }
            }
            catch { }

            // 2.1) Calendar overdue and pending counts
            try
            {
                var now2 = DateTime.Now;
                var allUserEvents = await _calendarService.GetUserEventsAsync(userId);
                if (allUserEvents != null)
                {
                    var overdueCount = allUserEvents.Count(e => !e.IsCompleted && e.EndDate < now2);
                    if (overdueCount > 0)
                    {
                        ephemeral.Add(new UnifiedNotificationItem
                        {
                            Id = $"e-cal-overdue-{now2.Ticks}",
                            Message = $"شما {overdueCount} رویداد تاریخ‌گذشته در تقویم دارید",
                            CreatedDate = now2,
                            IsRead = false,
                            Link = Url.Action("AppCalendar", "Calendar"),
                            Ephemeral = true,
                            Type = "calendar",
                            SourceTitle = "رویدادهای تاریخ‌گذشته"
                        });
                    }
                    var pendingCount = allUserEvents.Count(e => !e.IsCompleted && e.EndDate >= now2);
                    if (pendingCount > 0)
                    {
                        ephemeral.Add(new UnifiedNotificationItem
                        {
                            Id = $"e-cal-pending-{now2.Ticks}",
                            Message = $"شما {pendingCount} رویداد در انتظار انجام در تقویم دارید",
                            CreatedDate = now2,
                            IsRead = false,
                            Link = Url.Action("AppCalendar", "Calendar"),
                            Ephemeral = true,
                            Type = "calendar",
                            SourceTitle = "رویدادهای در انتظار"
                        });
                    }
                    var completedCount = allUserEvents.Count(e => e.IsCompleted);
                    if (completedCount > 0)
                    {
                        ephemeral.Add(new UnifiedNotificationItem
                        {
                            Id = $"e-cal-done-{now2.Ticks}",
                            Message = $"شما {completedCount} رویداد انجام‌شده در تقویم دارید",
                            CreatedDate = now2,
                            IsRead = false,
                            Link = Url.Action("AppCalendar", "Calendar"),
                            Ephemeral = true,
                            Type = "calendar",
                            SourceTitle = "رویدادهای انجام‌شده"
                        });
                    }
                }
            }
            catch { }

            // 3) Unread letters count that disappears when user has no unread letters
            try
            {
                // Candidate inbox letters for user's org
                var allLettersRes2 = await _letterService.GetAllLettersAsync();
                if (allLettersRes2.Success && allLettersRes2.Data is IEnumerable<BE.LetterAutomation.Letter> letters2)
                {
                    var userOrg = _context.UserOrganizations
                        .Include(uo => uo.Organization)
                        .FirstOrDefault(uo => uo.UserId == userId);
                    if (userOrg?.Organization != null && userOrg.IsActive)
                    {
                        var orgId = userOrg.Organization.Id.ToString();
                        var inboxCandidates = letters2.Where(x => !x.IsDeleted &&
                                                                  x.Status != BE.LetterAutomation.LetterStatus.Deleted &&
                                                                  x.Status != BE.LetterAutomation.LetterStatus.Archived &&
                                                                  x.Status != BE.LetterAutomation.LetterStatus.InReview &&
                                                                  x.Status != BE.LetterAutomation.LetterStatus.Returned &&
                                                                  x.Status != BE.LetterAutomation.LetterStatus.Rejected &&
                                                                  (x.Receiver == orgId ||
                                                                   (x.CopyReceivers != null &&
                                                                    !string.IsNullOrEmpty(x.CopyReceivers.FirstOrDefault()) &&
                                                                    x.CopyReceivers.FirstOrDefault()!
                                                                       .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                                       .Any(c => c.Equals(orgId))))).ToList();

                        var candidateIds = inboxCandidates.Select(l => l.Id).ToList();
                        var viewedIds = await _context.LetterActions
                            .Where(a => a.UserId == userId && a.ActionDescription == "Viewed" && candidateIds.Contains(a.LetterId))
                            .Select(a => a.LetterId)
                            .Distinct()
                            .ToListAsync();
                        var unreadCount = inboxCandidates.Count(l => !viewedIds.Contains(l.Id));

                        if (unreadCount > 0)
                        {
                            ephemeral.Add(new UnifiedNotificationItem
                            {
                                Id = $"e-unread-letters",
                                Message = $"شما {unreadCount} نامه خوانده‌نشده دارید",
                                CreatedDate = DateTime.Now,
                                IsRead = false,
                                Link = Url.Action("KartableVaredeh", "Letter"),
                                Ephemeral = true,
                                Type = "letter",
                                SourceTitle = "نامه"
                            });
                        }
                    }
                }
            }
            catch { }

            var items = ephemeral.Concat(unreadStored)
                .OrderByDescending(x => x.CreatedDate)
                .ToList();

            return (items.Count, items);
        }

        [HttpPost("Read/{id}")] 
        public async Task<IActionResult> MarkAsRead(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();
            var ok = await _notificationService.MarkAsRead(id);
            return Json(new { success = ok });
        }

        [HttpPost("Delete/{id}")] 
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();
            var ok = await _notificationService.DeleteNotification(id);
            return Json(new { success = ok });
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("Send")] 
        public async Task<IActionResult> SendToUser([FromForm] string userId, [FromForm] string message, [FromForm] string? link)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(message))
            {
                return BadRequest(new { success = false, message = "شناسه کاربر و پیام الزامی است" });
            }
            var ok = await _notificationService.SendNotification(userId, message, link ?? string.Empty);
            return Json(new { success = ok });
        }

        // ارسال اعلان مدیریتی برای همه کاربران + پیامک انبوه
        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost("SendToAll")] 
        public async Task<IActionResult> SendToAll([FromForm] string message, [FromForm] string? link)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return BadRequest(new { success = false, message = "متن اعلان الزامی است" });
            }

            try
            {
                var allUsers = await _userManager.Users.ToListAsync();
                int created = 0;
                foreach (var u in allUsers)
                {
                    var ok = await _notificationService.SendNotification(u.Id, message, link ?? string.Empty);
                    if (ok) created++;
                }

                var phoneNumbers = allUsers
                    .Select(u => u.PhoneNumber)
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .Select(p => p!)
                    .ToList();

                string smsResult = "Skipped: no phone numbers";
                if (phoneNumbers.Any())
                {
                    smsResult = await _smsSender.SendBulkSmsAsync(message, phoneNumbers);
                }

                return Json(new { success = true, createdNotifications = created, totalUsers = allUsers.Count, smsResult });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}


