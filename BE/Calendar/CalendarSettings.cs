using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE.Calendar
{
    public class CalendarSettings
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;
        
        [Required]
        public string DefaultView { get; set; } = "month";
        
        [Required]
        public bool ShowWeekends { get; set; } = true;
        
        [Required]
        public bool ShowBusinessHours { get; set; } = true;
        
        [Required]
        public TimeSpan BusinessStartTime { get; set; } = new TimeSpan(8, 0, 0);
        
        [Required]
        public TimeSpan BusinessEndTime { get; set; } = new TimeSpan(17, 0, 0);
        
        [Required]
        public string FirstDayOfWeek { get; set; } = "Saturday";
        
        [Required]
        public bool EnableNotifications { get; set; } = true;
        
        [Required]
        public int NotificationAdvanceMinutes { get; set; } = 15;
        
        [Required]
        public string Language { get; set; } = "fa";
        
        [Required]
        public string CalendarType { get; set; } = "persian";
        
        [Required]
        public bool EnableDragAndDrop { get; set; } = true;
        
        [Required]
        public bool EnableResize { get; set; } = true;
        
        [Required]
        public bool ShowEventDetailsOnClick { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedAt { get; set; }
    }
}

