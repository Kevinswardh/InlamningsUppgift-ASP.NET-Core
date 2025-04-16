using _5.DataAccessLayer_DAL_.Repositories.UserRepository.Interface;
using DomainLayer_BusinessLogicLayer_.Entities;
using Microsoft.AspNetCore.Identity;
using SecurityLayer.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _5.DataAccessLayer_DAL_.Repositories.UserRepository
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserRepository(UserManager<ApplicationUser> userManager)
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
                Role = "User" // kan utökas senare
            };
        }

        public async Task<bool> ValidatePasswordAsync(string email, string password)
        {
            var identityUser = await _userManager.FindByEmailAsync(email);
            return identityUser != null && await _userManager.CheckPasswordAsync(identityUser, password);
        }
    }
}
