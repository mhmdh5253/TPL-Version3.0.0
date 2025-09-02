using System.ComponentModel.DataAnnotations;

namespace TPLWeb.Models.LetterAutomation
{
    public class LetterDTO
    {
        [Required(ErrorMessage = "نوع مکاتبه الزامی است")]
        public string? CorrespondenceType { get; set; }

        [Required(ErrorMessage = "نوع ارتباط نامه الزامی است")]
        public string? LetterRelationType { get; set; }

        public int? RelatedLetterId { get; set; }

        [Required(ErrorMessage = "تاریخ ثبت نامه الزامی است")]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        public DateTime? FollowUpDate { get; set; }

        [Required(ErrorMessage = "ارجحیت نامه الزامی است")]
        public string? Priority { get; set; }

        [Required(ErrorMessage = "طبقه بندی نامه الزامی است")]
        public string? Classification { get; set; }

        [Display(Name = "فایل پیوست")]
        public IFormFile? Attachment { get; set; }

        public bool ShowOnDashboard { get; set; } = false;

        [Required(ErrorMessage = "کد کلاسه نامه الزامی است")]
        public string? FileCode { get; set; }

        [Required(ErrorMessage = "توضیح کلاسه نامه الزامی است")]
        public string? FileDescription { get; set; }

        [Required(ErrorMessage = "فرستنده نامه الزامی است")]
        public string? Sender { get; set; }

        [Required(ErrorMessage = "گیرنده نامه الزامی است")]
        public string? Receiver { get; set; }

        [Required(ErrorMessage = "موضوع نامه الزامی است")]
        public string? Subject { get; set; }

        [Display(Name = "تصویر نامه")]
        public IFormFile? Image { get; set; }

        public string? CopyReceivers { get; set; }

        public string? Keywords { get; set; }

        [Required(ErrorMessage = "نام امضا کننده الزامی است")]
        public string? SignerName { get; set; }

        [Required(ErrorMessage = "مقام امضا کننده الزامی است")]
        public string? SignerPosition { get; set; }

        [Required(ErrorMessage = "متن نامه الزامی است")]
        public string? Content { get; set; }
    }
}
