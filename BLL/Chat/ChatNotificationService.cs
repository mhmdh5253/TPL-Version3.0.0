using System;
using System.Threading.Tasks;
using BE.Chat;
using Microsoft.Extensions.Logging;
using BE.Chat.DTOs;

namespace BLL.Chat
{
    public class ChatNotificationService : IChatNotificationService
    {
        private readonly ILogger<ChatNotificationService> _logger;

        public ChatNotificationService(ILogger<ChatNotificationService> logger)
        {
            _logger = logger;
        }

        public Task<bool> SendMessageNotificationAsync(ChatMessageDto message, string recipientPhoneNumber)
        {
            try
            {
                _logger.LogInformation("Message notification would be sent for message {MessageId} to {PhoneNumber}", 
                    message.Id, recipientPhoneNumber);
                
                // SMS functionality removed - just log the action
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in message notification for message {MessageId} to {PhoneNumber}", 
                    message.Id, recipientPhoneNumber);
                return Task.FromResult(false);
            }
        }

        public Task<bool> SendChatInvitationAsync(string phoneNumber, string chatRoomName, string inviterName)
        {
            try
            {
                _logger.LogInformation("Chat invitation would be sent to {PhoneNumber} for chat {ChatRoom}", 
                    phoneNumber, chatRoomName);
                
                // SMS functionality removed - just log the action
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in chat invitation to {PhoneNumber} for chat {ChatRoom}", 
                    phoneNumber, chatRoomName);
                return Task.FromResult(false);
            }
        }

        public Task<bool> SendOtpForChatAccessAsync(string phoneNumber, string otpCode)
        {
            try
            {
                _logger.LogInformation("OTP would be sent to {PhoneNumber} for chat access", phoneNumber);
                
                // SMS functionality removed - just log the action
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OTP to {PhoneNumber} for chat access", phoneNumber);
                return Task.FromResult(false);
            }
        }
    }
}
