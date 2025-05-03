using _1.PresentationLayer.ViewModels;
using ApplicationLayer_ServiceLayer_.Authentication.AuthService.Interface;
using CrossCuttingConcerns.FormDTOs;
using Microsoft.AspNetCore.Mvc;

namespace _1.PresentationLayer.Controllers
{
    public class RegisterController : Controller
    {
        private readonly IAuthService _authService;

        public RegisterController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Index", model); // 🟢 visa formuläret igen med fel

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
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code ?? string.Empty, error.Description);
                }

                return View("Index", model); // 🟥 återgå med felmeddelanden
            }

            // ✅ Registrering lyckades
            return RedirectToAction("Index", "Login");
        }
    }
}
