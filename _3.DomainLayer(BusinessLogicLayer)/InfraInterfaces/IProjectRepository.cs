using DomainLayer_BusinessLogicLayer_.DomainModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DomainLayer_BusinessLogicLayer_.InfraInterfaces
{
    public interface IProjectRepository
    {
        // Skapa nytt projekt
        Task<ProjectEntity> CreateProjectAsync(ProjectEntity project);

        // Hämta alla projekt (inkl. kund och medlemmar)
        Task<IEnumerable<ProjectEntity>> GetAllAsync();

        // Hämta projekt för en viss användare (via ExternalUserId)
        Task<IEnumerable<ProjectEntity>> GetProjectsByUserIdAsync(string externalUserId);

        // Hämta projekt baserat på ID
        Task<ProjectEntity> GetByIdAsync(int projectId);

        // Uppdatera projekt
        Task<ProjectEntity> UpdateProjectAsync(ProjectEntity project);

        // Ta bort projekt
        Task<bool> DeleteProjectAsync(int projectId);
    }
}
