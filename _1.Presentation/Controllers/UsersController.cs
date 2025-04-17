using _1.PresentationLayer.ViewModels.MembersViewModels;
using ApplicationLayer_ServiceLayer_.UserManagment.UserService.Interface;
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
        var dto = await _userService.GetUsersFilteredAsync("User", search, tab, sortBy, page, 6);

        var viewModel = new NewMembersViewModel
        {
            Members = dto.Members.Select(m => new MemberItemViewModel
            {
                Email = m.Email,
                UserName = m.UserName,
                PhoneNumber = m.PhoneNumber,
                Position = m.Position,
                Role = m.Role,
                IsOnline = m.IsOnline
            }).ToList(),
            CurrentPage = dto.CurrentPage,
            TotalPages = dto.TotalPages,
            SelectedTab = tab,
            SelectedSort = sortBy,
            SearchQuery = search,
            Filter = dto.Filter
        };

        return View("NewMembers", viewModel);
    }
}
