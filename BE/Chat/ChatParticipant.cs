// ChatParticipant.cs (اصلاح شده)
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE.Chat
{
    public class ChatParticipant
    {
        [Key]
        public int Id { get; set; }

        public int ChatRoomId { get; set; }

        [ForeignKey("ChatRoomId")]
        public virtual ChatRoom ChatRoom { get; set; } = null!;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        public DateTime JoinedDate { get; set; } = DateTime.Now;

        public DateTime LastSeenDate { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        public bool IsAdmin { get; set; } = false;

        public bool IsMuted { get; set; } = false;

        // For personal contacts list
        public bool IsFavorite { get; set; } = false;

        public DateTime? AddedToContactsDate { get; set; }
    }
}