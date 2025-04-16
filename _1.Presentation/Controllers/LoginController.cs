using _1.PresentationLayer.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ApplicationLayer_ServiceLayer_.Authentication.AuthService.Interface;
using ApplicationLayer_ServiceLayer_.Authentication.AuthService;
using Presentation.Models;
using System.Diagnostics;
using CrossCuttingConcerns.FormDTOs;
using Microsoft.AspNetCore.Authorization;

namespace _1.PresentationLayer.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        private readonly ILogger<LoginController> _logger;
        private readonly IAuthService _authService;

        public LoginController(ILogger<LoginController> logger, IAuthService authService)
        {
            _logger = logger;
            _authService = authService;
        }

        public IActionResult Index()
        {
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
