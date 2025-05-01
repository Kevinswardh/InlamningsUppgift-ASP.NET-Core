using DomainLayer_BusinessLogicLayer_.DomainModel;
using System.Threading.Tasks;

namespace DomainLayer_BusinessLogicLayer_.InfraInterfaces
{
    public interface IAuthRepository
    {
        Task<UserEntity?> GetUserByEmailAsync(string email);
        Task<bool> ValidatePasswordAsync(string email, string password);
        Task<bool> CreateUserAsync(UserEntity user, string password); // bool är neutral, vi vill inte exponera IdentityResult här
    }
}
