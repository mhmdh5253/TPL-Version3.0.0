using BE.Calendar;
using DAL.Calendar;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

namespace BLL.Calendar
{
    public class CalendarService : ICalendarService
    {
        private readonly ICalendarEventRepository _eventRepository;
        private readonly ICalendarCategoryRepository _categoryRepository;
        private readonly UserManager<BE.ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CalendarService(
            ICalendarEventRepository eventRepository,
            ICalendarCategoryRepository categoryRepository,
            UserManager<BE.ApplicationUser> userManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _eventRepository = eventRepository;
            _categoryRepository = categoryRepository;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<CalendarEvent>> GetAllEventsAsync()
        {
            return await _eventRepository.GetAllAsync();
        }

        public async Task<CalendarEvent?> GetEventByIdAsync(int id)
        {
            return await _eventRepository.GetByIdAsync(id);
        }

        public async Task<CalendarEvent> CreateEventAsync(object model, string userId)
        {
            var isManagerAnnouncement = (bool)(model.GetType().GetProperty("IsManagerAnnouncement")?.GetValue(model) ?? false);
            
            // Map from CalendarEventCreateViewModel or compatible object
            var calendarEvent = new CalendarEvent
            {
                Title = model.GetType().GetProperty("Title")?.GetValue(model)?.ToString() ?? "رویداد",
                Description = model.GetType().GetProperty("Description")?.GetValue(model)?.ToString(),
                StartDate = (DateTime)(model.GetType().GetProperty("StartDate")?.GetValue(model) ?? DateTime.Now),
                EndDate = (DateTime)(model.GetType().GetProperty("EndDate")?.GetValue(model) ?? DateTime.Now.AddHours(1)),
                IsAllDay = (bool)(model.GetType().GetProperty("IsAllDay")?.GetValue(model) ?? false),
                Color = model.GetType().GetProperty("Color")?.GetValue(model)?.ToString() ?? "#3788d8",
                EventType = (EventType)(model.GetType().GetProperty("EventType")?.GetValue(model) ?? EventType.Personal),
                Visibility = (EventVisibility)(model.GetType().GetProperty("Visibility")?.GetValue(model) ?? EventVisibility.Private),
                UserId = userId,
                Location = model.GetType().GetProperty("Location")?.GetValue(model)?.ToString(),
                Priority = model.GetType().GetProperty("Priority")?.GetValue(model)?.ToString() ?? "Normal",
                IsRecurring = (bool)(model.GetType().GetProperty("IsRecurring")?.GetValue(model) ?? false),
                RecurrencePattern = model.GetType().GetProperty("RecurrencePattern")?.GetValue(model)?.ToString(),
                CategoryId = (int?)model.GetType().GetProperty("CategoryId")?.GetValue(model),
                IsManagerAnnouncement = isManagerAnnouncement,
                IsCompleted = (bool)(model.GetType().GetProperty("IsCompleted")?.GetValue(model) ?? false)
            };

            // Auto-assign management announcement category if it's a management announcement
            if (isManagerAnnouncement && !calendarEvent.CategoryId.HasValue)
            {
                var managementCategory = await GetOrCreateManagementAnnouncementCategory(userId);
                if (managementCategory != null)
                {
                    calendarEvent.CategoryId = managementCategory.Id;
                }
            }

            var result = await _eventRepository.AddAsync(calendarEvent);
            
            // Add participants if specified
            var participantUserIds = model.GetType().GetProperty("ParticipantUserIds")?.GetValue(model) as List<string>;
            if (participantUserIds != null && participantUserIds.Any())
            {
                foreach (var participantId in participantUserIds)
                {
                    if (!string.IsNullOrEmpty(participantId) && participantId != userId)
                    {
                        var participant = new CalendarEventParticipant
                        {
                            CalendarEventId = result.Id,
                            UserId = participantId,
                            Status = ParticipantStatus.Pending,
                            Role = ParticipantRole.Attendee
                        };
                        result.Participants.Add(participant);
                    }
                }
                
                // Update the event with participants
                if (result.Participants.Any())
                {
                    await _eventRepository.UpdateAsync(result);
                }
            }
            
            // Log activity
            await LogActivityAsync(userId, ActivityType.Create, "ایجاد رویداد تقویم", 
                $"رویداد جدید ایجاد شد", "CalendarEvent", result.Id);
            
            return result;
        }

        public async Task<CalendarEvent> UpdateEventAsync(int id, object model, string userId)
        {
            var existingEvent = await _eventRepository.GetByIdAsync(id);
            if (existingEvent is null)
                throw new InvalidOperationException("رویداد مورد نظر یافت نشد");

            if (!await CanUserManageEventAsync(id, userId))
                throw new UnauthorizedAccessException("شما مجاز به ویرایش این رویداد نیستید");

            // Map basic updatable fields
            existingEvent.Title = model.GetType().GetProperty("Title")?.GetValue(model)?.ToString() ?? existingEvent.Title;
            existingEvent.Description = model.GetType().GetProperty("Description")?.GetValue(model)?.ToString() ?? existingEvent.Description;
            existingEvent.StartDate = (DateTime)(model.GetType().GetProperty("StartDate")?.GetValue(model) ?? existingEvent.StartDate);
            existingEvent.EndDate = (DateTime)(model.GetType().GetProperty("EndDate")?.GetValue(model) ?? existingEvent.EndDate);
            existingEvent.IsAllDay = (bool)(model.GetType().GetProperty("IsAllDay")?.GetValue(model) ?? existingEvent.IsAllDay);
            existingEvent.Color = model.GetType().GetProperty("Color")?.GetValue(model)?.ToString() ?? existingEvent.Color;
            var eventTypeObj = model.GetType().GetProperty("EventType")?.GetValue(model);
            if (eventTypeObj != null) existingEvent.EventType = (EventType)eventTypeObj;
            var visibilityObj = model.GetType().GetProperty("Visibility")?.GetValue(model);
            if (visibilityObj != null) existingEvent.Visibility = (EventVisibility)visibilityObj;
            existingEvent.Location = model.GetType().GetProperty("Location")?.GetValue(model)?.ToString() ?? existingEvent.Location;
            existingEvent.Priority = model.GetType().GetProperty("Priority")?.GetValue(model)?.ToString() ?? existingEvent.Priority;
            var isCompletedObj = model.GetType().GetProperty("IsCompleted")?.GetValue(model);
            if (isCompletedObj != null) existingEvent.IsCompleted = (bool)isCompletedObj;
            var categoryIdObj = model.GetType().GetProperty("CategoryId")?.GetValue(model);
            if (categoryIdObj != null) existingEvent.CategoryId = (int?)categoryIdObj;
            var isManagerAnnouncementObj = model.GetType().GetProperty("IsManagerAnnouncement")?.GetValue(model);
            if (isManagerAnnouncementObj != null) existingEvent.IsManagerAnnouncement = (bool)isManagerAnnouncementObj;

            var result = await _eventRepository.UpdateAsync(existingEvent);
            
            // Log activity
            await LogActivityAsync(userId, ActivityType.Update, "ویرایش رویداد تقویم", 
                $"رویداد ویرایش شد", "CalendarEvent", result.Id);
            
            return result;
        }

        public async Task<bool> DeleteEventAsync(int id, string userId)
        {
            if (!await CanUserManageEventAsync(id, userId))
                throw new UnauthorizedAccessException("شما مجاز به حذف این رویداد نیستید");

            var result = await _eventRepository.DeleteAsync(id);
            
            if (result)
            {
                // Log activity
                await LogActivityAsync(userId, ActivityType.Delete, "حذف رویداد تقویم", 
                    $"رویداد با شناسه {id} حذف شد", "CalendarEvent", id);
            }
            
            return result;
        }

        public async Task<IEnumerable<CalendarEvent>> GetUserEventsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            return await _eventRepository.GetUserEventsAsync(userId, startDate, endDate);
        }

