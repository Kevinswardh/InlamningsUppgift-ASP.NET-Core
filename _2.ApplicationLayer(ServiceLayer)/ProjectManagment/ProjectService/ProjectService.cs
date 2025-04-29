using ApplicationLayer_ServiceLayer_.ProjectManagment.ProjectService.Interface;
using DomainLayer_BusinessLogicLayer_.InfraInterfaces;
using __Cross_cutting_Concerns.FormDTOs;
using DomainLayer_BusinessLogicLayer_.DomainModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApplicationLayer_ServiceLayer_.ProjectManagment.ProjectService
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;

        public ProjectService(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<IEnumerable<ProjectForm>> GetProjectsForUserRoleAsync(IEnumerable<string> roles, string userId)
        {
            var isAdminOrManager = roles.Contains("Admin") || roles.Contains("Manager");

            IEnumerable<Project> projects;
            if (isAdminOrManager)
            {
                projects = await _projectRepository.GetAllAsync();
            }
            else
            {
                projects = await _projectRepository.GetProjectsByUserIdAsync(userId);
            }

            return projects.Select(p => new ProjectForm
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                ImageUrl = p.ImageUrl ?? "~/Pictures/Icons/projectlogo.svg",
                EndDate = p.EndDate,
                Members = p.Users.Select(m => new MemberItemDTO
                {
                    Id = m.Id,
                    UserName = m.UserName,
                    Email = m.Email,
                    PhoneNumber = m.PhoneNumber,
                    Position = m.Position,
                    Role = m.Role,
                    ImageUrl = m.ImageUrl,
                    IsOnline = m.IsOnline
                }).ToList(),
                IsCompleted = p.EndDate <= DateTime.UtcNow
            }).ToList();
        }

        // Skapa ett nytt projekt
        public async Task<ProjectForm> CreateProjectAsync(ProjectForm projectForm)
        {
            var project = new Project
            {
                Name = projectForm.Name,
                Description = projectForm.Description,
                ImageUrl = projectForm.ImageUrl,
                StartDate = DateTime.UtcNow,
                EndDate = projectForm.EndDate,
                Users = projectForm.Members.Select(m => new User
                {
                    Id = m.Id,
                    UserName = m.UserName,
                    Email = m.Email,
                    PhoneNumber = m.PhoneNumber,
                    Position = m.Position,
                    Role = m.Role,
                    ImageUrl = m.ImageUrl,
                    IsOnline = m.IsOnline
                }).ToList()
            };

            var createdProject = await _projectRepository.CreateProjectAsync(project);

            return new ProjectForm
            {
                Id = createdProject.Id,
                Name = createdProject.Name,
                Description = createdProject.Description,
                ImageUrl = createdProject.ImageUrl,
                EndDate = createdProject.EndDate,
                Members = createdProject.Users.Select(u => new MemberItemDTO
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Position = u.Position,
                    Role = u.Role,
                    ImageUrl = u.ImageUrl,
                    IsOnline = u.IsOnline
                }).ToList(),
                IsCompleted = createdProject.EndDate <= DateTime.UtcNow
            };
        }

        // Uppdatera ett projekt
        public async Task<ProjectForm> UpdateProjectAsync(ProjectForm projectForm)
        {
            var project = await _projectRepository.GetByIdAsync(projectForm.Id);
            if (project == null)
            {
                return null; // Projektet finns inte
            }

            project.Name = projectForm.Name;
            project.Description = projectForm.Description;
            project.ImageUrl = projectForm.ImageUrl;
            project.EndDate = projectForm.EndDate;
            project.Users = projectForm.Members.Select(m => new User
            {
                Id = m.Id,
                UserName = m.UserName,
                Email = m.Email,
                PhoneNumber = m.PhoneNumber,
                Position = m.Position,
                Role = m.Role,
                ImageUrl = m.ImageUrl,
                IsOnline = m.IsOnline
            }).ToList();

            var updatedProject = await _projectRepository.UpdateProjectAsync(project);

            return new ProjectForm
            {
                Id = updatedProject.Id,
                Name = updatedProject.Name,
                Description = updatedProject.Description,
                ImageUrl = updatedProject.ImageUrl,
                EndDate = updatedProject.EndDate,
                Members = updatedProject.Users.Select(u => new MemberItemDTO
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Position = u.Position,
                    Role = u.Role,
                    ImageUrl = u.ImageUrl,
                    IsOnline = u.IsOnline
                }).ToList(),
                IsCompleted = updatedProject.EndDate <= DateTime.UtcNow
            };
        }

        // Ta bort ett projekt
        public async Task<bool> DeleteProjectAsync(int projectId)
        {
            return await _projectRepository.DeleteProjectAsync(projectId);
        }

        // Hämta projekt baserat på ID
        public async Task<ProjectForm> GetProjectByIdAsync(int projectId)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
                return null;

            return new ProjectForm
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                ImageUrl = project.ImageUrl ?? "~/Pictures/Icons/projectlogo.svg",
                EndDate = project.EndDate,
                Members = project.Users.Select(m => new MemberItemDTO
                {
                    Id = m.Id,
                    UserName = m.UserName,
                    Email = m.Email,
                    PhoneNumber = m.PhoneNumber,
                    Position = m.Position,
                    Role = m.Role,
                    ImageUrl = m.ImageUrl,
                    IsOnline = m.IsOnline
                }).ToList(),
                IsCompleted = project.EndDate <= DateTime.UtcNow
            };
        }
    }
}
