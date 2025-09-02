using System.ComponentModel.DataAnnotations;

namespace TPLWeb.Models.Ticketing
{
    public class NotificationViewModel
    {
        public int Id { get; set; }
        public string? Message { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsRead { get; set; }
        public string? Link { get; set; }
    }
    public class CategoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "نام دسته‌بندی الزامی است")]
        [StringLength(50, ErrorMessage = "نام دسته‌بندی نمی‌تواند بیش از 50 کاراکتر باشد")]
        public string? Name { get; set; }

        [StringLength(200, ErrorMessage = "توضیحات نمی‌تواند بیش از 200 کاراکتر باشد")]
        public string? Description { get; set; }
    }
}
