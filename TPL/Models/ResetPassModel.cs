using System.ComponentModel.DataAnnotations;

namespace TPLWeb.Models
{
    public class ResetPassModel
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = " رمز عبور نمی تواند خالی باشد ")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required(ErrorMessage = "تکرار رمز عبور نمی تواند خالی باشد")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "رمز عبور و تکرار آن با هم برابر نیستند")]
        public string? Password2 { get; set; }
    }
}