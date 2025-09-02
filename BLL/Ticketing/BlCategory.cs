using BE.Ticketing.SupportTicketSystem.Models;
using DAL.Ticketing;

namespace BLL.Ticketing
{
    public class BlCategory : ICategoryService
    {
        private readonly DlCategory _category;
        public BlCategory(DlCategory category)
        {
            _category = category;
        }
        public async Task<bool> CreateCategory(Category category)
        {
            return await _category.CreateCategory(category);
        }

        public async Task<List<Category>> GetAllCategories()
        {
            return await _category.GetAllCategories();
        }

        public async Task<bool> DeleteCategory(int categoryId)
        {
            if (categoryId == 1)
            {
                return false;
            }
            return await _category.DeleteCategory(categoryId);
        }

        public async Task<bool> UpdateCategory(Category category)
        {
            return await _category.UpdateCategory(category);
        }
    }
}
