using DomainLayer_BusinessLogicLayer_.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer_BusinessLogicLayer_.InfraInterfaces
{
    public interface IAuthRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> ValidatePasswordAsync(string email, string password);
        Task<bool> CreateUserAsync(User user, string password); // bool är neutral, vi vill inte exponera IdentityResult här
    }
}
