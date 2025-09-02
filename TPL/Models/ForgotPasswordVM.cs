using System.ComponentModel.DataAnnotations;

namespace TPLWeb.Models
{
    public class ForgotPasswordVM
    {
        [Required][EmailAddress] public string? Email { get; set; }
    }
}