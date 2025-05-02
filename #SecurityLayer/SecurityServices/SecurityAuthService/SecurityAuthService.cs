using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using SecurityLayer.Identity;
using SecurityLayer.SecurityServices.SecurityAuthService.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SecurityLayer.SecurityServices.SecurityAuthService
{
    public class SecurityAuthService : ISecurityAuthService
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public SecurityAuthService(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public async Task<ApplicationUser?> FindByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<SignInResult> PasswordSignInAsync(ApplicationUser user, string password, bool isPersistent, bool lockoutOnFailure)
        {
            return await _signInManager.PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);
        }

        public async Task SignInAsync(ApplicationUser user, bool isPersistent, IEnumerable<Claim> claims)
        {
            var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
            var principal = new ClaimsPrincipal(identity);
            await _signInManager.SignInAsync(user, isPersistent, authenticationMethod: null);
            await _signInManager.Context.SignInAsync(IdentityConstants.ApplicationScheme, principal);
        }




        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public AuthenticationProperties ConfigureExternalAuthProperties(string provider, string? redirectUrl)
        {
            return _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        }

        public async Task AddLoginAsync(ApplicationUser user, ExternalLoginInfo info)
        {
            await _userManager.AddLoginAsync(user, info);
        }

        public async Task AddToRoleAsync(ApplicationUser user, string role)
        {
            await _userManager.AddToRoleAsync(user, role);
        }

        public async Task<IList<UserLoginInfo>> GetLoginsAsync(ApplicationUser user)
        {
            return await _userManager.GetLoginsAsync(user);
        }

        public async Task<ExternalLoginInfo?> GetExternalLoginInfoAsync()
        {
            return await _signInManager.GetExternalLoginInfoAsync();
        }

        public async Task<SignInResult> ExternalLoginSignInAsync(string loginProvider, string providerKey)
        {
            return await _signInManager.ExternalLoginSignInAsync(loginProvider, providerKey, isPersistent: false);
        }

        public async Task UpdateUserAsync(ApplicationUser user)
        {
            await _userManager.UpdateAsync(user);
        }

        public async Task<string?> GetExternalLoginProviderAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var logins = await _userManager.GetLoginsAsync(user);
            return logins.FirstOrDefault()?.LoginProvider;
        }
    }
}
