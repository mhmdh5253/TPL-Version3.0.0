using BE.Ticketing.SupportTicketSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Ticketing
{
    public interface INotificationService
    {
        /// <summary>
        /// ارسال اعلان جدید
        /// </summary>
        /// <param name="userId">شناسه کاربر دریافت کننده</param>
        /// <param name="message">پیام اعلان</param>
        /// <param name="link">لینک مرتبط (اختیاری)</param>
        /// <returns>نتیجه عملیات (true/false)</returns>
        Task<bool> SendNotification(string userId, string message, string link = null);

        /// <summary>
        /// دریافت اعلان‌های کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>لیست اعلان‌ها</returns>
        Task<List<Notification>> GetUserNotifications(string userId);

        /// <summary>
        /// علامت گذاری اعلان به عنوان خوانده شده
        /// </summary>
        /// <param name="notificationId">شناسه اعلان</param>
        /// <returns>نتیجه عملیات (true/false)</returns>
        Task<bool> MarkAsRead(string notificationId);

        /// <summary>
        /// حذف اعلان
        /// </summary>
        /// <param name="notificationId">شناسه اعلان</param>
        /// <returns>نتیجه عملیات (true/false)</returns>
        Task<bool> DeleteNotification(string notificationId);
    }
    public class DlNotification : INotificationService
    {
        private readonly Db _context;

        public DlNotification(Db context)
        {
            _context = context;
        }

        public async Task<bool> SendNotification(string userId, string message, string link = null)
        {
            try
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Message = message,
                    Link = link,
                    CreatedDate = DateTime.Now
                };

                await _context.Notifications.AddAsync(notification);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Notification>> GetUserNotifications(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }

        public async Task<bool> MarkAsRead(string notificationId)
        {
            try
            {
                var notification = await _context.Notifications.FindAsync(int.Parse(notificationId));
                if (notification == null) return false;

                notification.IsRead = true;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteNotification(string notificationId)
        {
            try
            {
                var notification = await _context.Notifications.FindAsync(int.Parse(notificationId));
                if (notification == null) return false;

                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
