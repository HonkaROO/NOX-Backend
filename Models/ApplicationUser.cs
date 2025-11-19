using Microsoft.AspNetCore.Identity;

namespace NOX_Backend.Models;

/// <summary>
/// Extended user model that adds custom properties to ASP.NET Core Identity's IdentityUser.
/// This allows us to store additional user information beyond the standard Identity fields.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// User's first name.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// User's last name.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Foreign key to the department this user belongs to.
    /// </summary>
    public int DepartmentId { get; set; }

    /// <summary>
    /// Navigation property: Department this user belongs to.
    /// </summary>
    public virtual Department? Department { get; set; }

    /// <summary>
    /// Navigation property: Department managed by this user (if any).
    /// A user can manage at most one department, and must belong to that department.
    /// </summary>
    public virtual Department? ManagedDepartment { get; set; }

    /// <summary>
    /// Indicates whether the user account is active. Allows for soft deactivation without deleting.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Timestamp when the user account was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the user account was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// User's phone number.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// User's physical address.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// User's employment start date.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Unique employee ID (e.g., "NPAX-2024-001").
    /// </summary>
    public string? EmployeeId { get; set; }

    /// <summary>
    /// Gets the user's full name by combining first and last names.
    /// </summary>
    public string GetFullName() => $"{FirstName} {LastName}".Trim();

}
