using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using OnlineCourses.Infrastructure.Identity;

namespace OnlineCourses.Infrastructure.Data
{
    public class DbSeeder
    {
        public static async Task SeedAsync(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            // Seed Roles
            string[] roleNames = { "Admin", "User" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Seed Admin
            var adminEmail = configuration["ADMIN_EMAIL"] ?? "admin@example.com";
            var adminPassword = configuration["ADMIN_PASSWORD"] ?? "AdminPassword123!";

            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin == null)
            {
                var adminUser = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Seed Regular User
            var userEmail = "user@example.com";
            var userPassword = "UserPassword123!";
            var existingUser = await userManager.FindByEmailAsync(userEmail);
            if (existingUser == null)
            {
                var regularUser = new AppUser
                {
                    UserName = userEmail,
                    Email = userEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(regularUser, userPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(regularUser, "User");
                }
            }
        }
    }
}
