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
                user.IsDarkModeEnabled = enable;
                await _userService.UpdateUserAsync(user);

                var identityUser = await _securityAuthService.FindByEmailAsync(user.Email);

                if (identityUser != null)
                {
                    await _securityAuthService.SignOutAsync();

                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, identityUser.Id),
                new Claim("darkMode", enable.ToString().ToLower()),
                new Claim(ClaimTypes.Name, identityUser.UserName),
                new Claim(ClaimTypes.Email, identityUser.Email),
                new Claim(ClaimTypes.Role, user.Role ?? "User")
            };

                    await _securityAuthService.SignInAsync(identityUser, isPersistent: false, claims);
                }
            }

            return Redirect(Request.Headers["Referer"].ToString());
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult SaveCookiePreferences(string consent, bool functional = false, bool analytics = false, bool marketing = false)
        {
            var values = new Dictionary<string, bool>
            {
                ["functional"] = consent == "all" || functional,
                ["analytics"] = consent == "all" || analytics,
                ["marketing"] = consent == "all" || marketing
            };

            var cookieValue = System.Text.Json.JsonSerializer.Serialize(values);
            var options = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true
            };

            Response.Cookies.Append("CookiePreferences", cookieValue, options);
            return Redirect(Request.Headers["Referer"].ToString());
        }

    }
}
