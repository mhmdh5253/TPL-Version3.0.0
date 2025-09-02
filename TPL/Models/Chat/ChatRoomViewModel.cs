using System.ComponentModel.DataAnnotations;

namespace TPLWeb.Models.Chat
{
    public class ChatRoomViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "نام چت")]
        public string Name { get; set; } = string.Empty;
        
        [Display(Name = "توضیحات")]
        public string? Description { get; set; }
        
        [Display(Name = "تاریخ آخرین فعالیت")]
        public DateTime LastActivityDate { get; set; }
        
        [Display(Name = "آیا گروه است")]
        public bool IsGroup { get; set; }
        
        [Display(Name = "تعداد پیام‌های نخوانده")]
        public int UnreadCount { get; set; }
        
        [Display(Name = "آخرین پیام")]
        public string? LastMessage { get; set; }
        
        [Display(Name = "تاریخ آخرین پیام")]
        public DateTime? LastMessageDate { get; set; }
        
        [Display(Name = "نام فرستنده آخرین پیام")]
        public string? LastMessageSenderName { get; set; }
        
        [Display(Name = "شرکت‌کنندگان")]
        public List<ChatParticipantViewModel> Participants { get; set; } = new List<ChatParticipantViewModel>();
    }

    public class ChatParticipantViewModel
    {
        public string UserId { get; set; } = string.Empty;
        
        [Display(Name = "نام کاربری")]
        public string UserName { get; set; } = string.Empty;
        
        [Display(Name = "نام کامل")]
        public string FullName { get; set; } = string.Empty;
        
        [Display(Name = "آواتار")]
        public string? Avatar { get; set; }
        
        [Display(Name = "آنلاین است")]
        public bool IsOnline { get; set; }
        
        [Display(Name = "تاریخ آخرین بازدید")]
        public DateTime LastSeenDate { get; set; }
        
        [Display(Name = "مدیر است")]
        public bool IsAdmin { get; set; }
        
        [Display(Name = "بی‌صدا است")]
        public bool IsMuted { get; set; }
    }
}
