using Microsoft.AspNetCore.Identity;

namespace SecurityLayer.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string Position { get; set; }
        public string? ImageUrl { get; set; } // path eller filnamn

    }

}
