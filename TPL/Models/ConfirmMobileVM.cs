using System.ComponentModel.DataAnnotations;

namespace TPLWeb.Models
{
    public class ConfirmMobileVM
    {
        [Required(ErrorMessage = "شماره تلفن الزامی است")]
        [Display(Name = "شماره تلفن")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "توکن الزامی است")]
        [Display(Name = "توکن")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "کد تایید الزامی است")]
        [Display(Name = "کد تایید")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "کد تایید باید 6 رقم باشد")]
        public string OtpCode { get; set; } = string.Empty;
    }
}
