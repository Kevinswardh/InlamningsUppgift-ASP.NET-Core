using CrossCuttingConcerns.FormDTOs;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace ApplicationLayer_ServiceLayer_.Authentication.AuthService.Interface
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(LoginForm model);
        Task<IdentityResult> RegisterUserAsync(RegisterForm model);
    }
}
