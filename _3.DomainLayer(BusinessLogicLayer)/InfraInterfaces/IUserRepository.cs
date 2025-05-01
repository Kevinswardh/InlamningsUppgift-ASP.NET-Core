using DomainLayer_BusinessLogicLayer_.DomainModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DomainLayer_BusinessLogicLayer_.InfraInterfaces
{
    public interface IUserRepository
    {
        // Hämtar användare med viss roll
        Task<List<UserEntity>> GetUsersByRoleAsync(string roleName);

        // CRUD för individuella användare
        Task<UserEntity> GetByIdAsync(string id);
        Task UpdateUserAsync(UserEntity updatedUser); // inkluderar ev. rollbyte
        Task DeleteUserAsync(string id);

        // Roll-hantering
        Task<bool> IsInRoleAsync(UserEntity user, string role);
        Task UpdateUserRoleAsync(string userId, string newRole);
        Task UpdateExternalImageUrlAsync(string userId, string externalImageUrl);
        Task<List<UserEntity>> GetAllUsersAsync();
        Task<UserEntity?> GetUserByEmailAsync(string email);
    }
}
