using BE.LetterAutomation;
using Microsoft.EntityFrameworkCore;

namespace DAL.LetterAutomation
{
    public interface IOrganizationService
    {
        Task<UserOrganization> GetNextApprover(int organizationId, string excludeUserId);
    }

    public class OrganizationService : IOrganizationService
    {
        private readonly Db _context;

        public OrganizationService(Db context)
        {
            _context = context;
        }

        public async Task<UserOrganization> GetNextApprover(int organizationId, string excludeUserId)
        {
            var org = await _context.Organizations
                .Include(o => o.Parent)
                .FirstOrDefaultAsync(o => o.Id == organizationId);

            if (org == null)
                return null!;

            // یافتن مسئولین این سازمان (به جز کاربر فعلی)
            var approvers = await _context.UserOrganizations
                .Include(uo => uo.User)
                .Where(uo => uo.OrganizationId == org.Id &&
                             uo.UserId != excludeUserId &&
                             uo.IsActive &&
                             (uo.IsHead || uo.IsDeputy || uo.User!.HaqEmza))
                .OrderByDescending(uo => uo.IsHead)
                .ThenByDescending(uo => uo.IsDeputy)
                .ThenByDescending(uo => uo.User!.HaqEmza)
                .ToListAsync();

            if (approvers.Any())
                return approvers.First();

            // اگر در این سطح کسی نبود، به سطح بالاتر برو
            if (org.ParentId != null)
                return await GetNextApprover(org.ParentId.Value, excludeUserId);

            return null!;
        }
    }
}