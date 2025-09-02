using BE.Calendar;
using System.Linq.Expressions;

namespace DAL.Calendar
{
    public interface ICalendarEventRepository
    {
        Task<IEnumerable<CalendarEvent>> GetAllAsync();
        Task<IEnumerable<CalendarEvent>> GetAsync(Expression<Func<CalendarEvent, bool>> predicate);
        Task<CalendarEvent?> GetByIdAsync(int id);
        Task<CalendarEvent> AddAsync(CalendarEvent entity);
        Task<CalendarEvent> UpdateAsync(CalendarEvent entity);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<int> CountAsync(Expression<Func<CalendarEvent, bool>>? predicate = null);
        
        // Calendar specific methods
        Task<IEnumerable<CalendarEvent>> GetUserEventsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<CalendarEvent>> GetManagerAnnouncementsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<CalendarEvent>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate, string? userId = null);
        Task<IEnumerable<CalendarEvent>> GetTodayEventsAsync(string? userId = null);
        Task<IEnumerable<CalendarEvent>> GetUpcomingEventsAsync(string? userId = null, int days = 7);
        Task<IEnumerable<CalendarEvent>> GetEventsByCategoryAsync(int categoryId, string? userId = null);
        Task<IEnumerable<CalendarEvent>> SearchEventsAsync(string searchTerm, string? userId = null);
    }
}

