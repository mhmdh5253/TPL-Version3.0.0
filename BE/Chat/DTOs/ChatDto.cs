// ChatDto.cs (اصلاح شده)
using BE.Chat;

namespace BE.Chat.DTOs
{
    public class ChatDto
    {
        public int ChatRoomId { get; set; }
        public string ChatRoomName { get; set; } = string.Empty;
        public bool IsGroup { get; set; }
        public List<ChatMessageDto> Messages { get; set; } = new List<ChatMessageDto>();
        public List<ChatParticipantDto> Participants { get; set; } = new List<ChatParticipantDto>();
    }
}