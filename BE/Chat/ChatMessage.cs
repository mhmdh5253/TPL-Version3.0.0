// ChatMessage.cs (اصلاح شده برای افزودن ویژگی‌های جدید)
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE.Chat
{
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }

        public int ChatRoomId { get; set; }

        [ForeignKey("ChatRoomId")]
        public virtual ChatRoom ChatRoom { get; set; } = null!;

        public string SenderId { get; set; } = string.Empty;

        [ForeignKey("SenderId")]
        public virtual ApplicationUser Sender { get; set; } = null!;

        [Required]
        [MaxLength(4000)]
        public string Content { get; set; } = string.Empty;

        public DateTime SentDate { get; set; } = DateTime.Now;

        public DateTime? DeliveredDate { get; set; }

        public DateTime? ReadDate { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedDate { get; set; }

        public string? DeletedById { get; set; }

        [ForeignKey("DeletedById")]
        public virtual ApplicationUser? DeletedBy { get; set; }

        // Message types
        public MessageType MessageType { get; set; } = MessageType.Text;

        // For file attachments
        [MaxLength(500)]
        public string? FilePath { get; set; }

        [MaxLength(200)]
        public string? FileName { get; set; }

        public long? FileSize { get; set; }

        [MaxLength(100)]
        public string? FileType { get; set; }

        // For voice messages
        [MaxLength(500)]
        public string? VoicePath { get; set; }

        public int? VoiceDuration { get; set; } // in seconds

        // Reply to another message
        public int? ReplyToMessageId { get; set; }

        [ForeignKey("ReplyToMessageId")]
        public virtual ChatMessage? ReplyToMessage { get; set; }

        // Message status
        public MessageStatus Status { get; set; } = MessageStatus.Sent;

        // New features
        public bool IsPinned { get; set; } = false;
        public bool IsEdited { get; set; } = false;
        public string? ForwardedFrom { get; set; }
    }

    public enum MessageType
    {
        Text = 1,
        Image = 2,
        File = 3,
        Voice = 4,
        System = 5
    }

    public enum MessageStatus
    {
        Sent = 1,
        Delivered = 2,
        Read = 3,
        Failed = 4
    }
}