using BE.Ticketing.SupportTicketSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace TPLWeb.Models.Ticketing
{
    public class TicketViewModel
    {
        public Ticket? Ticket { get; set; }
        public List<TicketComment>? Comments { get; set; }
        public List<Attachment>? Attachments { get; set; }
    }

    public class CreateTicketViewModel
    {
        [Required(ErrorMessage = "عنوان تیکت الزامی است")]
        [StringLength(100, ErrorMessage = "عنوان تیکت نمی‌تواند بیش از 100 کاراکتر باشد")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "توضیحات تیکت الزامی است")]
        public string? Description { get; set; }

        public Priority Priority { get; set; } = Priority.Medium;

        [Display(Name = "دسته‌بندی")]
        public int? CategoryId { get; set; }
    }
}
