// ChatRoomDto.cs (Updated to include pinned messages)
using BE.Chat;

namespace BE.Chat.DTOs
{
    public class ChatRoomDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime LastActivityDate { get; set; }
        public bool IsGroup { get; set; }
        public int UnreadCount { get; set; }
        public string LastMessage { get; set; } = string.Empty;
        public DateTime LastMessageDate { get; set; }
        public string LastMessageSenderName { get; set; } = string.Empty;
        public List<ChatParticipantDto> Participants { get; set; } = new List<ChatParticipantDto>();
        public List<int> PinnedMessageIds { get; set; } = new List<int>(); // Added for pinned messages
    }

    public class ChatParticipantDto
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public DateTime LastSeenDate { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsMuted { get; set; }
    }
}