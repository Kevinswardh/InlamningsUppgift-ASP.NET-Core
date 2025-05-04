using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApplicationLayer_ServiceLayer_.AdminPageManagment.Interface;
using Microsoft.AspNetCore.Identity;
using ApplicationLayer_ServiceLayer_.UserManagment.UserService.Interface;

namespace _1.PresentationLayer.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class AdminPageController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly IUserService _userService;

        public AdminPageController(IAdminService adminService, IUserService userService)
        {
            _adminService = adminService;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var (image, name) = await _userService.GetUserProfileForLayoutAsync(User);
            ViewBag.ProfileImage = image;
            ViewBag.UserName = name;
            var stats = await _adminService.GetAdminStatisticsAsync();
            return View(stats); // 🟢 Letar efter Views/AdminPage/Index.cshtml
        }

    }
}
