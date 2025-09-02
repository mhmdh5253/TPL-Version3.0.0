using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE.Calendar
{
    public class CalendarActivity
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;
        
        [Required]
        public ActivityType ActivityType { get; set; }
        
        [Required]
        public string Action { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [StringLength(100)]
        public string? EntityType { get; set; }
        
        public int? EntityId { get; set; }
        
        [StringLength(500)]
        public string? OldValues { get; set; }
        
        [StringLength(500)]
        public string? NewValues { get; set; }
        
        [StringLength(45)]
        public string? IpAddress { get; set; }
        
        [StringLength(500)]
        public string? UserAgent { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.Now;
        
        [StringLength(100)]
        public string? SessionId { get; set; }
    }
    
    public enum ActivityType
    {
        Create = 1,
        Read = 2,
        Update = 3,
        Delete = 4,
        View = 5,
        Export = 6,
        Import = 7,
        Login = 8,
        Logout = 9,
        PermissionChange = 10
    }
}

