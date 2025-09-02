using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE.Calendar
{
    public class CalendarEventParticipant
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int CalendarEventId { get; set; }
        
        [ForeignKey("CalendarEventId")]
        public virtual CalendarEvent CalendarEvent { get; set; } = null!;
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;
        
        [Required]
        public ParticipantStatus Status { get; set; }
        
        [Required]
        public ParticipantRole Role { get; set; }
        
        public DateTime? ResponseDate { get; set; }
        
        [StringLength(500)]
        public string? ResponseNote { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedAt { get; set; }
    }
    
    public enum ParticipantStatus
    {
        Pending = 1,
        Accepted = 2,
        Declined = 3,
        Tentative = 4,
        NoResponse = 5
    }
    
    public enum ParticipantRole
    {
        Attendee = 1,
        Organizer = 2,
        Required = 3,
        Optional = 4
    }
}

