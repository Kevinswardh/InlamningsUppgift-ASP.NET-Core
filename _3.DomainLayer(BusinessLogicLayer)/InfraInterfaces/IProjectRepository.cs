using DomainLayer_BusinessLogicLayer_.DomainModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DomainLayer_BusinessLogicLayer_.InfraInterfaces
{
    public interface IProjectRepository
    {
        // Läs alla projekt
        Task<IEnumerable<Project>> GetAllAsync();

        // Hämta projekt baserat på användar-ID
        Task<IEnumerable<Project>> GetProjectsByUserIdAsync(string userId);

        // Hämta projekt baserat på ID
        Task<Project> GetByIdAsync(int projectId);

        // Skapa ett nytt projekt
        Task<Project> CreateProjectAsync(Project project);

        // Uppdatera ett befintligt projekt
        Task<Project> UpdateProjectAsync(Project project);

        // Ta bort ett projekt
        Task<bool> DeleteProjectAsync(int projectId);
    }
}