        public async Task<IEnumerable<CalendarEvent>> GetTodayEventsAsync(string? userId = null)
        {
            return await _eventRepository.GetTodayEventsAsync(userId);
        }

        public async Task<IEnumerable<CalendarEvent>> GetUpcomingEventsAsync(string? userId = null, int days = 7)
        {
            return await _eventRepository.GetUpcomingEventsAsync(userId, days);
        }

        public async Task<IEnumerable<CalendarEvent>> GetManagerAnnouncementsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            return await _eventRepository.GetManagerAnnouncementsAsync(startDate, endDate);
        }

        public async Task<CalendarEvent> CreateManagerAnnouncementAsync(object model, string managerId)
        {
            if (!await IsUserManagerAsync(managerId))
                throw new UnauthorizedAccessException("فقط مدیران می‌توانند اعلان ایجاد کنند");

            var calendarEvent = new CalendarEvent
            {
                Title = model.GetType().GetProperty("Title")?.GetValue(model)?.ToString() ?? "اعلان مدیریتی",
                Description = model.GetType().GetProperty("Description")?.GetValue(model)?.ToString() ?? "توضیحات اعلان",
                StartDate = (DateTime)(model.GetType().GetProperty("StartDate")?.GetValue(model) ?? DateTime.Now),
                EndDate = (DateTime)(model.GetType().GetProperty("EndDate")?.GetValue(model) ?? DateTime.Now.AddHours(1)),
                IsAllDay = (bool)(model.GetType().GetProperty("IsAllDay")?.GetValue(model) ?? false),
                Color = model.GetType().GetProperty("Color")?.GetValue(model)?.ToString() ?? "#e74c3c",
                EventType = EventType.Announcement,
                Visibility = (EventVisibility)(model.GetType().GetProperty("Visibility")?.GetValue(model) ?? EventVisibility.Public),
                UserId = managerId,
                Location = model.GetType().GetProperty("Location")?.GetValue(model)?.ToString() ?? "",
                Priority = model.GetType().GetProperty("Priority")?.GetValue(model)?.ToString() ?? "High",
                IsRecurring = (bool)(model.GetType().GetProperty("IsRecurring")?.GetValue(model) ?? false),
                RecurrencePattern = model.GetType().GetProperty("RecurrencePattern")?.GetValue(model)?.ToString() ?? "",
                CategoryId = (int?)model.GetType().GetProperty("CategoryId")?.GetValue(model),
                IsManagerAnnouncement = true
            };

            // Auto-assign management announcement category
            if (!calendarEvent.CategoryId.HasValue)
            {
                var managementCategory = await GetOrCreateManagementAnnouncementCategory(managerId);
                if (managementCategory != null)
                {
                    calendarEvent.CategoryId = managementCategory.Id;
                }
            }

            var result = await _eventRepository.AddAsync(calendarEvent);
            
            // Log activity
            await LogActivityAsync(managerId, ActivityType.Create, "ایجاد اعلان مدیر", 
                $"اعلان '{calendarEvent.Title}' ایجاد شد", "CalendarEvent", result.Id);
            
            return result;
        }

