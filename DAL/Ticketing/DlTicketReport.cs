using BE.Ticketing.SupportTicketSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace DAL.Ticketing
{
    public interface ITicketReport
    {
        /// <summary>
        /// تولید گزارش وضعیت تیکت‌ها
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>مدل گزارش</returns>
        Task<Report> GenerateStatusReport(DateTime startDate, DateTime endDate, string userid);

        /// <summary>
        /// تولید گزارش دسته‌بندی تیکت‌ها
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>مدل گزارش</returns>
        Task<Report> GenerateCategoryReport(DateTime startDate, DateTime endDate, string userid);

        /// <summary>
        /// دریافت تاریخچه گزارش‌ها
        /// </summary>
        /// <returns>لیست گزارش‌های قبلی</returns>
        Task<List<Report>> GetReportHistory();

        /// <summary>
        /// دانلود گزارش
        /// </summary>
        /// <param name="reportId">شناسه گزارش</param>
        /// <returns>فایل گزارش</returns>
        Task<byte[]> DownloadReport(string reportId);

        Task<Report> GetReportById(int Id);
    }
    public class DlTicketReport : ITicketReport
    {
        private readonly Db _context;

        public DlTicketReport(Db context)
        {
            _context = context;
        }

        public async Task<Report> GenerateStatusReport(DateTime startDate, DateTime endDate, string userId)
        {
            var tickets = await _context.Tickets
                .Where(t => t.CreatedDate >= startDate && t.CreatedDate <= endDate)
                .ToListAsync();

            if (!tickets.Any())
            {
                // Return a report indicating no data rather than null
                return new Report
                {
                    Title = $"Status Report ({startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd})",
                    Description = "No tickets found in the selected date range",
                    GeneratedDate = DateTime.Now,
                    GeneratedByUserId = userId,
                    ReportData = "[]" // Empty JSON array
                };
            }

            var totalTickets = tickets.Count;
            var statusGroups = tickets
                .GroupBy(t => t.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count(),
                    Percentage = Math.Round((g.Count() / (double)totalTickets) * 100, 2)
                })
                .ToList();

            var report = new Report
            {
                Title = $"گزارش وضعیت ({startDate:yyyy/MM/dd} تا {endDate:yyyy/MM/dd})",
                Description = $"مجموع {totalTickets} تیکت بر اساس وضعیت",
                GeneratedDate = DateTime.Now,
                GeneratedByUserId = userId,
                ReportData = JsonSerializer.Serialize(statusGroups)
            };

            await _context.Reports.AddAsync(report);
            await _context.SaveChangesAsync();

            return report;
        }

        public async Task<Report> GenerateCategoryReport(DateTime startDate, DateTime endDate, string userid)
        {
            var tickets = await _context.Tickets
                .Include(t => t.Category)
                .Where(t => t.CreatedDate >= startDate && t.CreatedDate <= endDate)
                .ToListAsync();

            var categoryGroups = tickets
                .GroupBy(t => t.Category?.Name ?? "Uncategorized")
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToList();

            var report = new Report
            {
                Title = $"Category Report ({startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd})",
                Description = "Distribution of tickets by category",
                GeneratedDate = DateTime.Now,
                ReportData = System.Text.Json.JsonSerializer.Serialize(categoryGroups)
            };

            await _context.Reports.AddAsync(report);
            await _context.SaveChangesAsync();

            return report;
        }

        public async Task<List<Report>> GetReportHistory()
        {
            return await _context.Reports
                .OrderByDescending(r => r.GeneratedDate).Take(10).ToListAsync();
        }

        public async Task<byte[]> DownloadReport(string reportId)
        {
            var report = await _context.Reports.FindAsync(int.Parse(reportId));
            if (report == null) return null!;

            return Encoding.UTF8.GetBytes(report.ReportData);
        }


        public async Task<Report> GetReportById(int Id)
        {
            var rep = await _context.Reports.FirstOrDefaultAsync(t => t.Id >= Id);

            if (rep == null)
            {
                // Return a report indicating no data rather than null
                return new Report
                {
                    Title = $"Status Report",
                    Description = "No tickets found in the selected date range",
                    GeneratedDate = DateTime.Now,
                    GeneratedByUserId = "",
                    ReportData = "[]" // Empty JSON array
                };
            }
            return rep;
        }

    }
}
