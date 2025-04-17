using DomainLayer_BusinessLogicLayer_.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DomainLayer_BusinessLogicLayer_.InfraInterfaces
{
    public interface IUserRepository
    {
        Task<List<User>> GetUsersByRoleAsync(string roleName);
    }
}
