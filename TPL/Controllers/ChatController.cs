using BE;
using BE.Chat.DTOs;
using BLL.Chat;
using BLL.Ticketing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TPLWeb.Models.Chat;

namespace TPLWeb.Controllers
{
    [Authorize]
    [Route("Chat")]
    public class ChatController : Controller
    {
        private readonly IChatService _chatService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly BlNotification _blNotification;

        public ChatController(IChatService chatService, UserManager<ApplicationUser> userManager, BlNotification blNotification)
        {
            _chatService = chatService;
            _userManager = userManager;
            _blNotification = blNotification;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(string searchTerm = "")
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var chatRooms = await _chatService.GetUserChatRoomsAsync(currentUser.Id);
            if (!chatRooms.Success)
            {
                TempData["Error"] = chatRooms.Message;
                return View(new List<ChatRoomViewModel>());
            }

            var viewModels = chatRooms.Data!.Select(dto => new ChatRoomViewModel
            {
                Id = dto.Id,
                Name = dto.IsGroup ? dto.Name : (dto.Participants.FirstOrDefault(p => p.UserId != currentUser.Id)?.FullName ?? dto.Participants.FirstOrDefault(p => p.UserId != currentUser.Id)?.UserName ?? "کاربر ناشناس"),
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

            // Apply search filter if provided
            if (!string.IsNullOrEmpty(searchTerm))
            {
                viewModels = viewModels.Where(cr => 
                    cr.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    cr.Description?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true ||
                    cr.Participants.Any(p => p.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            ViewBag.CurrentUserName = currentUser.UserName!;
            ViewBag.CurrentUserAvatar = currentUser.Avatar!;
            ViewBag.CurrentUserId = currentUser.Id;
            ViewBag.CurrentUserRole = "کاربر سیستم";
            ViewBag.CurrentUserAbout = "کاربر سیستم چت";

            return View(viewModels);
        }

        // Room action removed - redirect to Index instead

        [HttpGet("Messages/{id}")]
        public async Task<IActionResult> Messages(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Json(new { success = false, message = "کاربر احراز هویت نشده" });
            }

            var messages = await _chatService.GetChatMessagesAsync(id, currentUser.Id);
            if (!messages.Success)
            {
                return BadRequest(messages.Message);
            }

            var viewModels = messages.Data!.Select(m => new ChatMessageViewModel
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
                IsPinned = m.IsPinned,
                IsEdited = m.IsEdited,
                ForwardedFrom = m.ForwardedFrom
            }).ToList();

            return Json(new { success = true, data = viewModels });
        }

        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage([FromForm] SendMessageViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var sendDto = new SendMessageDto
            {
                Content = model.Content,
                ChatRoomId = model.ChatRoomId,
                MessageType = (BE.Chat.MessageType)(int)model.MessageType,
                ReplyToMessageId = model.ReplyToMessageId
            };

            var result = await _chatService.SendMessageAsync(sendDto, currentUser.Id);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Json(new { success = true, message = "پیام با موفقیت ارسال شد" });
        }

        [HttpPost("CreateChatRoom")]
        public async Task<IActionResult> CreateChatRoom([FromForm] CreateChatRoomViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest("لطفاً تمام فیلدهای ضروری را پر کنید");
            }

            var createModel = new CreateChatRoomDto
            {
                Name = model.Name,
                Description = model.Description,
                IsGroup = model.IsGroup,
                ParticipantUserIds = model.ParticipantUserIds
            };

            var result = await _chatService.CreateChatRoomAsync(createModel, currentUser.Id);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Json(new { 
                success = true, 
                chatRoomId = result.Data!.Id,
                message = "گپ جدید با موفقیت ایجاد شد"
            });
        }

        [HttpGet("SearchUsers")]
        public async Task<IActionResult> SearchUsers(string term)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var result = await _chatService.SearchUsersAsync(term, currentUser.Id);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Json(result.Data);
        }

        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var users = await _userManager.Users
                .Where(u => u.Id != currentUser.Id && u.EmailConfirmed)
                .Select(u => new UserSearchViewModel
                {
                    UserId = u.Id,
                    UserName = u.UserName ?? u.Email!,
                    FirstName = u.FirstName ?? "",
                    LastName = u.LastName ?? "",
                    FullName = !string.IsNullOrEmpty(u.FirstName) && !string.IsNullOrEmpty(u.LastName) 
                        ? $"{u.FirstName} {u.LastName}".Trim() 
                        : (u.UserName ?? u.Email!),
                    Avatar = !string.IsNullOrEmpty(u.Avatar) ? u.Avatar : "1.png",
                    IsOnline = false,
                    IsInContacts = false,
                    Email = u.Email!
                })
                .ToListAsync();


            // Return with explicit property names to ensure proper JSON serialization
            var result = users.Select(u => new
            {
                userId = u.UserId,
                userName = u.UserName,
                firstName = u.FirstName,
                lastName = u.LastName,
                fullName = u.FullName,
                avatar = u.Avatar,
                isOnline = u.IsOnline,
                isInContacts = u.IsInContacts,
                email = u.Email
            }).ToList();

            var jsonString = JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });
            
