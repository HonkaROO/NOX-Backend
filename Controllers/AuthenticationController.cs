using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NOX_Backend.Models;
using NOX_Backend.Models.DTOs;

namespace NOX_Backend.Controllers;

/// <summary>
/// API controller for user authentication: login, logout, and profile management.
/// Uses cookie-based authentication via ASP.NET Core Identity.
///
/// Public Endpoints (no authorization required):
/// - POST /login: Authenticates user with email/password; creates authentication cookie.
/// - GET /access-denied: Returns 403 Forbidden for access denied scenarios.
///
/// Protected Endpoints ([Authorize] required):
/// - POST /logout: Clears authentication cookie and logs user out.
/// - GET /me: Returns current authenticated user's profile.
/// - PUT /me: Updates current user's profile information (FirstName, LastName, Phone, Address).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly AppDbContext _context;
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        AppDbContext context,
        ILogger<AuthenticationController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Login with email and password. Creates authentication cookie automatically.
    /// No authentication required.
    /// </summary>
    /// <param name="request">Email and password</param>
    /// <returns>User information if successful</returns>
    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Try to sign in the user (UserName is guaranteed non-null as it's set to Email on creation)
            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!,
                request.Password,
                isPersistent: false,
                lockoutOnFailure: true);

            // Check if user is active (after successful password validation)
            if (result.Succeeded && !user.IsActive)
            {
                await _signInManager.SignOutAsync();
                return Unauthorized(new { message = "Invalid email or password" });
            }

            if (!result.Succeeded)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            var roles = await _userManager.GetRolesAsync(user);
            _logger.LogInformation("User {UserId} logged in successfully", user.Id);

            return Ok(await MapToUserDtoAsync(user, roles));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging in user");
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }

    /// <summary>
    /// Logout the current authenticated user.
    /// Clears the authentication cookie.
    /// Requires valid authentication.
    /// </summary>
    /// <returns>Success message</returns>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User {UserId} logged out", userId);
            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging out user");
            return StatusCode(500, new { message = "An error occurred during logout" });
        }
    }

    /// <summary>
    /// Get HTTP 403 Forbidden response for access denied.
    /// Used by cookie configuration.
    /// </summary>
    [HttpGet("access-denied")]
    public IActionResult AccessDenied()
    {
        return StatusCode(StatusCodes.Status403Forbidden, new { message = "Access denied" });
    }

    /// <summary>
    /// Get current authenticated user's profile information.
    /// Requires valid authentication cookie.
    /// </summary>
    /// <returns>Current user's profile information</returns>
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(await MapToUserDtoAsync(user, roles));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current user profile");
            return StatusCode(500, new { message = "An error occurred while retrieving your profile" });
        }
    }

    /// <summary>
    /// Update the current authenticated user's profile information.
    /// Users can only update their own profile.
    /// Requires valid authentication cookie.
    /// </summary>
    /// <param name="request">Updated profile information</param>
    /// <returns>Updated user profile</returns>
    [Authorize]
    [HttpPut("me")]
    public async Task<ActionResult<UserDto>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Update provided fields
            if (!string.IsNullOrEmpty(request.FirstName))
                user.FirstName = request.FirstName;
            if (!string.IsNullOrEmpty(request.LastName))
                user.LastName = request.LastName;
            if (!string.IsNullOrEmpty(request.Phone))
                user.Phone = request.Phone;
            if (!string.IsNullOrEmpty(request.Address))
                user.Address = request.Address;

            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new { message = "Failed to update profile", errors });
            }

            var roles = await _userManager.GetRolesAsync(user);
            _logger.LogInformation("User {UserId} updated their profile", user.Id);
            return Ok(await MapToUserDtoAsync(user, roles));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile");
            return StatusCode(500, new { message = "An error occurred while updating your profile" });
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
            Phone = user.Phone,
            Address = user.Address,
            StartDate = user.StartDate,
            EmployeeId = user.EmployeeId,
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
