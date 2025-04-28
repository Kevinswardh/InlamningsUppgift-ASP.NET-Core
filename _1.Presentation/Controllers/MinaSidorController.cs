using _1.PresentationLayer.ViewModels.MembersViewModels;
using ApplicationLayer_ServiceLayer_.Authentication.AuthService.Interface;
using ApplicationLayer_ServiceLayer_.UserManagment.UserService.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace _1.PresentationLayer.Controllers
{
    [Authorize]
    public class MinaSidorController : Controller
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;

        public MinaSidorController(IUserService userService, IAuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var (image, name) = await _userService.GetUserProfileForLayoutAsync(User);
            ViewBag.ProfileImage = image;
            ViewBag.UserName = name;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return NotFound();

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null) return NotFound();

            // 🔍 Hämta login provider (via AuthService)
            var loginProvider = await _authService.GetExternalLoginProviderAsync(userId);

            var model = new MemberItemViewModel
            {
                Id = userId,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Position = user.Position,
                Role = user.Role,
                ImageUrl = user.ImageUrl,
                LoginProvider = loginProvider
            };

            return View(model);
        }


    }
}
