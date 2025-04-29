using DomainLayer_BusinessLogicLayer_.DomainModel;
using DomainLayer_BusinessLogicLayer_.InfraInterfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SecurityLayer.Identity;
using System.Threading.Tasks;

namespace _4.infrastructureLayer.Repositories.AuthRepository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IdentityDbContext _dbContext;

        public AuthRepository(UserManager<ApplicationUser> userManager, IdentityDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
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
                Role = "User" // ev. hämta riktig roll via _userManager.GetRolesAsync om det behövs
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
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var identityUser = new ApplicationUser
                {
                    Email = user.Email,
                    UserName = user.UserName,
                    PhoneNumber = user.PhoneNumber,
                    Position = user.Position
                };

                var result = await _userManager.CreateAsync(identityUser, password);
                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                await _userManager.AddToRoleAsync(identityUser, user.Role);
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
