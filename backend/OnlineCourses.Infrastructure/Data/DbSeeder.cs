using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using OnlineCourses.Infrastructure.Identity;

namespace OnlineCourses.Infrastructure.Data
{
    public class DbSeeder
    {
        public static async Task SeedAsync(UserManager<AppUser> userManager, IConfiguration configuration)
        {
            var adminEmail = configuration["ADMIN_EMAIL"];
            var adminPassword = configuration["ADMIN_PASSWORD"];

            if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
            {
                // Optionally log warning or return
                return;
            }

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
                    // Optionally add role if needed
                    // await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}
