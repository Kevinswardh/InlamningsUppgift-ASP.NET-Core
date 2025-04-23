using DomainLayer_BusinessLogicLayer_.Entities;
using DomainLayer_BusinessLogicLayer_.InfraInterfaces;
using Microsoft.AspNetCore.Identity;
using SecurityLayer.Identity;
using System.Threading.Tasks;

namespace _4.infrastructureLayer.Repositories.AuthRepository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthRepository(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            var identityUser = await _userManager.FindByEmailAsync(email);
            if (identityUser == null) return null;

            return new User
            {
                Email = identityUser.Email,
                UserName = identityUser.UserName,
                PhoneNumber = identityUser.PhoneNumber,
                Position = identityUser.Position,
                Role = "User"
            };
        }

        public async Task<bool> ValidatePasswordAsync(string email, string password)
        {
            var identityUser = await _userManager.FindByEmailAsync(email);
            if (identityUser == null) return false;

            return await _userManager.CheckPasswordAsync(identityUser, password);
        }

        public async Task<bool> CreateUserAsync(User user, string password)
        {
            var identityUser = new ApplicationUser
            {
                Email = user.Email,
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                Position = user.Position
            };

            var result = await _userManager.CreateAsync(identityUser, password);
            if (!result.Succeeded) return false;

            await _userManager.AddToRoleAsync(identityUser, user.Role);

            return true;
        }

    }
}
