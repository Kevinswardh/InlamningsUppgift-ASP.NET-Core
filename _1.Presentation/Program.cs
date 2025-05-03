using __Cross_cutting_Concerns.ServiceInterfaces;
using _4.infrastructureLayer.InfraServices.Statuses;
using _4.infrastructureLayer.Repositories.AuthRepository;
using _4.infrastructureLayer.Repositories.UserRepository;
using ApplicationLayer_ServiceLayer_.Authentication.AuthService;
using ApplicationLayer_ServiceLayer_.Authentication.AuthService.Interface;
using ApplicationLayer_ServiceLayer_.UserManagment.UserService;
using ApplicationLayer_ServiceLayer_.UserManagment.UserService.Interface;
using DomainLayer_BusinessLogicLayer_.InfraInterfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SecurityLayer.Identity;
using SecurityLayer.SecurityServices.SecurityAuthService;
using SecurityLayer.SecurityServices.SecurityAuthService.Interface;
using _IntegrationLayer.ExternalAuthService;
using _IntegrationLayer.ExternalAuthService.Interface;
using System.Security.Claims;
using _5.DataAccessLayer_DAL_;
using ApplicationLayer_ServiceLayer_.ProjectManagment.ProjectService.Interface;
using ApplicationLayer_ServiceLayer_.ProjectManagment.ProjectService;
using _4.infrastructureLayer.Repositories.ProjectRepository;
using _3.IntegrationLayer.Hubs;
using ApplicationLayer.Services;
using ApplicationLayer_ServiceLayer_.NotificationManagment.NotificationService.Interface;
using _4.infrastructureLayer.Repositories.NotificationRepository;
using BackgroundJobs.SeedData;




var builder = WebApplication.CreateBuilder(args);


// ========================================
// Defaults
// ========================================
builder.Services.AddSignalR();



// ========================================
// 0. Lägg till konfiguration från ConfigurationLayer
// ========================================
builder.Host.ConfigureAppConfiguration((context, config) =>
{
    var path = Path.Combine(Directory.GetCurrentDirectory(), "..", "_0.ConfigurationLayer", "appsettings.json");
    config.AddJsonFile(path, optional: false, reloadOnChange: true);
});

// ========================================
// 1. Lägg till EF Core och Identity
// ========================================

//Identity Databas
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//ProjectsDatabas
builder.Services.AddDbContext<ApplicationProjectDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProjectConnection")));


builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<IdentityDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Login/Index";
    options.LogoutPath = "/Auth/Logout";
    options.AccessDeniedPath = "/Login/Index";
});

// ========================================
// 2. Lägg till egna services & repositories
// ========================================

// SECURITY services först
builder.Services.AddScoped<ISecurityAuthService, SecurityAuthService>();

// INTEGRATION services
builder.Services.AddScoped<IExternalAuthService, ExternalAuthService>();

// APPLICATION services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<IUserStatusService, UserStatusService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<INotificationService, NotificationService>();


// REPOSITORIES
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();


// ========================================
// 3. Lägg till externa login providers
// ========================================
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme; // <-- Lägg till detta
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    options.Scope.Add("profile");
    options.ClaimActions.MapJsonKey("picture", "picture", "url");
})
.AddGitHub(options =>
{
    options.ClientId = builder.Configuration["Authentication:GitHub:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"];
    options.SaveTokens = true;

    options.Scope.Add("user:email"); // detta scope är viktigt för GitHub
    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
    options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
    options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
    options.ClaimActions.MapJsonKey("picture", "avatar_url");
})
.AddFacebook(options =>
{
    options.AppId = builder.Configuration["Authentication:Facebook:AppId"];
    options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
    options.Fields.Add("picture");
    options.ClaimActions.MapJsonSubKey("picture", "data", "url");
});
Console.WriteLine($"Google ClientId: {builder.Configuration["Authentication:Google:ClientId"]}");
Console.WriteLine($"GitHub ClientId: {builder.Configuration["Authentication:GitHub:ClientId"]}");
Console.WriteLine($"Facebook AppId: {builder.Configuration["Authentication:Facebook:AppId"]}");

// ========================================
// 4. Lägg till MVC
// ========================================
builder.Services.AddControllersWithViews();

// ========================================
// 5. Bygg appen
// ========================================
var app = builder.Build();

// ========================================
// 6. Skapa Admin-användare om den saknas
// ========================================
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    var adminEmail = "admin@admin.com";
    var adminPassword = "Admin123!";

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
// 6b. Skapa test användare
// ========================================
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // Din Admin-seed här...

    // 🔽 Lägg till detta för att seeda testanvändare:
    await SeedTestUsers.RunAsync(scope.ServiceProvider);
}

// ========================================
// 7. Middleware pipeline
// ========================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<NotificationHub>("/notificationHub");


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();
