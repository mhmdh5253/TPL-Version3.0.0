using BE.Calendar;

namespace BLL.Calendar
{
    public interface ICalendarService
    {
        // Event management
        Task<IEnumerable<CalendarEvent>> GetAllEventsAsync();
        Task<CalendarEvent?> GetEventByIdAsync(int id);
        Task<CalendarEvent> CreateEventAsync(object model, string userId);
        Task<CalendarEvent> UpdateEventAsync(int id, object model, string userId);
        Task<bool> DeleteEventAsync(int id, string userId);
        
        // User events
        Task<IEnumerable<CalendarEvent>> GetUserEventsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<CalendarEvent>> GetTodayEventsAsync(string? userId = null);
        Task<IEnumerable<CalendarEvent>> GetUpcomingEventsAsync(string? userId = null, int days = 7);
        
        // Manager announcements
        Task<IEnumerable<CalendarEvent>> GetManagerAnnouncementsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<CalendarEvent> CreateManagerAnnouncementAsync(object model, string managerId);
        
        // Calendar data for FullCalendar
        Task<IEnumerable<object>> GetEventsForCalendarAsync(string? userId = null, DateTime? startDate = null, DateTime? endDate = null);
        
        // Dashboard data
        Task<CalendarDashboardViewModel> GetDashboardDataAsync(string userId);
        
        // Search and filter
        Task<IEnumerable<CalendarEvent>> SearchEventsAsync(string searchTerm, string? userId = null);
        Task<IEnumerable<CalendarEvent>> GetEventsByCategoryAsync(int categoryId, string? userId = null);
        
        // Category management
        Task<IEnumerable<CalendarEventCategory>> GetCategoriesAsync(string? userId = null);
        Task<CalendarEventCategory> CreateCategoryAsync(CalendarEventCategory category, string userId);
        Task<CalendarEventCategory> UpdateCategoryAsync(int id, CalendarEventCategory category, string userId);
        Task<bool> DeleteCategoryAsync(int id, string userId);
        
        // Settings
        Task<CalendarSettings?> GetUserSettingsAsync(string userId);
        Task<CalendarSettings> SaveUserSettingsAsync(CalendarSettings settings, string userId);
        
        // Activity logging
        Task LogActivityAsync(string userId, ActivityType activityType, string action, string? description = null, string? entityType = null, int? entityId = null);
        
        // Permissions
        Task<bool> CanUserManageEventAsync(int eventId, string userId);
        Task<bool> CanUserViewEventAsync(int eventId, string userId);
        Task<bool> IsUserManagerAsync(string userId);
    }
}
