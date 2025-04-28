using DomainLayer_BusinessLogicLayer_.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DomainLayer_BusinessLogicLayer_.InfraInterfaces
{
    public interface IUserRepository
    {
        // Hämtar användare med viss roll
        Task<List<User>> GetUsersByRoleAsync(string roleName);

        // CRUD för individuella användare
        Task<User> GetByIdAsync(string id);
        Task UpdateUserAsync(User updatedUser); // inkluderar ev. rollbyte om det behövs
        Task DeleteUserAsync(string id);

        // Roll-hantering (kan användas internt vid behov)
        Task<bool> IsInRoleAsync(User user, string role);
        Task UpdateUserRoleAsync(string userId, string newRole);
        Task UpdateExternalImageUrlAsync(string userId, string externalImageUrl);

    }
}
