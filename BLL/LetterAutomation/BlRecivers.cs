using BE.LetterAutomation;
using DAL.LetterAutomation;

namespace BLL.LetterAutomation
{
    public class BlRecivers : IOrganizationRepository
    {
        private readonly DlOrganization _dlorganization;

        public BlRecivers(DlOrganization dlorganization)
        {
            _dlorganization = dlorganization;
        }

        public async Task<Organization> CreateOrganizationAsync(Organization Organization)
        {
            return await _dlorganization.CreateOrganizationAsync(Organization);
        }

        public async Task<Organization> GetOrganizationByIdAsync(int id)
        {
            return await _dlorganization.GetOrganizationByIdAsync(id);
        }

        public async Task<IEnumerable<Organization>> GetAllOrganizationsAsync()
        {
            return await _dlorganization.GetAllOrganizationsAsync();
        }

        public async Task<IEnumerable<Organization>> GetAllOrganizationsChainingAsync()
        {
            return await _dlorganization.GetAllOrganizationsChainingAsync();
        }

        public async Task<Organization> UpdateOrganizationAsync(Organization Organization)
        {
            return await _dlorganization.UpdateOrganizationAsync(Organization);
        }

        public async Task<bool> DeleteOrganizationAsync(int id)
        {
            return await _dlorganization.DeleteOrganizationAsync(id);
        }

        public async Task<string> GetReceiverFullPath(string receiverId)
        {
            return await _dlorganization.GetReceiverFullPath(receiverId);
        }
    }
}
