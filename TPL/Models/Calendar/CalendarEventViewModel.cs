using System.ComponentModel.DataAnnotations;
using BE.Calendar;

namespace TPLWeb.Models.Calendar
{
    public class CalendarEventViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "عنوان رویداد الزامی است")]
        [StringLength(200, ErrorMessage = "عنوان نمی‌تواند بیشتر از 200 کاراکتر باشد")]
        [Display(Name = "عنوان")]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(1000, ErrorMessage = "توضیحات نمی‌تواند بیشتر از 1000 کاراکتر باشد")]
        [Display(Name = "توضیحات")]
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "تاریخ شروع الزامی است")]
        [Display(Name = "تاریخ شروع")]
        public DateTime StartDate { get; set; }
        
        [Required(ErrorMessage = "تاریخ پایان الزامی است")]
        [Display(Name = "تاریخ پایان")]
        public DateTime EndDate { get; set; }
        
        [Display(Name = "تمام روز")]
        public bool IsAllDay { get; set; }
        
        [Display(Name = "رنگ")]
        public string Color { get; set; } = "#3788d8";
        
        [Required(ErrorMessage = "نوع رویداد الزامی است")]
        [Display(Name = "نوع رویداد")]
        public EventType EventType { get; set; }
        
        [Required(ErrorMessage = "میزان نمایش الزامی است")]
        [Display(Name = "میزان نمایش")]
        public EventVisibility Visibility { get; set; }
        
        [Display(Name = "مکان")]
        public string? Location { get; set; }
        
        [Display(Name = "اولویت")]
        public string Priority { get; set; } = "Normal";
        
        [Display(Name = "تکرار شونده")]
        public bool IsRecurring { get; set; }
        
        [Display(Name = "الگوی تکرار")]
        public string? RecurrencePattern { get; set; }
        
        [Display(Name = "دسته‌بندی")]
        public int? CategoryId { get; set; }
        
        [Display(Name = "اعلان مدیر")]
        public bool IsManagerAnnouncement { get; set; }

        [Display(Name = "انجام شده")]
        public bool IsCompleted { get; set; }
        
        // For display purposes
        public string? CategoryName { get; set; }
        public string? UserName { get; set; }
        public string? StartDateShamsi { get; set; }
        public string? EndDateShamsi { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
    }
    
    public class CalendarEventCreateViewModel : CalendarEventViewModel
    {
        [Display(Name = "شرکت‌کنندگان")]
        public List<string> ParticipantUserIds { get; set; } = new List<string>();
        
        [Display(Name = "یادآوری")]
        public List<ReminderType> ReminderTypes { get; set; } = new List<ReminderType>();
    }
    
    public class CalendarEventEditViewModel : CalendarEventViewModel
    {
        [Display(Name = "شرکت‌کنندگان")]
        public List<string> ParticipantUserIds { get; set; } = new List<string>();
        
        [Display(Name = "یادآوری")]
        public List<ReminderType> ReminderTypes { get; set; } = new List<ReminderType>();
    }
    
    public class CalendarEventListViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsAllDay { get; set; }
        public string Color { get; set; } = string.Empty;
        public EventType EventType { get; set; }
        public EventVisibility Visibility { get; set; }
        public bool IsManagerAnnouncement { get; set; }
        public string? CategoryName { get; set; }
        public string? UserName { get; set; }
        public string? Location { get; set; }
        public string? Priority { get; set; }
    }
}

