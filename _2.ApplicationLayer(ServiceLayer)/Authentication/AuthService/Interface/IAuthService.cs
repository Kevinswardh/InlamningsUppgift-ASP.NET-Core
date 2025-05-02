using CrossCuttingConcerns.FormDTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using SecurityLayer.Identity;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ApplicationLayer_ServiceLayer_.Authentication.AuthService.Interface
{
    public interface IAuthService
    {
        // Internal authentication
        Task<bool> LoginAsync(LoginForm model);
        Task<IdentityResult> RegisterUserAsync(RegisterForm model);
        Task LogoutAsync(string email);

        // External authentication
        Task<SignInResult> ExternalLoginSignInAsync();
        Task<(IdentityResult result, ApplicationUser user, ExternalLoginInfo info)> CreateExternalUserAsync(ExternalLoginInfo info);

        // Add the new method for external login properties
        Task<AuthenticationProperties> GetExternalLoginProperties(string provider, string? redirectUrl);
        Task<string?> GetExternalLoginProviderAsync(string userId);

        Task<ExternalLoginInfo> GetExternalLoginInfoAsync();
        
     }
}
