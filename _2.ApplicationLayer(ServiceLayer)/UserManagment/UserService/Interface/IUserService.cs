using __Cross_cutting_Concerns.FormDTOs;
using CrossCuttingConcerns.FormDTOs;
using System.Threading.Tasks;

namespace ApplicationLayer_ServiceLayer_.UserManagment.UserService.Interface
{
    public interface IUserService
    {
        Task<NewMembersDTO> GetUsersFilteredAsync(string role, string? search, string tab, string sortBy, int page, int pageSize);
    }
}
