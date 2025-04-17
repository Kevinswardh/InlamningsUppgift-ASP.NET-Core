using ApplicationLayer_ServiceLayer_.Authentication.AuthService.Interface;
using CrossCuttingConcerns.FormDTOs;
using DomainLayer_BusinessLogicLayer_.Entities;
using DomainLayer_BusinessLogicLayer_.InfraInterfaces;
using Microsoft.AspNetCore.Identity;
using SecurityLayer.Identity;
using __Cross_cutting_Concerns.ServiceInterfaces;



namespace ApplicationLayer_ServiceLayer_.Authentication.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepo;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStatusService _statusService;

        public AuthService(
            IAuthRepository authRepo,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IUserStatusService statusService)
        {
            _authRepo = authRepo;
            _signInManager = signInManager;
            _userManager = userManager;
            _statusService = statusService;
        }

        public async Task<bool> LoginAsync(LoginForm model)
        {
            var valid = await _authRepo.ValidatePasswordAsync(model.Email, model.Password);
            if (!valid) return false;

            var identityUser = await _userManager.FindByEmailAsync(model.Email);
            if (identityUser == null) return false;

            var result = await _signInManager.PasswordSignInAsync(
                identityUser,
                model.Password,
                isPersistent: false,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _statusService.SetOnline(identityUser.Email);
            }

            return result.Succeeded;
        }

        public async Task<IdentityResult> RegisterUserAsync(RegisterForm model)
        {
            var user = new User
            {
                Email = model.Email,
                UserName = model.UserName,
                PhoneNumber = model.PhoneNumber,
                Position = "New Member",
                Role = "User"
            };

            var created = await _authRepo.CreateUserAsync(user, model.Password);
            if (!created)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = "Registration failed"
                });
            }

            var identityUser = await _userManager.FindByEmailAsync(user.Email);
            if (identityUser != null)
            {
                await _signInManager.SignInAsync(identityUser, isPersistent: false);
                _statusService.SetOnline(identityUser.Email); // ← sätt online
            }

            return IdentityResult.Success;
        }
        public async Task LogoutAsync(string email)
        {
            if (!string.IsNullOrEmpty(email))
            {
                _statusService.SetOffline(email);
            }

            await _signInManager.SignOutAsync();
        }

    }
}
