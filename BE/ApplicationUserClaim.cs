using Microsoft.AspNetCore.Identity;

namespace BE
{
    public class ApplicationUserClaim : IdentityUserClaim<string>
    {
        public virtual ApplicationUser? User { get; set; }
    }
}