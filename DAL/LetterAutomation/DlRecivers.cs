// DAL/OrganizationAutomation/IOrganizationRepository.cs
using BE.LetterAutomation;
using Microsoft.EntityFrameworkCore;

namespace DAL.LetterAutomation
{
    public interface IOrganizationRepository
    {
        Task<Organization> CreateOrganizationAsync(Organization Organization);
        Task<Organization> GetOrganizationByIdAsync(int id);
        Task<IEnumerable<Organization>> GetAllOrganizationsAsync();
        Task<IEnumerable<Organization>> GetAllOrganizationsChainingAsync();
        Task<Organization> UpdateOrganizationAsync(Organization Organization);
        Task<bool> DeleteOrganizationAsync(int id);
        Task<string> GetReceiverFullPath(string receiverId);
    }

    public class DlOrganization(Db context) : IOrganizationRepository
    {
        private readonly Db _context = context;

        public async Task<Organization> CreateOrganizationAsync(Organization Organization)
        {
            _context.Organizations.Add(Organization);
            await _context.SaveChangesAsync();
            return Organization;
        }

        public async Task<bool> DeleteOrganizationAsync(int id)
        {
            var Organization = await _context.Organizations.FindAsync(id);
            if (Organization == null) return false;

            _context.Organizations.Remove(Organization);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Organization>> GetAllOrganizationsAsync()
        {
            return await _context.Organizations.ToListAsync();
        }
        public async Task<IEnumerable<Organization>> GetAllOrganizationsChainingAsync()
        {
            return await _context.Organizations
                .Include(x => x.Children)
                .ThenInclude(child => child.Children)
                .ThenInclude(grandChild => grandChild.Children)
                .ToListAsync();
        }
        public async Task<Organization> GetOrganizationByIdAsync(int id)
        {
            return (await _context.Organizations.FirstOrDefaultAsync(l => l.Id == id))!;
        }


        public async Task<Organization> UpdateOrganizationAsync(Organization Organization)
        {
            _context.Organizations.Update(Organization);
            await _context.SaveChangesAsync();
            return Organization;
        }

        public async Task<string> GetReceiverFullPath(string receiverId)
        {
            var orgs = await _context.Organizations.ToListAsync();
            var receiver = orgs?.FirstOrDefault(x => x.Id == int.Parse(receiverId ?? "0"));
            if (receiver == null) return "نامشخص";

            var path = new List<string> { receiver.Name };
            var current = receiver;

            while (current!.ParentId != null)
            {
                current = orgs.FirstOrDefault(x => x.Id == current.ParentId);
                if (current != null) path.Insert(0, current.Name);
            }

            return string.Join(" - ", path);
        }

    }

}