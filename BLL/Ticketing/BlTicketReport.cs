using BE.Ticketing.SupportTicketSystem.Models;
using DAL.Ticketing;

namespace BLL.Ticketing
{
    public class BlTicketReport : ITicketReport
    {
        private readonly DlTicketReport _ticketReport;
        public BlTicketReport(DlTicketReport ticketReport)
        {
            _ticketReport = ticketReport;
        }
        public async Task<Report> GenerateStatusReport(DateTime startDate, DateTime endDate, string userid)
        {
            return await _ticketReport.GenerateStatusReport(startDate, endDate, userid);
        }

        public async Task<Report> GenerateCategoryReport(DateTime startDate, DateTime endDate, string userid)
        {
            return await _ticketReport.GenerateCategoryReport(startDate, endDate, userid);
        }

        public async Task<List<Report>> GetReportHistory()
        {
            return await _ticketReport.GetReportHistory();
        }

        public async Task<byte[]> DownloadReport(string reportId)
        {
            return await _ticketReport.DownloadReport(reportId);
        }

        public async Task<Report> GetReportById(int Id)
        {
            return await _ticketReport.GetReportById(Id);
        }
    }
}
