using _1.PresentationLayer.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ApplicationLayer_ServiceLayer_.Authentication.AuthService.Interface;
using Presentation.Models;
using System.Diagnostics;
using CrossCuttingConcerns.FormDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using __Cross_cutting_Concerns.ServiceInterfaces;
using System.Security.Claims;
using SecurityLayer.Identity;
using ApplicationLayer_ServiceLayer_.UserManagment.UserService.Interface;

namespace _1.PresentationLayer.Controllers
{
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthService _authService;
        private readonly IUserStatusService _statusService;
        private readonly IUserService _userService;

        public AuthController(
            ILogger<AuthController> logger,
            IAuthService authService,
            IUserStatusService statusService,
            IUserService userService) // 👈 lägg till
        {
            _logger = logger;
            _authService = authService;
            _statusService = statusService;
            _userService = userService; // 👈 spara fältet
        }


        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Login/Index.cshtml", model);
            }

            var loginForm = new LoginForm
            {
                Email = model.Email,
                Password = model.Password
            };

            var result = await _authService.LoginAsync(loginForm);

            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View("~/Views/Login/Index.cshtml", model);
            }

            // ✅ Kolla användarens roll
            var user = await _userService.GetUserByEmailAsync(model.Email);
            if (user != null)
            {
                if (user.Role == "User") // vanliga användare
                {
                    return RedirectToAction("Index", "Managers");
                }
            }

            return RedirectToAction("Index", "Projects"); // andra roller
        }




        // External login method
        [HttpPost]
        public async Task<IActionResult> ExternalLogin(string provider, string? returnUrl = null)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Auth", new { returnUrl });

            // Await the method to get the AuthenticationProperties
            var properties = await _authService.GetExternalLoginProperties(provider, redirectUrl);

            // Pass the properties to Challenge() method
            return Challenge(properties, provider);
        }

        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"External provider error: {remoteError}");
                return RedirectToAction("Index", "Login");
            }

            var info = await _authService.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ModelState.AddModelError(string.Empty, "External login information is not available.");
                return RedirectToAction("Index", "Login");
            }

            var result = await _authService.ExternalLoginSignInAsync();
            if (result.Succeeded)
            {
                // Hämta roll för inloggad användare
                var roles = await _userService.GetUserRolesAsync(User);
                if (roles.Contains("User"))
                    return RedirectToAction("Index", "Managers");
                else
                    return RedirectToAction("Index", "Projects");
            }

            var createResult = await _authService.CreateExternalUserAsync(info);
            if (createResult.result.Succeeded)
            {
                var roles = await _userService.GetUserRolesAsync(User);
                if (roles.Contains("User"))
                    return RedirectToAction("Index", "Managers");
                else
                    return RedirectToAction("Index", "Projects");
            }

            foreach (var error in createResult.result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return RedirectToAction("Index", "Login");
        }





        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            var email = User.FindFirstValue(ClaimTypes.Email); // Viktigt: hämta korrekt claim

            await _authService.LogoutAsync(email); // <- ny metod via service

            return RedirectToAction("Index", "Login");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
