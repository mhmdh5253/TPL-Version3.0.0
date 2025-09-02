using Microsoft.AspNetCore.Identity;

namespace BE
{
    public class ApplicationUserToken : IdentityUserToken<string>
    {
        public virtual ApplicationUser? User { get; set; }
    }
}