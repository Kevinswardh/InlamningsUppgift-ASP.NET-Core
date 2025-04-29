using Microsoft.AspNetCore.Mvc;
using Presentation.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using ApplicationLayer_ServiceLayer_.UserManagment.UserService.Interface;
using _1.PresentationLayer.ViewModels.MembersViewModels;
using _1.PresentationLayer.ViewModels.ProjectsViewModels;
using ApplicationLayer_ServiceLayer_.ProjectManagment.ProjectService.Interface;
using __Cross_cutting_Concerns.FormDTOs;

namespace Presentation.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly ILogger<ProjectsController> _logger;
        private readonly IUserService _userService;
        private readonly IProjectService _projectService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProjectsController(ILogger<ProjectsController> logger, IUserService userService, IProjectService projectService, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _userService = userService;
            _projectService = projectService;
            _httpContextAccessor = httpContextAccessor;
        }

        // Index - Visa alla projekt
        public async Task<IActionResult> Index()
        {
            var (image, name) = await _userService.GetUserProfileForLayoutAsync(User);
            ViewBag.ProfileImage = image;
            ViewBag.UserName = name;

            var roles = await _userService.GetUserRolesAsync(User);
            if (roles.Contains("User"))
            {
                return Forbid(); // 🚫 NewMembers (User) får ej access
            }

            var userId = _userService.GetUserId(User);
            var projects = await _projectService.GetProjectsForUserRoleAsync(roles, userId);

            var viewModel = new ProjectsViewModel
            {
                Projects = projects.Select(project => new ProjectViewModel
                {
                    Id = project.Id,
                    Name = project.Name,
                    ClientName = project.ClientName,
                    Description = project.Description,
                    ImageUrl = project.ImageUrl,
                    TimeLeft = CalculateTimeLeft(project.EndDate),
                    Members = project.Members.Select(m => new MemberItemViewModel
                    {
                        Id = m.Id,
                        UserName = m.UserName,
                        Email = m.Email,
                        ImageUrl = m.ImageUrl
                    }).ToList(),
                    IsCompleted = project.IsCompleted
                }).ToList()
            };

            ViewBag.CanAddProject = roles.Any(r => r == "Admin" || r == "Manager" || r == "TeamMember");

            return View(viewModel);
        }

        // Create - Skapa nytt projekt
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProjectForm projectForm)
        {
            if (ModelState.IsValid)
            {
                var createdProject = await _projectService.CreateProjectAsync(projectForm);
                return RedirectToAction("Index");
            }

            return View(projectForm);
        }

        // Edit - Uppdatera projekt
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            var projectForm = new ProjectForm
            {
                Id = project.Id,
                Name = project.Name,
                ClientName = project.ClientName,
                Description = project.Description,
                ImageUrl = project.ImageUrl,
                EndDate = project.EndDate,
                Members = project.Members
            };

            return View(projectForm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, ProjectForm projectForm)
        {
            if (id != projectForm.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                var updatedProject = await _projectService.UpdateProjectAsync(projectForm);
                if (updatedProject == null)
                {
                    return NotFound();
                }

                return RedirectToAction("Index");
            }

            return View(projectForm);
        }

        // Delete - Ta bort projekt
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _projectService.DeleteProjectAsync(id);
            if (!success)
            {
                return NotFound();
            }

            return RedirectToAction("Index");
        }

        // Details - Visa detaljer för projekt
        public async Task<IActionResult> Details(int id)
        {
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        private string CalculateTimeLeft(DateTime endDate)
        {
            var now = DateTime.UtcNow;
            var difference = endDate - now;
            if (difference.TotalDays < 1)
                return "Less than 1 day left";
            if (difference.TotalDays < 7)
                return $"{difference.Days} days left";
            if (difference.TotalDays < 30)
                return $"{difference.Days / 7} weeks left";
            return $"{difference.Days / 30} months left";
        }
    }
}
