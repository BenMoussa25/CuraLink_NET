using CuraLink.Domain.Common;
using CuraLink.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace CuraLink.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var roleName in new[] { AppRoles.Admin, AppRoles.Doctor, AppRoles.Patient })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }

    public static async Task<ApplicationUser> CreateUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string firstName,
        string lastName,
        string password,
        string role,
        DateTime nowUtc)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = firstName,
            LastName = lastName,
            CreatedAtUtc = nowUtc,
            IsActive = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"The demo account for the {role} role could not be created.");
        }

        await userManager.AddToRoleAsync(user, role);
        return user;
    }
}
