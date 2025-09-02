using System.ComponentModel.DataAnnotations;

namespace BE.Tokening
{
    public class Token
    {
        [Display(Name = "آی دی")]
        public int Id { get; set; }
        [Display(Name = "آی دی کاربر")]
        public string? UserId { get; set; }
        [Display(Name = "توکن")]
        public string? Value { get; set; }
        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }
        [Display(Name = "تاریخ انقضا")]
        public DateTime ExpiresAt { get; set; }
        [Display(Name = "وضعیت")]
        public bool IsActive { get; set; }

        [Display(Name = "توضیحات")]
        [MaxLength(250, ErrorMessage = "{0} نمی تواند بیشتر از {1} کاراکتر باشد .")]
        public string? Description { get; set; }

        // Navigation property
        public ApplicationUser? User { get; set; }
    }

    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public object Data { get; set; } = null!;
    }
}
