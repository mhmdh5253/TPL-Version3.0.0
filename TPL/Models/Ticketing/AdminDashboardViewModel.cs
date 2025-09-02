using BE.Ticketing.SupportTicketSystem.Models;

namespace TPLWeb.Models.Ticketing
{
    public class AdminDashboardViewModel
    {
        public List<Ticket>? TotalTickets { get; set; }
        public List<Category>? Categories { get; set; }
        public List<Ticket>? RecentTickets { get; set; }
    }
}