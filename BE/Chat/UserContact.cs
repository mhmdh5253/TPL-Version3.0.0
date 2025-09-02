// UserContact.cs (بدون تغییر عمده)
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BE.Chat
{
    public class UserContact
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        public string ContactUserId { get; set; } = string.Empty;

        [ForeignKey("ContactUserId")]
        public virtual ApplicationUser ContactUser { get; set; } = null!;

        public DateTime AddedDate { get; set; } = DateTime.Now;

        public bool IsFavorite { get; set; } = false;

        [MaxLength(100)]
        public string? Nickname { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime LastContactDate { get; set; } = DateTime.Now;

        public int UnreadCount { get; set; } = 0;

        // Ensure unique combination of user and contact
        [Index(nameof(UserId), nameof(ContactUserId), IsUnique = true)]
        public class UserContactIndex { }
    }
}