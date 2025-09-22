using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE.LetterAutomation
{
    /// <summary>
    /// متغیر های نامه ها
    /// </summary>
    #region ٍمتغیر های نامه 


    public enum CorrespondenceType
    {
        مکاتباتی,
        آموزشی,
        اطلاع_رسانی
    }

    public enum LetterRelationType
    {
        نامه_جدید,
        پاسخ,
        عطف,
        پیرو
    }

    public enum PriorityType
    {
        عادی,
        فوری,
        آنی,
        اقدام_سریع
    }

    public enum ClassificationType
    {
        عادی,
        محرمانه,
        خیلی_محرمانه
    }
    public enum LetterStatus
    {
        [Display(Name = "در انتظار بررسی")]
        Pending,

        [Display(Name = "ارجاع شده")]
        Referred,

        [Display(Name = "بایگانی شده")]
        Archived,

        [Display(Name = "تکمیل شده")]
        Completed,

        [Display(Name = "رد شده")]
        Rejected,

        [Display(Name = "ارسال به مقام بالاتر")]
        SentToHigherAuthority,

        [Display(Name = "در حال بررسی")]
        InReview,

        [Display(Name = "تایید شده")]
        Approved,

        [Display(Name = "عودت شده")]
        Returned,

        [Display(Name = "حذف شده")]
        Deleted
    }
    #endregion



    public class Letter
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "شناسه")]
        public int Id { get; set; }

        [Display(Name = "شماره نامه")]
        [StringLength(50, ErrorMessage = "{0} نمی‌تواند بیش از {1} کاراکتر باشد")]
        public string? LetterNumber { get; set; }

        [Display(Name = "نوع مکاتبه")]
        [Required(ErrorMessage = "{0} الزامی است")]
        public CorrespondenceType CorrespondenceType { get; set; }

        [Display(Name = "نوع ارتباط نامه")]
        [Required(ErrorMessage = "{0} الزامی است")]
        public LetterRelationType LetterRelationType { get; set; }

        [Display(Name = "نامه مرتبط")]
        [ForeignKey("RelatedLetterId")]
        public int? RelatedLetterId { get; set; }
        public virtual Letter? RelatedLetter { get; set; }

        [Display(Name = "تاریخ ثبت")]
        [Required(ErrorMessage = "{0} الزامی است")]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        [Display(Name = "تاریخ پیگیری")]
        public DateTime? FollowUpDate { get; set; }

        [Display(Name = "اولویت")]
        [Required(ErrorMessage = "{0} الزامی است")]
        public PriorityType Priority { get; set; }

        [Display(Name = "سطح محرمانگی")]
        [Required(ErrorMessage = "{0} الزامی است")]
        public ClassificationType Classification { get; set; }

        [Display(Name = "پیوست")]
        public string? AttachmentName { get; set; }

        [Display(Name = "نمایش در تابلو اعلانات")]
        public bool ShowOnDashboard { get; set; } = false;

        [Display(Name = "کد کلاسه")]
        [Required(ErrorMessage = "{0} الزامی است")]
        [StringLength(50, ErrorMessage = "{0} نمی‌تواند بیش از {1} کاراکتر باشد")]
        public string? FileCode { get; set; }

        [Display(Name = "توضیح کلاسه")]
        public string? FileDescription { get; set; }

        [Display(Name = "فرستنده")]
        [Required(ErrorMessage = "{0} الزامی است")]
        [StringLength(100, ErrorMessage = "{0} نمی‌تواند بیش از {1} کاراکتر باشد")]
        public string? Sender { get; set; }

        [Display(Name = "گیرنده")]
        [Required(ErrorMessage = "{0} الزامی است")]
        [StringLength(100, ErrorMessage = "{0} نمی‌تواند بیش از {1} کاراکتر باشد")]
        public string? Receiver { get; set; }

        [Display(Name = "موضوع")]
        [Required(ErrorMessage = "{0} الزامی است")]
        [StringLength(200, ErrorMessage = "{0} نمی‌تواند بیش از {1} کاراکتر باشد")]
        public string? Subject { get; set; }

        [Display(Name = "گیرندگان رونوشت")]
        public virtual List<string>? CopyReceivers { get; set; }

        [Display(Name = "سازمان‌های گیرنده رونوشت")]
        public ICollection<Organization> CopyReceiversList { get; set; } = new List<Organization>();

        [Display(Name = "کلیدواژه‌ها")]
        public string? Keywords { get; set; }

        [Display(Name = "لیست کلیدواژه‌ها")]
        public List<string> KeywordsList { get; set; } = new List<string>();

        [Display(Name = "امضا کننده")]
        [Required(ErrorMessage = "{0} الزامی است")]
        [StringLength(100, ErrorMessage = "{0} نمی‌تواند بیش از {1} کاراکتر باشد")]
        public string? SignerName { get; set; }
        public string? SignerUserName { get; set; }

        [Display(Name = "سمت امضا کننده")]
        [StringLength(100, ErrorMessage = "{0} نمی‌تواند بیش از {1} کاراکتر باشد")]
        public string? SignerPosition { get; set; }

        [Display(Name = "محتوا")]
        [Required(ErrorMessage = "{0} الزامی است")]
        [Column(TypeName = "ntext")]
        public string? Content { get; set; }

        [Display(Name = "کاربر ثبت کننده")]
        public string? Username { get; set; }

        [Display(Name = "وضعیت")]
        public LetterStatus Status { get; set; } = LetterStatus.Pending;

        [Display(Name = "کد بایگانی سازمانی")]
        public string? ArchiveCode { get; set; }

        [Display(Name = "تاریخ بایگانی سازمانی")]
        public DateTime? OrganizationArchiveDate { get; set; }

        [Display(Name = "ارجاع‌ها")]
        public virtual ICollection<LetterReferral> Referrals { get; set; } = new List<LetterReferral>();


        [Display(Name = "تاریخ آخرین ویرایش")]
        public DateTime? LastModifiedDate { get; set; }

        [Display(Name = "کاربر ویرایش کننده")]
        public string? LastModifiedBy { get; set; }

        [Display(Name = "آیا حذف شده است؟")]
        public bool IsDeleted { get; set; } = false;

        [Display(Name = "آیا خوانده شده است؟")]
        [NotMapped]
        public bool IsRead { get; set; } = false;
    }



    public class Archive
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("Letter")]
        public int LetterId { get; set; }
        public virtual Letter? Letter { get; set; }

        [Required]
        [StringLength(50)]
        public string? ArchiveCode { get; set; }

        [Required]
        public string? UserId { get; set; }

        [Required]
        public DateTime ArchiveDate { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? Category { get; set; }
    }


    public class LetterReferral
    {
        public int Id { get; set; }
        public int LetterId { get; set; }
        public Letter? Letter { get; set; }

        [Display(Name = "ارجاع به کاربر")]
        public string? ReferredToUserId { get; set; }

        [Display(Name = "ارجاع توسط")]
        public string? ReferredByUserId { get; set; }

        [Display(Name = "تاریخ ارجاع")]
        public DateTime ReferralDate { get; set; }

        [Display(Name = "توضیحات")]
        public string? Comment { get; set; }

        [Display(Name = "تکمیل شده")]
        public bool IsCompleted { get; set; }
    }


    public class ApiResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public object? Data { get; set; }
    }




    //  مدل های مربوط به بخش جستجوی نامه ها
    public class LetterSearchResult
    {
        public int Id { get; set; }
        public string? LetterNumber { get; set; }
        public string? Subject { get; set; }
        public string? RegistrationDate { get; set; }
    }

    public class LetterSearchParams
    {
        public string? Subject { get; set; }
        public string? LetterNumber { get; set; }
        public List<string>? KeywordsList { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        // برای محدودسازی نتایج به حوزه دسترسی کاربر جاری
        public string? CurrentUsername { get; set; }
        public int? UserOrganizationId { get; set; }
    }


    public class LetterAction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int LetterId { get; set; }

        [ForeignKey("LetterId")]
        public Letter? Letter { get; set; }

        [Required]
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [Required]
        public string? ActionDescription { get; set; } // تنها فیلد مورد نیاز شما

        [Required]
        public DateTime ActionDate { get; set; } = DateTime.Now;
    }
    [AtLeastOnePropertyAttribute]
    public class AdvancedLetterSearchParams
    {
        public string? Subject { get; set; }
        public string? LetterNumber { get; set; }
        public string? Sender { get; set; }
        public string? Receiver { get; set; }
        public PriorityType? Priority { get; set; }
        public ClassificationType? Classification { get; set; }
        public LetterStatus? Status { get; set; }
        public string? Keywords { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? SignerName { get; set; }
        public string? FileCode { get; set; }
        // برای محدودسازی نتایج به حوزه دسترسی کاربر جاری
        public string? CurrentUsername { get; set; }
        public int? UserOrganizationId { get; set; }
      
    }
    public class AtLeastOnePropertyAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            var properties = value!.GetType().GetProperties();
            foreach (var property in properties)
            {
                var propValue = property.GetValue(value);

                if (propValue != null && !string.IsNullOrEmpty(propValue.ToString()))
                {
                    return ValidationResult.Success!;
                }

                // For nullable value types
                if (Nullable.GetUnderlyingType(property.PropertyType) != null && propValue != null)
                {
                    return ValidationResult.Success!;
                }
            }

            return new ValidationResult("حداقل یکی از فیلدهای جستجو باید پر باشد");
        }
    }
    public class AdvancedLetterSearchResult
    {
        public int Id { get; set; }
        public string? LetterNumber { get; set; }
        public string? Subject { get; set; }
        public string? Sender { get; set; }
        public string? Receiver { get; set; }
        public string? RegistrationDate { get; set; }
        public string? Priority { get; set; }
        public string? Classification { get; set; }
        public string? Status { get; set; }
        public string? SignerName { get; set; }
        public string? FileCode { get; set; }
    }
}