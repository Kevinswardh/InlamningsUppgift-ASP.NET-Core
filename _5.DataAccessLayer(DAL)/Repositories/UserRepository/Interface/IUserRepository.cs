using DomainLayer_BusinessLogicLayer_.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _5.DataAccessLayer_DAL_.Repositories.UserRepository.Interface
{
    public interface IUserRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> ValidatePasswordAsync(string email, string password);
    }
}
