using BE;
using BE.LetterAutomation;
using DAL.Ticketing;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DAL.LetterAutomation
{
    public interface ILetterApprovalService
    {
        Task<UserOrganization> GetNextApprover(int organizationId, string excludeUserId);
        Task<ApiResponse> SubmitLetterForApproval(int letterId, string userId);
        Task<ApiResponse> GetApprovalHistory(int letterId);
    }

    public class LetterApprovalService : ILetterApprovalService
    {
        private readonly Db _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;

        public LetterApprovalService(
            Db context,
            UserManager<ApplicationUser> userManager,
            INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        public async Task<UserOrganization> GetNextApprover(int organizationId, string excludeUserId)
        {
            var org = await _context.Organizations
                .Include(o => o.Parent)
                .FirstOrDefaultAsync(o => o.Id == organizationId);

            if (org == null)
                return null!;

            // یافتن مسئولین سازمان فعلی (به جز کاربر فعلی)
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

        public async Task<ApiResponse> SubmitLetterForApproval(int letterId, string userId)
        {
            var letter = await _context.Letters.FindAsync(letterId);
            if (letter == null)
                return new ApiResponse()
                {
                    Data = null,
                    Message = "نامه مورد نظر یافت نشد",
                    Success = false
                };

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new ApiResponse()
                {
                    Data = null,
                    Message = "کاربر یافت نشد",
                    Success = false
                };

            // اگر کاربر حق امضا دارد، مستقیماً تایید می‌شود
            if (user.HaqEmza)
            {
                var approval = new LetterApproval
                {
                    LetterId = letterId,
                    RequesterUserId = userId,
                    ApproverUserId = userId,
                    OrganizationId = await GetUserOrganizationId(userId),
                    ApprovalLevel = 1,
                    Status = ApprovalStatus.Approved,
                    RequestDate = DateTime.Now,
                    ActionDate = DateTime.Now,
                    Comment = "تایید مستقیم توسط امضاکننده"
                };

                _context.LetterApprovals.Add(approval);
                letter.Status = LetterStatus.Completed;
                await _context.SaveChangesAsync();

                return new ApiResponse()
                {
                    Message = "نامه با موفقیت تایید شد",
                    Success = true,
                };
            }

            // یافتن سازمان کاربر
            var userOrgId = await GetUserOrganizationId(userId);
            if (userOrgId == 0)
                return new ApiResponse()
                {
                    Message = "سازمان کاربر مشخص نشده است",
                    Success = false,
                };

            // یافتن اولین تاییدکننده
            var firstApprover = await GetNextApprover(userOrgId, userId);
            if (firstApprover == null)
                return new ApiResponse()
                {
                    Message = "مسیر تایید برای این نامه تعریف نشده است",
                    Success = false,
                };

            // ایجاد درخواست تایید
            var approvalRequest = new LetterApproval
            {
                LetterId = letterId,
                RequesterUserId = userId,
                ApproverUserId = firstApprover.UserId,
                OrganizationId = firstApprover.OrganizationId,
                ApprovalLevel = 1,
                Status = ApprovalStatus.Pending,
                RequestDate = DateTime.Now
            };

            _context.LetterApprovals.Add(approvalRequest);
            letter.Status = LetterStatus.InReview;
            await _context.SaveChangesAsync();

            // ارسال نوتیفیکیشن
            await _notificationService.SendNotification(
                firstApprover.UserId,
                "درخواست تایید نامه جدید",
                $"نامه شماره {letter.LetterNumber} با موضوع '{letter.Subject}' برای تایید به شما ارجاع داده شده است.");

            return new ApiResponse() { Message = "نامه برای تایید به مسئول مربوطه ارسال شد", Success = true };
        }

        public async Task<ApiResponse> GetApprovalHistory(int letterId)
        {
            var approvals = await _context.LetterApprovals
                .Include(la => la.Approver)
                .Include(la => la.Organization)
                .Where(la => la.LetterId == letterId)
                .OrderBy(la => la.ApprovalLevel)
                .ToListAsync();

            return new ApiResponse() { Message = "تاریخچه تاییدها دریافت شد", Success = true };
        }

        private async Task<int> GetUserOrganizationId(string userId)
        {
            var userOrg = await _context.UserOrganizations
                .Where(uo => uo.UserId == userId && uo.IsActive)
                .OrderByDescending(uo => uo.IsHead)
                .FirstOrDefaultAsync();

            return userOrg?.OrganizationId ?? 0;
        }
    }
}