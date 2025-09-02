using System.ComponentModel.DataAnnotations;
using BE.LetterAutomation;

namespace TPLWeb.Models.ManageUser
{
    public class EditViewModel
    {
        public string? Id { get; set; }
        public string? UserName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Ostan { get; set; }
        public string? Password { get; set; }
        [Display(Name = "تصویر پروفایل")]
        public string? Avatar { get; set; }
        public IFormFile? AvatarPic { get; set; }
        [Display(Name = "نوع حساب")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [MaxLength(20, ErrorMessage = "{0} نمی تواند بیشتر از {1} کاراکتر باشد .")]
        public string? AccountType { get; set; }

        [Display(Name = "نام سازمان")]
        [MaxLength(300, ErrorMessage = "{0} نمی تواند بیشتر از {1} کاراکتر باشد .")]
        public string? CompanyName { get; set; }
        [Display(Name = "نام اداره")]
        [MaxLength(200, ErrorMessage = "{0} نمی تواند بیشتر از {1} کاراکتر باشد .")]
        public string? NameEdareh { get; set; }
        [Display(Name = "سمت")]
        [MaxLength(200, ErrorMessage = "{0} نمی تواند بیشتر از {1} کاراکتر باشد .")]
        public string? Semat { get; set; }
        [Display(Name = "حق امضا")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public bool HaqEmza { get; set; } = false;

        [Display(Name = "امضا")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string? Emza { get; set; }
        public virtual IFormFile? EmzaPic { get; set; }


        [Display(Name = "آدرس")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [MaxLength(450, ErrorMessage = "{0} نمی تواند بیشتر از {1} کاراکتر باشد .")]
        public string? Address { get; set; }

        // اضافه کردن اطلاعات سازمان
        public virtual Organization? Organization { get; set; }
    }

    public class AddViewModel
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "نام کاربری الزامی است")]
        [Display(Name = "نام کاربری")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "رمز عبور الزامی است")]
        [DataType(DataType.Password)]
        [Display(Name = "رمز عبور")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "نام الزامی است")]
        [Display(Name = "نام")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "نام خانوادگی الزامی است")]
        [Display(Name = "نام خانوادگی")]
        public string? LastName { get; set; }

        [Display(Name = "استان")]
        public string? Ostan { get; set; }

        [Required(ErrorMessage = "ایمیل الزامی است")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل صحیح نیست")]
        [Display(Name = "ایمیل")]
        public string? Email { get; set; }

        [Display(Name = "موبایل")]
        public string? Phone { get; set; }

        [Display(Name = "نام شرکت")]
        public string? CompanyName { get; set; }

        [Display(Name = "نام اداره")]
        public string? NameEdareh { get; set; }

        [Display(Name = "سمت")]
        public string? Semat { get; set; }

        [Display(Name = "حق امضا نامه")]
        public bool HaqEmza { get; set; }

        [Display(Name = "تصویر امضا")]
        public IFormFile? EmzaPic { get; set; }

        [Display(Name = "نوع حساب")]
        public string? AccountType { get; set; }

        [Display(Name = "آدرس")]
        public string? Address { get; set; }

        public string? Emza { get; set; }
        public Organization? Organization { get; set; }
        public string? Avatar { get; set; }
    }


}