            return Content(jsonString, "application/json");
        }

        [HttpGet("TestUsers")]
        public async Task<IActionResult> TestUsers()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            // Test with a simple user object
            var testUser = new
            {
                userId = "test-id-123",
                userName = "testuser",
                fullName = "Test User",
                email = "test@example.com"
            };

            return Json(new { success = true, user = testUser, message = "Test user created successfully" });
        }

        [HttpPost("UploadFile")]
        public async Task<IActionResult> UploadFile(IFormFile file, int chatRoomId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest("فایل انتخاب نشده است");
            }

            // Check file size (1GB limit)
            if (file.Length > 1073741824)
            {
                return BadRequest("حجم فایل نمی‌تواند بیشتر از 1 گیگابایت باشد");
            }

            var result = await _chatService.UploadChatFileAsync(file, currentUser.Id, chatRoomId);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Json(new { success = true, filePath = result.Data });
        }

        [HttpPost("UploadVoice")]
        public async Task<IActionResult> UploadVoice(IFormFile voiceFile, int chatRoomId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            if (voiceFile == null || voiceFile.Length == 0)
            {
                return BadRequest("فایل صوتی انتخاب نشده است");
            }

            var result = await _chatService.UploadVoiceMessageAsync(voiceFile, currentUser.Id, chatRoomId);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Json(new { success = true, voicePath = result.Data });
        }

        [HttpPost("SendFileMessage")]
        public async Task<IActionResult> SendFileMessage(IFormFile file, int chatRoomId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest("فایل انتخاب نشده است");
            }

            // Check file size (1GB limit)
            if (file.Length > 1073741824)
            {
                return BadRequest("حجم فایل نمی‌تواند بیشتر از 1 گیگابایت باشد");
            }

            var result = await _chatService.SendFileMessageAsync(file, currentUser.Id, chatRoomId);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Json(new { success = true, message = "فایل با موفقیت ارسال شد" });
        }

        [HttpPost("SendVoiceMessage")]
        public async Task<IActionResult> SendVoiceMessage(IFormFile audioFile, int chatRoomId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            if (audioFile == null || audioFile.Length == 0)
            {
                return BadRequest("فایل صوتی انتخاب نشده است");
            }

            var result = await _chatService.SendVoiceMessageAsync(audioFile, currentUser.Id, chatRoomId);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Json(new { success = true, message = "پیام صوتی با موفقیت ارسال شد" });
        }

        [HttpDelete("DeleteChatRoom/{chatRoomId}")]
        public async Task<IActionResult> DeleteChatRoom( int chatRoomId)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Unauthorized();
                }

                var result = await _chatService.DeleteChatRoomAsync(chatRoomId, currentUser.Id);
                if (!result.Success)
                {
                    return BadRequest(result.Message);
                }

                return Json(new { success = true, message = "چت با موفقیت حذف شد" });
            }
            catch (Exception ex)
            {
                return BadRequest("خطا در حذف چت: " + ex.Message);
            }
        }

        [HttpPost("EditMessage")]
        public async Task<IActionResult> EditMessage(int messageId, string newContent)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var result = await _chatService.EditMessageAsync(messageId, newContent, currentUser.Id);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Json(new { success = true, data = result.Data });
        }

        [HttpPost("ForwardMessage")]
        public async Task<IActionResult> ForwardMessage(int messageId, int targetChatRoomId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var result = await _chatService.ForwardMessageAsync(messageId, targetChatRoomId, currentUser.Id);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Json(new { success = true, data = result.Data });
        }

        [HttpPost("PinMessage")]
        public async Task<IActionResult> PinMessage(int messageId, int chatRoomId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var result = await _chatService.PinMessageAsync(messageId, chatRoomId);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Json(new { success = true });
        }

        [HttpGet("SearchMessages")]
        public async Task<IActionResult> SearchMessages(int chatRoomId, string query)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var result = await _chatService.SearchMessagesAsync(chatRoomId, query, currentUser.Id);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Json(result.Data);
        }

        [HttpGet("GetPinnedMessage")]
        public async Task<IActionResult> GetPinnedMessage(int chatRoomId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var result = await _chatService.GetPinnedMessageAsync(chatRoomId, currentUser.Id);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Json(result.Data);
        }

        [HttpPost("MarkAsRead")]
        public async Task<IActionResult> MarkAsRead(int chatRoomId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var result = await _chatService.MarkChatAsReadAsync(chatRoomId, currentUser.Id);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Json(new { success = true });
        }

        [HttpGet("GetChatParticipants")]
        public async Task<IActionResult> GetChatParticipants(int chatRoomId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var result = await _chatService.GetChatParticipantsAsync(chatRoomId, currentUser.Id);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Json(result.Data);
        }

        [HttpPost("RemoveParticipant")]
        public async Task<IActionResult> RemoveParticipant(int chatRoomId, string participantId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var result = await _chatService.RemoveParticipantAsync(chatRoomId, participantId, currentUser.Id);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Json(new { success = true });
        }

        [HttpPost("CopyMessage")]
        public async Task<IActionResult> CopyMessage(int messageId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var result = await _chatService.GetMessageContentAsync(messageId, currentUser.Id);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Json(new { success = true, content = result.Data });
        }

        [HttpPost("DeleteMessage")]
        public async Task<IActionResult> DeleteMessage([FromBody] int messageId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var result = await _chatService.DeleteMessageAsync(messageId, currentUser.Id);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Json(new { success = true });
        }

        private string GetAvatarPath(string avatar)
        {
            if (string.IsNullOrEmpty(avatar))
            {
                return Url.Content($"~/CompanyVariables/avatars/1.png");
            }

            if (avatar.StartsWith("/") || avatar.StartsWith("http"))
            {
                return avatar;
            }

            return Url.Content($"~/CompanyVariables/avatars/{avatar}");
        }
    }
}