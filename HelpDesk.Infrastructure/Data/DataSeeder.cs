using HelpDesk.Core.Entities;
using HelpDesk.Core.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Infrastructure.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            // Resolve the Identity managers from the DI container
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1. Seed Roles
            string[] roleNames = {
                UserRole.Admin.ToString(),
                UserRole.SupportAgent.ToString(),
                UserRole.RegularUser.ToString()
            };

            foreach (var roleName in roleNames)
            {
                // Check if the role already exists before creating it
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Seed Admin User
            string adminEmail = "admin@helpdesk.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            // Check if the admin user already exists
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    EmailConfirmed = true,

                    // NEW: Assign the Admin to the default 'General' department we seeded in DbContext!
                    DepartmentId = 1
                };

                // Create the user with a default password
                var result = await userManager.CreateAsync(adminUser, "Admin@123");

                if (result.Succeeded)
                {
                    // Assign the Admin role to the newly created user
                    await userManager.AddToRoleAsync(adminUser, UserRole.Admin.ToString());
                }
            }
        }
    }
}