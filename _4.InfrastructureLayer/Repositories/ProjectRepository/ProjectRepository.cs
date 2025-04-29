using DomainLayer_BusinessLogicLayer_.DomainModel;
using DomainLayer_BusinessLogicLayer_.InfraInterfaces;
using Microsoft.EntityFrameworkCore;
using _5.DataAccessLayer_DAL_;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _4.infrastructureLayer.Repositories.ProjectRepository
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly ApplicationProjectDbContext _context;

        // Konstruktor
        public ProjectRepository(ApplicationProjectDbContext context)
        {
            _context = context;
        }

        // Skapa ett nytt projekt
        public async Task<Project> CreateProjectAsync(Project project)
        {
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
            return project;
        }

        // Hämta alla projekt
        public async Task<IEnumerable<Project>> GetAllAsync()
        {
            return await _context.Projects
                .Include(p => p.Users) // Ladda relaterade användare
                .ToListAsync();
        }

        // Hämta projekt för en specifik användare
        public async Task<IEnumerable<Project>> GetProjectsByUserIdAsync(string userId)
        {
            return await _context.Projects
                .Where(p => p.Users.Any(u => u.Id == userId) || p.CreatedByUserId == userId)
                .Include(p => p.Users) // Ladda relaterade användare
                .ToListAsync();
        }

        // Hämta ett projekt baserat på ID
        public async Task<Project> GetByIdAsync(int projectId)
        {
            return await _context.Projects
                .Include(p => p.Users) // Ladda relaterade användare
                .FirstOrDefaultAsync(p => p.Id == projectId);
        }

        // Uppdatera ett projekt
        public async Task<Project> UpdateProjectAsync(Project project)
        {
            var existingProject = await _context.Projects
                .Include(p => p.Users) // Ladda relaterade användare
                .FirstOrDefaultAsync(p => p.Id == project.Id);

            if (existingProject != null)
            {
                existingProject.Name = project.Name;
                existingProject.Description = project.Description;
                existingProject.StartDate = project.StartDate;
                existingProject.EndDate = project.EndDate;
                existingProject.Users = project.Users; // Uppdatera användare
                existingProject.CreatedByUserId = project.CreatedByUserId;

                await _context.SaveChangesAsync();
                return existingProject;
            }

            return null; // Projektet finns inte
        }

        // Ta bort ett projekt
        public async Task<bool> DeleteProjectAsync(int projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project != null)
            {
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}
