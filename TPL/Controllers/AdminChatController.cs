// AdminChatController.cs (اصلاح شده برای افزودن ویژگی‌های شبیه تلگرام مانند ریپلای، فوروارد، ادیت، پین، و غیره)
using BLL.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using TPLWeb.Models.Chat;

namespace TPLWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Chat")]
    public class AdminChatController : Controller
    {
        private readonly IChatService _chatService;

        public AdminChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var result = await _chatService.GetAllChatRoomsAsync();
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View(new List<ChatRoomViewModel>());
            }

            ViewBag.CurrentUserId = User.Identity?.Name ?? "Unknown";

            var viewModels = result.Data?.Select(dto => new ChatRoomViewModel
            {
                Id = dto.Id,
                Name = dto.IsGroup ? dto.Name : dto.Participants.FirstOrDefault(p => p.UserId != User.Identity?.Name)?.FullName ?? "کاربر ناشناس",
                Description = dto.Description,
                LastActivityDate = dto.LastActivityDate,
                IsGroup = dto.IsGroup,
                UnreadCount = dto.UnreadCount,
                LastMessage = dto.LastMessage,
                LastMessageDate = dto.LastMessageDate,
                LastMessageSenderName = dto.LastMessageSenderName,
                Participants = dto.Participants.Select(p => new ChatParticipantViewModel
                {
                    UserId = p.UserId,
                    UserName = p.UserName,
                    FullName = p.FullName,
                    Avatar = p.Avatar,
                    IsOnline = p.IsOnline,
                    LastSeenDate = p.LastSeenDate,
                    IsAdmin = p.IsAdmin,
                    IsMuted = p.IsMuted
                }).ToList()
            }).ToList();

            return View(viewModels);
        }

        [HttpGet("Room/{id}")]
        public async Task<IActionResult> ChatRoom(int id)
        {
            var result = await _chatService.GetAdminChatViewAsync(id);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("Index");
            }

            var viewModel = new ChatViewModel
            {
                ChatRoomId = result.Data?.ChatRoomId ?? 0,
                ChatRoomName = result.Data?.ChatRoomName ?? "نامشخص",
                IsGroup = result.Data?.IsGroup ?? false,
                Messages = result.Data?.Messages?.Select(m => new ChatMessageViewModel
                {
                    Id = m.Id,
                    Content = m.Content,
                    SenderId = m.SenderId,
                    SenderName = m.SenderName,
                    SenderAvatar = m.SenderAvatar,
                    SentDate = m.SentDate,
                    IsOwnMessage = m.IsOwnMessage,
                    MessageType = (MessageType)m.MessageType,
                    FilePath = m.FilePath,
                    FileName = m.FileName,
                    FileSize = m.FileSize,
                    FileType = m.FileType,
                    VoicePath = m.VoicePath,
                    VoiceDuration = m.VoiceDuration,
                    IsRead = m.IsRead,
                    ReplyToMessageId = m.ReplyToMessageId,
                    ReplyToContent = m.ReplyToContent,
                    Status = (MessageStatus)m.Status,
                    IsPinned = m.IsPinned,  // افزودن پین پیام
                    IsEdited = m.IsEdited,  // افزودن ادیت پیام
                    ForwardedFrom = m.ForwardedFrom  // افزودن فوروارد
                })?.ToList() ?? new List<ChatMessageViewModel>(),
                Participants = result.Data?.Participants?.Select(p => new ChatParticipantViewModel
                {
                    UserId = p.UserId,
                    UserName = p.UserName,
                    FullName = p.FullName,
                    Avatar = p.Avatar,
                    IsOnline = p.IsOnline,
                    LastSeenDate = p.LastSeenDate,
                    IsAdmin = p.IsAdmin,
                    IsMuted = p.IsMuted
                })?.ToList() ?? new List<ChatParticipantViewModel>()
            };

            return View(viewModel);
        }

        [HttpGet("Monitoring")]
        public IActionResult Monitoring()
        {
            // Redirect to main admin panel or show a simple message
            return Content(@"
                <!DOCTYPE html>
                <html lang='fa' dir='rtl'>
                <head>
                    <meta charset='utf-8'>
                    <title>مانیتورینگ چت</title>
                    <style>
                        body { font-family: Arial, sans-serif; text-align: center; padding: 50px; direction: rtl; }
                        .container { max-width: 600px; margin: 0 auto; }
                        .btn { display: inline-block; padding: 12px 24px; background: #007bff; color: white; text-decoration: none; border-radius: 6px; margin: 10px; }
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h1>مانیتورینگ چت</h1>
                        <p>برای مشاهده چت‌ها، از پنل اصلی استفاده کنید.</p>
                        <a href='/Admin' class='btn'>بازگشت به پنل اصلی</a>
                    </div>
                </body>
                </html>", "text/html");
        }

        [HttpPost("SendFileMessage")]
        public async Task<IActionResult> SendFileMessage(IFormFile file, int chatRoomId)
        {
            var result = await _chatService.SendFileMessageAsync(file, User.Identity?.Name ?? "Unknown", chatRoomId);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Json(new { success = true, data = result.Data });
        }

        [HttpPost("SendVoiceMessage")]
        public async Task<IActionResult> SendVoiceMessage(IFormFile audioFile, int chatRoomId)
        {
            var result = await _chatService.SendVoiceMessageAsync(audioFile, User.Identity?.Name ?? "Unknown", chatRoomId);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Json(new { success = true, data = result.Data });
        }

        // ویژگی جدید: ادیت پیام
        [HttpPost("EditMessage")]
        public async Task<IActionResult> EditMessage(int messageId, string newContent)
        {
            var result = await _chatService.EditMessageAsync(messageId, newContent, User.Identity?.Name ?? "Unknown");
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Json(new { success = true, data = result.Data });
        }

        // ویژگی جدید: فوروارد پیام
        [HttpPost("ForwardMessage")]
        public async Task<IActionResult> ForwardMessage(int messageId, int targetChatRoomId)
        {
            var result = await _chatService.ForwardMessageAsync(messageId, targetChatRoomId, User.Identity?.Name ?? "Unknown");
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Json(new { success = true, data = result.Data });
        }

        // ویژگی جدید: پین پیام
        [HttpPost("PinMessage")]
        public async Task<IActionResult> PinMessage(int messageId, int chatRoomId)
        {
            var result = await _chatService.PinMessageAsync(messageId, chatRoomId);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Json(new { success = true });
        }
    }
}