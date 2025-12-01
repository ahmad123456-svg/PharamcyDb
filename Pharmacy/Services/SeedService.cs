using Microsoft.AspNetCore.Identity;
using Pharmacy.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Pharmacy.Services
{
    public class SeedService
    {
        public static async Task SeedData(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<Pharmacy.Data.AppDbContext>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Users>>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<SeedService>>();

                try
                {
                    // Ensure the database is ready
                    logger.LogInformation("Starting data seeding...");
                    await context.Database.EnsureCreatedAsync();

                    // Add roles
                    logger.LogInformation("Seeding roles...");
                    await AddRoleAsync(roleManager, "SuperAdmin");
                    await AddRoleAsync(roleManager, "Admin");
                    await AddRoleAsync(roleManager, "User");

                    // Add SuperAdmin user
                    logger.LogInformation("Seeding super admin user...");
                    var superAdminEmail = "superadmin@codehub.com";
                    var superAdminUser = await userManager.FindByEmailAsync(superAdminEmail);
                    if (superAdminUser == null)
                    {
                        superAdminUser = new Users
                        {
                            FullName = "Super Admin",
                            UserName = superAdminEmail,
                            Email = superAdminEmail,
                            EmailConfirmed = true,
                            NormalizedUserName = superAdminEmail.ToUpper(),
                            NormalizedEmail = superAdminEmail.ToUpper(),
                            SecurityStamp = Guid.NewGuid().ToString()
                        };

                        var result = await userManager.CreateAsync(superAdminUser, "SuperAdmin@123");
                        if (result.Succeeded)
                        {
                            logger.LogInformation("Assigning SuperAdmin role to the super admin user.");
                            await userManager.AddToRoleAsync(superAdminUser, "SuperAdmin");
                        }
                        else
                        {
                             logger.LogError("Failed to create super admin user: {Errors}",
                                string.Join(", ", result.Errors.Select(e => e.Description)));
                        }
                    }

                    // Add admin user
                    logger.LogInformation("Seeding admin user...");
                    var adminEmail = "admin@codehub.com";
                    var adminUser = await userManager.FindByEmailAsync(adminEmail);
                    if (adminUser == null)
                    {
                        adminUser = new Users
                        {
                            FullName = "Code Hub",
                            UserName = adminEmail,
                            Email = adminEmail,
                            EmailConfirmed = true,
                            NormalizedUserName = adminEmail.ToUpper(),
                            NormalizedEmail = adminEmail.ToUpper(),
                            SecurityStamp = Guid.NewGuid().ToString()
                        };

                        var result = await userManager.CreateAsync(adminUser, "Admin@123");
                        if (result.Succeeded)
                        {
                            logger.LogInformation("Assigning Admin role to the admin user.");
                            await userManager.AddToRoleAsync(adminUser, "Admin");
                        }
                        else
                        {
                            logger.LogError("Failed to create admin user: {Errors}",
                                string.Join(", ", result.Errors.Select(e => e.Description)));
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }
        }

        // Helper method added to fix the error
        private static async Task AddRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(roleName));

                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        } 
    }     
}
