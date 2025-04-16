using Microsoft.AspNetCore.Identity;

namespace SecurityLayer.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string Position { get; set; }
    }

}
