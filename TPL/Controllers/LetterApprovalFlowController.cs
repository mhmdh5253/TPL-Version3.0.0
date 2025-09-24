using BE;
using BE.LetterAutomation;
using BLL.LetterAutomation;
using DAL;
using DAL.LetterAutomation;
using DAL.Ticketing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace TPLWeb.Controllers
{
    [Authorize]
    [Route("ApprovalFlow")]
    public class LetterApprovalFlowController : Controller
    {
        private readonly Db _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApprovalService _approvalService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<LetterApprovalController> _logger;
        private readonly BlLetter _letter;

        public LetterApprovalFlowController(
            Db context,
            UserManager<ApplicationUser> userManager,
            IApprovalService approvalService,
            INotificationService notificationService,
            ILogger<LetterApprovalController> logger,
            BlLetter letter)
        {
            _context = context;
            _userManager = userManager;
            _approvalService = approvalService;
            _notificationService = notificationService;
            _logger = logger;
            _letter = letter;
        }

        // شروع فرآیند تایید
        [HttpPost("start")]
        public async Task<IActionResult> StartApprovalProcess([FromBody] int letterId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var letter = await _context.Letters.FindAsync(letterId);

            if (letter == null)
                return NotFound(new { Message = "نامه مورد نظر یافت نشد" });

            // بررسی مالکیت نامه
            if (letter.Username != currentUser!.UserName && !User.IsInRole("Admin"))
                return Forbid();

            // ثبت اقدام
            await AddActionVoid(new ActionModel()
            {
                LetterId = letterId,
                Description = "فرآیند تایید نامه آغاز شد"
            });

            // اگر کاربر حق امضا دارد، مستقیماً تایید می‌شود
            if (currentUser.HaqEmza)
            {
                var res = await FinalApprove(letterId, currentUser.Id);
                var respo = new
                {
                    success = res.Success,
                    message = res.Message,
                    data = (object)res.Data!
                };

                return Ok(respo);
            }

            // شروع فرآیند تایید سلسله مراتبی
            var result = await _approvalService.StartApprovalProcess(letterId, currentUser.Id);

            return Ok(new
            {
                success = result.Success,
                message = result.Message,
                data = result.Data, // اگر وجود دارد
            });
        }

        /// <summary>
        /// تایید نهایی نامه توسط کاربر دارای حق امضا
        /// </summary>
        private async Task<ApiResponse> FinalApprove(int letterId, string approverUserId)
        {
            try
            {
                // دریافت نامه از پایگاه داده
                var letter = await _context.Letters
                    .Include(l => l.Referrals)
                    .FirstOrDefaultAsync(l => l.Id == letterId);

                if (letter == null)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "نامه مورد نظر یافت نشد"
                    };
                }

                // دریافت اطلاعات کاربر تاییدکننده
                var approver = await _userManager.FindByIdAsync(approverUserId);
                if (approver == null)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "اطلاعات کاربر تاییدکننده نامعتبر است"
                    };
                }

                // بررسی حق امضای کاربر
                if (!approver.HaqEmza)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "شما مجوز تایید نهایی این نامه را ندارید"
                    };
                }

                // بررسی وضعیت فعلی نامه
                if (letter.Status == LetterStatus.Approved)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "این نامه قبلاً تایید شده است"
                    };
                }

                if (letter.Status == LetterStatus.Rejected)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "این نامه قبلاً رد شده و قابل تایید نیست"
                    };
                }

                var org= await GetUserOrganizationId(approverUserId);
                if (string.IsNullOrEmpty($"{org}"))
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "مقام شما هنوز مشخص نشده است "
                    };
                }
                // ایجاد رکورد تایید نهایی
                var finalApproval = new LetterApproval
                {
                    LetterId = letterId,
                    RequesterUserId = letter.Username!,
                    ApproverUserId = approverUserId,
                    OrganizationId = await GetUserOrganizationId(approverUserId),
                    ApprovalLevel = 1,
                    Status = ApprovalStatus.Approved,
                    Comment = "تایید نهایی توسط امضاکننده مجاز",
                    RequestDate = DateTime.Now,
                    ActionDate = DateTime.Now
                };

                _context.LetterApprovals.Add(finalApproval);

                // به روزرسانی وضعیت نامه
                letter.Status = LetterStatus.Approved;
                letter.LastModifiedDate = DateTime.Now;
                letter.LastModifiedBy = approver.UserName;
                letter.LetterNumber = _letter.GenerateLetterNumber();

                // بستن تمام ارجاعات مرتبط
                foreach (var referral in letter.Referrals.Where(r => !r.IsCompleted))
                {
                    referral.IsCompleted = true;
                    referral.ReferralDate = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                // ثبت اقدام
                await AddActionVoid(new ActionModel()
                {
                    LetterId = letterId,
                    Description = $"نامه توسط {approver.FirstName} {approver.LastName} تایید نهایی شد"
                });

                // ارسال نوتیفیکیشن به کاربر ثبت کننده
                await _notificationService.SendNotification(
                    letter.Username!,
                    "نامه شما تایید شد",
                    $"نامه شماره {letter.LetterNumber} با موضوع '{letter.Subject}' توسط {approver.FirstName} {approver.LastName} تایید نهایی شد.");

                // ثبت در تاریخچه سیستم
                _logger.LogInformation(
                   $"تایید نهایی نامه {letter.LetterNumber}",
                   $"نامه با شناسه {letterId} توسط کاربر {approver.UserName} تایید شد",
                   approverUserId);

                return new ApiResponse
                {
                    Success = true,
                    Message = "نامه با موفقیت تایید نهایی شد",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در تایید نهایی نامه {LetterId}", letterId);
                return new ApiResponse
                {
                    Success = false,
                    Message = "خطای داخلی در پردازش درخواست تایید نهایی"
                };
            }
        }

        // تایید نامه در یک سطح
        [HttpPost("approve")]
        public async Task<IActionResult> Approve([FromBody] ApprovalCommentModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var approval = await _context.LetterApprovals
                .Include(a => a.Letter)
                .Include(a => a.Approver)
                .FirstOrDefaultAsync(a => a.Id == model.approvalId);

            if (approval == null)
                return Ok(new { success = false, message = "درخواست تایید یافت نشد" });

            // بررسی مجوز تایید
            if (approval.ApproverUserId != currentUser!.Id)
                return Forbid();

            // تایید در سطح جاری
            approval.Status = ApprovalStatus.Approved;
            approval.Comment = model.Comment;
            approval.ActionDate = DateTime.Now;
            _context.Update(approval);

            // ثبت اقدام
            await AddActionVoid(new ActionModel()
            {
                LetterId = approval.LetterId,
                Description = $"نامه توسط {currentUser.FirstName} {currentUser.LastName} در سطح {approval.ApprovalLevel} تایید شد" +
                             (!string.IsNullOrEmpty(model.Comment) ? $". توضیحات: {model.Comment}" : "")
            });

            // بررسی نیاز به تایید سطح بالاتر
            if (!await _approvalService.IsFinalApproval(approval))
            {
                // ایجاد درخواست تایید سطح بعد
                var nextApproval = await _approvalService.CreateNextLevelApproval(approval);
                await _notificationService.SendNotification(nextApproval.ApproverUserId!,
                    "درخواست تایید جدید",
                    $"نامه شماره {approval.Letter!.LetterNumber} برای تایید به شما ارجاع داده شد");
            }
            else
            {
                // تایید نهایی نامه
                approval.Letter!.Status = LetterStatus.Approved;
                await _notificationService.SendNotification(approval.RequesterUserId!,
                    "نامه شما تایید شد",
                    $"نامه شماره {approval.Letter.LetterNumber} با موفقیت تایید شد");
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "نامه با موفقیت تایید شد" });
        }

        // رد نامه
        [HttpPost("reject")]
        public async Task<IActionResult> Reject([FromBody] ApprovalCommentModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var approval = await _context.LetterApprovals
                .Include(a => a.Letter)
                .Include(a => a.Requester)
                .FirstOrDefaultAsync(a => a.LetterId == model.approvalId);

            if (approval == null)
                return Ok(new { success = false, message = "درخواست تایید یافت نشد" });

            if (approval.ApproverUserId != currentUser!.Id)
                return Forbid();

            approval.Status = ApprovalStatus.Rejected;
            approval.Comment = model.Comment;
            approval.ActionDate = DateTime.Now;
            approval.Letter!.Status = LetterStatus.Rejected;

            // ثبت اقدام
            await AddActionVoid(new ActionModel()
            {
                LetterId = approval.LetterId,
                Description = $"نامه توسط {currentUser.FirstName} {currentUser.LastName} رد شد. دلیل: {model.Comment}"
            });

            await _notificationService.SendNotification(approval.RequesterUserId!,
                "نامه شما رد شد",
                $"نامه شماره {approval.Letter.LetterNumber} توسط {currentUser.FirstName} {currentUser.LastName} رد شد. دلیل: {model.Comment}");

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "نامه با موفقیت رد شد" });
        }

        // بازگشت نامه به سطح قبل
        [HttpPost("return/{approvalId}")]
        public async Task<IActionResult> ReturnToPreviousLevel(int approvalId, [FromBody] ApprovalCommentModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var approval = await _context.LetterApprovals
                .Include(a => a.Letter)
                .FirstOrDefaultAsync(a => a.Id == approvalId);

            if (approval == null)
                return NotFound(new { Message = "درخواست تایید یافت نشد" });

            if (approval.ApproverUserId != currentUser!.Id)
                return Forbid();

            // ثبت اقدام
            await AddActionVoid(new ActionModel()
            {
                LetterId = approval.LetterId,
                Description = $"نامه توسط {currentUser.FirstName} {currentUser.LastName} به سطح قبل بازگردانده شد. دلیل: {model.Comment}"
            });

            // بازگشت به سطح قبل
            var result = await _approvalService.ReturnToPreviousLevel(approval, model.Comment!);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("organization-structure")]
        public async Task<IActionResult> GetOrganizationStructure()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var userOrg = await _context.UserOrganizations
                .Where(uo => uo.UserId == currentUser!.Id && uo.IsActive)
                .FirstOrDefaultAsync();

            if (userOrg == null)
                return Forbid();

            var structure = await _approvalService.GetOrganizationTree(userOrg.OrganizationId);
            return Ok(new { Success = true, Data = structure });
        }

        [HttpPost("refer")]
        public async Task<IActionResult> ReferLetter([FromBody] ReferralRequestModel model)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var letter = await _context.Letters.FindAsync(model.LetterId);

                if (letter == null)
                    return NotFound(new { Success = false, Message = "نامه مورد نظر یافت نشد" });

                if (letter.Username != currentUser!.UserName && !User.IsInRole("Admin"))
                    return Forbid();

                var receiver = await _userManager.FindByIdAsync(model.ReceiverUserId!);
                if (receiver == null)
                    return BadRequest(new { Success = false, Message = "گیرنده نامعتبر است" });

                var currentUserOrg = await GetUserOrganizationId(currentUser.Id);
                var receiverOrg = await GetUserOrganizationId(receiver.Id);

                if (currentUserOrg == 0 || receiverOrg == 0 || currentUserOrg != receiverOrg)
                    return BadRequest(new { Success = false, Message = "فقط امکان ارجاع به هم سازمانی‌ها وجود دارد" });

                var referral = new LetterReferral
                {
                    LetterId = model.LetterId,
                    ReferredByUserId = currentUser.Id,
                    ReferredToUserId = receiver.Id,
                    ReferralDate = DateTime.Now,
                    Comment = model.Comment,
                    IsCompleted = false,
                };

                _context.LetterReferrals.Add(referral);
                await _context.SaveChangesAsync();

                // ثبت اقدام
                await AddActionVoid(new ActionModel()
                {
                    LetterId = model.LetterId,
                    Description = $"نامه به {receiver.FirstName} {receiver.LastName} ارجاع داده شد" +
                                 (!string.IsNullOrEmpty(model.Comment) ? $". توضیحات: {model.Comment}" : "")
                });

                await _notificationService.SendNotification(
                    receiver.Id,
                    "ارجاع نامه جدید",
                    $"نامه شماره {letter.LetterNumber} با موضوع '{letter.Subject}' توسط {currentUser.FirstName} {currentUser.LastName} به شما ارجاع داده شد.");

                return Ok(new
                {
                    Success = true,
                    Message = "نامه با موفقیت ارجاع داده شد",
                    Data = new
                    {
                        ReferralId = referral.Id,
                        ReceiverName = $"{receiver.FirstName} {receiver.LastName}"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ارجاع نامه {LetterId}", model.LetterId);
                return StatusCode(500, new { Success = false, Message = "خطا در پردازش درخواست ارجاع" });
            }
        }

        [HttpPost("multi-refer")]
        public async Task<IActionResult> MultiReferLetter([FromBody] MultiReferralRequestModel model)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var letter = await _context.Letters.FindAsync(model.LetterId);

                if (letter == null)
                    return NotFound(new { Success = false, Message = "نامه مورد نظر یافت نشد" });

                if (letter.Username != currentUser!.UserName && !User.IsInRole("Admin"))
                    return Forbid();

                if (model.ReceiverUserIds.Count > 100)
                    return BadRequest(new { Success = false, Message = "حداکثر 100 گیرنده مجاز است" });

                var currentUserOrg = await GetUserOrganizationId(currentUser.Id);
                if (currentUserOrg == 0)
                    return BadRequest(new { Success = false, Message = "سازمان شما نامعتبر است" });

                var referrals = new List<LetterReferral>();
                var successfulReferrals = new List<string>();

                foreach (var receiverId in model.ReceiverUserIds)
                {
                    var receiverOrg = await GetUserOrganizationId(receiverId);
                    //if (currentUserOrg != receiverOrg)
                    //    continue;

                    var referral = new LetterReferral
                    {
                        IsCompleted = false,
                        LetterId = model.LetterId,
                        ReferredByUserId = currentUser.Id,
                        ReferredToUserId = receiverId,
                        ReferralDate = DateTime.Now,
                        Comment = model.CommonComment + (model.PersonalComments.TryGetValue(receiverId, out var personalComment) ? $"\nپیام شخصی: {personalComment}" : " ")
                    };

                    _context.LetterReferrals.Add(referral);
                    successfulReferrals.Add(receiverId);
                }

                await _context.SaveChangesAsync();

                // ثبت اقدام
                await AddActionVoid(new ActionModel()
                {
                    LetterId = model.LetterId,
                    Description = $"نامه به {model.ReceiverUserIds.Count} نفر ارجاع داده شد" +
                                 (!string.IsNullOrEmpty(model.CommonComment) ? $". توضیحات عمومی: {model.CommonComment}" : "")
                });

                foreach (var receiverId in successfulReferrals)
                {
                    await _notificationService.SendNotification(
                        receiverId,
                        "ارجاع نامه جدید",
                        $"نامه شماره {letter.LetterNumber} با موضوع '{letter.Subject}' توسط {currentUser.FirstName} {currentUser.LastName} به شما ارجاع داده شد.");
                }

                return Ok(new
                {
                    Success = true,
                    Message = $"نامه با موفقیت به {successfulReferrals.Count} نفر ارجاع داده شد",
                    Data = successfulReferrals
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ارجاع گروهی نامه {LetterId}", model.LetterId);
                return StatusCode(500, new { Success = false, Message = "خطا در پردازش درخواست ارجاع گروهی" });
            }
        }

        [HttpGet("organization-users")]
        public async Task<IActionResult> GetOrganizationUsers()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserOrg = await GetUserOrganizationId(currentUser!.Id);

            if (currentUserOrg == 0)
                return Forbid();

            var users = await _context.UserOrganizations
                .Where(uo =>
                    uo.IsActive &&
                    (uo.OrganizationId == currentUserOrg || (uo.Organization != null && uo.Organization.ParentId == currentUserOrg)))
                .Select(uo => new
                {
                    Id = uo.User!.Id,
                    Name = $"{uo.User.FirstName} {uo.User.LastName}",
                    Position = uo.User.Semat,
                    Organization = uo.Organization!.Name,
                    IsHead = uo.IsHead
                })
                .ToListAsync();

            return Ok(new { Success = true, Data = users });
        }

        [HttpGet("eligible-return-destinations")]
        public async Task<IActionResult> GetEligibleReturnDestinations([FromQuery] int letterId)
        {
            try
            {
                var letter = await _context.Letters
                    .Include(l => l.Referrals)
                    .FirstOrDefaultAsync(l => l.Id == letterId);
                if (letter == null)
                    return Ok(new { Success = false, Message = "نامه یافت نشد" });

                // Previous recipients from referrals
                var previousUserIds = await _context.LetterReferrals
                    .Where(r => r.LetterId == letterId && r.ReferredToUserId != null)
                    .OrderByDescending(r => r.ReferralDate)
                    .Select(r => r.ReferredToUserId!)
                    .Distinct()
                    .Take(20) // limit
                    .ToListAsync();

                // Also include approvers from approval chain
                var approverIds = await _context.LetterApprovals
                    .Where(a => a.LetterId == letterId && a.ApproverUserId != null)
                    .OrderByDescending(a => a.ActionDate)
                    .Select(a => a.ApproverUserId!)
                    .Distinct()
                    .Take(20)
                    .ToListAsync();

                var userIds = previousUserIds
                    .Concat(approverIds)
                    .Distinct()
                    .ToList();

                var users = await _context.Users
                    .Where(u => userIds.Contains(u.Id))
                    .Select(u => new { u.Id, Name = (u.FirstName + " " + u.LastName).Trim(), UserName = u.UserName, Position = u.Semat })
                    .ToListAsync();

                // Organizations of those users
                var orgIds = await _context.UserOrganizations
                    .Where(uo => userIds.Contains(uo.UserId!) && uo.IsActive)
                    .Select(uo => uo.OrganizationId)
                    .Distinct()
                    .ToListAsync();

                var organizations = await _context.Organizations
                    .Where(o => orgIds.Contains(o.Id))
                    .Select(o => new { o.Id, o.Name, o.FullName, o.OrgType })
                    .ToListAsync();

                return Ok(new { Success = true, Data = new { Users = users, Organizations = organizations } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting eligible return destinations for letter {LetterId}", letterId);
                return StatusCode(500, new { Success = false, Message = "خطا در پردازش" });
            }
        }

        [HttpPost("delete")]
        public async Task<IActionResult> DeleteLetter([FromBody] int letterId)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var letter = await _context.Letters
                    .Include(l => l.Referrals)
                    .Include(l => l.CopyReceiversList)
                    .FirstOrDefaultAsync(l => l.Id == letterId);

                if (letter == null)
                    return Ok(new { Success = false, Message = "نامه مورد نظر یافت نشد" });

                bool isAdmin = User.IsInRole("Admin");
                bool isOwner = letter.Username == currentUser!.UserName;

                if (!isOwner && !isAdmin)
                    return Ok(new { Success = false, Message = "شما دسترسی حذف نامه را ندارید" });

                if (letter.Status == LetterStatus.Approved && !isAdmin)
                {
                    return Ok(new
                    {
                        Success = false,
                        Message = "حذف نامه‌های تایید شده فقط توسط مدیر سیستم امکان‌پذیر است"
                    });
                }

                if (letter.Status == LetterStatus.Pending && !isAdmin)
                {
                    return Ok(new
                    {
                        Success = false,
                        Message = "نامه در حال فرآیند تایید است و فقط توسط مدیر سیستم قابل حذف می‌باشد"
                    });
                }

                letter.Status = LetterStatus.Deleted;
                letter.IsDeleted = true;

                _context.Letters.Update(letter);
                await _context.SaveChangesAsync();

                // ثبت اقدام
                await AddActionVoid(new ActionModel()
                {
                    LetterId = letterId,
                    Description = $"نامه توسط {currentUser.FirstName} {currentUser.LastName} حذف شد"
                });

                _logger.LogInformation(
                    $"حذف نامه {letter.LetterNumber}",
                    $"نامه با شناسه {letterId} توسط کاربر {currentUser.UserName} حذف شد",
                    currentUser.Id);

                return Ok(new
                {
                    Success = true,
                    Message = "نامه با موفقیت حذف شد"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در حذف نامه {LetterId}", letterId);
                return Ok(new
                {
                    Success = false,
                    Message = "خطا در پردازش درخواست حذف نامه"
                });
            }
        }

        [HttpPost("AddAction")]
        public async Task<IActionResult> AddAction([FromBody] ActionModel model)
        {
            try
            {
                await AddActionVoid(model);
                return Ok(new { Success = true, Message = "اقدام با موفقیت ثبت شد" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ثبت اقدام برای نامه {LetterId}", model.LetterId);
                return StatusCode(500, new { Success = false, Message = "خطا در ثبت اقدام" });
            }
        }

        [HttpPost("GetActions")]
        public async Task<IActionResult> GetActions([FromBody] int letterId)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                var actions = await _context.LetterActions
                    .Where(a => a.LetterId == letterId)
                    .OrderByDescending(a => a.ActionDate)
                    .Select(a => new
                    {
                        a.Id,
                        a.ActionDescription,
                        ActionDate = a.ActionDate,
                        UserFullName = a.User!.FirstName + " " + a.User.LastName,
                        IsCurrentUser = a.UserId == currentUserId,
                        TimeAgo = GetTimeAgo(a.ActionDate)
                    })
                    .ToListAsync();

                var response = new
                {
                    Success = true,
                    Data = new
                    {
                        Actions = actions,
                        TotalCount = actions.Count,
                        LastAction = actions.FirstOrDefault(),
                        CurrentUserId = currentUserId
                    }
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت اقدامات نامه {LetterId}", letterId);

                var errorResponse = new
                {
                    Success = false,
                    Error = new
                    {
                        Message = "خطا در دریافت لیست اقدامات",
                        Code = "GET_ACTIONS_ERROR",
                        Details = ex.Message ?? null,
                        Timestamp = DateTime.Now
                    },
                    Meta = new
                    {
                        ServerTime = DateTime.Now.ToLocalTime()
                    }
                };

                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost("return")]
        public async Task<IActionResult> ReturnLetter([FromBody] ReturnLetterModel model)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var letter = await _context.Letters.FindAsync(model.LetterId);

                if (letter == null)
                    return NotFound(new { Success = false, Message = "نامه مورد نظر یافت نشد" });

                if (letter.Username != currentUser!.UserName && !User.IsInRole("Admin"))
                    return Forbid();

                letter.Status = LetterStatus.Returned;
                letter.LastModifiedDate = DateTime.Now;
                letter.LastModifiedBy = currentUser.UserName;

                ApplicationUser? receiver = null;
                // Compute eligible destinations (previous recipients and approvers)
                var previousUserIds = await _context.LetterReferrals
                    .Where(r => r.LetterId == model.LetterId && r.ReferredToUserId != null)
                    .OrderByDescending(r => r.ReferralDate)
                    .Select(r => r.ReferredToUserId!)
                    .Distinct()
                    .ToListAsync();
                var approverIds = await _context.LetterApprovals
                    .Where(a => a.LetterId == model.LetterId && a.ApproverUserId != null)
                    .OrderByDescending(a => a.ActionDate)
                    .Select(a => a.ApproverUserId!)
                    .Distinct()
                    .ToListAsync();
                var eligibleUserIds = previousUserIds.Concat(approverIds).Distinct().ToList();
                var eligibleOrgIds = await _context.UserOrganizations
                    .Where(uo => eligibleUserIds.Contains(uo.UserId!) && uo.IsActive)
                    .Select(uo => uo.OrganizationId)
                    .Distinct()
                    .ToListAsync();
                // Prepare reason/description text once
                var reasonText = $"{model.Reason}";
                var reasonWithDesc = string.IsNullOrWhiteSpace(model.Description)
                    ? reasonText
                    : reasonText + " - " + model.Description;
                if (!string.IsNullOrWhiteSpace(model.DestinationUserId))
                {
                    if (eligibleUserIds.Count > 0 && !eligibleUserIds.Contains(model.DestinationUserId))
                    {
                        return BadRequest(new { Success = false, Message = "مقصد انتخاب‌شده مجاز نیست" });
                    }
                    receiver = await _userManager.FindByIdAsync(model.DestinationUserId);
                    if (receiver == null)
                        return BadRequest(new { Success = false, Message = "مقصد عودت نامعتبر است" });

                    var referral = new LetterReferral
                    {
                        LetterId = model.LetterId,
                        ReferredByUserId = currentUser.Id,
                        ReferredToUserId = receiver.Id,
                        ReferralDate = DateTime.Now,
                        Comment = "عودت: " + reasonWithDesc,
                        IsCompleted = false
                    };
                    _context.LetterReferrals.Add(referral);
                }
                else if (model.DestinationOrganizationId.HasValue)
                {
                    if (eligibleOrgIds.Count > 0 && !eligibleOrgIds.Contains(model.DestinationOrganizationId.Value))
                    {
                        return BadRequest(new { Success = false, Message = "سازمان مقصد مجاز نیست" });
                    }
                    // Create referrals for all active users in the organization (or just heads)
                    var orgUsers = await _context.UserOrganizations
                        .Where(uo => uo.OrganizationId == model.DestinationOrganizationId.Value && uo.IsActive)
                        .Select(uo => uo.UserId!)
                        .Distinct()
                        .ToListAsync();

                    foreach (var userId in orgUsers)
                    {
                        var referral = new LetterReferral
                        {
                            LetterId = model.LetterId,
                            ReferredByUserId = currentUser.Id,
                            ReferredToUserId = userId,
                            ReferralDate = DateTime.Now,
                            Comment = "عودت به سازمان: " + reasonWithDesc,
                            IsCompleted = false
                        };
                        _context.LetterReferrals.Add(referral);
                    }
                }

                var action = new LetterAction
                {
                    LetterId = model.LetterId,
                    UserId = currentUser.Id,
                    ActionDescription = (receiver != null)
                        ? $"عودت نامه به {receiver.FirstName} {receiver.LastName} به دلیل: {reasonWithDesc}"
                        : $"عودت نامه به دلیل: {reasonWithDesc}"
                };
                _context.LetterActions.Add(action);

                await _context.SaveChangesAsync();

                if (receiver != null)
                {
                    await _notificationService.SendNotification(
                        receiver.Id,
                        "عودت نامه",
                        $"نامه شماره {letter.LetterNumber} با موضوع '{letter.Subject}' به شما عودت داده شد. دلیل: {model.Reason}");
                }
                else if (model.DestinationOrganizationId.HasValue)
                {
                    // notify all users in org
                    var orgUsers = await _context.UserOrganizations
                        .Where(uo => uo.OrganizationId == model.DestinationOrganizationId.Value && uo.IsActive)
                        .Select(uo => uo.UserId!)
                        .Distinct()
                        .ToListAsync();
                    foreach (var userId in orgUsers)
                    {
                        await _notificationService.SendNotification(
                            userId,
                            "عودت نامه",
                            $"نامه شماره {letter.LetterNumber} با موضوع '{letter.Subject}' به سازمان شما عودت داده شد. دلیل: {model.Reason}");
                    }
                }

                return Ok(new
                {
                    Success = true,
                    Message = "نامه با موفقیت عودت داده شد",
                    Data = new { LetterId = model.LetterId, NewStatus = letter.Status.ToString() }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در عودت نامه {LetterId}", model.LetterId);
                return StatusCode(500, new { Success = false, Message = "خطا در پردازش درخواست عودت" });
            }
        }

        [HttpPost("archive")]
        public async Task<IActionResult> ArchiveLetter([FromBody] ArchiveLetterModel model)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var letter = await _context.Letters.FindAsync(model.LetterId);

                if (letter == null)
                    return NotFound(new { Success = false, Message = "نامه مورد نظر یافت نشد" });

                if (letter.Username != currentUser!.UserName && !User.IsInRole("Admin"))
                    return Forbid();

                var classification = await _context.Kelasehnamehha.FirstOrDefaultAsync(k => k.CodeKelaseh == model.ClassificationCode);
                if (classification == null)
                    return BadRequest(new { Success = false, Message = "کد کلاسه نامعتبر است" });

                letter.ArchiveCode = model.ClassificationCode.ToString();
                letter.OrganizationArchiveDate = DateTime.Now;
                letter.Status = LetterStatus.Archived;

                // ثبت اقدام
                await AddActionVoid(new ActionModel()
                {
                    LetterId = model.LetterId,
                    Description = $"نامه در کلاسه {model.ClassificationCode} بایگانی شد" +
                                 (!string.IsNullOrEmpty(model.Description) ? $". توضیحات: {model.Description}" : "")
                });

                var action = new LetterAction
                {
                    LetterId = model.LetterId,
                    UserId = currentUser.Id,
                    ActionDescription = $"بایگانی نامه در کلاسه {model.ClassificationCode} - {classification.NameKelaseh}"
                };
                _context.LetterActions.Add(action);

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Success = true,
                    Message = "نامه با موفقیت بایگانی شد",
                    Data = new
                    {
                        LetterId = model.LetterId,
                        ArchiveCode = model.ClassificationCode,
                        ArchiveDate = DateTime.Now
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بایگانی نامه {LetterId}", model.LetterId);
                return StatusCode(500, new { Success = false, Message = "خطا در پردازش درخواست بایگانی" });
            }
        }

        [HttpPost("quick-archive")]
        public async Task<IActionResult> QuickArchive([FromBody] int letterId)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var letter = await _context.Letters.FindAsync(letterId);

                if (letter == null)
                    return NotFound(new { Success = false, Message = "نامه مورد نظر یافت نشد" });

                if (letter.Username != currentUser!.UserName && !User.IsInRole("Admin"))
                    return Forbid();

                letter.Status = LetterStatus.Archived;
                letter.LastModifiedDate = DateTime.Now;
                letter.LastModifiedBy = currentUser.UserName;

                // ثبت اقدام
                await AddActionVoid(new ActionModel()
                {
                    LetterId = letterId,
                    Description = "نامه بایگانی سریع شد"
                });

                var action = new LetterAction
                {
                    LetterId = letterId,
                    UserId = currentUser.Id,
                    ActionDescription = "بایگانی سریع نامه"
                };
                _context.LetterActions.Add(action);

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Success = true,
                    Message = "نامه با موفقیت بایگانی شد",
                    Data = new { LetterId = letterId, NewStatus = letter.Status.ToString() }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بایگانی سریع نامه {LetterId}", letterId);
                return StatusCode(500, new { Success = false, Message = "خطا در پردازش درخواست بایگانی سریع" });
            }
        }

        /// <summary>
        /// دریافت شناسه سازمان کاربر
        /// </summary>
        private async Task<int> GetUserOrganizationId(string userId)
        {
            var userOrg = await _context.UserOrganizations
                .Where(uo => uo.UserId == userId && uo.IsActive)
                .OrderByDescending(uo => uo.IsHead)
                .FirstOrDefaultAsync();

            return userOrg?.OrganizationId ?? 0;
        }

        /// <summary>
        /// ثبت اقدامات به صورت غیرهمزمان
        /// </summary>
        private async Task AddActionVoid(ActionModel model)
        {
            try
            {
                var action = new LetterAction
                {
                    LetterId = model.LetterId,
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!,
                    ActionDescription = model.Description,
                    ActionDate = DateTime.Now
                };

                _context.LetterActions.Add(action);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ثبت اقدام برای نامه {LetterId}", model.LetterId);
            }
        }

        // تابع کمکی برای نمایش زمان به صورت گذشته‌شده
        private static string GetTimeAgo(DateTime date)
        {
            var timeSpan = DateTime.Now - date;

            if (timeSpan.TotalDays > 30)
                return date.ToLocalTime().ToString("F");

            if (timeSpan.TotalDays > 1)
                return $"{(int)timeSpan.TotalDays} روز پیش";

            if (timeSpan.TotalHours > 1)
                return $"{(int)timeSpan.TotalHours} ساعت پیش";

            if (timeSpan.TotalMinutes > 1)
                return $"{(int)timeSpan.TotalMinutes} دقیقه پیش";

            return "همین الان";
        }
    }

    public class MultiReferralRequestModel
    {
        public int LetterId { get; set; }
        public List<string> ReceiverUserIds { get; set; } = new List<string>();
        public string? CommonComment { get; set; }
        public Dictionary<string, string> PersonalComments { get; set; } = new Dictionary<string, string>();
    }

    public class ReferralRequestModel
    {
        public int LetterId { get; set; }
        public string? ReceiverUserId { get; set; }
        public string? Comment { get; set; }
    }

    public class ApprovalCommentModel
    {
        public int approvalId { get; set; }
        public string? Comment { get; set; }
    }

    public class ActionModel
    {
        public int LetterId { get; set; }
        public string? Description { get; set; }
    }

    public class ReturnLetterModel
    {
        public int LetterId { get; set; }
        public string? DestinationUserId { get; set; }
        public int? DestinationOrganizationId { get; set; }
        public string? Reason { get; set; }
        public string? Description { get; set; }
    }

    // لیست سازمان‌ها برای UI
    [ApiController]
    [Route("ApprovalFlow/Organizations")] // separate route for clarity
    public class OrganizationsController : ControllerBase
    {
        private readonly Db _context;
        public OrganizationsController(Db context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrganizations()
        {
            var orgs = await _context.Organizations
                .Where(o => o.IsActive)
                .Select(o => new { o.Id, Name = o.FullName ?? o.Name, o.OrgType, o.ParentId })
                .ToListAsync();
            return new JsonResult(new { Success = true, Data = orgs });
        }
    }

    public class ArchiveLetterModel
    {
        public int LetterId { get; set; }
        public int ClassificationCode { get; set; }
        public string? Description { get; set; }
    }
}