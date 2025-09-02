using System.ComponentModel.DataAnnotations;

namespace TPLWeb.Models
{
    /// <summary>
    /// Enhanced login model with SMS provider selection
    /// </summary>
    public class EnhancedLoginWithPhoneVM
    {
        [Required(ErrorMessage = "شماره موبایل الزامی است")]
        [RegularExpression(@"^09\d{9}$", ErrorMessage = "شماره موبایل باید با 09 شروع شود و 11 رقم باشد")]
        [Display(Name = "شماره موبایل")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "ارائه‌دهنده پیامک")]
        public string? SmsProvider { get; set; }

        [Display(Name = "به خاطر بسپار")]
        public bool RememberMe { get; set; }

        [Display(Name = "آدرس بازگشت")]
        public string? ReturnUrl { get; set; }
    }
}