        public async Task<IEnumerable<object>> GetEventsForCalendarAsync(string? userId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var rangeStart = startDate ?? DateTime.Today.AddMonths(-1);
            var rangeEnd = endDate ?? DateTime.Today.AddMonths(2);
            var events = await _eventRepository.GetEventsByDateRangeAsync(rangeStart, rangeEnd, userId);
            
            return events.Select(e => new
            {
                Id = e.Id,
                Title = e.Title,
                Start = e.StartDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                End = e.EndDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                AllDay = e.IsAllDay,
                Color = e.Color,
                Description = e.Description,
                Location = e.Location,
                Priority = e.Priority,
                IsCompleted = e.IsCompleted,
                EventType = e.EventType.ToString(),
                Visibility = e.Visibility.ToString(),
                IsManagerAnnouncement = e.IsManagerAnnouncement,
                CategoryName = e.Category?.Name,
                Category = e.Category?.Name,
                UserName = e.User?.FirstName + " " + e.User?.LastName,
                StartTime = e.StartDate.ToString("HH:mm"),
                EndTime = e.EndDate.ToString("HH:mm"),
                StartDate = e.StartDate,
                EndDate = e.EndDate
            });
        }

        public async Task<CalendarDashboardViewModel> GetDashboardDataAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("کاربر یافت نشد");

