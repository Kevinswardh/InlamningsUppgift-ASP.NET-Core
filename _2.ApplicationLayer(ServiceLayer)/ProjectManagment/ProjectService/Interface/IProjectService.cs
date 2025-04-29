using __Cross_cutting_Concerns.FormDTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApplicationLayer_ServiceLayer_.ProjectManagment.ProjectService.Interface
{
    public interface IProjectService
    {
        // Hämtar alla projekt baserat på användarens roller
        Task<IEnumerable<ProjectForm>> GetProjectsForUserRoleAsync(IEnumerable<string> roles, string userId);

        // Skapar ett nytt projekt
        Task<ProjectForm> CreateProjectAsync(ProjectForm projectForm);

        // Uppdaterar ett projekt
        Task<ProjectForm> UpdateProjectAsync(ProjectForm projectForm);

        // Tar bort ett projekt
        Task<bool> DeleteProjectAsync(int projectId);

        // Hämtar ett projekt baserat på dess ID
        Task<ProjectForm> GetProjectByIdAsync(int projectId);
    }
}
