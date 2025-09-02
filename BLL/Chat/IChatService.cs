// IChatService.cs (اصلاح شده برای افزودن ویژگی‌های جدید)
using BE.Chat;
using BE.Chat.DTOs;
using Microsoft.AspNetCore.Http;

namespace BLL.Chat
{
    public interface IChatService
    {
        // Chat Room Management
        Task<ServiceResult<ChatRoomDto>> CreateChatRoomAsync(CreateChatRoomDto model, string creatorId);
        Task<ServiceResult<ChatRoomDto>> GetChatRoomAsync(int chatRoomId, string userId);
        Task<ServiceResult<List<ChatRoomDto>>> GetUserChatRoomsAsync(string userId);
        Task<ServiceResult<ChatRoomDto>> GetExistingPrivateChatRoomAsync(string userId1, string userId2);
        Task<ServiceResult<ChatRoomDto>> GetExistingGroupChatRoomAsync(List<string> userIds);
        Task<ServiceResult<bool>> DeleteChatRoomAsync(int chatRoomId, string userId);

        // Message Management
        Task<ServiceResult<ChatMessageDto>> SendMessageAsync(SendMessageDto model, string senderId);
        Task<ServiceResult<List<ChatMessageDto>>> GetChatMessagesAsync(int chatRoomId, string userId, int page = 1, int pageSize = 50);
        Task<ServiceResult<bool>> MarkMessageAsReadAsync(int messageId, string userId);
        Task<ServiceResult<bool>> DeleteMessageAsync(int messageId, string userId);
        Task<ServiceResult<ChatMessageDto>> EditMessageAsync(int messageId, string newContent, string userId);
        Task<ServiceResult<ChatMessageDto>> ForwardMessageAsync(int messageId, int targetChatRoomId, string userId);
        Task<ServiceResult<bool>> PinMessageAsync(int messageId, int chatRoomId);
        Task<ServiceResult<List<ChatMessageDto>>> SearchMessagesAsync(int chatRoomId, string query, string userId);

        // User Management
        Task<ServiceResult<List<UserSearchDto>>> SearchUsersAsync(string searchTerm, string currentUserId);
        Task<ServiceResult<bool>> AddUserToContactsAsync(string userId, string contactUserId);
        Task<ServiceResult<bool>> RemoveUserFromContactsAsync(string userId, string contactUserId);
        Task<ServiceResult<List<UserSearchDto>>> GetUserContactsAsync(string userId);

        // File Upload
        Task<ServiceResult<string>> UploadChatFileAsync(IFormFile file, string userId, int chatRoomId);
        Task<ServiceResult<string>> UploadVoiceMessageAsync(IFormFile voiceFile, string userId, int chatRoomId);

        // New Message Types
        Task<ServiceResult<ChatMessageDto>> SendFileMessageAsync(IFormFile file, string senderId, int chatRoomId);
        Task<ServiceResult<ChatMessageDto>> SendVoiceMessageAsync(IFormFile audioFile, string senderId, int chatRoomId);

        // Notifications
        Task<ServiceResult<bool>> SendChatNotificationAsync(string recipientId, string senderName, string message);

        // Admin Functions
        Task<ServiceResult<List<ChatRoomDto>>> GetAllChatRoomsAsync();
        Task<ServiceResult<ChatDto>> GetAdminChatViewAsync(int chatRoomId);

        // Additional Features
        Task<ServiceResult<ChatMessageDto>> GetPinnedMessageAsync(int chatRoomId, string userId);
        Task<ServiceResult<bool>> MarkChatAsReadAsync(int chatRoomId, string userId);
        Task<ServiceResult<List<ChatParticipantDto>>> GetChatParticipantsAsync(int chatRoomId, string userId);
        Task<ServiceResult<bool>> RemoveParticipantAsync(int chatRoomId, string participantId, string userId);
        Task<ServiceResult<string>> GetMessageContentAsync(int messageId, string userId);
    }

    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }

        public static ServiceResult<T> SuccessResult(T data, string? message = null)
        {
            return new ServiceResult<T>
            {
                Success = true,
                Data = data,
                Message = message
            };
        }

        public static ServiceResult<T> FailureResult(string message, List<string>? errors = null)
        {
            return new ServiceResult<T>
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }
    }
}