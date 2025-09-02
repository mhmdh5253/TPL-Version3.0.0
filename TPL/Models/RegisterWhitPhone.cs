using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace TPLWeb.Models
{
    public class RegisterWhitPhone
    {
        [Required(ErrorMessage = "وارد کردن تلفن اجباری می باشد")]
        [Phone]
        [Remote("IsAnyPhone", "Account", HttpMethod = "Post", AdditionalFields = "__RequestVerificationToken")]
        public string? Phone { get; set; }

        [Display(Name = "کد ارسالی")]
        [MaxLength(6, ErrorMessage = "{0} نمی تواند بیشتر از {1} کاراکتر باشد .")]
        public string? SmsCode { get; set; }
        [Display(Name = "توکن")]
        public string? Code { get; set; }


    }
}