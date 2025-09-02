using BE.Calendar;

namespace BLL.Calendar
{
    public class CalendarDashboardViewModel
    {
        public IEnumerable<CalendarEvent> PersonalEvents { get; set; } = new List<CalendarEvent>();
        public IEnumerable<CalendarEvent> ManagerAnnouncements { get; set; } = new List<CalendarEvent>();
        public IEnumerable<CalendarEvent> TodayEvents { get; set; } = new List<CalendarEvent>();
        public IEnumerable<CalendarEvent> UpcomingEvents { get; set; } = new List<CalendarEvent>();
        public IEnumerable<CalendarEventCategory> Categories { get; set; } = new List<CalendarEventCategory>();
        public bool IsManager { get; set; }
        public string CurrentUserId { get; set; } = string.Empty;
        public string CurrentUserName { get; set; } = string.Empty;
        public DateTime CurrentDate { get; set; }
    }
}







