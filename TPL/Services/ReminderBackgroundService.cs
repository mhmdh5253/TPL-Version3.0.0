using DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using TPLWeb.Tools;

namespace TPLWeb.Services
{
    public class ReminderBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ReminderBackgroundService> _logger;

        public ReminderBackgroundService(IServiceScopeFactory scopeFactory, ILogger<ReminderBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ReminderBackgroundService started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<Db>();
                    var sms = scope.ServiceProvider.GetRequiredService<ISmsSender>();

                    var now = DateTime.Now;
                    var due = await db.CalendarReminders
                        .Include(r => r.User)
                        .Where(r => !r.IsSent && r.ReminderTime <= now)
                        .ToListAsync(stoppingToken);

                    if (due.Any())
                    {
                        var grouped = due.GroupBy(r => new { r.Message }).ToList();
                        foreach (var g in grouped)
                        {
                            var phones = g.Select(r => r.User.PhoneNumber)
                                .Where(p => !string.IsNullOrWhiteSpace(p))
                                .Select(p => p!)
                                .ToList();
                            if (phones.Any())
                            {
                                try { await sms.SendBulkSmsAsync(g.Key.Message ?? "یادآوری رویداد", phones); }
                                catch (Exception ex) { _logger.LogError(ex, "Error sending bulk SMS reminders"); }
                            }

                            foreach (var r in g)
                            {
                                r.IsSent = true;
                                r.SentAt = DateTime.Now;
                            }
                        }
                        await db.SaveChangesAsync(stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ReminderBackgroundService loop error");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
