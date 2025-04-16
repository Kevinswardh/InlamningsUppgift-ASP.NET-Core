using _5.DataAccessLayer_DAL_.Repositories.UserRepository.Interface;
using ApplicationLayer_ServiceLayer_.Authentication.AuthService.Interface;
using CrossCuttingConcerns.FormDTOs;
using Microsoft.AspNetCore.Identity;
using SecurityLayer.Identity;
using SecurityLayer.IdentityFactory;
using System.Threading.Tasks;

namespace ApplicationLayer_ServiceLayer_.Authentication.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthService(IUserRepository userRepo, SignInManager<ApplicationUser> signInManager)
        {
            _userRepo = userRepo;
            _signInManager = signInManager;
        }

        public async Task<bool> LoginAsync(LoginForm model)
        {
            var valid = await _userRepo.ValidatePasswordAsync(model.Email, model.Password);
            if (!valid) return false;

            var user = await _signInManager.UserManager.FindByEmailAsync(model.Email);
            if (user == null) return false;

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
            return result.Succeeded;
        }

        public async Task<IdentityResult> RegisterUserAsync(RegisterForm model)
        {
            var user = ApplicationUserFactory.CreateFromRegisterForm(model);

            var result = await _signInManager.UserManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
            }

            return result;
        }
    }
}
