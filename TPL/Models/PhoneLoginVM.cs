using System.ComponentModel.DataAnnotations;

namespace TPLWeb.Models
{
    public class PhoneLoginVM
    {
        [Required(ErrorMessage = "شماره تلفن الزامی است")]
        [Display(Name = "شماره تلفن")]
        [Phone(ErrorMessage = "فرمت شماره تلفن صحیح نیست")]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
