using _1.PresentationLayer.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ApplicationLayer_ServiceLayer_.Authentication.AuthService.Interface;
using ApplicationLayer_ServiceLayer_.Authentication.AuthService;
using Presentation.Models;
using System.Diagnostics;
using CrossCuttingConcerns.FormDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace _1.PresentationLayer.Controllers
{
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthService _authService;

        public AuthController(ILogger<AuthController> logger, IAuthService authService)
        {
            _logger = logger;
            _authService = authService;
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

            return RedirectToAction("Index", "Projects");
        }



        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(e => e.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                return BadRequest(errors); // skickar JSON med valideringsfel
            }

            var form = new RegisterForm
            {
                Email = model.Email,
                Password = model.Password,
                ConfirmPassword = model.ConfirmPassword,
                UserName = model.UserName,
                PhoneNumber = model.PhoneNumber
            };

            var result = await _authService.RegisterUserAsync(form);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToArray();
                return BadRequest(new { generalErrors = errors });
            }

            return Ok(); // 200 → JS kan redirecta
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return RedirectToAction("Index", "Login");
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
