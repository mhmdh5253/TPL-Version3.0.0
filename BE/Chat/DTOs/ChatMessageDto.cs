// ChatMessageDto.cs (اصلاح شده برای افزودن ویژگی‌های جدید)
using BE.Chat;

namespace BE.Chat.DTOs
{
    public class ChatMessageDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string SenderId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string SenderAvatar { get; set; } = string.Empty;
        public DateTime SentDate { get; set; }
        public bool IsOwnMessage { get; set; }
        public MessageType MessageType { get; set; }
        public string? FilePath { get; set; }
        public string? FileName { get; set; }
        public long? FileSize { get; set; }
        public string? FileType { get; set; }
        public string? VoicePath { get; set; }
        public int? VoiceDuration { get; set; }
        public bool IsRead { get; set; }
        public int? ReplyToMessageId { get; set; }
        public string? ReplyToContent { get; set; }
        public MessageStatus Status { get; set; }
        public bool IsPinned { get; set; }
        public bool IsEdited { get; set; }
        public string? ForwardedFrom { get; set; }
    }
}