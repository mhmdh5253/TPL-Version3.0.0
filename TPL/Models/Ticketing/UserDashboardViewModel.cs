using BE.Ticketing.SupportTicketSystem.Models;

namespace TPLWeb.Models.Ticketing
{
    public class UserDashboardViewModel
    {
        public List<Ticket>? MyTickets { get; set; }
        public List<Notification>? Notifications { get; set; }
    }
}
