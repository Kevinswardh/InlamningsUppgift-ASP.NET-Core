using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using SecurityLayer.Identity;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SecurityLayer.SecurityServices.SecurityAuthService.Interface
{
    public interface ISecurityAuthService
    {
        // Authentication-related methods
        Task<ApplicationUser?> FindByEmailAsync(string email);
        Task<SignInResult> PasswordSignInAsync(ApplicationUser user, string password, bool isPersistent, bool lockoutOnFailure);
        Task SignInAsync(ApplicationUser user, bool isPersistent, IEnumerable<Claim> claims);

        Task SignOutAsync();

        // External authentication methods
        AuthenticationProperties ConfigureExternalAuthProperties(string provider, string? redirectUrl);
        Task<ExternalLoginInfo?> GetExternalLoginInfoAsync();
        Task<SignInResult> ExternalLoginSignInAsync(string loginProvider, string providerKey);

        // User login methods
        Task AddLoginAsync(ApplicationUser user, ExternalLoginInfo info);
        Task AddToRoleAsync(ApplicationUser user, string role);
        Task<IList<UserLoginInfo>> GetLoginsAsync(ApplicationUser user);
        Task<string?> GetExternalLoginProviderAsync(string userId);
        Task<ApplicationUser?> FindByUserNameAsync(string username);

    }
}
