// CreateChatRoomDto.cs (No major changes needed, kept for consistency)
using System.ComponentModel.DataAnnotations;

namespace BE.Chat.DTOs
{
    public class CreateChatRoomDto
    {
        [Required(ErrorMessage = "نام اتاق چت الزامی است")]
        [StringLength(450, ErrorMessage = "نام اتاق چت نمی‌تواند بیشتر از 450 کاراکتر باشد")]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(1000, ErrorMessage = "توضیحات نمی‌تواند بیشتر از 1000 کاراکتر باشد")]
        public string? Description { get; set; }
        
        public bool IsGroup { get; set; }
        
        [Required(ErrorMessage = "لیست شرکت‌کنندگان الزامی است")]
        public List<string> ParticipantUserIds { get; set; } = new List<string>();
    }
}