using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE.LetterAutomation
{
    public class Organization
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "نام سازمان الزامی است")]
        [StringLength(100, ErrorMessage = "نام سازمان نمی‌تواند بیش از 100 کاراکتر باشد")]
        [Display(Name = "نام سازمان")]
        public string? Name { get; set; }
        [Display(Name = "نام")]
        public string? FullName { get; set; }
        [Required]
        [Display(Name = "نوع سازمان")]
        public string? OrgType { get; set; } // main, department, unit, section

        [Required]
        [Display(Name = "سازمان اصلی؟")]
        public bool IsMain { get; set; }

        [Display(Name = "والد")]
        public int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        [Display(Name = "سازمان والد")]
        public virtual Organization? Parent { get; set; }

        [Display(Name = "زیرمجموعه‌ها")]
        public virtual ICollection<Organization> Children { get; set; } = new List<Organization>();

        [StringLength(50, ErrorMessage = "کد سازمان نمی‌تواند بیش از 50 کاراکتر باشد")]
        [Display(Name = "کد سازمان")]
        public string? Code { get; set; }

        [StringLength(20, ErrorMessage = "تلفن نمی‌تواند بیش از 20 کاراکتر باشد")]
        [Display(Name = "تلفن")]
        public string? Phone { get; set; }

        [StringLength(100, ErrorMessage = "آدرس ایمیل نمی‌تواند بیش از 100 کاراکتر باشد")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است")]
        [Display(Name = "ایمیل")]
        public string? Email { get; set; }

        [StringLength(200, ErrorMessage = "آدرس نمی‌تواند بیش از 200 کاراکتر باشد")]
        [Display(Name = "آدرس")]
        public string? Address { get; set; }

        [StringLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد")]
        [Display(Name = "توضیحات")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "فعال؟")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "تاریخ آخرین ویرایش")]
        public DateTime? UpdatedAt { get; set; }

        public ApplicationUser? User { get; set; }
    }

    // مدل ویو برای نمایش در صفحات
    public class OrganizationViewModel
    {
        public List<OrganizationTreeItem>? MainOrganizations { get; set; }
        public OrganizationFormModel? FormModel { get; set; }
    }

    public class OrganizationTreeItem
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? OrgType { get; set; }
        public List<OrganizationTreeItem> SubOrganizations { get; set; } = new List<OrganizationTreeItem>();
        public bool HasChildren => SubOrganizations.Any();
    }

    public class OrganizationFormModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "نام سازمان الزامی است")]
        [Display(Name = "نام سازمان")]
        public string? Name { get; set; }

        [Display(Name = "نوع سازمان")]
        public string? OrgType { get; set; }

        [Display(Name = "سازمان والد")]
        public int? ParentId { get; set; }

        [Display(Name = "کد سازمان")]
        public string? Code { get; set; }
        [StringLength(20, ErrorMessage = "تلفن نمی‌تواند بیش از 20 کاراکتر باشد")]
        [Display(Name = "تلفن")]
        public string? Phone { get; set; }

        [StringLength(100, ErrorMessage = "آدرس ایمیل نمی‌تواند بیش از 100 کاراکتر باشد")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است")]
        [Display(Name = "ایمیل")]
        public string? Email { get; set; }

        [StringLength(200, ErrorMessage = "آدرس نمی‌تواند بیش از 200 کاراکتر باشد")]
        [Display(Name = "آدرس")]
        public string? Address { get; set; }

        [Display(Name = "توضیحات")]
        public string? Description { get; set; }
    }

    // مدل رابطه کاربر با سازمان
    public class UserOrganization
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        [Required]
        public int OrganizationId { get; set; }
        public virtual Organization? Organization { get; set; }

        [Required]
        [StringLength(100)]
        public string? Position { get; set; } // سمت کاربر در سازمان

        [Required]
        public bool IsHead { get; set; } // آیا مسئول این بخش است؟

        [Required]
        public bool IsDeputy { get; set; } // آیا جانشین مسئول است؟

        [Required]
        public DateTime StartDate { get; set; } = DateTime.Now;

        public DateTime? EndDate { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;
    }

    // مدل درخواست تایید
    public class LetterApproval
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int LetterId { get; set; }
        public virtual Letter? Letter { get; set; }

        [Required]
        public string? RequesterUserId { get; set; } // کاربر درخواست کننده
        public virtual ApplicationUser? Requester { get; set; }

        [Required]
        public string? ApproverUserId { get; set; } // کاربر تایید کننده
        public virtual ApplicationUser? Approver { get; set; }

        [Required]
        public int OrganizationId { get; set; } // سازمان مربوطه
        public virtual Organization? Organization { get; set; }

        [Required]
        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

        public string? Comment { get; set; }

        [Required]
        public DateTime RequestDate { get; set; } = DateTime.Now;

        public DateTime? ActionDate { get; set; }

        [Required]
        public int ApprovalLevel { get; set; } // سطح در سلسله مراتب
    }

    public enum ApprovalStatus
    {
        Pending,
        Approved,
        Rejected,
        Returned
    }
}