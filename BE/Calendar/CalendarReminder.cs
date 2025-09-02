using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE.Calendar
{
    public class CalendarReminder
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
        public DateTime ReminderTime { get; set; }
        
        [Required]
        public ReminderType ReminderType { get; set; }
        
        [Required]
        public bool IsSent { get; set; } = false;
        
        public DateTime? SentAt { get; set; }
        
        [StringLength(100)]
        public string? NotificationMethod { get; set; } = "InApp";
        
        [StringLength(500)]
        public string? Message { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
    
    public enum ReminderType
    {
        OnTime = 1,
        FiveMinutesBefore = 2,
        FifteenMinutesBefore = 3,
        ThirtyMinutesBefore = 4,
        OneHourBefore = 5,
        OneDayBefore = 6,
        Custom = 7
    }
}

