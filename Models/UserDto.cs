namespace NOX_Backend.Models;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateTime? StartDate { get; set; }
    public string? EmployeeId { get; set; }
    public int DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
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
    [Required]
    [StringLength(100)]
    public string UserName { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    [StringLength(100)]
    public string? FirstName { get; set; }

    [StringLength(100)]
    public string? LastName { get; set; }

    [Phone]
    public string? Phone { get; set; }

    [StringLength(255)]
    public string? Address { get; set; }

    public DateTime? StartDate { get; set; }

    [StringLength(50)]
    public string? EmployeeId { get; set; }

    [Required(ErrorMessage = "Department ID is required.")]
    public int DepartmentId { get; set; }

    [StringLength(50)]
    public string? Role { get; set; }
}

/// <summary>
/// Request DTO for updating user information.
/// </summary>
public class UpdateUserRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    [Phone]
    public string? Phone { get; set; }

    [StringLength(255)]
    public string? Address { get; set; }

    public DateTime? StartDate { get; set; }

    [StringLength(50)]
    public string? EmployeeId { get; set; }

    public int? DepartmentId { get; set; }
    public bool? IsActive { get; set; }
}

/// <summary>
/// Request DTO for resetting a user's password.
/// </summary>
public class ResetPasswordRequest
{
    [Required]
    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = null!;

    [Required]
    public string ResetToken { get; set; } = null!;

    public string? UserId { get; set; }
}

/// <summary>
/// Request DTO for assigning a role to a user.
/// </summary>
public class AssignRoleRequest
{
    public string RoleName { get; set; } = null!;
}

/// <summary>
/// Request DTO for user login.
/// </summary>
public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;
}

/// <summary>
/// Request DTO for user updating their own profile.
/// </summary>
public class UpdateProfileRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    [Phone]
    public string? Phone { get; set; }

    [StringLength(255)]
    public string? Address { get; set; }
}
