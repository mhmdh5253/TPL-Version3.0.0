using System.Threading.Tasks;
using BE.Chat.DTOs;

namespace BLL.Chat
{
    public interface IChatNotificationService
    {
        Task<bool> SendMessageNotificationAsync(ChatMessageDto message, string recipientPhoneNumber);
        Task<bool> SendChatInvitationAsync(string phoneNumber, string chatRoomName, string inviterName);
        Task<bool> SendOtpForChatAccessAsync(string phoneNumber, string otpCode);
    }
}

