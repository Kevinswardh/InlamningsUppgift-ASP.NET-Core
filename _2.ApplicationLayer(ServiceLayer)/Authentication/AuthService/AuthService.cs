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
using ApplicationLayer_ServiceLayer_.UserManagment.UserService.Interface;

namespace ApplicationLayer_ServiceLayer_.Authentication.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepo;
        private readonly IUserStatusService _statusService;
        private readonly IExternalAuthService _externalAuthService;
        private readonly ISecurityAuthService _securityAuthService;
        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        public AuthService(
            IAuthRepository authRepo,
            IUserStatusService statusService,
            IExternalAuthService externalAuthService,
            ISecurityAuthService securityAuthService,
            IUserRepository userRepository,
            IUserService userService) 
        {
            _authRepo = authRepo;
            _statusService = statusService;
            _externalAuthService = externalAuthService;
            _securityAuthService = securityAuthService;
            _userRepository = userRepository;
            _userService = userService; 
        }


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

                // Hämta extra-info för claim
                var user = await _userRepository.GetByIdAsync(identityUser.Id);

                var claims = new List<Claim>
{
    new Claim(ClaimTypes.Name, identityUser.UserName),
    new Claim(ClaimTypes.Email, identityUser.Email),
    new Claim(ClaimTypes.NameIdentifier, identityUser.Id), // Lägger till användarens ID som ett claim
    new Claim("darkMode", user?.IsDarkModeEnabled.ToString().ToLower() ?? "false"),
    new Claim(ClaimTypes.Role, user?.Role ?? "User") // Lägg till användarens roll i claims
};



                await _securityAuthService.SignOutAsync(); // först utloggning om något redan finns
                await _securityAuthService.SignInAsync(identityUser, false, claims);
            }


            return result.Succeeded;
        }
        public async Task<IdentityResult> RegisterUserAsync(RegisterForm model)
        {
            var errors = new List<IdentityError>();

            // Kontrollera om e-post redan finns
            var existingEmailUser = await _securityAuthService.FindByEmailAsync(model.Email);
            if (existingEmailUser != null)
                errors.Add(new IdentityError { Code = nameof(model.Email), Description = "Email already exists" });

            // Kontrollera om användarnamn redan finns
            var existingUsernameUser = await _securityAuthService.FindByUserNameAsync(model.UserName);
            if (existingUsernameUser != null)
                errors.Add(new IdentityError { Code = nameof(model.UserName), Description = "Username is already taken" });

            if (errors.Any())
                return IdentityResult.Failed(errors.ToArray());

            var user = new UserEntity
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
                    Code = "", // generellt fel
                    Description = "Registration failed"
                });
            }

            return IdentityResult.Success;
        }



        public async Task LogoutAsync(string email)
        {
            if (!string.IsNullOrEmpty(email))
            {
                _statusService.SetOffline(email);
            }

            await _securityAuthService.SignOutAsync();
        }

        public async Task<SignInResult> ExternalLoginSignInAsync()
        {
            var info = await _externalAuthService.GetExternalLoginInfoAsync();
            if (info == null)
                return SignInResult.Failed;

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
                return SignInResult.Failed;

            var identityUser = await _securityAuthService.FindByEmailAsync(email);

            if (identityUser != null)
            {
                // Om användaren redan finns, uppdatera profilbild om den finns
                var pictureUrl = info.Principal.FindFirstValue("picture");
                if (!string.IsNullOrEmpty(pictureUrl))
                {
                    identityUser.ExternalImageUrl = pictureUrl;
                    await _userRepository.UpdateExternalImageUrlAsync(identityUser.Id, pictureUrl);
                }

                // Skapa claims och logga in användaren
                var userEntity = await _userRepository.GetByIdAsync(identityUser.Id);
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, identityUser.UserName),
            new Claim(ClaimTypes.Email, identityUser.Email),
            new Claim(ClaimTypes.NameIdentifier, identityUser.Id),
            new Claim("darkMode", userEntity?.IsDarkModeEnabled.ToString().ToLower() ?? "false"),
            new Claim(ClaimTypes.Role, userEntity?.Role ?? "User") // Lägg till användarens roll i claims
        };

                await _securityAuthService.SignInAsync(identityUser, isPersistent: false, claims);
                _statusService.SetOnline(identityUser.Email);
                return SignInResult.Success;
            }
            else
            {
                // Om användaren inte finns, skapa en ny användare
                var (createResult, newUser, externalInfo) = await CreateExternalUserAsync(info);
                if (createResult.Succeeded)
                {
                    var userEntity = await _userRepository.GetByIdAsync(newUser.Id);
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, newUser.UserName),
                new Claim(ClaimTypes.Email, newUser.Email),
                new Claim(ClaimTypes.NameIdentifier, newUser.Id),
                new Claim("darkMode", userEntity?.IsDarkModeEnabled.ToString().ToLower() ?? "false"),
                new Claim(ClaimTypes.Role, "User")  // Sätt rollen till "User" vid skapande
            };

                    await _securityAuthService.SignInAsync(newUser, isPersistent: false, claims);
                    _statusService.SetOnline(newUser.Email);
                    return SignInResult.Success;
                }
            }

            return SignInResult.Failed;
        }

        public async Task<(IdentityResult result, ApplicationUser user, ExternalLoginInfo info)> CreateExternalUserAsync(ExternalLoginInfo info)
        {
            if (info == null)
                return (IdentityResult.Failed(new IdentityError { Description = "No login info" }), null, null);

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
                return (IdentityResult.Failed(new IdentityError { Description = "Email not provided" }), null, info);

            // Skapa en ny användare
            var user = new UserEntity
            {
                Email = email,
                UserName = email,
                Position = "New Member",
                Role = "User"  // Vi sätter rollen till "User" här
            };

            // Skapa användaren i databasen
            var created = await _authRepo.CreateUserAsync(user, "DummyPassword123!");  // Du kan använda ett mer säkert lösenord här
            if (!created)
                return (IdentityResult.Failed(new IdentityError { Description = "Failed creating external user" }), null, info);

            var identityUser = await _securityAuthService.FindByEmailAsync(email);
            if (identityUser == null)
                return (IdentityResult.Failed(new IdentityError { Description = "User not found after creation" }), null, info);

            // Lägg till den externa login-informationen
            await _securityAuthService.AddLoginAsync(identityUser, info);
            // Lägg till användaren till rollen "User"
            await _securityAuthService.AddToRoleAsync(identityUser, "User");

            // Hämta den nyss skapade användaren för att kolla på DarkMode-inställning
            var userEntity = await _userRepository.GetByIdAsync(identityUser.Id);

            // Skapa en lista med claims för användaren
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, identityUser.Id),
        new Claim("darkMode", userEntity?.IsDarkModeEnabled.ToString().ToLower() ?? "false"),
        new Claim(ClaimTypes.Name, identityUser.UserName),
        new Claim(ClaimTypes.Email, identityUser.Email),
        new Claim(ClaimTypes.Role, "User")  // Lägg till rollen "User" som claim
    };

            // Logga in användaren
            await _securityAuthService.SignInAsync(identityUser, isPersistent: false, claims);
            _statusService.SetOnline(identityUser.Email);  // Sätt användaren som online i systemet

            return (IdentityResult.Success, identityUser, info);
        }



        public async Task<AuthenticationProperties> GetExternalLoginProperties(string provider, string? redirectUrl)
        {
            return _externalAuthService.GetExternalLoginProperties(provider, redirectUrl);
        }

        public async Task<string?> GetExternalLoginProviderAsync(string userId)
        {
            return await _externalAuthService.GetExternalLoginProviderAsync(userId);
        }
        public async Task<ExternalLoginInfo> GetExternalLoginInfoAsync()
        {
            // Hämtar information om den externa inloggningen
            var info = await _externalAuthService.GetExternalLoginInfoAsync();
            return info;
        }

    }
}
