using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NOX_Backend.Models;

namespace NOX_Backend.Services;

/// <summary>
/// Service responsible for seeding the database with default departments.
/// Runs on application startup to ensure default departments always exist.
/// </summary>
public class DepartmentSeederService
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the DepartmentSeederService.
    /// </summary>
    public DepartmentSeederService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Seeds the database with default departments.
    /// Creates a default "Unassigned" department for users without a specific department.
    /// This method is idempotent - it will not create duplicate departments.
    /// </summary>
    public async Task SeedAsync()
    {
        try
        {
            // Check if departments already exist
            if (await _context.Departments.AnyAsync())
            {
                System.Console.WriteLine("Departments already exist. Skipping seeding.");
                return;
            }

            // Define default departments
            var defaultDepartments = new[]
            {
                new Department
                {
                    Name = "Unassigned",
                    Description = "Default department for users without a specific assignment",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Department
                {
                    Name = "Engineering",
                    Description = "Software development and engineering team",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Department
                {
                    Name = "Human Resources",
                    Description = "HR and personnel management",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Department
                {
                    Name = "Sales",
                    Description = "Sales and business development",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Department
                {
                    Name = "Support",
                    Description = "Customer support and success",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await _context.Departments.AddRangeAsync(defaultDepartments);
            await _context.SaveChangesAsync();

            System.Console.WriteLine("✓ Default departments created successfully.");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"✗ Error seeding departments: {ex.Message}");
            throw;
        }
    }
}
