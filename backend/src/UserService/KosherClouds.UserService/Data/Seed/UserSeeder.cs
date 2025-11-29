using KosherClouds.Common.Seed;
using KosherClouds.UserService.Entities;
using Microsoft.AspNetCore.Identity;

namespace KosherClouds.UserService.Data.Seed
{
    public static class UserSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles = [SharedSeedData.RoleAdmin, SharedSeedData.RoleManager, SharedSeedData.RoleUser];

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new ApplicationRole { Name = role });
            }

            if (await userManager.FindByEmailAsync(SharedSeedData.AdminEmail) is null)
            {
                var admin = new ApplicationUser
                {
                    Id = SharedSeedData.AdminId,
                    UserName = "Admin",
                    Email = SharedSeedData.AdminEmail,
                    FirstName = SharedSeedData.AdminFirstName,
                    LastName = SharedSeedData.AdminLastName,
                    EmailConfirmed = true,
                    PhoneNumber = SharedSeedData.AdminPhoneNumber,
                    PhoneNumberConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, SharedSeedData.AdminPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, SharedSeedData.RoleAdmin);
            }

            if (await userManager.FindByEmailAsync(SharedSeedData.ManagerEmail) is null)
            {
                var manager = new ApplicationUser
                {
                    Id = SharedSeedData.ManagerId,
                    UserName = "Manager",
                    Email = SharedSeedData.ManagerEmail,
                    FirstName = SharedSeedData.ManagerFirstName,
                    LastName = SharedSeedData.ManagerLastName,
                    EmailConfirmed = true,
                    PhoneNumber = SharedSeedData.ManagerPhoneNumber,
                    PhoneNumberConfirmed = true
                };

                var result = await userManager.CreateAsync(manager, SharedSeedData.ManagerPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(manager, SharedSeedData.RoleManager);
            }

            if (await userManager.FindByEmailAsync(SharedSeedData.UserEmail) is null)
            {
                var user = new ApplicationUser
                {
                    Id = SharedSeedData.UserId,
                    UserName = "User",
                    Email = SharedSeedData.UserEmail,
                    FirstName = SharedSeedData.UserFirstName,
                    LastName = SharedSeedData.UserLastName,
                    EmailConfirmed = true,
                    PhoneNumber = SharedSeedData.UserPhoneNumber,
                    PhoneNumberConfirmed = true
                };

                var result = await userManager.CreateAsync(user, SharedSeedData.UserPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(user, SharedSeedData.RoleUser);
            }
        }
    }
}