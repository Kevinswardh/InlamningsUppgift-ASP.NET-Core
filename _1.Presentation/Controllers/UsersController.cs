using _1.PresentationLayer.ViewModels.MembersViewModels;
using _IntegrationLayer.Hubs;
using ApplicationLayer_ServiceLayer_.Authentication.AuthService.Interface;
using ApplicationLayer_ServiceLayer_.UserManagment.UserService.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

public class UsersController : Controller
{
    private readonly IUserService _userService;
    private readonly IAuthService _authService;

    public UsersController(IUserService userService, IAuthService authService)
    {
        _userService = userService;
        _authService = authService;
    }

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
        if (!ModelState.IsValid)
            return Redirect(returnUrl);

        var user = await _userService.GetUserByIdAsync(model.Id);
        if (user == null)
            return NotFound();

        // 📝 Uppdatera användarens data
        user.UserName = model.UserName;
        user.Email = model.Email;
        user.PhoneNumber = model.PhoneNumber;
        user.Position = model.Position;
        user.Role = model.Role;

        await _userService.UpdateUserAsync(user);

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (currentUserId == model.Id)
        {
            // 🔄 Om användaren redigerar sig själv: uppdatera claims direkt
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim("darkMode", user.IsDarkModeEnabled.ToString().ToLower()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role ?? "User")
        };

            var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal);
        }
        else
        {
            Console.WriteLine($"📡 Sänder SignalR till user.Id = {user.Id}");

            var hubContext = HttpContext.RequestServices.GetRequiredService<IHubContext<UserHub>>();
            await hubContext.Clients.User(user.Id).SendAsync("RefreshClaims");
        }


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

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> RefreshClaims()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null) return Unauthorized();

        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim("darkMode", user.IsDarkModeEnabled.ToString().ToLower()),
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role ?? "User")
    };

        var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal);

        Console.WriteLine($"✅ Claims uppdaterade för {user.UserName} ({user.Role})");
        return Ok();
    }

}