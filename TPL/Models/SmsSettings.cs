using System.ComponentModel.DataAnnotations;

namespace TPLWeb.Models
{
    public class SmsSettings
    {
        [Required(ErrorMessage = "انتخاب سامانه پیامکی الزامی است")]
        [Display(Name = "سامانه پیامکی")]
        public string Provider { get; set; } = "ApiIr";

        [Display(Name = "آدرس API")]
        public string ApiUrl { get; set; } = string.Empty;

        [Display(Name = "توکن Bearer (برای Api.ir)")]
        public string BearerToken { get; set; } = string.Empty;

        [Display(Name = "نام کاربری (برای ملی پیامک)")]
        public string Username { get; set; } = string.Empty;

        [Display(Name = "رمز عبور (برای ملی پیامک)")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "شماره فرستنده الزامی است")]
        [Display(Name = "شماره فرستنده")]
        public string Sender { get; set; } = string.Empty;

        [Display(Name = "کد قالب OTP")]
        public string OtpPattern { get; set; } = string.Empty;

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;
    }

    public class SmsResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string GatewayResponse { get; set; } = string.Empty;
        public string? MessageId { get; set; }
    }

    public class OtpRequest
    {
        [Required(ErrorMessage = "شماره تلفن الزامی است")]
        [Phone(ErrorMessage = "فرمت شماره تلفن صحیح نیست")]
        [Display(Name = "شماره تلفن")]
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class OtpVerification
    {
        [Required(ErrorMessage = "شماره تلفن الزامی است")]
        [Display(Name = "شماره تلفن")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "کد تایید الزامی است")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "کد تایید باید 6 رقم باشد")]
        [Display(Name = "کد تایید")]
        public string Code { get; set; } = string.Empty;
    }
}
