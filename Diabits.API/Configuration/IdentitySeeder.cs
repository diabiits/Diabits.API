using Diabits.API.Models;
using Microsoft.AspNetCore.Identity;

namespace Diabits.API.Configuration;

/// <summary>
/// Ensures required Identity roles exist and creates an initial admin user from configuration if missing.
/// </summary>
public static class IdentitySeeder
{
    public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<DiabitsUser>>();

        // Application roles used by authorization checks in the app.
        var roles = new[] { "Admin", "User" };

        // Ensure each required role exists; create it when missing.
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Read admin credentials from configuration; throw to fail fast in case of missing secrets.
        var config = serviceProvider.GetRequiredService<IConfiguration>();

        var adminEmail = config["Credentials:AdminEmail"] ?? throw new Exception("AdminEmail is not set in config");
        var adminPassword = config["Credentials:AdminPassword"] ?? throw new Exception("AdminPassword is not set in config");

        // If an admin account with the configured email doesn't exist, create one and assign the Admin role.
        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin == null)
        {
            var adminUser = new DiabitsUser
            {
                UserName = "admin",
                Email = adminEmail
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded) await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}