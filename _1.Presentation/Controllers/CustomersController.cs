using _1.PresentationLayer.ViewModels.MembersViewModels;
using ApplicationLayer_ServiceLayer_.UserManagment.UserService.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models;
using System.Diagnostics;

namespace Presentation.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly ILogger<CustomersController> _logger;
        private readonly IUserService _userService;

        public CustomersController(IUserService userService, ILogger<CustomersController> logger)
        {
            _logger = logger;
            _userService = userService;
        }

        public async Task<IActionResult> Index(string search = "", string tab = "All", string sortBy = "Name", int page = 1)
        {
            // 1. H�mta alla anv�ndare f�r att r�kna flikarna
            var allUsers = await _userService.GetUsersFilteredAsync("Customer", search, "All", sortBy, 1, 9999);

            // 2. H�mta filtrerade + paginerade
            var result = await _userService.GetUsersFilteredAsync("Customer", search, tab, sortBy, page, 10);

            // 3. Bygg upp viewModel med inb�ddad TabData
            var viewModel = new UserListViewModel
            {
                Members = result.Members.Select(m => new MemberItemViewModel
                {
                    Id = m.Id,
                    Email = m.Email,
                    UserName = m.UserName,
                    PhoneNumber = m.PhoneNumber,
                    Position = m.Position,
                    Role = m.Role,
                    IsOnline = m.IsOnline
                }).ToList(),

                SelectedSort = sortBy,
                CurrentPage = result.CurrentPage,
                TotalPages = result.TotalPages,
                SearchQuery = search,
                Filter = result.Filter,

                TabData = new TabListViewModel
                {
                    SelectedTab = tab,
                    AllCount = allUsers.Members.Count,
                    OnlineCount = allUsers.Members.Count(m => m.IsOnline),
                    OfflineCount = allUsers.Members.Count(m => !m.IsOnline),
                    CurrentController = this.ControllerContext.RouteData.Values["controller"]?.ToString()
                }
            };

            return View(viewModel);
        }



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}