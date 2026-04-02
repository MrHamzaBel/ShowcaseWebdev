using Microsoft.AspNetCore.Identity;
using Showcase_Chat.Models;

namespace Showcase_Chat.Data
{
    // Wordt eenmalig aangeroepen bij opstarten om rollen en admin-account aan te maken
    public static class DbSeeder
    {
        public static async Task SeedAsync(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration config,
            ILogger logger)
        {
            // Rollen aanmaken als ze nog niet bestaan
            foreach (var role in new[] { "Admin", "User" })
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                    logger.LogInformation("Rol '{Role}' aangemaakt", role);
                }
            }

            // Standaard admin-account aanmaken via user-secrets
            // dotnet user-secrets set "AdminSeed:Email" "admin@showcase.nl"
            // dotnet user-secrets set "AdminSeed:Password" "Admin@1234!"
            var adminEmail = config["AdminSeed:Email"];
            var adminPassword = config["AdminSeed:Password"];

            if (!string.IsNullOrEmpty(adminEmail) && !string.IsNullOrEmpty(adminPassword))
            {
                if (await userManager.FindByEmailAsync(adminEmail) == null)
                {
                    var admin = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        DisplayName = "Admin",
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(admin, adminPassword);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(admin, "Admin");
                        logger.LogInformation("Admin-account '{Email}' aangemaakt", adminEmail);
                    }
                    else
                    {
                        logger.LogWarning("Aanmaken admin mislukt: {Errors}",
                            string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
            }
        }
    }
}
