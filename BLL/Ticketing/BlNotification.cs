using BE.Ticketing.SupportTicketSystem.Models;
using DAL.Ticketing;

namespace BLL.Ticketing
{
    public class BlNotification : INotificationService
    {
        private readonly DlNotification _notification;
        public BlNotification(DlNotification notification)
        {
            _notification = notification;
        }
        public async Task<bool> SendNotification(string userId, string message, string link = null!)
        {
            return await _notification.SendNotification(userId, message, link);
        }

        public async Task<List<Notification>> GetUserNotifications(string userId)
        {
            return await _notification.GetUserNotifications(userId);
        }

        public async Task<bool> MarkAsRead(string notificationId)
        {
            return await _notification.MarkAsRead(notificationId);
        }

        public async Task<bool> DeleteNotification(string notificationId)
        {
            return await _notification.DeleteNotification(notificationId);
        }
    }
}
