// UserSearchDto.cs (No changes needed, kept for consistency)
namespace BE.Chat.DTOs
{
    public class UserSearchDto
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public bool IsInContacts { get; set; }
    }
}