using Microsoft.AspNetCore.Authentication;
using System.ComponentModel.DataAnnotations;

namespace TPLWeb.Models
{
    public class LoginVM
    {
        [Required]
        [Display(Name = "نام کاربری")]
        [StringLength(200)]

        public string? UserName { get; set; }


        [Required]
        [DataType(DataType.Password)]

        public string? Password { get; set; }

        [Display(Name = "مرا به خاطر بسپار؟")] public bool RememberMe { get; set; }
    }

    public class LoginWithPhoneVM
    {
        [Required][Phone] public string? Phone { get; set; }
    }


    public class LoginWhitExternalLogins
    {
        public string? ReturnUrl { get; set; }

        public List<AuthenticationScheme>? ExternalLogins { get; set; }
    }

}