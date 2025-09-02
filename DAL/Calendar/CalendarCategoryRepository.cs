using BE.Calendar;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DAL.Calendar
{
    public class CalendarCategoryRepository : ICalendarCategoryRepository
    {
        private readonly Db _context;

        public CalendarCategoryRepository(Db context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CalendarEventCategory>> GetAllAsync()
        {
            return await _context.CalendarEventCategories
                .Include(c => c.CreatedByUser)
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<CalendarEventCategory>> GetAsync(Expression<Func<CalendarEventCategory, bool>> predicate)
        {
            return await _context.CalendarEventCategories
                .Include(c => c.CreatedByUser)
                .Where(predicate)
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<CalendarEventCategory?> GetByIdAsync(int id)
        {
            return await _context.CalendarEventCategories
                .Include(c => c.CreatedByUser)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<CalendarEventCategory> AddAsync(CalendarEventCategory entity)
        {
            entity.CreatedAt = DateTime.Now;
            entity.IsDeleted = false;
            
            _context.CalendarEventCategories.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<CalendarEventCategory> UpdateAsync(CalendarEventCategory entity)
        {
            var existingEntity = await _context.CalendarEventCategories.FindAsync(entity.Id);
            if (existingEntity == null)
                throw new InvalidOperationException($"Category with ID {entity.Id} not found.");

            existingEntity.Name = entity.Name;
            existingEntity.Description = entity.Description;
            existingEntity.Color = entity.Color;
            existingEntity.Icon = entity.Icon;
            existingEntity.IsActive = entity.IsActive;
            existingEntity.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return existingEntity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.CalendarEventCategories.FindAsync(id);
            if (entity == null)
                return false;

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.Now;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.CalendarEventCategories
                .AnyAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<int> CountAsync(Expression<Func<CalendarEventCategory, bool>>? predicate = null)
        {
            var query = _context.CalendarEventCategories.Where(c => !c.IsDeleted);
            
            if (predicate != null)
                query = query.Where(predicate);

            return await query.CountAsync();
        }

        public async Task<IEnumerable<CalendarEventCategory>> GetActiveCategoriesAsync()
        {
            return await _context.CalendarEventCategories
                .Where(c => c.IsActive && !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<CalendarEventCategory>> GetCategoriesByUserAsync(string userId)
        {
            return await _context.CalendarEventCategories
                .Where(c => c.CreatedByUserId == userId && !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<CalendarEventCategory>> GetSystemCategoriesAsync()
        {
            return await _context.CalendarEventCategories
                .Where(c => c.IsSystem && !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
    }
}







