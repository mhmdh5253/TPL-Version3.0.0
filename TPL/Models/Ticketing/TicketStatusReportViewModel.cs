namespace TPLWeb.Models.Ticketing
{
    public class TicketStatusReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Dictionary<string, int>? StatusCounts { get; set; }
    }
}
