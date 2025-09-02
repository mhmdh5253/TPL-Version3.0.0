using System.ComponentModel.DataAnnotations;

namespace TPLWeb.Models
{
    public class LoginConfirmationVM
    {
        [Required][StringLength(6)] public string? SmsCode { get; set; }

        public string? Phone { get; set; }
        public string? Code { get; set; }
    }
}