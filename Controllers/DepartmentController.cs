using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NOX_Backend.Models;

namespace NOX_Backend.Controllers;

/// <summary>
/// Controller for managing departments.
/// Provides CRUD operations for departments with role-based authorization.
/// </summary>
[ApiController]
[Route("api/departments")]
public class DepartmentController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    /// Initializes a new instance of the DepartmentController.
    /// </summary>
    public DepartmentController(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    /// <summary>
    /// Gets all departments.
    /// </summary>
    /// <returns>List of all departments with user count.</returns>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetDepartments()
    {
        var departments = await _context.Departments
            .Include(d => d.Users)
            .Include(d => d.Manager)
            .OrderBy(d => d.Name)
            .ToListAsync();

        var departmentDtos = departments.Select(MapToDepartmentDto).ToList();

        return Ok(departmentDtos);
    }

    /// <summary>
    /// Gets a specific department by ID.
    /// </summary>
    /// <param name="id">The department ID.</param>
    /// <returns>The requested department with details.</returns>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<DepartmentDto>> GetDepartment(int id)
    {
        var department = await _context.Departments
            .Include(d => d.Users)
            .Include(d => d.Manager)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (department == null)
        {
            return NotFound(new { message = "Department not found." });
        }

        return Ok(MapToDepartmentDto(department));
    }

    /// <summary>
    /// Creates a new department.
    /// Requires SuperAdmin or Admin role.
    /// </summary>
    /// <param name="request">The department creation request.</param>
    /// <returns>The created department.</returns>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<DepartmentDto>> CreateDepartment(CreateDepartmentRequest request)
    {
        // Validate unique department name
        var existingDepartment = await _context.Departments
            .FirstOrDefaultAsync(d => d.Name == request.Name);

        if (existingDepartment != null)
        {
            return BadRequest(new { message = "A department with this name already exists." });
        }

        var department = new Department
        {
            Name = request.Name,
            Description = request.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // If a manager is specified, validate they belong to this department
        if (!string.IsNullOrEmpty(request.ManagerId))
        {
            var manager = await _userManager.FindByIdAsync(request.ManagerId);
            if (manager == null)
            {
                return BadRequest(new { message = "Manager user not found." });
            }

            department.ManagerId = request.ManagerId;
        }

        _context.Departments.Add(department);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDepartment), new { id = department.Id }, MapToDepartmentDto(department));
    }

    /// <summary>
    /// Updates an existing department.
    /// Requires SuperAdmin or Admin role.
    /// </summary>
    /// <param name="id">The department ID.</param>
    /// <param name="request">The department update request.</param>
    /// <returns>The updated department.</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<DepartmentDto>> UpdateDepartment(int id, UpdateDepartmentRequest request)
    {
        var department = await _context.Departments
            .Include(d => d.Users)
            .Include(d => d.Manager)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (department == null)
        {
            return NotFound(new { message = "Department not found." });
        }

        // Check if new name conflicts with another department
        if (request.Name != department.Name)
        {
            var conflictingDepartment = await _context.Departments
                .FirstOrDefaultAsync(d => d.Name == request.Name && d.Id != id);

            if (conflictingDepartment != null)
            {
                return BadRequest(new { message = "A department with this name already exists." });
            }

            department.Name = request.Name;
        }

        department.Description = request.Description;
        department.UpdatedAt = DateTime.UtcNow;

        // Update manager if specified
        if (!string.IsNullOrEmpty(request.ManagerId))
        {
            var manager = await _userManager.FindByIdAsync(request.ManagerId);
            if (manager == null)
            {
                return BadRequest(new { message = "Manager user not found." });
            }

            if (manager.DepartmentId != id)
            {
                return BadRequest(new { message = "Manager must belong to this department." });
            }

            department.ManagerId = request.ManagerId;
        }
        else
        {
            // Allow clearing the manager
            department.ManagerId = null;
        }

        _context.Departments.Update(department);
        await _context.SaveChangesAsync();

        return Ok(MapToDepartmentDto(department));
    }

    /// <summary>
    /// Assigns a manager to a department.
    /// The manager must belong to the department.
    /// Requires SuperAdmin or Admin role.
    /// </summary>
    /// <param name="id">The department ID.</param>
    /// <param name="request">The assign manager request containing the manager user ID.</param>
    /// <returns>The updated department.</returns>
    [HttpPut("{id}/manager")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<DepartmentDto>> AssignManager(int id, AssignManagerRequest request)
    {
        var department = await _context.Departments
            .Include(d => d.Users)
            .Include(d => d.Manager)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (department == null)
        {
            return NotFound(new { message = "Department not found." });
        }

        var manager = await _userManager.FindByIdAsync(request.ManagerId);
        if (manager == null)
        {
            return BadRequest(new { message = "Manager user not found." });
        }

        // Validate that manager belongs to this department
        if (manager.DepartmentId != id)
        {
            return BadRequest(new { message = "Manager must belong to this department." });
        }

        department.ManagerId = request.ManagerId;
        department.UpdatedAt = DateTime.UtcNow;

        _context.Departments.Update(department);
        await _context.SaveChangesAsync();

        return Ok(MapToDepartmentDto(department));
    }

    /// <summary>
    /// Soft-deletes a department by marking it as inactive.
    /// Requires SuperAdmin role.
    /// Cannot delete a department that has users assigned to it.
    /// </summary>
    /// <param name="id">The department ID.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> DeleteDepartment(int id)
    {
        var department = await _context.Departments
            .Include(d => d.Users)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (department == null)
        {
            return NotFound(new { message = "Department not found." });
        }

        if (department.Users.Any())
        {
            return BadRequest(new { message = "Cannot delete a department with assigned users. Please reassign users first." });
        }

        department.IsActive = false;
        department.UpdatedAt = DateTime.UtcNow;

        _context.Departments.Update(department);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Maps a Department entity to a DepartmentDto.
    /// </summary>
    private DepartmentDto MapToDepartmentDto(Department department)
    {
        return new DepartmentDto
        {
            Id = department.Id,
            Name = department.Name,
            Description = department.Description,
            IsActive = department.IsActive,
            CreatedAt = department.CreatedAt,
            UpdatedAt = department.UpdatedAt,
            UserCount = department.Users?.Count ?? 0,
            Manager = department.Manager == null
                ? null
                : new ManagerDto
                {
                    Id = department.Manager.Id,
                    Email = department.Manager.Email!,
                    FullName = department.Manager.GetFullName()
                }
        };
    }
}
