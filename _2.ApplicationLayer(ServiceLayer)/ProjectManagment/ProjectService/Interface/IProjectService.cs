using __Cross_cutting_Concerns.FormDTOs;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ApplicationLayer_ServiceLayer_.ProjectManagment.ProjectService.Interface
{
    public interface IProjectService
    {
        // Hämtar alla projekt baserat på användarens roller
        Task<IEnumerable<ProjectForm>> GetProjectsForUserRoleAsync(IEnumerable<string> roles, string userId);

        // Uppdaterar ett projekt
        Task<ProjectForm> UpdateProjectAsync(ProjectForm projectForm);

        // Tar bort ett projekt
        Task<bool> DeleteProjectAsync(int projectId);

        // Hämtar ett projekt baserat på dess ID
        Task<ProjectForm> GetProjectByIdAsync(int projectId);

        // Skapar ett projekt med kopplade användare och kund
        Task CreateProjectWithUsersAsync(ProjectForm form, ClaimsPrincipal user);

        Task<bool> MarkProjectAsCompletedAsync(int projectId);


    }
}
