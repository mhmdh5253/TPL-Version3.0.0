using BE.Ticketing.SupportTicketSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Ticketing
{
    public interface ICategoryService
    {
        /// <summary>
        /// ایجاد دسته‌بندی جدید
        /// </summary>
        /// <param name="category">مدل دسته‌بندی</param>
        /// <returns>نتیجه عملیات (true/false)</returns>
        Task<bool> CreateCategory(Category category);

        /// <summary>
        /// دریافت تمام دسته‌بندی‌ها
        /// </summary>
        /// <returns>لیست دسته‌بندی‌ها</returns>
        Task<List<Category>> GetAllCategories();

        /// <summary>
        /// حذف یک دسته‌بندی
        /// </summary>
        /// <param name="categoryId">شناسه دسته‌بندی</param>
        /// <returns>نتیجه عملیات (true/false)</returns>
        Task<bool> DeleteCategory(int categoryId);

        /// <summary>
        /// به‌روزرسانی اطلاعات دسته‌بندی
        /// </summary>
        /// <param name="category">مدل دسته‌بندی با اطلاعات جدید</param>
        /// <returns>نتیجه عملیات (true/false)</returns>
        Task<bool> UpdateCategory(Category category);
    }


    public class DlCategory : ICategoryService
    {
        private readonly Db _context;

        public DlCategory(Db context)
        {
            _context = context;
        }

        public async Task<bool> CreateCategory(Category category)
        {
            try
            {
                await _context.Categories.AddAsync(category);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Category>> GetAllCategories()
        {
            return await _context.Categories.Include(c => c.Tickets).ToListAsync();
        }

        public async Task<bool> DeleteCategory(int categoryId)
        {

            var category = await _context.Categories
                .Include(c => c.Tickets)
                .FirstOrDefaultAsync(c => c.Id == categoryId);

            if (category == null)
            {
                return false;
            }

            // بررسی وجود تیکت‌های مرتبط
            if (category.Tickets.Any())
            {
                // انتقال تیکت‌ها به دسته‌بندی پیش‌فرض
                var defaultCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Name == "عمومی");

                if (defaultCategory == null)
                {
                    defaultCategory = new Category { Name = "عمومی", Description = "عمومی" };
                    _context.Categories.Add(defaultCategory);
                    await _context.SaveChangesAsync();
                }

                foreach (var ticket in category.Tickets)
                {
                    ticket.CategoryId = defaultCategory.Id;
                }
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateCategory(Category category)
        {
            try
            {
                _context.Categories.Update(category);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

}
