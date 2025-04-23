using _1.PresentationLayer.ViewModels.MembersViewModels;
using ApplicationLayer_ServiceLayer_.UserManagment.UserService.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public class UsersController : Controller
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<IActionResult> NewMembers(int page = 1, string search = "", string tab = "All", string sortBy = "Name")
    {
        // Hämta alla users för att räkna flikar
        var allUsers = await _userService.GetUsersFilteredAsync("User", search, "All", sortBy, 1, 9999);

        // Hämta filtrerat resultat för aktuell tab
        var dto = await _userService.GetUsersFilteredAsync("User", search, tab, sortBy, page, 6);

        var viewModel = new UserListViewModel
        {
            Members = dto.Members.Select(m => new MemberItemViewModel
            {
                Id = m.Id,
                Email = m.Email,
                UserName = m.UserName,
                PhoneNumber = m.PhoneNumber,
                Position = m.Position,
                Role = m.Role,
                IsOnline = m.IsOnline
            }).ToList(),

            CurrentPage = dto.CurrentPage,
            TotalPages = dto.TotalPages,
            SelectedSort = sortBy,
            SearchQuery = search,
            Filter = dto.Filter,

            TabData = new TabListViewModel
            {
                SelectedTab = tab,
                AllCount = allUsers.Members.Count,
                OnlineCount = allUsers.Members.Count(m => m.IsOnline),
                OfflineCount = allUsers.Members.Count(m => !m.IsOnline),
                CurrentController = this.ControllerContext.RouteData.Values["controller"]?.ToString()
            }
        };

        return View("NewMembers", viewModel);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Edit(MemberItemViewModel model, string returnUrl)
    {
        if (!ModelState.IsValid) return Redirect(returnUrl);


        var user = await _userService.GetUserByIdAsync(model.Id);
        if (user == null) return NotFound();

        user.UserName = model.UserName;
        user.Email = model.Email;
        user.PhoneNumber = model.PhoneNumber;
        user.Position = model.Position;
        user.Role = model.Role;

        await _userService.UpdateUserAsync(user);


        return Redirect(string.IsNullOrEmpty(returnUrl) ? "/NewMembers" : returnUrl);



    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null) return NotFound();

        if (User.IsInRole("Manager") && await _userService.IsInRoleAsync(user, "Manager"))
            return Forbid();

        await _userService.DeleteUserAsync(id);

        return RedirectToAction("NewMembers");
    }
}