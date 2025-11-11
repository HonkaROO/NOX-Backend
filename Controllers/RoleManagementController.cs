using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NOX_Backend.Models;

namespace NOX_Backend.Controllers;

/// <summary>
/// API controller for managing roles and user role assignments.
/// Only SuperAdmin role can access these endpoints.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin")]
public class RoleManagementController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AppDbContext _context;
    private readonly ILogger<RoleManagementController> _logger;

    public RoleManagementController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        AppDbContext context,
        ILogger<RoleManagementController> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all available roles in the system.
    /// </summary>
    /// <returns>List of all roles</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<string>>> GetAllRoles()
    {
        try
        {
            var roles = await _roleManager.Roles
                .Where(r => r.Name != null)
                .Select(r => r.Name)
                .ToListAsync();
            return Ok(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles");
            return StatusCode(500, new { message = "An error occurred while retrieving roles" });
        }
    }

    /// <summary>
    /// Get all roles assigned to a specific user.
    /// </summary>
    /// <param name="userId">The user's ID</param>
    /// <returns>List of roles assigned to the user</returns>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IList<string>>> GetUserRoles(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles for user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while retrieving user roles" });
        }
    }

    /// <summary>
    /// Assign a role to a user. SuperAdmin only.
    /// </summary>
    /// <param name="userId">The user's ID</param>
    /// <param name="request">Role assignment request</param>
    /// <returns>Assignment result</returns>
    [HttpPost("user/{userId}/assign")]
    public async Task<IActionResult> AssignRoleToUser(string userId, [FromBody] AssignRoleRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Validate user exists
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Validate role exists
            var roleExists = await _roleManager.RoleExistsAsync(request.RoleName);
            if (!roleExists)
            {
                return BadRequest(new { message = $"Role '{request.RoleName}' does not exist" });
            }

            // Check if user already has this role
            var hasRole = await _userManager.IsInRoleAsync(user, request.RoleName);
            if (hasRole)
            {
                return BadRequest(new { message = $"User already has the '{request.RoleName}' role" });
            }

            // Assign role to user
            var result = await _userManager.AddToRoleAsync(user, request.RoleName);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new { message = "Failed to assign role", errors });
            }

            _logger.LogInformation("Role '{RoleName}' assigned to user {UserId} by SuperAdmin", request.RoleName, userId);
            return Ok(new { message = $"Role '{request.RoleName}' assigned successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role to user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while assigning the role" });
        }
    }

    /// <summary>
    /// Remove a role from a user. SuperAdmin only.
    /// </summary>
    /// <param name="userId">The user's ID</param>
    /// <param name="roleName">The role name to remove</param>
    /// <returns>Removal result</returns>
    [HttpDelete("user/{userId}/remove/{roleName}")]
    public async Task<IActionResult> RemoveRoleFromUser(string userId, string roleName)
    {
        try
        {
            // Validate user exists
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Check if user has this role
            var hasRole = await _userManager.IsInRoleAsync(user, roleName);
            if (!hasRole)
            {
                return BadRequest(new { message = $"User does not have the '{roleName}' role" });
            }

            // Prevent SuperAdmin from removing the SuperAdmin role from themselves
            var currentUserId = _userManager.GetUserId(User);
            if (roleName == "SuperAdmin" && user.Id == currentUserId)
            {
                return BadRequest(new { message = "You cannot remove the SuperAdmin role from your own account" });
            }

            // Remove role from user
            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new { message = "Failed to remove role", errors });
            }

            _logger.LogInformation("Role '{RoleName}' removed from user {UserId} by SuperAdmin", roleName, userId);
            return Ok(new { message = $"Role '{roleName}' removed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role from user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while removing the role" });
        }
    }

    /// <summary>
    /// Get all users assigned to a specific role.
    /// </summary>
    /// <param name="roleName">The role name</param>
    /// <returns>List of users with the specified role</returns>
    [HttpGet("{roleName}/users")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsersInRole(string roleName)
    {
        try
        {
            // Validate role exists
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                return NotFound(new { message = $"Role '{roleName}' does not exist" });
            }

            // Get all users in the role
            var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
            var userIds = usersInRole.Select(u => u.Id).ToList();

            // Load all user-role mappings in a single query to avoid N+1
            var rolesDictionary = await _context.UserRoles
                .Where(ur => userIds.Contains(ur.UserId))
                .Join(_context.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => new { ur.UserId, RoleName = r.Name })
                .GroupBy(x => x.UserId)
                .ToDictionaryAsync(g => g.Key, g => g.Select(x => x.RoleName ?? string.Empty).ToList());

            var userDtos = new List<UserDto>();
            foreach (var user in usersInRole)
            {
                var roles = rolesDictionary.ContainsKey(user.Id) ? rolesDictionary[user.Id] : new List<string>();
                userDtos.Add(await MapToUserDtoAsync(user, roles));
            }

            return Ok(userDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users for role {RoleName}", roleName);
            return StatusCode(500, new { message = "An error occurred while retrieving users in role" });
        }
    }

    /// <summary>
    /// Helper method to map ApplicationUser to UserDto.
    /// </summary>
    private async Task<UserDto> MapToUserDtoAsync(ApplicationUser user, IList<string> roles)
    {
        var department = await _context.Departments.FindAsync(user.DepartmentId);
        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            DepartmentId = user.DepartmentId,
            DepartmentName = department?.Name,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            Roles = roles
        };
    }
}
