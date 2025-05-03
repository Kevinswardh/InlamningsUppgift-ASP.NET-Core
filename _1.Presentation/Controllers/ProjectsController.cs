using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ApplicationLayer_ServiceLayer_.UserManagment.UserService.Interface;
using ApplicationLayer_ServiceLayer_.ProjectManagment.ProjectService.Interface;
using __Cross_cutting_Concerns.FormDTOs;
using _1.PresentationLayer.ViewModels.MembersViewModels;
using _1.PresentationLayer.ViewModels.ProjectsViewModels;
using _3.IntegrationLayer.Hubs;
using ApplicationLayer_ServiceLayer_.NotificationManagment.NotificationService.Interface;
using Microsoft.AspNetCore.SignalR;

namespace Presentation.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly ILogger<ProjectsController> _logger;
        private readonly IUserService _userService;
        private readonly IProjectService _projectService;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<NotificationHub> _hubContext;
        public ProjectsController(
            ILogger<ProjectsController> logger,
            IUserService userService,
            IProjectService projectService,
            INotificationService notificationService,
            IHubContext<NotificationHub> hubContext)
        {
            _logger = logger;
            _userService = userService;
            _projectService = projectService;
            _notificationService = notificationService;
            _hubContext = hubContext;
        }


        public async Task<IActionResult> Index(string tab = "All", int page = 1)
        {
            var roles = await _userService.GetUserRolesAsync(User);
            if (roles.Contains("User")) return Forbid();

            var userId = _userService.GetUserId(User);
            var currentUser = await _userService.GetUserByIdAsync(userId);
            var currentEmail = currentUser.Email;

            // Hämta projekt enligt roll
            var allProjects = await _projectService.GetProjectsForUserRoleAsync(roles, userId);

            // Om användaren INTE är Admin eller Manager: Filtrera bort projekt hen inte är med i
            if (!roles.Contains("Admin") && !roles.Contains("Manager"))
            {
                allProjects = allProjects.Where(p =>
                    p.CreatedByUserId == userId ||
                    p.ClientEmail == currentEmail ||
                    p.Members.Any(m => m.Id == userId)).ToList();
            }

            var allUsers = await _userService.GetAllUsersAsync();
            var allMembers = allUsers.Select(u => new MemberItemViewModel
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Position = u.Position,
                Role = u.Role,
                IsOnline = u.IsOnline,
                ImageUrl = u.ImageUrl
            }).ToList();

            // Filtrera på flik
            var filtered = tab switch
            {
                "Started" => allProjects.Where(p => !p.IsCompleted).ToList(),
                "Completed" => allProjects.Where(p => p.IsCompleted).ToList(),
                _ => allProjects.ToList()
            };

            int pageSize = 8;
            int totalProjects = filtered.Count;
            var paginated = filtered.Skip((page - 1) * pageSize).Take(pageSize);

            var viewModel = new ProjectsViewModel
            {
                Projects = paginated.Select(project => new ProjectViewModel
                {
                    Id = project.Id,
                    Name = project.Name,
                    ClientEmail = project.ClientEmail,
                    Description = project.Description,
                    ImageUrl = project.ImageUrl,
                    TimeLeft = CalculateTimeLeft(project.EndDate, project.IsCompleted),
                    Members = project.Members.Select(m => new MemberItemViewModel
                    {
                        Id = m.Id,
                        UserName = m.UserName,
                        Email = m.Email,
                        ImageUrl = m.ImageUrl
                    }).ToList(),
                    IsCompleted = project.IsCompleted
                }).ToList(),

                AllMembers = allMembers,
                SelectedTab = tab,
                AllCount = allProjects.Count(),
                StartedCount = allProjects.Count(p => !p.IsCompleted),
                CompletedCount = allProjects.Count(p => p.IsCompleted),
                PageSize = pageSize,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalProjects / pageSize)
            };

            ViewBag.CanAddProject = roles.Any(r => r is "Admin" or "Manager" or "TeamMember");

            var (image, name) = await _userService.GetUserProfileForLayoutAsync(User);
            ViewBag.ProfileImage = image;
            ViewBag.UserName = name;

            return View(viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> Create(CreateProjectViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["FormError"] = "Något gick fel i formuläret.";
                return RedirectToAction("Index");
            }

            var userId = _userService.GetUserId(User);
            var projectId = Guid.NewGuid().ToString();

            string? imageUrl = null;
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine("wwwroot", "Pictures", "ProjectPictures", projectId);
                Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = $"{projectId}{Path.GetExtension(model.ImageFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await model.ImageFile.CopyToAsync(stream);

                imageUrl = $"/Pictures/ProjectPictures/{projectId}/{uniqueFileName}";
            }

            var allUsers = await _userService.GetAllUsersAsync();
            var selectedMembers = allUsers.Where(u => model.MemberIds.Contains(u.Id)).ToList();

            var memberDTOs = selectedMembers.Select(u => new MemberItemDTO
            {
                Id = u.Id,
                Email = u.Email,
                ImageUrl = u.ImageUrl
            }).ToList();

            var form = new ProjectForm
            {
                Id = 0,
                Name = model.Name,
                Description = model.Description,
                ClientEmail = model.ClientEmail,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Budget = model.Budget,
                ImageUrl = imageUrl,
                CreatedByUserId = userId,
                Members = memberDTOs,
                IsCompleted = false
            };


            await _projectService.CreateProjectWithUsersAsync(form, User);
            foreach (var member in memberDTOs)
            {
                var notif = new NotificationForm
                {
                    Title = "Du har lagts till i ett projekt",
                    Message = $"Projektet \"{model.Name}\" har skapats och du är tillagd.",
                    ReceiverUserId = member.Id,
                    CreatedAt = DateTime.UtcNow
                };
                await _notificationService.AddNotificationAsync(notif);
                await _hubContext.Clients.User(member.Id).SendAsync("ReceiveNotification", notif.Title, notif.Message);
            }

            // Om kunden finns som användare
            var customer = allUsers.FirstOrDefault(u => u.Email == model.ClientEmail);
            if (customer != null)
            {
                var notif = new NotificationForm
                {
                    Title = "Du är kontaktperson för nytt projekt",
                    Message = $"Projektet \"{model.Name}\" har skapats och du är listad som kund.",
                    ReceiverUserId = customer.Id,
                    CreatedAt = DateTime.UtcNow
                };
                await _notificationService.AddNotificationAsync(notif);
                await _hubContext.Clients.User(customer.Id).SendAsync("ReceiveNotification", notif.Title, notif.Message);
            }
            var currentUser = await _userService.GetUserByIdAsync(userId);
            var managersAndAdmins = allUsers.Where(u => u.Id != userId && (u.Role == "Admin" || u.Role == "Manager")).ToList();
            foreach (var manager in managersAndAdmins)
            {
                var notif = new NotificationForm
                {
                    Title = "Nytt projekt skapat",
                    Message = $"Användaren \"{currentUser.UserName}\" har skapat projektet \"{model.Name}\".",
                    ReceiverUserId = manager.Id,
                    CreatedAt = DateTime.UtcNow
                };
                await _notificationService.AddNotificationAsync(notif);
                await _hubContext.Clients.User(manager.Id).SendAsync("ReceiveNotification", notif.Title, notif.Message);
            }

            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id, EditProjectViewModel model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            string? imageUrl = model.ImageUrl; // Behåll gamla bild-URL:n som default

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine("wwwroot", "Pictures", "ProjectPictures", id.ToString());
                Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = $"{id}{Path.GetExtension(model.ImageFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await model.ImageFile.CopyToAsync(stream);

                imageUrl = $"/Pictures/ProjectPictures/{id}/{uniqueFileName}";
            }

            var memberDTOs = model.MemberIds.Select(mid => new MemberItemDTO
            {
                Id = mid
            }).ToList();

            var form = new ProjectForm
            {
                Id = model.Id,
                Name = model.Name,
                ClientEmail = model.ClientEmail,
                Description = model.Description,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Budget = model.Budget,
                ImageUrl = imageUrl,
                Members = memberDTOs
            };

            var updatedProject = await _projectService.UpdateProjectAsync(form);
            if (updatedProject == null) return NotFound();
            foreach (var member in memberDTOs)
            {
                var notif = new NotificationForm
                {
                    Title = "Projekt uppdaterat",
                    Message = $"Projektet \"{model.Name}\" har uppdaterats.",
                    ReceiverUserId = member.Id,
                    CreatedAt = DateTime.UtcNow
                };
                await _notificationService.AddNotificationAsync(notif);
                await _hubContext.Clients.User(member.Id).SendAsync("ReceiveNotification", notif.Title, notif.Message);
            }

            var allUsers = await _userService.GetAllUsersAsync();
            var customer = allUsers.FirstOrDefault(u => u.Email == model.ClientEmail);
            if (customer != null)
            {
                var notif = new NotificationForm
                {
                    Title = "Ditt projekt har uppdaterats",
                    Message = $"Projektet \"{model.Name}\" har uppdaterats.",
                    ReceiverUserId = customer.Id,
                    CreatedAt = DateTime.UtcNow
                };
                await _notificationService.AddNotificationAsync(notif);
                await _hubContext.Clients.User(customer.Id).SendAsync("ReceiveNotification", notif.Title, notif.Message);
            }
            var currentUser = await _userService.GetUserByIdAsync(_userService.GetUserId(User));
            var managersAndAdmins = allUsers.Where(u => u.Id != currentUser.Id && (u.Role == "Admin" || u.Role == "Manager")).ToList();
            foreach (var manager in managersAndAdmins)
            {
                var notif = new NotificationForm
                {
                    Title = "Projekt uppdaterat",
                    Message = $"Användaren \"{currentUser.UserName}\" har uppdaterat projektet \"{model.Name}\".",
                    ReceiverUserId = manager.Id,
                    CreatedAt = DateTime.UtcNow
                };
                await _notificationService.AddNotificationAsync(notif);
                await _hubContext.Clients.User(manager.Id).SendAsync("ReceiveNotification", notif.Title, notif.Message);
            }


            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Complete(int id)
        {
            var success = await _projectService.MarkProjectAsCompletedAsync(id);
            if (!success) return NotFound();
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project != null)
            {
                foreach (var member in project.Members)
                {
                    var notif = new NotificationForm
                    {
                        Title = "Projektet är färdigställt",
                        Message = $"Projektet \"{project.Name}\" har markerats som klart.",
                        ReceiverUserId = member.Id,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _notificationService.AddNotificationAsync(notif);
                    await _hubContext.Clients.User(member.Id).SendAsync("ReceiveNotification", notif.Title, notif.Message);
                }

                var customer = (await _userService.GetAllUsersAsync()).FirstOrDefault(u => u.Email == project.ClientEmail);
                if (customer != null)
                {
                    var notif = new NotificationForm
                    {
                        Title = "Projekt slutfört",
                        Message = $"Projektet \"{project.Name}\" är nu klart.",
                        ReceiverUserId = customer.Id,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _notificationService.AddNotificationAsync(notif);
                    await _hubContext.Clients.User(customer.Id).SendAsync("ReceiveNotification", notif.Title, notif.Message);
                }
            }
            var currentUser = await _userService.GetUserByIdAsync(_userService.GetUserId(User));
            var allUsers = await _userService.GetAllUsersAsync();
            var managersAndAdmins = allUsers.Where(u => u.Id != currentUser.Id && (u.Role == "Admin" || u.Role == "Manager")).ToList();
            foreach (var manager in managersAndAdmins)
            {
                var notif = new NotificationForm
                {
                    Title = "Projekt markerat som klart",
                    Message = $"Användaren \"{currentUser.UserName}\" har markerat projektet \"{project.Name}\" som slutfört.",
                    ReceiverUserId = manager.Id,
                    CreatedAt = DateTime.UtcNow
                };
                await _notificationService.AddNotificationAsync(notif);
                await _hubContext.Clients.User(manager.Id).SendAsync("ReceiveNotification", notif.Title, notif.Message);
            }


            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // 1. Hämta projektet INNAN det raderas
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null) return NotFound();

            // 2. Förbered användardata
            var currentUser = await _userService.GetUserByIdAsync(_userService.GetUserId(User));
            var allUsers = await _userService.GetAllUsersAsync();
            var managersAndAdmins = allUsers
                .Where(u => u.Id != currentUser.Id && (u.Role == "Admin" || u.Role == "Manager"))
                .ToList();

            // 3. Skicka notifikationer före borttagning
            foreach (var member in project.Members)
            {
                var notif = new NotificationForm
                {
                    Title = "Projekt raderat",
                    Message = $"Projektet \"{project.Name}\" har tagits bort.",
                    ReceiverUserId = member.Id,
                    CreatedAt = DateTime.UtcNow
                };
                await _notificationService.AddNotificationAsync(notif);
                await _hubContext.Clients.User(member.Id).SendAsync("ReceiveNotification", notif.Title, notif.Message);
            }

            var customer = allUsers.FirstOrDefault(u => u.Email == project.ClientEmail);
            if (customer != null)
            {
                var notif = new NotificationForm
                {
                    Title = "Projekt borttaget",
                    Message = $"Projektet \"{project.Name}\" har tagits bort.",
                    ReceiverUserId = customer.Id,
                    CreatedAt = DateTime.UtcNow
                };
                await _notificationService.AddNotificationAsync(notif);
                await _hubContext.Clients.User(customer.Id).SendAsync("ReceiveNotification", notif.Title, notif.Message);
            }

            foreach (var manager in managersAndAdmins)
            {
                var notif = new NotificationForm
                {
                    Title = "Projekt raderat",
                    Message = $"Användaren \"{currentUser.UserName}\" har raderat projektet \"{project.Name}\".",
                    ReceiverUserId = manager.Id,
                    CreatedAt = DateTime.UtcNow
                };
                await _notificationService.AddNotificationAsync(notif);
                await _hubContext.Clients.User(manager.Id).SendAsync("ReceiveNotification", notif.Title, notif.Message);
            }

            // 4. Nu raderas projektet
            var success = await _projectService.DeleteProjectAsync(id);
            if (!success) return NotFound();

            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Details(int id)
        {
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null) return NotFound();

            return View(project);
        }

        private string CalculateTimeLeft(DateTime endDate, bool isCompleted)
        {
            if (isCompleted && endDate < DateTime.UtcNow)
                return "CompletedLate"; // markerar att projektet avslutats sent

            if (isCompleted)
                return "Completed";

            if (endDate < DateTime.UtcNow)
                return "Expired";

            var difference = endDate - DateTime.UtcNow;
            if (difference.TotalDays < 1)
                return "Less than 1 day left";
            if (difference.TotalDays < 7)
                return $"{difference.Days} days left";
            if (difference.TotalDays < 30)
                return $"{difference.Days / 7} weeks left";

            return $"{difference.Days / 30} months left";
        }



        [HttpGet]
        public async Task<IActionResult> GetProjectJson(int id)
        {
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
                return NotFound();

            return Json(project);
        }

    }
}
