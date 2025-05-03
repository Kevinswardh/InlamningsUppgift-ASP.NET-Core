using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SecurityLayer.Identity;

namespace BackgroundJobs.SeedData
{
    public static class SeedTestUsers
    {
        public static async Task RunAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles = { "Admin", "Manager", "TeamMember", "Customer", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            await CreateUsersForRole(userManager, "Newbie", "User", 4);
            await CreateUsersForRole(userManager, "Customer", "Customer", 4);
            await CreateUsersForRole(userManager, "Teamie", "TeamMember", 4);
            await CreateUsersForRole(userManager, "Manager", "Manager", 4);
        }

        private static async Task CreateUsersForRole(UserManager<ApplicationUser> userManager, string prefix, string role, int count)
        {
            for (int i = 1; i <= count; i++)
            {
                var email = $"{prefix.ToLower()}{i}@mail.com";
                var userName = $"{prefix}{i}";

                if (await userManager.FindByEmailAsync(email) != null)
                    continue;

                var user = new ApplicationUser
                {
                    UserName = userName,
                    Email = email,
                    Position = role + " Test",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "Test123!");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(user, role);
            }
        }
    }
}
