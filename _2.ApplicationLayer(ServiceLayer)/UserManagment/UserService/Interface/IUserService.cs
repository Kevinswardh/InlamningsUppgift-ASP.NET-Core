using __Cross_cutting_Concerns.FormDTOs;
using CrossCuttingConcerns.FormDTOs;
using DomainLayer_BusinessLogicLayer_.DomainModel;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ApplicationLayer_ServiceLayer_.UserManagment.UserService.Interface
{
    public interface IUserService
    {
        Task<NewMembersDTO> GetUsersFilteredAsync(string role, string? search, string tab, string sortBy, int page, int pageSize);
        Task<UserEntity> GetUserByIdAsync(string id);
        Task UpdateUserAsync(UserEntity user);
        Task DeleteUserAsync(string id);
        Task<bool> IsInRoleAsync(UserEntity user, string role);
        Task<(string? ImageUrl, string? UserName)> GetUserProfileForLayoutAsync(ClaimsPrincipal user);
        Task<bool> UseExternalProfilePictureAsync(string userId);
        Task<List<string>> GetUserRolesAsync(ClaimsPrincipal user);
        string GetUserId(ClaimsPrincipal user);
        Task<List<MemberItemDTO>> GetAllUsersAsync();
        Task<UserEntity?> GetUserByEmailAsync(string email);

    }
}
