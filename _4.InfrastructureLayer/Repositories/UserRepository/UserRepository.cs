using DomainLayer_BusinessLogicLayer_.DomainModel;
using DomainLayer_BusinessLogicLayer_.InfraInterfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SecurityLayer.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _4.infrastructureLayer.Repositories.UserRepository
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IdentityDbContext _dbContext;

        public UserRepository(UserManager<ApplicationUser> userManager, IdentityDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        public async Task<List<UserEntity>> GetUsersByRoleAsync(string roleName)
        {
            var identityUsers = await _userManager.GetUsersInRoleAsync(roleName);

            return identityUsers.Select(u => new UserEntity
            {
                Id = u.Id,
                Email = u.Email,
                UserName = u.UserName,
                PhoneNumber = u.PhoneNumber,
                Position = u.Position,
                Role = roleName,
                ImageUrl = u.ImageUrl
            }).ToList();
        }

        public async Task<UserEntity> GetByIdAsync(string id)
        {
            var identityUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (identityUser == null) return null;

            var roles = await _userManager.GetRolesAsync(identityUser);
            return new UserEntity
            {
                Id = identityUser.Id,
                Email = identityUser.Email,
                UserName = identityUser.UserName,
                PhoneNumber = identityUser.PhoneNumber,
                Position = identityUser.Position,
                Role = roles.FirstOrDefault() ?? "User",
                ImageUrl = identityUser.ImageUrl,
                ExternalImageUrl = identityUser.ExternalImageUrl
            };
        }

        public async Task UpdateUserAsync(UserEntity user)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var identityUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
                if (identityUser == null) return;

                identityUser.UserName = user.UserName;
                identityUser.Email = user.Email;
                identityUser.PhoneNumber = user.PhoneNumber;
                identityUser.Position = user.Position;
                identityUser.ImageUrl = user.ImageUrl;
                identityUser.ExternalImageUrl = user.ExternalImageUrl;

                var currentRoles = await _userManager.GetRolesAsync(identityUser);
                var currentRole = currentRoles.FirstOrDefault();

                if (!string.IsNullOrEmpty(user.Role) && user.Role != currentRole)
                {
                    if (currentRoles.Any())
                        await _userManager.RemoveFromRolesAsync(identityUser, currentRoles);

                    await _userManager.AddToRoleAsync(identityUser, user.Role);
                }

                await _userManager.UpdateAsync(identityUser);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteUserAsync(string id)
        {
            var identityUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (identityUser != null)
            {
                await _userManager.DeleteAsync(identityUser);
            }
        }

        public async Task<bool> IsInRoleAsync(UserEntity user, string role)
        {
            var identityUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            if (identityUser == null) return false;

            return await _userManager.IsInRoleAsync(identityUser, role);
        }

        public async Task UpdateUserRoleAsync(string userId, string newRole)
        {
            var identityUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (identityUser == null) return;

            var currentRoles = await _userManager.GetRolesAsync(identityUser);
            await _userManager.RemoveFromRolesAsync(identityUser, currentRoles);
            await _userManager.AddToRoleAsync(identityUser, newRole);
        }

        public async Task UpdateExternalImageUrlAsync(string userId, string externalImageUrl)
        {
            var identityUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (identityUser != null)
            {
                identityUser.ExternalImageUrl = externalImageUrl;
                await _userManager.UpdateAsync(identityUser);
            }
        }

        public async Task<List<UserEntity>> GetAllUsersAsync()
        {
            var identityUsers = await _userManager.Users.ToListAsync();
            var result = new List<UserEntity>();

            foreach (var identityUser in identityUsers)
            {
                var roles = await _userManager.GetRolesAsync(identityUser);
                result.Add(new UserEntity
                {
                    Id = identityUser.Id,
                    UserName = identityUser.UserName,
                    Email = identityUser.Email,
                    PhoneNumber = identityUser.PhoneNumber,
                    Position = identityUser.Position,
                    Role = roles.FirstOrDefault() ?? "User",
                    ImageUrl = identityUser.ImageUrl,
                    ExternalImageUrl = identityUser.ExternalImageUrl
                });
            }

            return result;
        }

        public async Task<UserEntity?> GetUserByEmailAsync(string email)
        {
            var identityUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (identityUser == null) return null;

            var roles = await _userManager.GetRolesAsync(identityUser);
            var role = roles.FirstOrDefault() ?? "NoRole";

            return new UserEntity
            {
                Id = identityUser.Id,
                UserName = identityUser.UserName,
                Email = identityUser.Email,
                PhoneNumber = identityUser.PhoneNumber,
                Position = identityUser.Position,
                Role = role,
                ImageUrl = identityUser.ImageUrl
            };
        }
    }
}
