using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NOX_Backend.Models;

namespace NOX_Backend.Services;

/// <summary>
/// Service for seeding initial roles and SuperAdmin user in the database.
/// This runs once during application startup to ensure default roles exist.
/// </summary>
public class RoleSeederService
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _context;

    public RoleSeederService(
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        AppDbContext context)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _context = context;
    }

    /// <summary>
    /// Seeds default roles and creates a default SuperAdmin user if they don't exist.
    /// </summary>
    public async Task SeedAsync()
    {
        // Define the three roles required by the NOX system
        string[] roles = { "SuperAdmin", "Admin", "User" };

        // Create roles if they don't already exist
        foreach (string role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                var result = await _roleManager.CreateAsync(new IdentityRole(role));
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to create role '{role}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

        // Create default SuperAdmin user if no users exist yet
        if (await _userManager.Users.CountAsync() == 0)
        {
            // Ensure "System Administration" department exists
            var systemAdminDepartment = await _context.Departments
                .FirstOrDefaultAsync(d => d.Name == "System Administration");

            if (systemAdminDepartment == null)
            {
                systemAdminDepartment = new Department
                {
                    Name = "System Administration",
                    Description = "System administrators and technical staff",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Departments.Add(systemAdminDepartment);
                await _context.SaveChangesAsync();
            }

            var superAdmin = new ApplicationUser
            {
                UserName = "superadmin@nox.local",
                Email = "superadmin@nox.local",
                EmailConfirmed = true,
                FirstName = "Super",
                LastName = "Administrator",
                DepartmentId = systemAdminDepartment.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(superAdmin, "SuperAdmin@2024!Nox");
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create SuperAdmin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            // Assign SuperAdmin role to the created user
            var roleResult = await _userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to assign SuperAdmin role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            }

            // Set the SuperAdmin as manager of the System Administration department
            systemAdminDepartment.ManagerId = superAdmin.Id;
            _context.Departments.Update(systemAdminDepartment);
            await _context.SaveChangesAsync();
        }
    }
}
