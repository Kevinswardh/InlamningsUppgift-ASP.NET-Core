using _1.PresentationLayer.ViewModels.MembersViewModels;
using ApplicationLayer_ServiceLayer_.UserManagment.UserService.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public class UsersController : Controller
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    //Kommenterar ut pga vi använder sidladdningarna via Index på respektive sida. (Kan återanvändas när vi ska hämta signal R delar ränker jag?
    /*  public async Task<IActionResult> NewMembers(int page = 1, string search = "", string tab = "All", string sortBy = "Name")
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
      }*/
    public async Task SetProfilePictureToLayout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return;

        var currentUser = await _userService.GetUserByIdAsync(userId);
        if (currentUser != null)
        {
            ViewBag.ProfileImage = currentUser.ImageUrl;
            ViewBag.UserName = currentUser.UserName;
        }
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditMyPage(MemberItemViewModel model)
    {
        if (!ModelState.IsValid)
            return RedirectToAction("Index", "MinaSidor");

        var user = await _userService.GetUserByIdAsync(model.Id);
        if (user == null)
            return NotFound();

        user.UserName = model.UserName;
        user.Email = model.Email;
        user.PhoneNumber = model.PhoneNumber;
        user.Position = model.Position;
        user.Role = model.Role;

        await _userService.UpdateUserAsync(user);

        TempData["SuccessMessage"] = "Dina uppgifter har uppdaterats.";
        return RedirectToAction("Index", "MinaSidor");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadProfileImage(MemberItemViewModel model)
    {
        if (model.ProfileImage == null || model.ProfileImage.Length == 0)
        {
            TempData["ErrorMessage"] = "Ingen bild valdes.";
            return RedirectToAction("Index", "MinaSidor");
        }

        // ✅ Hämta användaren från databasen
        var user = await _userService.GetUserByIdAsync(model.Id);
        if (user == null) return NotFound();

        // ✅ Ta bort gammal bild (om den finns)
        if (!string.IsNullOrEmpty(user.ImageUrl))
        {
            var wwwRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var relativePath = user.ImageUrl.Replace("/", Path.DirectorySeparatorChar.ToString()).TrimStart(Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(wwwRoot, relativePath);

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }

        // ✅ Skapa användarmapp (wwwroot/Pictures/Profiles/{UserId})
        var uploadsFolder = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "Pictures",
            "Profiles",
            user.Id
        );
        Directory.CreateDirectory(uploadsFolder);

        // ✅ Sätt unikt filnamn
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.ProfileImage.FileName)}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await model.ProfileImage.CopyToAsync(stream);
        }

        // ✅ Spara relativ sökväg
        user.ImageUrl = $"/Pictures/Profiles/{user.Id}/{fileName}";
        await _userService.UpdateUserAsync(user);

        TempData["SuccessMessage"] = "Profilbilden har uppdaterats.";
        return RedirectToAction("Index", "MinaSidor");
    }


    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UseExternalProfilePicture()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return RedirectToAction("Index", "MinaSidor");

        var success = await _userService.UseExternalProfilePictureAsync(userId);

        if (!success)
        {
            TempData["ErrorMessage"] = "Kunde inte uppdatera profilbilden från extern inloggning.";
        }
        else
        {
            TempData["SuccessMessage"] = "Profilbilden uppdaterades från extern inloggning.";
        }

        return RedirectToAction("Index", "MinaSidor");
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
    public async Task<IActionResult> Delete(string id, string returnUrl)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null) return NotFound();

        if (User.IsInRole("Manager") && await _userService.IsInRoleAsync(user, "Manager"))
            return Forbid();

        await _userService.DeleteUserAsync(id);

        // Redirect to returnUrl, or default to NewMembers
        return Redirect(string.IsNullOrEmpty(returnUrl) ? "/NewMembers" : returnUrl);
    }

}