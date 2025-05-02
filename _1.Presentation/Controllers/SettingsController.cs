using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApplicationLayer_ServiceLayer_.UserManagment.UserService.Interface;
using SecurityLayer.SecurityServices.SecurityAuthService.Interface;
using System.Security.Claims;

namespace _1.PresentationLayer.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly IUserService _userService;
        private readonly ISecurityAuthService _securityAuthService;

        public SettingsController(IUserService userService, ISecurityAuthService securityAuthService)
        {
            _userService = userService;
            _securityAuthService = securityAuthService;
        }

        [HttpPost]
        public async Task<IActionResult> ToggleDarkMode(bool enable)
        {
            var userId = _userService.GetUserId(User);
            var user = await _userService.GetUserByIdAsync(userId);

            if (user != null)
            {
                // 1. Uppdatera i databasen
                user.IsDarkModeEnabled = enable;
                await _userService.UpdateUserAsync(user);

                // 2. Hämta ApplicationUser från IdentityDb
                var identityUser = await _securityAuthService.FindByEmailAsync(user.Email);

                if (identityUser != null)
                {
                    // 3. Logga ut gammal session
                    await _securityAuthService.SignOutAsync();

                    // 4. Skapa claims inkl. ID
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, identityUser.Id), // 🟢 viktig!
                new Claim("darkMode", enable.ToString().ToLower())
            };

                    // 5. Logga in med nya claims
                    await _securityAuthService.SignInAsync(identityUser, isPersistent: false, claims);
                }
            }

            return Redirect(Request.Headers["Referer"].ToString());
        }

    }
}
