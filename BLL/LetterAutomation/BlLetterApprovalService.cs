using BE.LetterAutomation;
using DAL.LetterAutomation;

namespace BLL.LetterAutomation
{
    public class BlLetterApprovalService : ILetterApprovalService
    {
        private readonly LetterApprovalService _letterApprovalService;
        public BlLetterApprovalService(LetterApprovalService letterApprovalService)
        {
            _letterApprovalService = letterApprovalService;
        }
        public async Task<UserOrganization> GetNextApprover(int organizationId, string excludeUserId)
        {
            return await _letterApprovalService.GetNextApprover(organizationId, excludeUserId);
        }

        public async Task<ApiResponse> SubmitLetterForApproval(int letterId, string userId)
        {
            return await _letterApprovalService.SubmitLetterForApproval(letterId, userId);
        }

        public async Task<ApiResponse> GetApprovalHistory(int letterId)
        {
            return await _letterApprovalService.GetApprovalHistory(letterId);
        }
    }
}
