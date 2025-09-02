// ChatViewModel.cs (اصلاح شده برای افزودن ویژگی‌های جدید)
using System.ComponentModel.DataAnnotations;
using BE.Chat;

namespace TPLWeb.Models.Chat
{
    public class ChatViewModel
    {
        public int ChatRoomId { get; set; }
        public string ChatRoomName { get; set; } = string.Empty;
        public bool IsGroup { get; set; }
        public List<ChatMessageViewModel> Messages { get; set; } = new List<ChatMessageViewModel>();
        public List<ChatParticipantViewModel> Participants { get; set; } = new List<ChatParticipantViewModel>();
        public string CurrentUserId { get; set; } = string.Empty;
        public string CurrentUserName { get; set; } = string.Empty;
        public string CurrentUserAvatar { get; set; } = string.Empty;
    }

    public class ChatMessageViewModel
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



    public class UserSearchViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public bool IsInContacts { get; set; }
        public string Email { get; set; } = string.Empty;
    }

    public class SendMessageViewModel
    {
        [Required(ErrorMessage = "پیام نمی‌تواند خالی باشد")]
        [MaxLength(4000, ErrorMessage = "پیام نمی‌تواند بیشتر از 4000 کاراکتر باشد")]
        public string Content { get; set; } = string.Empty;

        public int ChatRoomId { get; set; }
        public MessageType MessageType { get; set; } = MessageType.Text;
        public int? ReplyToMessageId { get; set; }
    }

    public class CreateChatRoomViewModel
    {
        [Required(ErrorMessage = "نام چت نمی‌تواند خالی باشد")]
        [MaxLength(450, ErrorMessage = "نام چت نمی‌تواند بیشتر از 450 کاراکتر باشد")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "توضیحات نمی‌تواند بیشتر از 1000 کاراکتر باشد")]
        public string? Description { get; set; }

        public bool IsGroup { get; set; } = false;

        [Required(ErrorMessage = "حداقل یک کاربر باید انتخاب شود")]
        public List<string> ParticipantUserIds { get; set; } = new List<string>();
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