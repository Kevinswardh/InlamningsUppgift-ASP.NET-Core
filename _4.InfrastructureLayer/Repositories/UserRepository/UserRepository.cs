using DomainLayer_BusinessLogicLayer_.Entities;
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

        public UserRepository(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<List<User>> GetUsersByRoleAsync(string roleName)
        {
            var identityUsers = await _userManager.GetUsersInRoleAsync(roleName);

            return identityUsers.Select(u => new User
            {
                Id = u.Id,
                Email = u.Email,
                UserName = u.UserName,
                PhoneNumber = u.PhoneNumber,
                Position = u.Position,
                Role = roleName
            }).ToList();
        }

        public async Task<User> GetByIdAsync(string id)
        {
            var identityUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (identityUser == null) return null;

            var roles = await _userManager.GetRolesAsync(identityUser);
            return new User
            {
                Id = identityUser.Id,
                Email = identityUser.Email,
                UserName = identityUser.UserName,
                PhoneNumber = identityUser.PhoneNumber,
                Position = identityUser.Position,
                Role = roles.FirstOrDefault() ?? "User"
            };
        }

        public async Task UpdateUserAsync(User user)
        {
            var identityUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            if (identityUser == null) return;

            // Uppdatera fält
            identityUser.UserName = user.UserName;
            identityUser.Email = user.Email;
            identityUser.PhoneNumber = user.PhoneNumber;
            identityUser.Position = user.Position;

            // Hämta aktuell roll
            var currentRoles = await _userManager.GetRolesAsync(identityUser);
            var currentRole = currentRoles.FirstOrDefault();

            // Om rollen har ändrats, byt roll
            if (!string.IsNullOrEmpty(user.Role) && user.Role != currentRole)
            {
                if (currentRoles.Any())
                    await _userManager.RemoveFromRolesAsync(identityUser, currentRoles);

                await _userManager.AddToRoleAsync(identityUser, user.Role);
            }

            await _userManager.UpdateAsync(identityUser);
        }


        public async Task DeleteUserAsync(string id)
        {
            var identityUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (identityUser != null)
            {
                await _userManager.DeleteAsync(identityUser);
            }
        }

        public async Task<bool> IsInRoleAsync(User user, string role)
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
    }
}
