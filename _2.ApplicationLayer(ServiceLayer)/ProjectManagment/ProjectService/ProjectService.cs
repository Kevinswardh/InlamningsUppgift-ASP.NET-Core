﻿using ApplicationLayer_ServiceLayer_.ProjectManagment.ProjectService.Interface;
using ApplicationLayer_ServiceLayer_.UserManagment.UserService.Interface;
using __Cross_cutting_Concerns.FormDTOs;
using DomainLayer_BusinessLogicLayer_.DomainModel;
using DomainLayer_BusinessLogicLayer_.InfraInterfaces;
using System.Security.Claims;

namespace ApplicationLayer_ServiceLayer_.ProjectManagment.ProjectService
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IUserService _userService;

        public ProjectService(IProjectRepository projectRepository, IUserService userService)
        {
            _projectRepository = projectRepository;
            _userService = userService;
        }
        public async Task<IEnumerable<ProjectForm>> GetProjectsForUserRoleAsync(IEnumerable<string> roles, string userId)
        {
            List<ProjectEntity> projects;

            if (roles.Contains("Admin") || roles.Contains("Manager"))
            {
                projects = (await _projectRepository.GetAllAsync()).ToList();
            }
            else if (roles.Contains("Customer"))
            {
                var user = await _userService.GetUserByIdAsync(userId);
                projects = (await _projectRepository.GetProjectsForCustomerAsync(user.Email)).ToList();
            }
            else
            {
                projects = (await _projectRepository.GetProjectsByUserIdAsync(userId)).ToList();
            }

            // 🔄 Hämta alla användare från Identity för bild och status
            var allUsers = await _userService.GetAllUsersAsync();

            return projects.Select(p => new ProjectForm
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                ImageUrl = p.ImageUrl ?? "~/Pictures/Icons/projectlogo.svg",
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Budget = p.Budget,
                CreatedByUserId = p.CreatedByUserId,
                ClientEmail = p.Customer?.Email ?? "",
                Members = p.ProjectMembers.Select(pm =>
                {
                    var match = allUsers.FirstOrDefault(u => u.Email == pm.TeamMember.Email);
                    return new MemberItemDTO
                    {
                        Id = pm.TeamMember.ExternalUserId,
                        Email = pm.TeamMember.Email,
                        UserName = pm.TeamMember.Name,
                        ImageUrl = match?.ImageUrl,
                        IsOnline = match?.IsOnline ?? false
                    };
                }).ToList(),
                IsCompleted = p.IsCompleted
            }).ToList();
        }


        public async Task<ProjectForm> GetProjectByIdAsync(int projectId)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null) return null;

            return new ProjectForm
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                ImageUrl = project.ImageUrl,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                Budget = project.Budget,
                CreatedByUserId = project.CreatedByUserId,
                ClientEmail = project.Customer?.Email ?? "",
                Members = project.ProjectMembers.Select(pm => new MemberItemDTO
                {
                    Id = pm.TeamMember.ExternalUserId,
                    Email = pm.TeamMember.Email,
                    UserName = pm.TeamMember.Name,
                    ImageUrl = "",
                    IsOnline = false
                }).ToList(),
           
            };
        }

        public async Task CreateProjectWithUsersAsync(ProjectForm form, ClaimsPrincipal user)
        {
            var userId = form.CreatedByUserId;

            // 🔍 Hämta kund från Identity
            var customer = await _userService.GetUserByEmailAsync(form.ClientEmail);
            if (customer == null)
                throw new Exception("Kund hittades inte med angiven e-post.");

            var customerEntity = new CustomerEntity
            {
                Email = customer.Email,
                Name = customer.UserName
            };

            var projectMembers = new List<ProjectMemberEntity>();

            // 🔁 Hämta och hantera teammedlemmar sekventiellt (undvik parallella EF-anrop!)
            foreach (var member in form.Members)
            {
                var teamUser = await _userService.GetUserByIdAsync(member.Id);
                if (teamUser == null)
                    continue;

                // ✅ Kontrollera om teammember redan finns i projekt-databasen
                var existing = await _projectRepository.GetTeamMemberByExternalIdAsync(teamUser.Id);

                if (existing == null)
                {
                    existing = new TeamMemberEntity
                    {
                        Id = teamUser.Id, // 👈 Måste sättas eftersom Id inte är int
                        ExternalUserId = teamUser.Id,
                        Name = teamUser.UserName,
                        Email = teamUser.Email
                    };

                    await _projectRepository.CreateTeamMemberAsync(existing);
                }

                // 🔗 Lägg till relation
                projectMembers.Add(new ProjectMemberEntity
                {
                    TeamMemberId = existing.Id
                });
            }

            // 📦 Skapa nytt projekt
            var project = new ProjectEntity
            {
                Name = form.Name,
                Description = form.Description,
                ImageUrl = form.ImageUrl,
                StartDate = form.StartDate,
                EndDate = form.EndDate,
                Budget = form.Budget,
                CreatedByUserId = userId,
                Customer = customerEntity,
                ProjectMembers = projectMembers
            };

            await _projectRepository.CreateProjectAsync(project);
        }


        public async Task<ProjectForm> UpdateProjectAsync(ProjectForm form)
        {
            var project = await _projectRepository.GetByIdAsync(form.Id);
            if (project == null)
                return null;

            // 🔁 Hämta alla användare via ExternalUserId
            var teamMemberUsers = await Task.WhenAll(
                form.Members.Select(m => _userService.GetUserByIdAsync(m.Id))
            );

            var projectMembers = new List<ProjectMemberEntity>();

            foreach (var user in teamMemberUsers)
            {
                // ✅ Se till att vi söker på ExternalUserId, inte bara Id
                var existing = await _projectRepository.GetTeamMemberByExternalIdAsync(user.Id);
                if (existing == null)
                {
                    existing = new TeamMemberEntity
                    {
                        Id = user.Id, // 🔑 Viktigt: sätt som string
                        ExternalUserId = user.Id,
                        Name = user.UserName,
                        Email = user.Email
                    };

                    // ✅ Se till att den sparas direkt
                    await _projectRepository.CreateTeamMemberAsync(existing);
                }

                // 🔗 Koppla projektet till team medlemmen
                projectMembers.Add(new ProjectMemberEntity
                {
                    TeamMemberId = existing.Id,
                    ProjectId = form.Id,
                    TeamMember = existing // 👈 detta är nyckeln!
                });

            }

            // 🛠️ Uppdatera projektets fält
            project.Name = form.Name;
            project.Description = form.Description;
            project.ImageUrl = form.ImageUrl;
            project.StartDate = form.StartDate;
            project.EndDate = form.EndDate;
            project.Budget = form.Budget;

            // 💾 Ersätt medlemmar
            project.ProjectMembers = projectMembers;

            // 🧠 Sparar till databasen och returnerar ny version
            var updated = await _projectRepository.UpdateProjectAsync(project);
            return await GetProjectByIdAsync(updated.Id); // ❗ denna MÅSTE ha ThenInclude för TeamMember
        }






        public async Task<bool> DeleteProjectAsync(int projectId)
        {
            return await _projectRepository.DeleteProjectAsync(projectId);
        }


        public async Task<bool> MarkProjectAsCompletedAsync(int projectId)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null) return false;

            project.IsCompleted = true;
            await _projectRepository.UpdateProjectAsync(project);

            return true;
        }


    }
}
