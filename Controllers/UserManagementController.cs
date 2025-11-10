using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NOX_Backend.Models;

namespace NOX_Backend.Controllers;

/// <summary>
/// API controller for managing users. Only SuperAdmin role can access these endpoints.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin")]
public class UserManagementController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<UserManagementController> _logger;

    public UserManagementController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<UserManagementController> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    /// <summary>
    /// Get all users with their assigned roles.
    /// </summary>
    /// <returns>List of all users with role information</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
    {
        try
        {
            var users = await _userManager.Users.ToListAsync();
            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDtos.Add(MapToUserDto(user, roles));
            }

            return Ok(userDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all users");
            return StatusCode(500, new { message = "An error occurred while retrieving users" });
        }
    }

    /// <summary>
    /// Get a specific user by ID with their roles.
    /// </summary>
    /// <param name="userId">The user's ID</param>
    /// <returns>User details with assigned roles</returns>
    [HttpGet("{userId}")]
    public async Task<ActionResult<UserDto>> GetUserById(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(MapToUserDto(user, roles));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while retrieving the user" });
        }
    }

    /// <summary>
    /// Create a new user account. SuperAdmin only.
    /// </summary>
    /// <param name="request">User creation request with credentials and details</param>
    /// <returns>Created user information</returns>
    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "User with this email already exists" });
            }

            // Create new user
            var user = new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email,
                EmailConfirmed = true,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Department = request.Department,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new { message = "Failed to create user", errors });
            }

            // Assign default "User" role to new users
            await _userManager.AddToRoleAsync(user, "User");

            var roles = await _userManager.GetRolesAsync(user);
            _logger.LogInformation("User {UserId} created by SuperAdmin", user.Id);
            return CreatedAtAction(nameof(GetUserById), new { userId = user.Id }, MapToUserDto(user, roles));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, new { message = "An error occurred while creating the user" });
        }
    }

    /// <summary>
    /// Update user information. SuperAdmin only.
    /// </summary>
    /// <param name="userId">The user's ID</param>
    /// <param name="request">User update request</param>
    /// <returns>Updated user information</returns>
    [HttpPut("{userId}")]
    public async Task<ActionResult<UserDto>> UpdateUser(string userId, [FromBody] UpdateUserRequest request)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Update user properties
            if (!string.IsNullOrEmpty(request.FirstName))
                user.FirstName = request.FirstName;
            if (!string.IsNullOrEmpty(request.LastName))
                user.LastName = request.LastName;
            if (!string.IsNullOrEmpty(request.Department))
                user.Department = request.Department;
            if (request.IsActive.HasValue)
                user.IsActive = request.IsActive.Value;

            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new { message = "Failed to update user", errors });
            }

            var roles = await _userManager.GetRolesAsync(user);
            _logger.LogInformation("User {UserId} updated by SuperAdmin", userId);
            return Ok(MapToUserDto(user, roles));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while updating the user" });
        }
    }

    /// <summary>
    /// Deactivate a user account. SuperAdmin only.
    /// </summary>
    /// <param name="userId">The user's ID</param>
    /// <returns>Deactivation result</returns>
    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeactivateUser(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Don't allow deactivation of the account making the request
            if (user.Id == User.FindFirst("sub")?.Value)
            {
                return BadRequest(new { message = "You cannot deactivate your own account" });
            }

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new { message = "Failed to deactivate user", errors });
            }

            _logger.LogInformation("User {UserId} deactivated by SuperAdmin", userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while deactivating the user" });
        }
    }

    /// <summary>
    /// Reset a user's password. SuperAdmin only.
    /// </summary>
    /// <param name="userId">The user's ID</param>
    /// <param name="request">New password request</param>
    /// <returns>Password reset result</returns>
    [HttpPost("{userId}/reset-password")]
    public async Task<IActionResult> ResetPassword(string userId, [FromBody] ResetPasswordRequest request)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Remove old password
            var removeResult = await _userManager.RemovePasswordAsync(user);
            if (!removeResult.Succeeded)
            {
                var errors = string.Join(", ", removeResult.Errors.Select(e => e.Description));
                return BadRequest(new { message = "Failed to reset password", errors });
            }

            // Add new password
            var addResult = await _userManager.AddPasswordAsync(user, request.NewPassword);
            if (!addResult.Succeeded)
            {
                var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                return BadRequest(new { message = "Failed to set new password", errors });
            }

            _logger.LogInformation("Password reset for user {UserId} by SuperAdmin", userId);
            return Ok(new { message = "Password has been reset successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while resetting the password" });
        }
    }

    /// <summary>
    /// Helper method to map ApplicationUser to UserDto.
    /// </summary>
    private UserDto MapToUserDto(ApplicationUser user, IList<string> roles)
    {
        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Department = user.Department,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            Roles = roles
        };
    }
}
