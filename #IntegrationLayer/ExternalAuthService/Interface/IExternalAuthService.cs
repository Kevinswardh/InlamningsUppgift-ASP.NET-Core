using _IntegrationLayer.ExternalAuthService.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using SecurityLayer.Identity;
using SecurityLayer.SecurityServices.SecurityAuthService.Interface;
using System.Security.Claims;
using System.Threading.Tasks;


namespace _IntegrationLayer.ExternalAuthService.Interface
{
    public interface IExternalAuthService
    {
        Task<ExternalLoginInfo?> GetExternalLoginInfoAsync();
        Task<SignInResult> ExternalLoginSignInAsync(string loginProvider, string providerKey);

        // Returns external user data without modifying the user in the database
        Task<(string email, string? pictureUrl)> GetExternalUserDataAsync(ExternalLoginInfo info);

        Task<string?> GetExternalLoginProviderAsync(string userId);

        // This method sets external user data (e.g., profile picture URL) but doesn't update the user model
        Task SetExternalDataAsync(ExternalLoginInfo info, ApplicationUser? userOverride = null);

        public AuthenticationProperties GetExternalLoginProperties(string provider, string? redirectUrl);
    }
}
