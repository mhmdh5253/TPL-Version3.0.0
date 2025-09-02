using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE.Calendar
{
    public class CalendarEventCategory
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [StringLength(7)]
        public string Color { get; set; } = "#3788d8";
        
        [StringLength(100)]
        public string? Icon { get; set; }
        
        [Required]
        public bool IsActive { get; set; } = true;
        
        [Required]
        public bool IsSystem { get; set; } = false;
        
        public string? CreatedByUserId { get; set; }
        
        [ForeignKey("CreatedByUserId")]
        public virtual ApplicationUser? CreatedByUser { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedAt { get; set; }
        
        public bool IsDeleted { get; set; } = false;
        
        // Navigation property for events
        public virtual ICollection<CalendarEvent> Events { get; set; } = new List<CalendarEvent>();
    }
}
