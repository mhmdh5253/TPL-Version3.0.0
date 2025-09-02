// ChatRoom.cs (اصلاح شده برای پین شده‌ها)
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE.Chat
{
    public class ChatRoom
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(450)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime LastActivityDate { get; set; } = DateTime.Now;

        public bool IsGroup { get; set; } = false;

        public string? CreatedById { get; set; }

        [ForeignKey("CreatedById")]
        public virtual ApplicationUser? CreatedBy { get; set; }

        // Navigation properties
        public virtual ICollection<ChatParticipant> Participants { get; set; } = new List<ChatParticipant>();
        public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}