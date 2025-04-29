using CrossCuttingConcerns.FormDTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using SecurityLayer.Identity;
using SecurityLayer.SecurityServices.SecurityAuthService.Interface;
using _IntegrationLayer.ExternalAuthService.Interface;
using System.Security.Claims;
using System.Threading.Tasks;
using __Cross_cutting_Concerns.ServiceInterfaces;
using ApplicationLayer_ServiceLayer_.Authentication.AuthService.Interface;
using DomainLayer_BusinessLogicLayer_.DomainModel;
using DomainLayer_BusinessLogicLayer_.InfraInterfaces;
using _4.infrastructureLayer.Repositories.UserRepository;

namespace ApplicationLayer_ServiceLayer_.Authentication.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepo;
        private readonly IUserStatusService _statusService;
        private readonly IExternalAuthService _externalAuthService;
        private readonly ISecurityAuthService _securityAuthService;
        private readonly IUserRepository _userRepository;


        public AuthService(
      IAuthRepository authRepo,
      IUserStatusService statusService,
      IExternalAuthService externalAuthService,
      ISecurityAuthService securityAuthService,
      IUserRepository userRepository) // <-- LÄGG TILL DENNA
        {
            _authRepo = authRepo;
            _statusService = statusService;
            _externalAuthService = externalAuthService;
            _securityAuthService = securityAuthService;
            _userRepository = userRepository; // <-- OCH DENNA
        }



        // Internal login
        public async Task<bool> LoginAsync(LoginForm model)
        {
            var valid = await _authRepo.ValidatePasswordAsync(model.Email, model.Password);
            if (!valid) return false;

            var identityUser = await _securityAuthService.FindByEmailAsync(model.Email);
            if (identityUser == null) return false;

            var result = await _securityAuthService.PasswordSignInAsync(
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

        // Internal registration
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

            var identityUser = await _securityAuthService.FindByEmailAsync(user.Email);
            if (identityUser != null)
            {
                await _securityAuthService.SignInAsync(identityUser, isPersistent: false);
                _statusService.SetOnline(identityUser.Email);
            }

            return IdentityResult.Success;
        }

        // Logout
        public async Task LogoutAsync(string email)
        {
            if (!string.IsNullOrEmpty(email))
            {
                _statusService.SetOffline(email);
            }

            await _securityAuthService.SignOutAsync();
        }

        // External Login - SignInAsync
        public async Task<SignInResult> ExternalLoginSignInAsync()
        {
            var info = await _externalAuthService.GetExternalLoginInfoAsync();
            if (info == null)
                return SignInResult.Failed;

            var result = await _externalAuthService.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey);

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var pictureUrl = info.Principal.FindFirstValue("picture");

            if (string.IsNullOrEmpty(email))
                return SignInResult.Failed;

            var identityUser = await _securityAuthService.FindByEmailAsync(email);

            if (identityUser != null)
            {
                // Om användaren finns, uppdatera ExternalImageUrl och logga in
                if (!string.IsNullOrEmpty(pictureUrl))
                {
                    identityUser.ExternalImageUrl = pictureUrl;
                    await _userRepository.UpdateExternalImageUrlAsync(identityUser.Id, pictureUrl);
                }

                if (result.Succeeded)
                {
                    await _securityAuthService.SignInAsync(identityUser, isPersistent: false);
                    _statusService.SetOnline(identityUser.Email);
                    return SignInResult.Success;
                }
                else
                {
                    var existingLogins = await _securityAuthService.GetLoginsAsync(identityUser);
                    bool alreadyLinked = existingLogins.Any(l =>
                        l.LoginProvider == info.LoginProvider &&
                        l.ProviderKey == info.ProviderKey);

                    if (!alreadyLinked)
                        await _securityAuthService.AddLoginAsync(identityUser, info);

                    await _securityAuthService.SignInAsync(identityUser, isPersistent: false);
                    _statusService.SetOnline(identityUser.Email);
                    return SignInResult.Success;
                }
            }
            else
            {
                // Om användaren inte finns, skapa en ny användare
                var (createResult, newUser, externalInfo) = await CreateExternalUserAsync();

                // Kontrollera om användaren skapades korrekt
                if (createResult.Succeeded)
                {
                    await _securityAuthService.SignInAsync(newUser, isPersistent: false);
                    _statusService.SetOnline(newUser.Email);
                    return SignInResult.Success;
                }

                // Om användaren inte kunde skapas, returnera ett misslyckat resultat
                return SignInResult.Failed;
            }
        }


        // External user creation
        public async Task<(IdentityResult result, ApplicationUser user, ExternalLoginInfo info)> CreateExternalUserAsync()
        {
            var info = await _externalAuthService.GetExternalLoginInfoAsync();
            if (info == null)
                return (IdentityResult.Failed(new IdentityError { Description = "No login info" }), null, null);

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                return (IdentityResult.Failed(new IdentityError { Description = "Email not provided" }), null, info);
            }

            // Skapa användare utan lösenord för externa inloggningar
            var user = new User
            {
                Email = email,
                UserName = email, // Här använder vi e-post som användarnamn
                Position = "New Member",
                Role = "User"
            };

            // Skapa användaren, och använd ett dummylösenord (det spelar ingen roll för externa logins)
            var created = await _authRepo.CreateUserAsync(user, password: "DummyPassword123!"); // Använd ett dummy lösenord
            if (!created)
            {
                return (IdentityResult.Failed(new IdentityError { Description = "Failed creating external user" }), null, info);
            }

            // Nu hittar vi användaren via _securityAuthService
            var identityUser = await _securityAuthService.FindByEmailAsync(email);
            if (identityUser == null)
            {
                return (IdentityResult.Failed(new IdentityError { Description = "User not found after creation" }), null, info);
            }

            // Lägg till login (extern login)
            await _securityAuthService.AddLoginAsync(identityUser, info);

            // Lägg till rätt roll
            await _securityAuthService.AddToRoleAsync(identityUser, "User");

            // SignIn the user after successful external login
            await _securityAuthService.SignInAsync(identityUser, isPersistent: false);

            // Mark the user as online
            _statusService.SetOnline(identityUser.Email);

            return (IdentityResult.Success, identityUser, info);
        }


        public async Task<AuthenticationProperties> GetExternalLoginProperties(string provider, string? redirectUrl)
        {
            // Calls the ExternalAuthService to retrieve the authentication properties for the external login
            return _externalAuthService.GetExternalLoginProperties(provider, redirectUrl);
        }
        public async Task<string?> GetExternalLoginProviderAsync(string userId)
        {
            return await _externalAuthService.GetExternalLoginProviderAsync(userId);
        }


    }
}
