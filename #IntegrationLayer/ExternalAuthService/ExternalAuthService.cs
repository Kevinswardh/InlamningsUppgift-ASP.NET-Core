using _IntegrationLayer.ExternalAuthService.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using SecurityLayer.Identity;
using SecurityLayer.SecurityServices.SecurityAuthService.Interface;
using System.Security.Claims;
using System.Threading.Tasks;

namespace _IntegrationLayer.ExternalAuthService
{
    public class ExternalAuthService : IExternalAuthService
    {
        private readonly ISecurityAuthService _securityAuthService;

        public ExternalAuthService(ISecurityAuthService securityAuthService)
        {
            _securityAuthService = securityAuthService;
        }

        // Retrieves external login info (login provider and provider key)
        public async Task<ExternalLoginInfo?> GetExternalLoginInfoAsync()
        {
            return await _securityAuthService.GetExternalLoginInfoAsync();
        }

        // Signs in the user with the external login provider info
        public async Task<SignInResult> ExternalLoginSignInAsync(string loginProvider, string providerKey)
        {
            return await _securityAuthService.ExternalLoginSignInAsync(loginProvider, providerKey);
        }

        // Retrieves external user data (email and picture URL) but does not update the user
        public async Task<(string email, string? pictureUrl)> GetExternalUserDataAsync(ExternalLoginInfo info)
        {
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var pictureUrl = info.Principal.FindFirstValue("picture");
            return (email, pictureUrl);
        }

        // Gets the external login provider for a specific user
        public async Task<string?> GetExternalLoginProviderAsync(string userId)
        {
            return await _securityAuthService.GetExternalLoginProviderAsync(userId);
        }

        // Sets external data like the profile picture URL, but doesn't update the user model
        public async Task SetExternalDataAsync(ExternalLoginInfo info, ApplicationUser? userOverride = null)
        {
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return;

            var user = userOverride ?? await _securityAuthService.FindByEmailAsync(email);
            if (user == null) return;

            var pictureUrl = info.Principal.FindFirstValue("picture");
            if (!string.IsNullOrEmpty(pictureUrl) && string.IsNullOrEmpty(user.ExternalImageUrl))
            {
                user.ExternalImageUrl = pictureUrl;
          
            }
        }
        public AuthenticationProperties GetExternalLoginProperties(string provider, string? redirectUrl)
        {
            return _securityAuthService.ConfigureExternalAuthProperties(provider, redirectUrl);
        }
    }
}
