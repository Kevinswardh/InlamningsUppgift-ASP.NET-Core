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

        public ProjectRepository(ApplicationProjectDbContext context)
        {
            _context = context;
        }

        // Skapa ett nytt projekt
        public async Task<ProjectEntity> CreateProjectAsync(ProjectEntity project)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));

            await _context.Projects.AddAsync(project);
            await _context.SaveChangesAsync();
            return project;
        }

        // Hämta alla projekt inkl. kund och teammedlemmar
        public async Task<IEnumerable<ProjectEntity>> GetAllAsync()
        {
            return await _context.Projects
                .Include(p => p.Customer)
                .Include(p => p.ProjectMembers)
                    .ThenInclude(pm => pm.TeamMember)
                .ToListAsync();
        }

        // Hämta projekt där teammedlem eller skapare matchar ExternalUserId
        public async Task<IEnumerable<ProjectEntity>> GetProjectsByUserIdAsync(string externalUserId)
        {
            return await _context.Projects
                .Include(p => p.Customer)
                .Include(p => p.ProjectMembers)
                    .ThenInclude(pm => pm.TeamMember)
                .Where(p =>
                    p.ProjectMembers.Any(pm => pm.TeamMember.ExternalUserId == externalUserId)
                    || p.CreatedByUserId == externalUserId)
                .ToListAsync();
        }

        // Hämta projekt baserat på ID
        public async Task<ProjectEntity> GetByIdAsync(int projectId)
        {
            return await _context.Projects
                .Include(p => p.Customer)
                .Include(p => p.ProjectMembers)
                    .ThenInclude(pm => pm.TeamMember)
                .FirstOrDefaultAsync(p => p.Id == projectId);
        }

        // Uppdatera projekt (OBS: vi ersätter teammedlemmarna)
        public async Task<ProjectEntity> UpdateProjectAsync(ProjectEntity project)
        {
            var existingProject = await _context.Projects
                .Include(p => p.ProjectMembers)
                .FirstOrDefaultAsync(p => p.Id == project.Id);

            if (existingProject == null)
                return null;

            // Uppdatera fält
            existingProject.Name = project.Name;
            existingProject.Description = project.Description;
            existingProject.StartDate = project.StartDate;
            existingProject.EndDate = project.EndDate;
            existingProject.Budget = project.Budget;
            existingProject.ImageUrl = project.ImageUrl;
            existingProject.IsCompleted = project.IsCompleted;
            existingProject.CreatedByUserId = project.CreatedByUserId;
            existingProject.CustomerId = project.CustomerId;

            // Ersätt teammedlemmar
            _context.ProjectMembers.RemoveRange(existingProject.ProjectMembers);
            existingProject.ProjectMembers = project.ProjectMembers;

            await _context.SaveChangesAsync();
            return existingProject;
        }

        // Ta bort projekt
        public async Task<bool> DeleteProjectAsync(int projectId)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectMembers)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                return false;

            _context.ProjectMembers.RemoveRange(project.ProjectMembers);
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
