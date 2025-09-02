using BE;
using BE.LetterAutomation;

namespace TPLWeb.Models.ManageUser
{
    public class UsersListViewNodel
    {
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Ostan { get; set; }
        public ICollection<ApplicationUserRole>? UserRoles { get; set; }
        public Organization? Organization { get; set; }
    }
}