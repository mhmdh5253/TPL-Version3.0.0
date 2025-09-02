// SendMessageDto.cs (Updated to include forwarded message support)
using BE.Chat;

namespace BE.Chat.DTOs
{
    public class SendMessageDto
    {
        public int ChatRoomId { get; set; }
        public string Content { get; set; } = string.Empty;
        public MessageType MessageType { get; set; } = MessageType.Text;
        public int? ReplyToMessageId { get; set; }
        public string? ForwardedFrom { get; set; } // Added for forwarded messages
    }
}