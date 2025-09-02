using System.ComponentModel;

namespace BE.Ticketing
{
    using System.ComponentModel.DataAnnotations;

    namespace SupportTicketSystem.Models
    {
        public enum TicketStatus
        {
            New,
            Open,
            InProgress,
            Resolved,
            Closed
        }

        public enum Priority
        {
            Low,
            Medium,
            High,
            Critical
        }

        public class Ticket
        {
            [Key]
            public int Id { get; set; }

            [Required]
            [StringLength(100)]
            [DisplayName("عنوان")]
            public string? Title { get; set; }

            [Required]
            [DisplayName("توضیحات")]
            public string? Description { get; set; }

            [Display(Name = "Created Date")]
            [DisplayName("تاریخ ایجاد")]
            public DateTime CreatedDate { get; set; } = DateTime.Now;

            [Display(Name = "Updated Date")]
            [DisplayName("تاریخ ویرایش")]
            public DateTime UpdatedDate { get; set; } = DateTime.Now;

            [DisplayName("وضعیت")]
            public TicketStatus Status { get; set; } = TicketStatus.New;
            [DisplayName("حساسیت")]
            public Priority Priority { get; set; } = Priority.Medium;

            [Display(Name = "Created By")]
            [DisplayName("ایجاد شده توسط")]
            public string? CreatedByUserId { get; set; }

            [Display(Name = "Assigned To")]
            [DisplayName("الحاق به کاربر")]
            public string? AssignedToUserId { get; set; }
            [DisplayName("دسته بندی")]
            public int? CategoryId { get; set; }
            [DisplayName("دسته بندی")]
            public Category? Category { get; set; }
            [DisplayName("نظرات")]
            public ICollection<TicketComment> Comments { get; set; } = [];
            [DisplayName("پیوست ها")]
            public ICollection<Attachment> Attachments { get; set; } = [];
        }

        public class TicketComment
        {
            [Key]
            public int Id { get; set; }

            [Required]
            public string? Content { get; set; }

            [Display(Name = "Created Date")]
            public DateTime CreatedDate { get; set; } = DateTime.Now;

            public string? CreatedByUserId { get; set; }

            public int TicketId { get; set; }
            public Ticket? Ticket { get; set; }

            public ICollection<Attachment> Attachments { get; set; } = [];
        }

        public class Category
        {
            [Key]
            public int Id { get; set; }

            [Required]
            [StringLength(50)]
            public string? Name { get; set; }

            [StringLength(200)]
            public string? Description { get; set; }

            public ICollection<Ticket> Tickets { get; set; } = [];
        }

        public class Attachment
        {
            [Key]
            public int Id { get; set; }

            [Required]
            public string? FileName { get; set; }

            [Required]
            public string? FilePath { get; set; }

            public string? ContentType { get; set; }
            public long FileSize { get; set; }

            public int? TicketId { get; set; }
            public Ticket? Ticket { get; set; }

            public int? CommentId { get; set; }
            public TicketComment? Comment { get; set; }

            public DateTime UploadDate { get; set; } = DateTime.Now;
            public string? UploadedByUserId { get; set; }
        }

        public class Notification
        {
            [Key]
            public int Id { get; set; }

            [Required]
            public string? UserId { get; set; }

            [Required]
            public string? Message { get; set; }

            public bool IsRead { get; set; } = false;
            public DateTime CreatedDate { get; set; } = DateTime.Now;
            public string? Link { get; set; }
        }

        public class Report
        {
            [Key]
            public int Id { get; set; }

            [Required]
            public string? Title { get; set; }

            public string? Description { get; set; }
            public DateTime GeneratedDate { get; set; } = DateTime.Now;
            public string? GeneratedByUserId { get; set; }
            public string? ReportData { get; set; }
        }

    }
}
