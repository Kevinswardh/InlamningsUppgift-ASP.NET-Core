using __Cross_cutting_Concerns.FormDTOs;
using CrossCuttingConcerns.FormDTOs;
using DomainLayer_BusinessLogicLayer_.Entities;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ApplicationLayer_ServiceLayer_.UserManagment.UserService.Interface
{
    public interface IUserService
    {
        Task<NewMembersDTO> GetUsersFilteredAsync(string role, string? search, string tab, string sortBy, int page, int pageSize);
        Task<User> GetUserByIdAsync(string id);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(string id);
        Task<bool> IsInRoleAsync(User user, string role);
        Task<(string? ImageUrl, string? UserName)> GetUserProfileForLayoutAsync(ClaimsPrincipal user);
        Task<bool> UseExternalProfilePictureAsync(string userId);


    }
}
