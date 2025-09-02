using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace TPLWeb.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        public async Task JoinChatRoom(string chatRoomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{chatRoomId}");
        }

        public async Task LeaveChatRoom(string chatRoomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chat_{chatRoomId}");
        }

        public async Task UserTyping(string chatRoomId, string userName)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await Clients.Group($"chat_{chatRoomId}").SendAsync("UserTyping", userId, userName, chatRoomId);
        }

        public async Task UserStoppedTyping(string chatRoomId)
        {
            await Clients.Group($"chat_{chatRoomId}").SendAsync("UserStoppedTyping", chatRoomId);
        }
        
        // Alias methods for frontend compatibility
        public async Task StartTyping(string chatRoomId, string userName)
        {
            await UserTyping(chatRoomId, userName);
        }
        
        public async Task StopTyping(string chatRoomId)
        {
            await UserStoppedTyping(chatRoomId);
        }

        public async Task SendMessageToGroup(string chatRoomId, object message)
        {
            await Clients.Group($"chat_{chatRoomId}").SendAsync("ReceiveMessage", message);
        }

        public async Task MessageEdited(string chatRoomId, int messageId, string newContent)
        {
            await Clients.Group($"chat_{chatRoomId}").SendAsync("MessageEdited", messageId, newContent);
        }

        public async Task MessageDeleted(string chatRoomId, int messageId)
        {
            await Clients.Group($"chat_{chatRoomId}").SendAsync("MessageDeleted", messageId);
        }

        public async Task MessageForwarded(string chatRoomId, object forwardedMessage)
        {
            await Clients.Group($"chat_{chatRoomId}").SendAsync("MessageForwarded", forwardedMessage);
        }

        public async Task MessagePinned(string chatRoomId, int messageId)
        {
            await Clients.Group($"chat_{chatRoomId}").SendAsync("MessagePinned", messageId);
        }

        public async Task MessageReplied(string chatRoomId, int messageId, int replyToMessageId)
        {
            await Clients.Group($"chat_{chatRoomId}").SendAsync("MessageReplied", messageId, replyToMessageId);
        }

        public async Task ParticipantAdded(string chatRoomId, string participantId, string participantName)
        {
            await Clients.Group($"chat_{chatRoomId}").SendAsync("ParticipantAdded", participantId, participantName);
        }

        public async Task ParticipantRemoved(string chatRoomId, string participantId)
        {
            await Clients.Group($"chat_{chatRoomId}").SendAsync("ParticipantRemoved", participantId);
        }

        public async Task ChatRoomUpdated(string chatRoomId, object chatRoomData)
        {
            await Clients.Group($"chat_{chatRoomId}").SendAsync("ChatRoomUpdated", chatRoomData);
        }

        public async Task UserOnline(string userId)
        {
            await Clients.All.SendAsync("UserOnline", userId);
        }

        public async Task UserOffline(string userId)
        {
            await Clients.All.SendAsync("UserOffline", userId);
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(userId))
            {
                await UserOnline(userId);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(userId))
            {
                await UserOffline(userId);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}