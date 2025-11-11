using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NOX_Backend.Models;

namespace NOX_Backend.Services;

/// <summary>
/// Service for seeding initial roles and SuperAdmin user in the database.
/// This runs once during application startup to ensure default roles exist.
///
/// SECURITY NOTE: Default SuperAdmin credentials are configured via appsettings.json and should be
/// changed immediately in production. The default user is marked as requiring a password change on first login.
/// </summary>
public class RoleSeederService
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RoleSeederService> _logger;

    public RoleSeederService(
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        AppDbContext context,
        IConfiguration configuration,
        ILogger<RoleSeederService> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _context = context;
        _configuration = configuration;
        _logger = logger;
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
                    var errorCodes = string.Join(", ", result.Errors.Select(e => e.Code));
                    throw new InvalidOperationException(
                        $"Failed to create role '{role}'. Error codes: {errorCodes}");
                }
            }
        }

        // Create default SuperAdmin user if no users exist yet
        if (await _userManager.Users.CountAsync() == 0)
        {
            // Get default superadmin credentials from configuration
            var defaultSuperAdminEmail = _configuration["Security:DefaultSuperAdminEmail"] ?? "superadmin@nox.local";
            var defaultSuperAdminPassword = _configuration["Security:DefaultSuperAdminPassword"];

            if (string.IsNullOrEmpty(defaultSuperAdminPassword))
            {
                throw new InvalidOperationException(
                    "Default SuperAdmin password not configured. Set 'Security:DefaultSuperAdminPassword' in appsettings.json or environment variables.");
            }

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
                UserName = defaultSuperAdminEmail,
                Email = defaultSuperAdminEmail,
                EmailConfirmed = true,
                FirstName = "Super",
                LastName = "Administrator",
                DepartmentId = systemAdminDepartment.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(superAdmin, defaultSuperAdminPassword);
            if (!result.Succeeded)
            {
                var errorCount = result.Errors.Count();
                _logger.LogError("Failed to create SuperAdmin user: {ErrorCount} error(s) occurred", errorCount);
                throw new InvalidOperationException(
                    $"Failed to create SuperAdmin user. See logs for details.");
            }

            // Assign SuperAdmin role to the created user
            var roleResult = await _userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
            if (!roleResult.Succeeded)
            {
                var errorCount = roleResult.Errors.Count();
                _logger.LogError("Failed to assign SuperAdmin role: {ErrorCount} error(s) occurred", errorCount);
                throw new InvalidOperationException(
                    $"Failed to assign SuperAdmin role. See logs for details.");
            }

            // Set the SuperAdmin as manager of the System Administration department
            systemAdminDepartment.ManagerId = superAdmin.Id;
            _context.Departments.Update(systemAdminDepartment);
            await _context.SaveChangesAsync();

            _logger.LogWarning("Default SuperAdmin user created. IMPORTANT: Change the default password immediately in production!");
        }
    }
}
