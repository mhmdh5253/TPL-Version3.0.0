using BE.Calendar;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DAL.Calendar
{
    public class CalendarEventRepository : ICalendarEventRepository
    {
        private readonly Db _context;

        public CalendarEventRepository(Db context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CalendarEvent>> GetAllAsync()
        {
            return await _context.CalendarEvents
                .Include(e => e.Category)
                .Include(e => e.User)
                .Include(e => e.Participants)
                .Include(e => e.Reminders)
                .Where(e => !e.IsDeleted)
                .OrderBy(e => e.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<CalendarEvent>> GetAsync(Expression<Func<CalendarEvent, bool>> predicate)
        {
            return await _context.CalendarEvents
                .Include(e => e.Category)
                .Include(e => e.User)
                .Include(e => e.Participants)
                .Include(e => e.Reminders)
                .Where(predicate)
                .Where(e => !e.IsDeleted)
                .OrderBy(e => e.StartDate)
                .ToListAsync();
        }

        public async Task<CalendarEvent?> GetByIdAsync(int id)
        {
            return await _context.CalendarEvents
                .Include(e => e.Category)
                .Include(e => e.User)
                .Include(e => e.Participants)
                .Include(e => e.Reminders)
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
        }

        public async Task<CalendarEvent> AddAsync(CalendarEvent entity)
        {
            entity.CreatedAt = DateTime.Now;
            _context.CalendarEvents.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<CalendarEvent> UpdateAsync(CalendarEvent entity)
        {
            entity.UpdatedAt = DateTime.Now;
            _context.CalendarEvents.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null) return false;
            
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.CalendarEvents.AnyAsync(e => e.Id == id && !e.IsDeleted);
        }

        public async Task<int> CountAsync(Expression<Func<CalendarEvent, bool>>? predicate = null)
        {
            var query = _context.CalendarEvents.Where(e => !e.IsDeleted);
            if (predicate != null)
                query = query.Where(predicate);
            return await query.CountAsync();
        }

        public async Task<IEnumerable<CalendarEvent>> GetUserEventsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.CalendarEvents
                .Include(e => e.Category)
                .Include(e => e.User)
                .Include(e => e.Participants)
                .Where(e => !e.IsDeleted && (e.UserId == userId || e.IsManagerAnnouncement || e.Participants.Any(p => p.UserId == userId)));

            if (startDate.HasValue)
                query = query.Where(e => e.StartDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(e => e.EndDate <= endDate.Value);

            return await query.OrderBy(e => e.StartDate).ToListAsync();
        }

        public async Task<IEnumerable<CalendarEvent>> GetManagerAnnouncementsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.CalendarEvents
                .Include(e => e.Category)
                .Include(e => e.User)
                .Where(e => !e.IsDeleted && e.IsManagerAnnouncement);

            if (startDate.HasValue)
                query = query.Where(e => e.StartDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(e => e.EndDate <= endDate.Value);

            return await query.OrderBy(e => e.StartDate).ToListAsync();
        }

        public async Task<IEnumerable<CalendarEvent>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate, string? userId = null)
        {
            // Normalize endDate to end-of-day to include events on the same day
            var normalizedStart = startDate;
            var normalizedEnd = endDate;
            if (normalizedEnd.TimeOfDay.Ticks == 0)
            {
                normalizedEnd = normalizedEnd.Date.AddDays(1).AddTicks(-1);
            }

            var query = _context.CalendarEvents
                .Include(e => e.Category)
                .Include(e => e.User)
                .Include(e => e.Participants)
                .Where(e => !e.IsDeleted && 
                           ((e.StartDate >= normalizedStart && e.StartDate <= normalizedEnd) ||
                            (e.EndDate >= normalizedStart && e.EndDate <= normalizedEnd) ||
                            (e.StartDate <= normalizedStart && e.EndDate >= normalizedEnd)));

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(e => e.UserId == userId || e.Visibility == EventVisibility.Public || e.IsManagerAnnouncement || e.Participants.Any(p => p.UserId == userId));

            return await query.OrderBy(e => e.StartDate).ToListAsync();
        }

        public async Task<IEnumerable<CalendarEvent>> GetTodayEventsAsync(string? userId = null)
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            
            return await GetEventsByDateRangeAsync(today, tomorrow, userId);
        }

        public async Task<IEnumerable<CalendarEvent>> GetUpcomingEventsAsync(string? userId = null, int days = 7)
        {
            var startDate = DateTime.Today;
            var endDate = startDate.AddDays(days);
            
            return await GetEventsByDateRangeAsync(startDate, endDate, userId);
        }

        public async Task<IEnumerable<CalendarEvent>> GetEventsByCategoryAsync(int categoryId, string? userId = null)
        {
            var query = _context.CalendarEvents
                .Include(e => e.Category)
                .Include(e => e.User)
                .Include(e => e.Participants)
                .Where(e => !e.IsDeleted && e.CategoryId == categoryId);

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(e => e.UserId == userId || e.Visibility == EventVisibility.Public || e.IsManagerAnnouncement || e.Participants.Any(p => p.UserId == userId));

            return await query.OrderBy(e => e.StartDate).ToListAsync();
        }

        public async Task<IEnumerable<CalendarEvent>> SearchEventsAsync(string searchTerm, string? userId = null)
        {
            var query = _context.CalendarEvents
                .Include(e => e.Category)
                .Include(e => e.User)
                .Include(e => e.Participants)
                .Where(e => !e.IsDeleted && 
                           (e.Title.Contains(searchTerm) || 
                            e.Description!.Contains(searchTerm) || 
                            e.Location!.Contains(searchTerm)));

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(e => e.UserId == userId || e.Visibility == EventVisibility.Public || e.IsManagerAnnouncement || e.Participants.Any(p => p.UserId == userId));

            return await query.OrderBy(e => e.StartDate).ToListAsync();
        }
    }
}

