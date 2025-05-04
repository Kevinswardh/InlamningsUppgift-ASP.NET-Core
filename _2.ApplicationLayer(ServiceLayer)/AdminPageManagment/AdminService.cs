using ApplicationLayer_ServiceLayer_.AdminPageManagment.Interface;
using ApplicationLayer_ServiceLayer_.ProjectManagment.ProjectService.Interface;
using ApplicationLayer_ServiceLayer_.UserManagment.UserService.Interface;
using __Cross_cutting_Concerns.FormDTOs;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ApplicationLayer_ServiceLayer_.AdminPageManagment
{
    public class AdminService : IAdminService
    {
        private readonly IUserService _userService;
        private readonly IProjectService _projectService;

        public AdminService(IUserService userService, IProjectService projectService)
        {
            _userService = userService;
            _projectService = projectService;
        }

        public async Task<AdminStatisticsDTO> GetAdminStatisticsAsync()
        {
            var users = await _userService.GetAllUsersAsync();
            var projects = await _projectService.GetProjectsForUserRoleAsync(new[] { "Admin", "Manager" }, ""); // tom userId eftersom admin vill se allt

            return new AdminStatisticsDTO
            {
                TotalUsers = users.Count,
                Managers = users.Count(u => u.Role == "Manager"),
                TeamMembers = users.Count(u => u.Role == "TeamMember"),
                Customers = users.Count(u => u.Role == "Customer"),
                NewMembers = users.Count(u => string.IsNullOrEmpty(u.Role) || u.Role == "User"),
                Projects = projects.Count(),
                CompletedProjects = projects.Count(p => p.EndDate <= DateTime.UtcNow),
                ActiveProjects = projects.Count(p => p.EndDate > DateTime.UtcNow),
                TotalBudget = projects.Sum(p => p.Budget)
            };
        }
    }
}