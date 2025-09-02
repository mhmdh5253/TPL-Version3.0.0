using BE.Calendar;
using System.Linq.Expressions;

namespace DAL.Calendar
{
    public interface ICalendarCategoryRepository
    {
        Task<IEnumerable<CalendarEventCategory>> GetAllAsync();
        Task<IEnumerable<CalendarEventCategory>> GetAsync(Expression<Func<CalendarEventCategory, bool>> predicate);
        Task<CalendarEventCategory?> GetByIdAsync(int id);
        Task<CalendarEventCategory> AddAsync(CalendarEventCategory entity);
        Task<CalendarEventCategory> UpdateAsync(CalendarEventCategory entity);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<int> CountAsync(Expression<Func<CalendarEventCategory, bool>>? predicate = null);
        Task<IEnumerable<CalendarEventCategory>> GetActiveCategoriesAsync();
        Task<IEnumerable<CalendarEventCategory>> GetCategoriesByUserAsync(string userId);
        Task<IEnumerable<CalendarEventCategory>> GetSystemCategoriesAsync();
    }
}







