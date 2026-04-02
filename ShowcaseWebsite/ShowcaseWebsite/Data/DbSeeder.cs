using Microsoft.AspNetCore.Identity;
using ShowcaseWebsite.Models;

namespace ShowcaseWebsite.Data;

/// <summary>
/// Seeded rollen en een standaard Admin-account bij eerste opstart.
/// Wachtwoord van admin staat in appsettings / user-secrets.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var config = services.GetRequiredService<IConfiguration>();

        // Maak rollen aan als ze nog niet bestaan
        foreach (var role in new[] { "Admin", "User" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Maak admin-account aan als het nog niet bestaat
        string adminEmail = config["Seeder:AdminEmail"] ?? "admin@showcase.nl";
        string adminPassword = config["Seeder:AdminPassword"] ?? "Admin@123!";

        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                DisplayName = "Administrator",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, adminPassword);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}
