using Microsoft.AspNetCore.Identity;

namespace BE
{
    public class ApplicationUserLogin : IdentityUserLogin<string>
    {
        public virtual ApplicationUser? User { get; set; }
    }
}