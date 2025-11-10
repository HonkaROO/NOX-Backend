namespace NOX_Backend.Models;

/// <summary>
/// Data Transfer Object for user information in API responses.
/// </summary>
public class UserDto
{
    public string Id { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Department { get; set; }
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public IList<string>? Roles { get; set; }
}

/// <summary>
/// Request DTO for creating a new user.
/// </summary>
public class CreateUserRequest
{
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Department { get; set; }
}

/// <summary>
/// Request DTO for updating user information.
/// </summary>
public class UpdateUserRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Department { get; set; }
    public bool? IsActive { get; set; }
}

/// <summary>
/// Request DTO for resetting a user's password.
/// </summary>
public class ResetPasswordRequest
{
    public string NewPassword { get; set; } = null!;
}

/// <summary>
/// Request DTO for assigning a role to a user.
/// </summary>
public class AssignRoleRequest
{
    public string RoleName { get; set; } = null!;
}
