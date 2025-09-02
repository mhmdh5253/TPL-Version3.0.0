using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE.Calendar
{
    public class CalendarEvent
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
        
        [Required]
        public bool IsAllDay { get; set; }
        
        [StringLength(50)]
        public string? Color { get; set; } = "#3788d8";
        
        [Required]
        public EventType EventType { get; set; }
        
        [Required]
        public EventVisibility Visibility { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedAt { get; set; }
        
        public bool IsDeleted { get; set; } = false;
        
        // For manager announcements
        public bool IsManagerAnnouncement { get; set; } = false;
        
        [StringLength(100)]
        public string? Location { get; set; }
        
        [StringLength(50)]
        public string? Priority { get; set; } = "Normal";
        
        public bool IsRecurring { get; set; } = false;
        
        [StringLength(50)]
        public string? RecurrencePattern { get; set; }
        
        // Category relationship
        public int? CategoryId { get; set; }
        
        [ForeignKey("CategoryId")]
        public virtual CalendarEventCategory? Category { get; set; }
        
        // Participants
        public virtual ICollection<CalendarEventParticipant> Participants { get; set; } = new List<CalendarEventParticipant>();
        
        // Reminders
        public virtual ICollection<CalendarReminder> Reminders { get; set; } = new List<CalendarReminder>();

        // Status
        public bool IsCompleted { get; set; } = false;
    }
    
    public enum EventType
    {
        Personal = 1,
        Work = 2,
        Meeting = 3,
        Reminder = 4,
        Holiday = 5,
        Announcement = 6,
        Other = 7
    }
    
    public enum EventVisibility
    {
        Private = 1,
        Public = 2,
        Organization = 3
    }
}
