using BE.LetterAutomation;
using Microsoft.EntityFrameworkCore;

namespace DAL.LetterAutomation
{
    public interface IApprovalService
    {
        Task<ApprovalResult> StartApprovalProcess(int letterId, string requesterId);
        Task<bool> IsFinalApproval(LetterApproval approval);
        Task<LetterApproval> CreateNextLevelApproval(LetterApproval currentApproval);
        Task<ApprovalResult> ReturnToPreviousLevel(LetterApproval currentApproval, string comment);
        Task<IEnumerable<ApprovalPathDto>> GetApprovalPath(int letterId);
        Task<OrganizationNode> GetOrganizationTree(int organizationId);
    }

    public class ApprovalService : IApprovalService
    {
        private readonly Db _context;
        private readonly IOrganizationService _organizationService;

        public ApprovalService(
            Db context,
            IOrganizationService organizationService)
        {
            _context = context;
            _organizationService = organizationService;
        }
        public async Task<OrganizationNode> GetOrganizationTree(int organizationId)
        {
            var organization = await _context.Organizations
                .Include(o => o.Children)
                .FirstOrDefaultAsync(o => o.Id == organizationId);

            if (organization == null)
                return null!;

            return await BuildOrganizationTree(organization);
        }

        private async Task<OrganizationNode> BuildOrganizationTree(Organization org)
        {
            var node = new OrganizationNode
            {
                Id = org.Id,
                Name = org.Name!,
                Users = await _context.UserOrganizations
                    .Where(uo => uo.OrganizationId == org.Id && uo.IsActive)
                    .Select(uo => new UserDto
                    {
                        Id = uo.User!.Id,
                        Name = $"{uo.User.FirstName} {uo.User.LastName}",
                        Position = uo.User.Semat!,
                        IsHead = uo.IsHead
                    })
                    .ToListAsync()
            };

            foreach (var child in org.Children)
            {
                node.Children.Add(await BuildOrganizationTree(child));
            }

            return node;
        }
        public async Task<ApprovalResult> StartApprovalProcess(int letterId, string requesterId)
        {
            var letter = await _context.Letters.FindAsync(letterId);
            if (letter == null)
                return new ApprovalResult(false, "نامه مورد نظر یافت نشد");

            // یافتن سازمان کاربر درخواست کننده
            var userOrg = await _context.UserOrganizations
                .Include(uo => uo.Organization)
                .FirstOrDefaultAsync(uo => uo.UserId == requesterId && uo.IsActive);

            if (userOrg == null)
                return new ApprovalResult(false, "سازمان کاربر مشخص نشده است");

            // یافتن اولین تایید کننده در سلسله مراتب
            var firstApprover = await _organizationService.GetNextApprover(userOrg.Organization!.Id, requesterId);
            if (firstApprover == null)
                return new ApprovalResult(false, "مسیر تایید برای این نامه تعریف نشده است");

            // ایجاد اولین درخواست تایید
            var approval = new LetterApproval
            {
                LetterId = letterId,
                RequesterUserId = requesterId,
                ApproverUserId = firstApprover.UserId,
                OrganizationId = firstApprover.OrganizationId,
                ApprovalLevel = 1,
                Status = ApprovalStatus.Pending,
                RequestDate = DateTime.Now
            };
            try
            {
                _context.LetterApprovals.Add(approval);
                letter.Status = LetterStatus.InReview;
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }


            return new ApprovalResult(true, "فرآیند تایید با موفقیت آغاز شد", approval);
        }

        public async Task<bool> IsFinalApproval(LetterApproval approval)
        {
            // آیا این آخرین سطح تایید است؟
            var maxLevel = await _context.LetterApprovals
                .Where(a => a.LetterId == approval.LetterId)
                .MaxAsync(a => (int?)a.ApprovalLevel) ?? 0;

            // آیا تایید کننده فعلی حق امضا دارد؟
            var approver = await _context.Users.FindAsync(approval.ApproverUserId);
            return approval.ApprovalLevel >= maxLevel || (approver?.HaqEmza ?? false);
        }

        public async Task<LetterApproval> CreateNextLevelApproval(LetterApproval currentApproval)
        {
            var nextLevel = currentApproval.ApprovalLevel + 1;
            var nextApprover = await _organizationService.GetNextApprover(
                currentApproval.OrganizationId,
                currentApproval.ApproverUserId);

            if (nextApprover == null)
                return null!;

            var newApproval = new LetterApproval
            {
                LetterId = currentApproval.LetterId,
                RequesterUserId = currentApproval.RequesterUserId,
                ApproverUserId = nextApprover.UserId,
                OrganizationId = nextApprover.OrganizationId,
                ApprovalLevel = nextLevel,
                Status = ApprovalStatus.Pending,
                RequestDate = DateTime.Now
            };

            _context.LetterApprovals.Add(newApproval);
            await _context.SaveChangesAsync();

            return newApproval;
        }

        public async Task<ApprovalResult> ReturnToPreviousLevel(LetterApproval currentApproval, string comment)
        {
            if (currentApproval.ApprovalLevel <= 1)
                return new ApprovalResult(false, "امکان بازگشت به سطح قبل وجود ندارد");

            var prevApproval = await _context.LetterApprovals
                .FirstOrDefaultAsync(a =>
                    a.LetterId == currentApproval.LetterId &&
                    a.ApprovalLevel == currentApproval.ApprovalLevel - 1);

            if (prevApproval == null)
                return new ApprovalResult(false, "سطح قبلی تایید یافت نشد");

            // به روزرسانی وضعیت سطح جاری
            currentApproval.Status = ApprovalStatus.Returned;
            currentApproval.Comment = comment;
            currentApproval.ActionDate = DateTime.Now;

            // بازگرداندن به سطح قبلی
            prevApproval.Status = ApprovalStatus.Pending;
            prevApproval.Comment += $"\nبازگشت داده شده توسط: {currentApproval.ApproverUserId}";

            await _context.SaveChangesAsync();
            return new ApprovalResult(true, "نامه با موفقیت به سطح قبل بازگردانده شد");
        }

        public async Task<IEnumerable<ApprovalPathDto>> GetApprovalPath(int letterId)
        {
            return await _context.LetterApprovals
                .Where(a => a.LetterId == letterId)
                .OrderBy(a => a.ApprovalLevel)
                .Select(a => new ApprovalPathDto
                {
                    Level = a.ApprovalLevel,
                    ApproverName = a.Approver!.FirstName + " " + a.Approver.LastName,
                    ApproverPosition = a.Approver.Semat!,
                    OrganizationName = a.Organization!.Name!,
                    Status = a.Status,
                    ActionDate = a.ActionDate,
                    Comment = a.Comment!
                })
                .ToListAsync();
        }
    }
    public class OrganizationNode
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<UserDto> Users { get; set; } = new List<UserDto>();
        public List<OrganizationNode> Children { get; set; } = new List<OrganizationNode>();
    }

    public class UserDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public bool IsHead { get; set; }
    }
    public class ApprovalPathDto
    {
        public int Level { get; set; }
        public string ApproverName { get; set; }
        public string ApproverPosition { get; set; }
        public string OrganizationName { get; set; }
        public ApprovalStatus Status { get; set; }
        public DateTime? ActionDate { get; set; }
        public string Comment { get; set; }
    }

    public class ApprovalResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }

        public ApprovalResult(bool success, string message, object data = null)
        {
            Success = success;
            Message = message;
            Data = data;
        }
    }
}