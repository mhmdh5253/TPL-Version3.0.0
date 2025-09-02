// ChatService.cs (اصلاح شده برای افزودن ویژگی‌های جدید)
using BE;
using BE.Chat;
using BE.Chat.DTOs;
using DAL;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BLL.Chat
{
    public class ChatService : IChatService
    {
        private readonly Db _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string _chatFilesPath;

        public ChatService(Db context, UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _chatFilesPath = Path.Combine(_webHostEnvironment.WebRootPath, "CompanyVariables", "Chat");

            // Ensure chat directory exists
            if (!Directory.Exists(_chatFilesPath))
            {
                Directory.CreateDirectory(_chatFilesPath);
            }
        }

        public async Task<ServiceResult<ChatRoomDto>> CreateChatRoomAsync(CreateChatRoomDto model, string creatorId)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    return ServiceResult<ChatRoomDto>.FailureResult("نام اتاق چت نمی‌تواند خالی باشد");
                }

                // Check if participants exist
                var participants = await _userManager.Users
                    .Where(u => model.ParticipantUserIds.Contains(u.Id))
                    .ToListAsync();

                if (!participants.Any())
                {
                    return ServiceResult<ChatRoomDto>.FailureResult("هیچ کاربری یافت نشد");
                }

                // Check if an active chat room already exists between these users
                var existingChatRoom = await CheckExistingChatRoomAsync(model.ParticipantUserIds, creatorId);
                if (existingChatRoom != null)
                {
                    // Return the existing chat room instead of creating a new one
                    return ServiceResult<ChatRoomDto>.SuccessResult(existingChatRoom, "چت‌روم موجود یافت شد");
                }

                // Create new chat room only if none exists
                var chatRoom = new ChatRoom
                {
                    Name = model.Name,
                    Description = model.Description,
                    IsGroup = model.IsGroup,
                    CreatedById = creatorId,
                    CreatedDate = DateTime.Now,
                    LastActivityDate = DateTime.Now
                };

                _context.ChatRooms.Add(chatRoom);
                await _context.SaveChangesAsync();

                // Add participants
                var chatParticipants = new List<ChatParticipant>();

                // Add creator
                chatParticipants.Add(new ChatParticipant
                {
                    ChatRoomId = chatRoom.Id,
                    UserId = creatorId,
                    IsAdmin = true,
                    JoinedDate = DateTime.Now,
                    LastSeenDate = DateTime.Now
                });

                // Add other participants
                foreach (var participant in participants)
                {
                    chatParticipants.Add(new ChatParticipant
                    {
                        ChatRoomId = chatRoom.Id,
                        UserId = participant.Id,
                        JoinedDate = DateTime.Now,
                        LastSeenDate = DateTime.Now
                    });
                }

                _context.ChatParticipants.AddRange(chatParticipants);
                await _context.SaveChangesAsync();

                // Return the created chat room directly
                var dto = new ChatRoomDto
                {
                    Id = chatRoom.Id,
                    Name = model.Name,
                    Description = model.Description,
                    IsGroup = model.IsGroup,
                    LastActivityDate = chatRoom.LastActivityDate,
                    Participants = chatParticipants.Select(p => new ChatParticipantDto
                    {
                        UserId = p.UserId,
                        UserName = "", // Will be filled later
                        FullName = "", // Will be filled later
                        Avatar = "", // Will be filled later
                        IsOnline = false,
                        LastSeenDate = p.LastSeenDate,
                        IsAdmin = p.IsAdmin,
                        IsMuted = p.IsMuted
                    }).ToList()
                };

                return ServiceResult<ChatRoomDto>.SuccessResult(dto, "چت‌روم جدید ایجاد شد");
            }
            catch (Exception ex)
            {
                // Log the full exception for debugging
                Console.WriteLine($"CreateChatRoomAsync Error: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                
                // Return a user-friendly error message
                if (ex.InnerException != null)
                {
                    return ServiceResult<ChatRoomDto>.FailureResult($"خطا در ایجاد اتاق چت: {ex.InnerException.Message}");
                }
                
                return ServiceResult<ChatRoomDto>.FailureResult($"خطا در ایجاد اتاق چت: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if an active chat room already exists between the specified users
        /// </summary>
        private async Task<ChatRoomDto> CheckExistingChatRoomAsync(List<string> participantUserIds, string creatorId)
        {
            try
            {
                // Include creator in the participant list for checking
                var allUserIds = new List<string>(participantUserIds) { creatorId };
                
                // Find chat rooms where all specified users are participants
                var existingChatRooms = await _context.ChatParticipants
                    .Where(cp => allUserIds.Contains(cp.UserId))
                    .GroupBy(cp => cp.ChatRoomId)
                    .Where(g => g.Count() == allUserIds.Count) // All users must be participants
                    .Select(g => g.Key)
                    .ToListAsync();

                if (!existingChatRooms.Any())
                    return null;

                // Get the most recent active chat room
                var mostRecentChatRoomId = await _context.ChatRooms
                    .Where(cr => existingChatRooms.Contains(cr.Id))
                    .OrderByDescending(cr => cr.LastActivityDate)
                    .Select(cr => cr.Id)
                    .FirstOrDefaultAsync();

                if (mostRecentChatRoomId == 0)
                    return null;

                // Get the chat room details with participants
                var chatRoom = await _context.ChatRooms
                    .Where(cr => cr.Id == mostRecentChatRoomId)
                    .FirstOrDefaultAsync();

                if (chatRoom == null)
                    return null;

                // Get participants with user information
                var chatParticipants = await _context.ChatParticipants
                    .Where(cp => cp.ChatRoomId == chatRoom.Id)
                    .Include(cp => cp.User)
                    .ToListAsync();

                // Create DTO
                var dto = new ChatRoomDto
                {
                    Id = chatRoom.Id,
                    Name = chatRoom.Name,
                    Description = chatRoom.Description,
                    IsGroup = chatRoom.IsGroup,
                    LastActivityDate = chatRoom.LastActivityDate,
                    Participants = chatParticipants.Select(p => new ChatParticipantDto
                    {
                        UserId = p.UserId,
                        UserName = p.User?.UserName ?? "",
                        FullName = $"{p.User?.FirstName} {p.User?.LastName}".Trim(),
                        Avatar = p.User?.Avatar ?? "",
                        IsOnline = false, // Will be updated via SignalR
                        LastSeenDate = p.LastSeenDate,
                        IsAdmin = p.IsAdmin,
                        IsMuted = p.IsMuted
                    }).ToList()
                };

                return dto;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CheckExistingChatRoomAsync Error: {ex.Message}");
                return null; // Return null on error to allow creation of new chat room
            }
        }

        public async Task<ServiceResult<ChatRoomDto>> GetChatRoomAsync(int chatRoomId, string userId)
        {
            try
            {
                var chatRoom = await _context.ChatRooms
                    .Include(cr => cr.Participants)
                    .ThenInclude(p => p.User)
                    .FirstOrDefaultAsync(cr => cr.Id == chatRoomId);

                if (chatRoom == null)
                {
                    return ServiceResult<ChatRoomDto>.FailureResult("اتاق چت یافت نشد");
                }

                // Check if user is participant
                if (!chatRoom.Participants.Any(p => p.UserId == userId))
                {
                    return ServiceResult<ChatRoomDto>.FailureResult("شما دسترسی به این اتاق چت ندارید");
                }

                var dto = new ChatRoomDto
                {
                    Id = chatRoom.Id,
                    Name = chatRoom.Name,
                    Description = chatRoom.Description,
                    IsGroup = chatRoom.IsGroup,
                    LastActivityDate = chatRoom.LastActivityDate,
                    Participants = chatRoom.Participants.Select(p => new ChatParticipantDto
                    {
                        UserId = p.UserId,
                        UserName = p.User.UserName ?? "",
                        FullName = $"{p.User.FirstName} {p.User.LastName}",
                        Avatar = p.User.Avatar ?? "",
                        IsOnline = false, // Will be updated via SignalR
                        LastSeenDate = p.LastSeenDate,
                        IsAdmin = p.IsAdmin,
                        IsMuted = p.IsMuted
                    }).ToList()
                };

                return ServiceResult<ChatRoomDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                return ServiceResult<ChatRoomDto>.FailureResult($"خطا در دریافت اتاق چت: {ex.Message}");
            }
        }

        public async Task<ServiceResult<List<ChatRoomDto>>> GetUserChatRoomsAsync(string userId)
        {
            try
            {
                var chatRooms = await _context.ChatParticipants
                    .Where(cp => cp.UserId == userId)
                    .Include(cp => cp.ChatRoom)
                    .ThenInclude(cr => cr.Participants)
                    .ThenInclude(p => p.User)
                    .Select(cp => cp.ChatRoom)
                    .ToListAsync();

                // Ensure all participants and users are loaded
                foreach (var cr in chatRooms)
                {
                    await _context.Entry(cr)
                        .Collection(c => c.Participants)
                        .Query()
                        .Include(p => p.User)
                        .LoadAsync();
                }

                var dtos = new List<ChatRoomDto>();

                foreach (var cr in chatRooms)
                {
                    var lastMessage = await _context.ChatMessages
                        .Where(m => m.ChatRoomId == cr.Id)
                        .OrderByDescending(m => m.SentDate)
                        .FirstOrDefaultAsync();

                    // Get the participant info for the current user in this chat room
                    var userParticipant = cr.Participants.FirstOrDefault(p => p.UserId == userId);

                    // For private chats, get the other participant's name
                    string chatName = cr.Name ?? "";
                    if (!cr.IsGroup)
                    {
                        var otherParticipant = cr.Participants.FirstOrDefault(p => p.UserId != userId);
                        if (otherParticipant != null && otherParticipant.User != null)
                        {
                            chatName = $"{otherParticipant.User.FirstName} {otherParticipant.User.LastName}".Trim();
                            if (string.IsNullOrEmpty(chatName))
                            {
                                chatName = otherParticipant.User.UserName ?? "کاربر ناشناس";
                            }
                        }
                    }

                    var dto = new ChatRoomDto
                    {
                        Id = cr.Id,
                        Name = chatName,
                        Description = cr.Description,
                        IsGroup = cr.IsGroup,
                        LastActivityDate = cr.LastActivityDate,
                        UnreadCount = await _context.ChatMessages
                            .Where(m => m.ChatRoomId == cr.Id && m.SentDate > (userParticipant != null ? userParticipant.LastSeenDate : DateTime.MinValue) && m.SenderId != userId)
                            .CountAsync(),
                        LastMessage = lastMessage?.Content ?? "",
                        LastMessageDate = lastMessage?.SentDate ?? cr.CreatedDate,
                        LastMessageSenderName = lastMessage != null ? (await _userManager.FindByIdAsync(lastMessage.SenderId))?.UserName ?? "" : "",
                        Participants = cr.Participants.Select(p => new ChatParticipantDto
                        {
                            UserId = p.UserId,
                            UserName = p.User.UserName ?? "",
                            FullName = $"{p.User.FirstName} {p.User.LastName}".Trim(),
                            Avatar = p.User.Avatar ?? "",
                            IsOnline = false,
                            LastSeenDate = p.LastSeenDate,
                            IsAdmin = p.IsAdmin,
                            IsMuted = p.IsMuted
                        }).ToList()
                    };

                    dtos.Add(dto);
                }

                return ServiceResult<List<ChatRoomDto>>.SuccessResult(dtos);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<ChatRoomDto>>.FailureResult($"خطا در دریافت اتاق‌های چت: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> DeleteChatRoomAsync(int chatRoomId, string userId)
        {
            try
            {
                var chatRoom = await _context.ChatRooms
                    .Include(cr => cr.Participants)
                    .FirstOrDefaultAsync(cr => cr.Id == chatRoomId);

                if (chatRoom == null)
                {
                    return ServiceResult<bool>.FailureResult("اتاق چت یافت نشد");
                }

                // Check if user is admin or creator
                var participant = chatRoom.Participants.FirstOrDefault(p => p.UserId == userId);
                if (participant == null || (!participant.IsAdmin && chatRoom.CreatedById != userId))
                {
                    return ServiceResult<bool>.FailureResult("شما مجوز حذف این اتاق چت را ندارید");
                }

                _context.ChatRooms.Remove(chatRoom);
                await _context.SaveChangesAsync();

                return ServiceResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.FailureResult($"خطا در حذف اتاق چت: {ex.Message}");
            }
        }

        public async Task<ServiceResult<ChatMessageDto>> SendMessageAsync(SendMessageDto model, string senderId)
        {
            try
            {
                // Check if user is participant
                var isParticipant = await _context.ChatParticipants
                    .AnyAsync(cp => cp.ChatRoomId == model.ChatRoomId && cp.UserId == senderId);

                if (!isParticipant)
                {
                    return ServiceResult<ChatMessageDto>.FailureResult("شما دسترسی به این اتاق چت ندارید");
                }

                var message = new ChatMessage
                {
                    ChatRoomId = model.ChatRoomId,
                    SenderId = senderId,
                    Content = model.Content,
                    MessageType = model.MessageType,
                    SentDate = DateTime.Now,
                    Status = MessageStatus.Sent,
                    ReplyToMessageId = model.ReplyToMessageId
                };

                _context.ChatMessages.Add(message);

                // Update chat room last activity
                var chatRoom = await _context.ChatRooms.FindAsync(model.ChatRoomId);
                if (chatRoom != null)
                {
                    chatRoom.LastActivityDate = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                // Get sender info
                var sender = await _userManager.FindByIdAsync(senderId);

                // Get reply content if exists
                string? replyContent = null;
                if (model.ReplyToMessageId.HasValue)
                {
                    var replyMessage = await _context.ChatMessages.FindAsync(model.ReplyToMessageId.Value);
                    if (replyMessage != null)
                    {
                        replyContent = replyMessage.Content;
                    }
                }

                var result = new ChatMessageDto
                {
                    Id = message.Id,
                    Content = message.Content,
                    SenderId = message.SenderId,
                    SenderName = sender?.UserName ?? "",
                    SenderAvatar = sender?.Avatar ?? "",
                    SentDate = message.SentDate,
                    IsOwnMessage = true,
                    MessageType = message.MessageType,
                    IsRead = false,
                    ReplyToMessageId = message.ReplyToMessageId,
                    ReplyToContent = replyContent,
                    Status = message.Status,
                    IsPinned = message.IsPinned,
                    IsEdited = message.IsEdited,
                    ForwardedFrom = message.ForwardedFrom
                };

                return ServiceResult<ChatMessageDto>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<ChatMessageDto>.FailureResult($"خطا در ارسال پیام: {ex.Message}");
            }
        }

        public async Task<ServiceResult<List<ChatMessageDto>>> GetChatMessagesAsync(int chatRoomId, string userId, int page = 1, int pageSize = 50)
        {
            try
            {
                // Check access
                var isParticipant = await _context.ChatParticipants
                    .AnyAsync(cp => cp.ChatRoomId == chatRoomId && cp.UserId == userId);

                if (!isParticipant)
                {
                    return ServiceResult<List<ChatMessageDto>>.FailureResult("شما دسترسی به این اتاق چت ندارید");
                }

                var messages = await _context.ChatMessages
                    .Where(m => m.ChatRoomId == chatRoomId && !m.IsDeleted)
                    .OrderByDescending(m => m.SentDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var dtos = new List<ChatMessageDto>();

                foreach (var m in messages)
                {
                    var sender = await _userManager.FindByIdAsync(m.SenderId);

                    string? replyContent = null;
                    if (m.ReplyToMessageId.HasValue)
                    {
                        var reply = await _context.ChatMessages.FindAsync(m.ReplyToMessageId.Value);
                        if (reply != null)
                        {
                            replyContent = reply.Content;
                        }
                    }

                    dtos.Add(new ChatMessageDto
                    {
                        Id = m.Id,
                        Content = m.Content,
                        SenderId = m.SenderId,
                        SenderName = sender?.UserName ?? "",
                        SenderAvatar = sender?.Avatar ?? "",
                        SentDate = m.SentDate,
                        IsOwnMessage = m.SenderId == userId,
                        MessageType = m.MessageType,
                        FilePath = m.FilePath,
                        FileName = m.FileName,
                        FileSize = m.FileSize,
                        FileType = m.FileType,
                        VoicePath = m.VoicePath,
                        VoiceDuration = m.VoiceDuration,
                        IsRead = m.ReadDate.HasValue,
                        ReplyToMessageId = m.ReplyToMessageId,
                        ReplyToContent = replyContent,
                        Status = m.Status,
                        IsPinned = m.IsPinned,
                        IsEdited = m.IsEdited,
                        ForwardedFrom = m.ForwardedFrom
                    });
                }

                // Update last seen
                var participant = await _context.ChatParticipants
                    .FirstOrDefaultAsync(cp => cp.ChatRoomId == chatRoomId && cp.UserId == userId);
                if (participant != null)
                {
                    participant.LastSeenDate = DateTime.Now;
                    await _context.SaveChangesAsync();
                }

                return ServiceResult<List<ChatMessageDto>>.SuccessResult(dtos);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<ChatMessageDto>>.FailureResult($"خطا در دریافت پیام‌ها: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> MarkMessageAsReadAsync(int messageId, string userId)
        {
            try
            {
                var message = await _context.ChatMessages.FindAsync(messageId);
                if (message == null)
                {
                    return ServiceResult<bool>.FailureResult("پیام یافت نشد");
                }

                // Check if user is in chat room
                var isParticipant = await _context.ChatParticipants
                    .AnyAsync(cp => cp.ChatRoomId == message.ChatRoomId && cp.UserId == userId);

                if (!isParticipant)
                {
                    return ServiceResult<bool>.FailureResult("شما دسترسی ندارید");
                }

                message.ReadDate = DateTime.Now;
                message.Status = MessageStatus.Read;

                await _context.SaveChangesAsync();

                return ServiceResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.FailureResult($"خطا در علامت‌گذاری به عنوان خوانده شده: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> DeleteMessageAsync(int messageId, string userId)
        {
            try
            {
                var message = await _context.ChatMessages.FindAsync(messageId);
                if (message == null)
                {
                    return ServiceResult<bool>.FailureResult("پیام یافت نشد");
                }

                if (message.SenderId != userId)
                {
                    return ServiceResult<bool>.FailureResult("شما فقط می‌توانید پیام‌های خود را حذف کنید");
                }

                message.IsDeleted = true;
                message.DeletedDate = DateTime.Now;
                message.DeletedById = userId;

                await _context.SaveChangesAsync();

                return ServiceResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.FailureResult($"خطا در حذف پیام: {ex.Message}");
            }
        }

        public async Task<ServiceResult<List<UserSearchDto>>> SearchUsersAsync(string searchTerm, string currentUserId)
        {
            try
            {
                IQueryable<ApplicationUser> query = _userManager.Users.Where(u => u.Id != currentUserId);

                // If searchTerm is provided, filter by it
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(u => u.UserName!.Contains(searchTerm) ||
                                           u.FirstName!.Contains(searchTerm) ||
                                           u.LastName!.Contains(searchTerm));
                }

                var users = await query
                    .Take(50) // Increased limit to show more users
                    .ToListAsync();

                var dtos = new List<UserSearchDto>();

                foreach (var user in users)
                {
                    var isInContacts = await _context.UserContacts
                        .AnyAsync(uc => uc.UserId == currentUserId && uc.ContactUserId == user.Id);

                    dtos.Add(new UserSearchDto
                    {
                        UserId = user.Id,
                        UserName = user.UserName ?? "",
                        FirstName = user.FirstName ?? "",
                        LastName = user.LastName ?? "",
                        FullName = $"{user.FirstName} {user.LastName}",
                        Avatar = user.Avatar ?? "",
                        IsOnline = false, // Update via SignalR
                        IsInContacts = isInContacts
                    });
                }

                return ServiceResult<List<UserSearchDto>>.SuccessResult(dtos);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<UserSearchDto>>.FailureResult($"خطا در جستجوی کاربران: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> AddUserToContactsAsync(string userId, string contactUserId)
        {
            try
            {
                if (await _context.UserContacts.AnyAsync(uc => uc.UserId == userId && uc.ContactUserId == contactUserId))
                {
                    return ServiceResult<bool>.FailureResult("این کاربر قبلاً به مخاطبین اضافه شده است");
                }

                var contact = new UserContact
                {
                    UserId = userId,
                    ContactUserId = contactUserId,
                    AddedDate = DateTime.Now
                };

                _context.UserContacts.Add(contact);
                await _context.SaveChangesAsync();

                return ServiceResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.FailureResult($"خطا در افزودن به مخاطبین: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> RemoveUserFromContactsAsync(string userId, string contactUserId)
        {
            try
            {
                var contact = await _context.UserContacts
                    .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ContactUserId == contactUserId);

                if (contact == null)
                {
                    return ServiceResult<bool>.FailureResult("این کاربر در مخاطبین یافت نشد");
                }

                _context.UserContacts.Remove(contact);
                await _context.SaveChangesAsync();

                return ServiceResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.FailureResult($"خطا در حذف از مخاطبین: {ex.Message}");
            }
        }

        public async Task<ServiceResult<List<UserSearchDto>>> GetUserContactsAsync(string userId)
        {
            try
            {
                var contacts = await _context.UserContacts
                    .Where(uc => uc.UserId == userId)
                    .Include(uc => uc.ContactUser)
                    .ToListAsync();

                var dtos = contacts.Select(uc => new UserSearchDto
                {
                    UserId = uc.ContactUserId,
                    UserName = uc.ContactUser.UserName ?? "",
                    FirstName = uc.ContactUser.FirstName ?? "",
                    LastName = uc.ContactUser.LastName ?? "",
                    FullName = $"{uc.ContactUser.FirstName} {uc.ContactUser.LastName}",
                    Avatar = uc.ContactUser.Avatar ?? "",
                    IsOnline = false,
                    IsInContacts = true
                }).ToList();

                return ServiceResult<List<UserSearchDto>>.SuccessResult(dtos);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<UserSearchDto>>.FailureResult($"خطا در دریافت مخاطبین: {ex.Message}");
            }
        }

        public async Task<ServiceResult<string>> UploadChatFileAsync(IFormFile file, string userId, int chatRoomId)
        {
            try
            {
                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var filePath = Path.Combine("CompanyVariables", "Chat", fileName);
                var fullPath = Path.Combine(_chatFilesPath, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return ServiceResult<string>.SuccessResult(filePath);
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.FailureResult($"خطا در آپلود فایل: {ex.Message}");
            }
        }

        public async Task<ServiceResult<string>> UploadVoiceMessageAsync(IFormFile voiceFile, string userId, int chatRoomId)
        {
            try
            {
                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}_voice.wav";
                var voicePath = Path.Combine("CompanyVariables", "voices", fileName);
                var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, voicePath);

                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await voiceFile.CopyToAsync(stream);
                }

                return ServiceResult<string>.SuccessResult(voicePath);
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.FailureResult($"خطا در آپلود صدا: {ex.Message}");
            }
        }

        public async Task<ServiceResult<ChatMessageDto>> SendFileMessageAsync(IFormFile file, string senderId, int chatRoomId)
        {
            try
            {
                // Check access
                var isParticipant = await _context.ChatParticipants
                    .AnyAsync(cp => cp.ChatRoomId == chatRoomId && cp.UserId == senderId);

                if (!isParticipant)
                {
                    return ServiceResult<ChatMessageDto>.FailureResult("شما دسترسی به این اتاق چت ندارید");
                }

                // Upload file
                var uploadResult = await UploadChatFileAsync(file, senderId, chatRoomId);
                if (!uploadResult.Success)
                {
                    return ServiceResult<ChatMessageDto>.FailureResult(uploadResult.Message);
                }

                var filePath = uploadResult.Data!;

                var message = new ChatMessage
                {
                    ChatRoomId = chatRoomId,
                    SenderId = senderId,
                    Content = "فایل ارسال شد",
                    MessageType = MessageType.File,
                    SentDate = DateTime.Now,
                    Status = MessageStatus.Sent,
                    FilePath = filePath,
                    FileName = file.FileName,
                    FileSize = file.Length,
                    FileType = file.ContentType
                };

                _context.ChatMessages.Add(message);

                // Update last activity
                var chatRoom = await _context.ChatRooms.FindAsync(chatRoomId);
                if (chatRoom != null)
                {
                    chatRoom.LastActivityDate = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                // Get sender info
                var sender = await _userManager.FindByIdAsync(senderId);

                var result = new ChatMessageDto
                {
                    Id = message.Id,
                    Content = message.Content,
                    SenderId = message.SenderId,
                    SenderName = sender?.UserName ?? "",
                    SenderAvatar = sender?.Avatar ?? "",
                    SentDate = message.SentDate,
                    IsOwnMessage = true,
                    MessageType = message.MessageType,
                    FilePath = message.FilePath,
                    FileName = message.FileName,
                    FileSize = message.FileSize,
                    FileType = message.FileType,
                    Status = message.Status,
                    IsPinned = message.IsPinned,
                    IsEdited = message.IsEdited,
                    ForwardedFrom = message.ForwardedFrom
                };

                return ServiceResult<ChatMessageDto>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<ChatMessageDto>.FailureResult($"خطا در ارسال فایل: {ex.Message}");
            }
        }

        public async Task<ServiceResult<ChatMessageDto>> SendVoiceMessageAsync(IFormFile audioFile, string senderId, int chatRoomId)
        {
            try
            {
                var isParticipant = await _context.ChatParticipants
                    .AnyAsync(cp => cp.ChatRoomId == chatRoomId && cp.UserId == senderId);

                if (!isParticipant)
                {
                    return ServiceResult<ChatMessageDto>.FailureResult("شما دسترسی به این اتاق چت ندارید");
                }

                // Upload voice
                var uploadResult = await UploadVoiceMessageAsync(audioFile, senderId, chatRoomId);
                if (!uploadResult.Success)
                {
                    return ServiceResult<ChatMessageDto>.FailureResult(uploadResult.Message);
                }

                var voicePath = uploadResult.Data!;

                var message = new ChatMessage
                {
                    ChatRoomId = chatRoomId,
                    SenderId = senderId,
                    Content = "پیام صوتی",
                    MessageType = MessageType.Voice,
                    SentDate = DateTime.Now,
                    Status = MessageStatus.Sent,
                    VoicePath = voicePath,
                    VoiceDuration = 0 // Calculate if needed
                };

                _context.ChatMessages.Add(message);

                var chatRoom = await _context.ChatRooms.FindAsync(chatRoomId);
                if (chatRoom != null)
                {
                    chatRoom.LastActivityDate = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                var sender = await _userManager.FindByIdAsync(senderId);

                var result = new ChatMessageDto
                {
                    Id = message.Id,
                    Content = message.Content,
                    SenderId = message.SenderId,
                    SenderName = sender?.UserName ?? "",
                    SenderAvatar = sender?.Avatar ?? "",
                    SentDate = message.SentDate,
                    IsOwnMessage = true,
                    MessageType = message.MessageType,
                    VoicePath = message.VoicePath,
                    VoiceDuration = message.VoiceDuration,
                    Status = message.Status,
                    IsPinned = message.IsPinned,
                    IsEdited = message.IsEdited,
                    ForwardedFrom = message.ForwardedFrom
                };

                return ServiceResult<ChatMessageDto>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<ChatMessageDto>.FailureResult($"خطا در ارسال پیام صوتی: {ex.Message}");
            }
        }

        public Task<ServiceResult<bool>> SendChatNotificationAsync(string recipientId, string senderName, string message)
        {
            // Implement notification logic, perhaps using SignalR or push notifications
            return Task.FromResult(ServiceResult<bool>.SuccessResult(true));
        }

        public async Task<ServiceResult<List<ChatRoomDto>>> GetAllChatRoomsAsync()
        {
            try
            {
                var chatRooms = await _context.ChatRooms
                    .Include(cr => cr.Participants)
                    .ThenInclude(p => p.User)
                    .Include(cr => cr.Messages)
                    .ToListAsync();

                var dtos = new List<ChatRoomDto>();

                foreach (var cr in chatRooms)
                {
                    // Get last message
                    var lastMessage = await _context.ChatMessages
                        .Where(m => m.ChatRoomId == cr.Id)
                        .OrderByDescending(m => m.SentDate)
                        .FirstOrDefaultAsync();

                    var dto = new ChatRoomDto
                    {
                        Id = cr.Id,
                        Name = cr.Name,
                        Description = cr.Description,
                        IsGroup = cr.IsGroup,
                        LastActivityDate = cr.LastActivityDate,
                        UnreadCount = await _context.ChatMessages
                            .CountAsync(m => m.ChatRoomId == cr.Id && !m.ReadDate.HasValue),
                        LastMessage = lastMessage?.Content ?? "",
                        LastMessageDate = lastMessage?.SentDate ?? cr.CreatedDate,
                        LastMessageSenderName = lastMessage != null ? (await _userManager.FindByIdAsync(lastMessage.SenderId))?.UserName ?? "" : "",
                        Participants = cr.Participants.Select(p => new ChatParticipantDto
                        {
                            UserId = p.UserId,
                            UserName = p.User.UserName ?? "",
                            FullName = $"{p.User.FirstName} {p.User.LastName}",
                            Avatar = p.User.Avatar ?? "",
                            IsOnline = false,
                            LastSeenDate = p.LastSeenDate,
                            IsAdmin = p.IsAdmin,
                            IsMuted = p.IsMuted
                        }).ToList(),
                        PinnedMessageIds = await _context.ChatMessages
                            .Where(m => m.ChatRoomId == cr.Id && m.IsPinned)
                            .Select(m => m.Id)
                            .ToListAsync()
                    };

                    dtos.Add(dto);
                }

                return ServiceResult<List<ChatRoomDto>>.SuccessResult(dtos);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<ChatRoomDto>>.FailureResult($"خطا در دریافت تمام اتاق‌های چت: {ex.Message}");
            }
        }

        public async Task<ServiceResult<ChatDto>> GetAdminChatViewAsync(int chatRoomId)
        {
            try
            {
                var chatRoom = await _context.ChatRooms
                    .Include(cr => cr.Participants)
                    .ThenInclude(p => p.User)
                    .Include(cr => cr.Messages)
                    .FirstOrDefaultAsync(cr => cr.Id == chatRoomId);

                if (chatRoom == null)
                {
                    return ServiceResult<ChatDto>.FailureResult("اتاق چت یافت نشد");
                }

                var dto = new ChatDto
                {
                    ChatRoomId = chatRoom.Id,
                    ChatRoomName = chatRoom.Name,
                    IsGroup = chatRoom.IsGroup,
                    Messages = chatRoom.Messages.Select(m => new ChatMessageDto
                    {
                        Id = m.Id,
                        Content = m.Content,
                        SenderId = m.SenderId,
                        SenderName = "", // Fill later
                        SenderAvatar = "", // Fill later
                        SentDate = m.SentDate,
                        IsOwnMessage = false, // For admin view
                        MessageType = m.MessageType,
                        FilePath = m.FilePath,
                        FileName = m.FileName,
                        FileSize = m.FileSize,
                        FileType = m.FileType,
                        VoicePath = m.VoicePath,
                        VoiceDuration = m.VoiceDuration,
                        IsRead = m.ReadDate.HasValue,
                        ReplyToMessageId = m.ReplyToMessageId,
                        ReplyToContent = "", // Fill if needed
                        Status = m.Status,
                        IsPinned = m.IsPinned,
                        IsEdited = m.IsEdited,
                        ForwardedFrom = m.ForwardedFrom
                    }).ToList(),
                    Participants = chatRoom.Participants.Select(p => new ChatParticipantDto
                    {
                        UserId = p.UserId,
                        UserName = p.User.UserName ?? "",
                        FullName = $"{p.User.FirstName} {p.User.LastName}",
                        Avatar = p.User.Avatar ?? "",
                        IsOnline = false,
                        LastSeenDate = p.LastSeenDate,
                        IsAdmin = p.IsAdmin,
                        IsMuted = p.IsMuted
                    }).ToList()
                };

                // Fill sender names and avatars
                foreach (var msg in dto.Messages)
                {
                    var sender = await _userManager.FindByIdAsync(msg.SenderId);
                    if (sender != null)
                    {
                        msg.SenderName = sender.UserName ?? "";
                        msg.SenderAvatar = sender.Avatar ?? "";
                    }

                    if (msg.ReplyToMessageId.HasValue)
                    {
                        var reply = await _context.ChatMessages.FindAsync(msg.ReplyToMessageId.Value);
                        if (reply != null)
                        {
                            msg.ReplyToContent = reply.Content;
                        }
                    }
                }

                return ServiceResult<ChatDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                return ServiceResult<ChatDto>.FailureResult($"خطا در دریافت ویو ادمین چت: {ex.Message}");
            }
        }

        public async Task<ServiceResult<ChatMessageDto>> EditMessageAsync(int messageId, string newContent, string userId)
        {
            try
            {
                var message = await _context.ChatMessages.FindAsync(messageId);
                if (message == null)
                {
                    return ServiceResult<ChatMessageDto>.FailureResult("پیام یافت نشد");
                }

                if (message.SenderId != userId)
                {
                    return ServiceResult<ChatMessageDto>.FailureResult("شما فقط می‌توانید پیام‌های خود را ویرایش کنید");
                }

                message.Content = newContent;
                message.IsEdited = true;

                await _context.SaveChangesAsync();

                var dto = new ChatMessageDto
                {
                    Id = message.Id,
                    Content = message.Content,
                    IsEdited = true
                };

                return ServiceResult<ChatMessageDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                return ServiceResult<ChatMessageDto>.FailureResult($"خطا در ویرایش پیام: {ex.Message}");
            }
        }

        public async Task<ServiceResult<ChatMessageDto>> ForwardMessageAsync(int messageId, int targetChatRoomId, string userId)
        {
            try
            {
                var originalMessage = await _context.ChatMessages.FindAsync(messageId);
                if (originalMessage == null)
                {
                    return ServiceResult<ChatMessageDto>.FailureResult("پیام یافت نشد");
                }

                // Check access to target chat room
                var isParticipant = await _context.ChatParticipants
                    .AnyAsync(cp => cp.ChatRoomId == targetChatRoomId && cp.UserId == userId);

                if (!isParticipant)
                {
                    return ServiceResult<ChatMessageDto>.FailureResult("شما دسترسی به اتاق چت مقصد ندارید");
                }

                var forwardedMessage = new ChatMessage
                {
                    ChatRoomId = targetChatRoomId,
                    SenderId = userId,
                    Content = originalMessage.Content,
                    MessageType = originalMessage.MessageType,
                    SentDate = DateTime.Now,
                    Status = MessageStatus.Sent,
                    ForwardedFrom = originalMessage.SenderId, // Or sender name
                    FilePath = originalMessage.FilePath,
                    FileName = originalMessage.FileName,
                    FileSize = originalMessage.FileSize,
                    FileType = originalMessage.FileType,
                    VoicePath = originalMessage.VoicePath,
                    VoiceDuration = originalMessage.VoiceDuration,
                    ReplyToMessageId = originalMessage.ReplyToMessageId
                };

                _context.ChatMessages.Add(forwardedMessage);

                var targetChatRoom = await _context.ChatRooms.FindAsync(targetChatRoomId);
                if (targetChatRoom != null)
                {
                    targetChatRoom.LastActivityDate = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                var sender = await _userManager.FindByIdAsync(userId);
                var originalSender = await _userManager.FindByIdAsync(originalMessage.SenderId);

                var dto = new ChatMessageDto
                {
                    Id = forwardedMessage.Id,
                    Content = forwardedMessage.Content,
                    SenderId = forwardedMessage.SenderId,
                    SenderName = sender?.UserName ?? "",
                    SenderAvatar = sender?.Avatar ?? "",
                    SentDate = forwardedMessage.SentDate,
                    IsOwnMessage = true,
                    MessageType = forwardedMessage.MessageType,
                    ForwardedFrom = originalSender?.UserName ?? "",
                    // Fill other fields
                };

                return ServiceResult<ChatMessageDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                return ServiceResult<ChatMessageDto>.FailureResult($"خطا در فوروارد پیام: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> PinMessageAsync(int messageId, int chatRoomId)
        {
            try
            {
                var message = await _context.ChatMessages.FindAsync(messageId);
                if (message == null || message.ChatRoomId != chatRoomId)
                {
                    return ServiceResult<bool>.FailureResult("پیام یافت نشد");
                }

                message.IsPinned = true;

                await _context.SaveChangesAsync();

                return ServiceResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.FailureResult($"خطا در پین پیام: {ex.Message}");
            }
        }

        public async Task<ServiceResult<ChatRoomDto>> GetExistingPrivateChatRoomAsync(string userId1, string userId2)
        {
            try
            {
                var existingChatRoom = await _context.ChatRooms
                    .Include(cr => cr.Participants)
                    .ThenInclude(p => p.User)
                    .Where(cr => !cr.IsGroup && cr.Participants.Count == 2)
                    .FirstOrDefaultAsync(cr => cr.Participants.Any(p => p.UserId == userId1) && 
                                             cr.Participants.Any(p => p.UserId == userId2));

                if (existingChatRoom == null)
                {
                    return ServiceResult<ChatRoomDto>.SuccessResult(null);
                }

                var dto = new ChatRoomDto
                {
                    Id = existingChatRoom.Id,
                    Name = existingChatRoom.Name,
                    Description = existingChatRoom.Description,
                    IsGroup = existingChatRoom.IsGroup,
                    LastActivityDate = existingChatRoom.LastActivityDate,
                    Participants = existingChatRoom.Participants.Select(p => new ChatParticipantDto
                    {
                        UserId = p.UserId,
                        UserName = p.User.UserName ?? "",
                        FullName = $"{p.User.FirstName} {p.User.LastName}".Trim(),
                        Avatar = p.User.Avatar ?? "",
                        IsOnline = false,
                        LastSeenDate = p.LastSeenDate,
                        IsAdmin = p.IsAdmin,
                        IsMuted = p.IsMuted
                    }).ToList()
                };

                return ServiceResult<ChatRoomDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                return ServiceResult<ChatRoomDto>.FailureResult($"خطا در بررسی چت موجود: {ex.Message}");
            }
        }

        public async Task<ServiceResult<ChatRoomDto>> GetExistingGroupChatRoomAsync(List<string> userIds)
        {
            try
            {
                // Find group chat rooms with exactly the same participants
                var existingGroupChats = await _context.ChatRooms
                    .Include(cr => cr.Participants)
                    .ThenInclude(p => p.User)
                    .Where(cr => cr.IsGroup && cr.Participants.Count == userIds.Count)
                    .ToListAsync();

                foreach (var chatRoom in existingGroupChats)
                {
                    var chatRoomUserIds = chatRoom.Participants.Select(p => p.UserId).ToList();
                    var hasAllUsers = userIds.All(id => chatRoomUserIds.Contains(id));
                    var hasSameCount = chatRoomUserIds.Count == userIds.Count;

                    if (hasAllUsers && hasSameCount)
                    {
                        var dto = new ChatRoomDto
                        {
                            Id = chatRoom.Id,
                            Name = chatRoom.Name,
                            Description = chatRoom.Description,
                            IsGroup = chatRoom.IsGroup,
                            LastActivityDate = chatRoom.LastActivityDate,
                            Participants = chatRoom.Participants.Select(p => new ChatParticipantDto
                            {
                                UserId = p.UserId,
                                UserName = p.User.UserName ?? "",
                                FullName = $"{p.User.FirstName} {p.User.LastName}".Trim(),
                                Avatar = p.User.Avatar ?? "",
                                IsOnline = false,
                                LastSeenDate = p.LastSeenDate,
                                IsAdmin = p.IsAdmin,
                                IsMuted = p.IsMuted
                            }).ToList()
                        };

                        return ServiceResult<ChatRoomDto>.SuccessResult(dto);
                    }
                }

                return ServiceResult<ChatRoomDto>.SuccessResult(null);
            }
            catch (Exception ex)
            {
                return ServiceResult<ChatRoomDto>.FailureResult($"خطا در بررسی گروه چت موجود: {ex.Message}");
            }
        }

        public async Task<ServiceResult<List<ChatMessageDto>>> SearchMessagesAsync(int chatRoomId, string query, string userId)
        {
            try
            {
                // Check access
                var isParticipant = await _context.ChatParticipants
                    .AnyAsync(cp => cp.ChatRoomId == chatRoomId && cp.UserId == userId);

                if (!isParticipant)
                {
                    return ServiceResult<List<ChatMessageDto>>.FailureResult("شما دسترسی ندارید");
                }

                var messages = await _context.ChatMessages
                    .Where(m => m.ChatRoomId == chatRoomId && m.Content.Contains(query))
                    .OrderByDescending(m => m.SentDate)
                    .Take(50)
                    .ToListAsync();

                var dtos = messages.Select(m => new ChatMessageDto
                {
                    Id = m.Id,
                    Content = m.Content,
                    SenderId = m.SenderId,
                    // Fill other fields as in GetChatMessagesAsync
                }).ToList();

                return ServiceResult<List<ChatMessageDto>>.SuccessResult(dtos);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<ChatMessageDto>>.FailureResult($"خطا در جستجوی پیام‌ها: {ex.Message}");
            }
        }

        // Additional Features Implementation
        public async Task<ServiceResult<ChatMessageDto>> GetPinnedMessageAsync(int chatRoomId, string userId)
        {
            try
            {
                // Check access
                var isParticipant = await _context.ChatParticipants
                    .AnyAsync(cp => cp.ChatRoomId == chatRoomId && cp.UserId == userId);

                if (!isParticipant)
                {
                    return ServiceResult<ChatMessageDto>.FailureResult("شما دسترسی ندارید");
                }

                var pinnedMessage = await _context.ChatMessages
                    .Include(m => m.Sender)
                    .Where(m => m.ChatRoomId == chatRoomId && m.IsPinned)
                    .OrderByDescending(m => m.SentDate)
                    .FirstOrDefaultAsync();

                if (pinnedMessage == null)
                {
                    return ServiceResult<ChatMessageDto>.SuccessResult(null);
                }

                var dto = new ChatMessageDto
                {
                    Id = pinnedMessage.Id,
                    Content = pinnedMessage.Content,
                    SenderId = pinnedMessage.SenderId,
                    SenderName = $"{pinnedMessage.Sender.FirstName} {pinnedMessage.Sender.LastName}".Trim(),
                    SenderAvatar = pinnedMessage.Sender.Avatar ?? "",
                    SentDate = pinnedMessage.SentDate,
                    IsPinned = true
                };

                return ServiceResult<ChatMessageDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                return ServiceResult<ChatMessageDto>.FailureResult($"خطا در دریافت پیام پین شده: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> MarkChatAsReadAsync(int chatRoomId, string userId)
        {
            try
            {
                // Check access
                var isParticipant = await _context.ChatParticipants
                    .AnyAsync(cp => cp.ChatRoomId == chatRoomId && cp.UserId == userId);

                if (!isParticipant)
                {
                    return ServiceResult<bool>.FailureResult("شما دسترسی ندارید");
                }

                // Mark all unread messages as read
                var unreadMessages = await _context.ChatMessages
                    .Where(m => m.ChatRoomId == chatRoomId && m.SenderId != userId && m.ReadDate == null)
                    .ToListAsync();

                foreach (var message in unreadMessages)
                {
                    message.ReadDate = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                return ServiceResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.FailureResult($"خطا در علامت‌گذاری پیام‌ها: {ex.Message}");
            }
        }

        public async Task<ServiceResult<List<ChatParticipantDto>>> GetChatParticipantsAsync(int chatRoomId, string userId)
        {
            try
            {
                // Check access
                var isParticipant = await _context.ChatParticipants
                    .AnyAsync(cp => cp.ChatRoomId == chatRoomId && cp.UserId == userId);

                if (!isParticipant)
                {
                    return ServiceResult<List<ChatParticipantDto>>.FailureResult("شما دسترسی ندارید");
                }

                var participants = await _context.ChatParticipants
                    .Include(cp => cp.User)
                    .Where(cp => cp.ChatRoomId == chatRoomId)
                    .ToListAsync();

                var dtos = participants.Select(p => new ChatParticipantDto
                {
                    UserId = p.UserId,
                    UserName = p.User.UserName ?? "",
                    FullName = $"{p.User.FirstName} {p.User.LastName}".Trim(),
                    Avatar = p.User.Avatar ?? "",
                    IsOnline = false, // This would need to be implemented with SignalR
                    LastSeenDate = p.LastSeenDate,
                    IsAdmin = p.IsAdmin,
                    IsMuted = p.IsMuted
                }).ToList();

                return ServiceResult<List<ChatParticipantDto>>.SuccessResult(dtos);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<ChatParticipantDto>>.FailureResult($"خطا در دریافت شرکت‌کنندگان: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> RemoveParticipantAsync(int chatRoomId, string participantId, string userId)
        {
            try
            {
                // Check if user is admin
                var isAdmin = await _context.ChatParticipants
                    .AnyAsync(cp => cp.ChatRoomId == chatRoomId && cp.UserId == userId && cp.IsAdmin);

                if (!isAdmin)
                {
                    return ServiceResult<bool>.FailureResult("فقط مدیران می‌توانند شرکت‌کنندگان را حذف کنند");
                }

                var participant = await _context.ChatParticipants
                    .FirstOrDefaultAsync(cp => cp.ChatRoomId == chatRoomId && cp.UserId == participantId);

                if (participant == null)
                {
                    return ServiceResult<bool>.FailureResult("شرکت‌کننده یافت نشد");
                }

                _context.ChatParticipants.Remove(participant);
                await _context.SaveChangesAsync();

                return ServiceResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.FailureResult($"خطا در حذف شرکت‌کننده: {ex.Message}");
            }
        }

        public async Task<ServiceResult<string>> GetMessageContentAsync(int messageId, string userId)
        {
            try
            {
                var message = await _context.ChatMessages
                    .Include(m => m.ChatRoom)
                    .ThenInclude(cr => cr.Participants)
                    .FirstOrDefaultAsync(m => m.Id == messageId);

                if (message == null)
                {
                    return ServiceResult<string>.FailureResult("پیام یافت نشد");
                }

                // Check access
                var isParticipant = message.ChatRoom.Participants
                    .Any(p => p.UserId == userId);

                if (!isParticipant)
                {
                    return ServiceResult<string>.FailureResult("شما دسترسی ندارید");
                }

                return ServiceResult<string>.SuccessResult(message.Content);
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.FailureResult($"خطا در دریافت محتوای پیام: {ex.Message}");
            }
        }
    }
}