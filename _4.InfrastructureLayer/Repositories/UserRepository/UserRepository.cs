using DomainLayer_BusinessLogicLayer_.Entities;
using DomainLayer_BusinessLogicLayer_.InfraInterfaces;
using Microsoft.AspNetCore.Identity;
using SecurityLayer.Identity;

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
                Email = u.Email,
                UserName = u.UserName,
                PhoneNumber = u.PhoneNumber,
                Position = u.Position,
                Role = roleName,

            }).ToList();
        }
    }
}