            var isManager = await IsUserManagerAsync(userId);
            
            var personalEvents = await _eventRepository.GetUserEventsAsync(userId);
            var managerAnnouncements = await _eventRepository.GetManagerAnnouncementsAsync();
            var todayEvents = await _eventRepository.GetTodayEventsAsync(userId);
            var upcomingEvents = await _eventRepository.GetUpcomingEventsAsync(userId, 7);
            var categories = await _categoryRepository.GetAllAsync();

            return new CalendarDashboardViewModel
            {
                PersonalEvents = personalEvents,
                ManagerAnnouncements = managerAnnouncements,
                TodayEvents = todayEvents,
                UpcomingEvents = upcomingEvents,
                Categories = categories,
                IsManager = isManager,
                CurrentUserId = userId,
                CurrentUserName = $"{user.FirstName} {user.LastName}",
                CurrentDate = DateTime.Now
            };
        }

        public async Task<IEnumerable<CalendarEvent>> SearchEventsAsync(string searchTerm, string? userId = null)
        {
            return await _eventRepository.SearchEventsAsync(searchTerm, userId);
        }

        public async Task<IEnumerable<CalendarEvent>> GetEventsByCategoryAsync(int categoryId, string? userId = null)
        {
            return await _eventRepository.GetEventsByCategoryAsync(categoryId, userId);
        }

        public async Task<IEnumerable<CalendarEventCategory>> GetCategoriesAsync(string? userId = null)
        {
            try
            {
                return await _categoryRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                // Log the error
                await LogActivityAsync(userId ?? "system", ActivityType.Create, "GetCategories", ex.Message);
                return new List<CalendarEventCategory>();
            }
        }

        public async Task<CalendarEventCategory> CreateCategoryAsync(CalendarEventCategory category, string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                    throw new ArgumentException("User ID cannot be null or empty");
                    
                category.CreatedByUserId = userId;
                category.CreatedAt = DateTime.Now;
                category.IsActive = true;
                category.IsDeleted = false;
                
                var result = await _categoryRepository.AddAsync(category);
                
                // Log the activity
                await LogActivityAsync(userId, ActivityType.Create, "CreateCategory", $"Category '{category.Name}' created");
                
                return result;
            }
            catch (Exception ex)
            {
                await LogActivityAsync(userId, ActivityType.Create, "CreateCategory", ex.Message);
                throw;
            }
        }

        public async Task<CalendarEventCategory> UpdateCategoryAsync(int id, CalendarEventCategory category, string userId)
        {
            try
            {
                var existingCategory = await _categoryRepository.GetByIdAsync(id);
                if (existingCategory == null)
                    throw new InvalidOperationException("Category not found");

                existingCategory.Name = category.Name;
                existingCategory.Description = category.Description;
                existingCategory.Color = category.Color;
                existingCategory.IsActive = category.IsActive;
                existingCategory.UpdatedAt = DateTime.Now;

                var result = await _categoryRepository.UpdateAsync(existingCategory);
                
                // Log the activity
                await LogActivityAsync(userId, ActivityType.Update, "UpdateCategory", $"Category '{category.Name}' updated");
                
                return result;
            }
            catch (Exception ex)
            {
                await LogActivityAsync(userId, ActivityType.Update, "UpdateCategory", ex.Message);
                throw;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id, string userId)
        {
            try
            {
                var result = await _categoryRepository.DeleteAsync(id);
                
                if (result)
                {
                    // Log the activity
                    await LogActivityAsync(userId, ActivityType.Delete, "DeleteCategory", $"Category with ID {id} deleted");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                await LogActivityAsync(userId, ActivityType.Delete, "DeleteCategory", ex.Message);
                throw;
            }
        }

        public Task<CalendarSettings?> GetUserSettingsAsync(string userId)
        {
            // This would need a separate repository for settings
            // For now, return default settings
            return Task.FromResult(new CalendarSettings
            {
                UserId = userId,
                DefaultView = "month",
                ShowWeekends = true,
                ShowBusinessHours = true,
                BusinessStartTime = new TimeSpan(8, 0, 0),
                BusinessEndTime = new TimeSpan(17, 0, 0),
                FirstDayOfWeek = "Saturday",
                EnableNotifications = true,
                NotificationAdvanceMinutes = 15,
                Language = "fa",
                CalendarType = "persian",
                EnableDragAndDrop = true,
                EnableResize = true,
                ShowEventDetailsOnClick = true
            })!;
        }

        public Task<CalendarSettings> SaveUserSettingsAsync(CalendarSettings settings, string userId)
        {
            settings.UserId = userId;
            settings.UpdatedAt = DateTime.Now;
            
            // This would need a separate repository for settings
            // For now, just return the settings
            return Task.FromResult(settings);
        }

        public Task LogActivityAsync(string userId, ActivityType activityType, string action, string? description = null, string? entityType = null, int? entityId = null)
        {
            var activity = new CalendarActivity
            {
                UserId = userId,
                ActivityType = activityType,
                Action = action,
                Description = description,
                EntityType = entityType,
                EntityId = entityId,
                IpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
                UserAgent = _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString(),
                Timestamp = DateTime.Now,
                SessionId = _httpContextAccessor.HttpContext?.Session.Id
            };

            // This would need a separate repository for activities
            // For now, just log to console
            Console.WriteLine($"Calendar Activity: {userId} - {action} - {description}");
            return Task.FromResult(Task.CompletedTask);
        }

        public async Task<bool> CanUserManageEventAsync(int eventId, string userId)
        {
            var calendarEvent = await _eventRepository.GetByIdAsync(eventId);
            if (calendarEvent == null) return false;
            
            return calendarEvent.UserId == userId || await IsUserManagerAsync(userId);
        }

        public async Task<bool> CanUserViewEventAsync(int eventId, string userId)
        {
            var calendarEvent = await _eventRepository.GetByIdAsync(eventId);
            if (calendarEvent == null) return false;
            
            return calendarEvent.UserId == userId || 
                   calendarEvent.Visibility == EventVisibility.Public ||
                   await IsUserManagerAsync(userId);
        }

        public async Task<bool> IsUserManagerAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;
            
            var roles = await _userManager.GetRolesAsync(user);
            return roles.Any(r => r.Contains("Admin") || r.Contains("Manager") || r.Contains("SuperAdmin"));
        }

        private async Task<CalendarEventCategory?> GetOrCreateManagementAnnouncementCategory(string userId)
        {
            try
            {
                var categories = await _categoryRepository.GetAllAsync();
                var managementCategory = categories.FirstOrDefault(c => c.Name == "اعلان مدیریتی" && !c.IsDeleted);
                
                if (managementCategory == null)
                {
                    var newCategory = new CalendarEventCategory
                    {
                        Name = "اعلان مدیریتی",
                        Color = "#e74c3c", // Red color for management announcements
                        Description = "دسته‌بندی پیش‌فرض برای اعلان‌های مدیریتی",
                        IsActive = true,
                        IsDeleted = false,
                        CreatedAt = DateTime.Now,
                        CreatedByUserId = userId
                    };
                    
                    managementCategory = await _categoryRepository.AddAsync(newCategory);
                    
                    // Log activity
                    await LogActivityAsync(userId, ActivityType.Create, "ایجاد دسته‌بندی اعلان مدیریتی", 
                        "دسته‌بندی پیش‌فرض اعلان مدیریتی ایجاد شد", "CalendarEventCategory", managementCategory.Id);
                }
                
                return managementCategory;
            }
            catch (Exception ex)
            {
                // Log the error but don't throw to prevent breaking the main functionality
                await LogActivityAsync(userId, ActivityType.Create, "خطا در ایجاد دسته‌بندی اعلان مدیریتی", 
                    ex.Message, "CalendarEventCategory", null);
                return null;
            }
        }
    }
}
