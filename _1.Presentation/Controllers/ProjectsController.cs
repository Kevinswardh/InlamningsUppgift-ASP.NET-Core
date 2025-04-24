using Microsoft.AspNetCore.Mvc;
using Presentation.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using ApplicationLayer_ServiceLayer_.UserManagment.UserService.Interface;


namespace Presentation.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly ILogger<ProjectsController> _logger;
        private readonly IUserService _userService;

        public ProjectsController(ILogger<ProjectsController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var (image, name) = await _userService.GetUserProfileForLayoutAsync(User);
            ViewBag.ProfileImage = image;
            ViewBag.UserName = name;

            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
