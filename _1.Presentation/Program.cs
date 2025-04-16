using _5.DataAccessLayer_DAL_.Repositories.UserRepository;
using _5.DataAccessLayer_DAL_.Repositories.UserRepository.Interface;
using ApplicationLayer_ServiceLayer_.Authentication.AuthService;
using ApplicationLayer_ServiceLayer_.Authentication.AuthService.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SecurityLayer.Identity;

var builder = WebApplication.CreateBuilder(args);

// 🔥 Lägg till configuration från ConfigurationLayer
builder.Host.ConfigureAppConfiguration((context, config) =>
{
    var path = Path.Combine(Directory.GetCurrentDirectory(), "..", "_0.ConfigurationLayer", "appsettings.json");
    config.AddJsonFile(path, optional: false, reloadOnChange: true);
});




// ========================================
// 1. Lägg till EF Core och Identity
// ========================================

builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<IdentityDbContext>()
    .AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Login/Index";          // Omdirigering vid [Authorize]
    options.LogoutPath = "/Auth/Logout";         // Inte ett måste men nice
    options.AccessDeniedPath = "/Login/Index";   // Om användaren saknar roll etc
});

// ========================================
// 2. Lägg till egna services
// ========================================

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();




builder.Services.AddControllersWithViews();

// ========================================
// 3. Bygg appen
// ========================================
var app = builder.Build();

// ========================================
// 4. Skapa Admin-användare om den saknas
// ========================================
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    var adminEmail = "admin@admin.com";
    var adminPassword = "Admin123!";

    // Skapa roller om de inte finns
    string[] roles = { "Admin", "Manager", "TeamMember", "Customer", "User" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        var newAdmin = new ApplicationUser
        {
            Email = adminEmail,
            UserName = "Admin",
            Position = "System Owner"
        };

        var result = await userManager.CreateAsync(newAdmin, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(newAdmin, "Admin");
        }
    }
}

// ========================================
// 5. Middleware pipeline
// ========================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // 🧠 Glöm inte denna!
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();
