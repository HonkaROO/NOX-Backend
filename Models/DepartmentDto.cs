using System.ComponentModel.DataAnnotations;

namespace NOX_Backend.Models;

/// <summary>
/// Data Transfer Object for Department responses.
/// Used to return department information in API responses.
/// </summary>
public class DepartmentDto
{
    /// <summary>
    /// Unique identifier for the department.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Department name.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Optional description of the department.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Manager information (simplified).
    /// </summary>
    public ManagerDto? Manager { get; set; }

    /// <summary>
    /// Indicates if the department is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Timestamp when the department was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the department was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Count of users in the department.
    /// </summary>
    public int UserCount { get; set; }
}

/// <summary>
/// Simplified manager information for Department responses.
/// </summary>
public class ManagerDto
{
    /// <summary>
    /// User ID of the manager.
    /// </summary>
    public string Id { get; set; } = null!;

    /// <summary>
    /// Manager's email address.
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Manager's full name.
    /// </summary>
    public string FullName { get; set; } = null!;
}

/// <summary>
/// Request model for creating a new department.
/// </summary>
public class CreateDepartmentRequest
{
    /// <summary>
    /// Department name. Must be unique.
    /// </summary>
    [Required(ErrorMessage = "Department name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Department name must be between 2 and 100 characters.")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Optional description of the department's purpose.
    /// </summary>
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string? Description { get; set; }

    /// <summary>
    /// Optional user ID of the manager.
    /// If provided, the user must belong to this department (validated in controller).
    /// </summary>
    public string? ManagerId { get; set; }
}

/// <summary>
/// Request model for updating an existing department.
/// </summary>
public class UpdateDepartmentRequest
{
    /// <summary>
    /// Department name. Must be unique among other departments.
    /// </summary>
    [Required(ErrorMessage = "Department name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Department name must be between 2 and 100 characters.")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Optional description of the department's purpose.
    /// </summary>
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string? Description { get; set; }

    /// <summary>
    /// Optional user ID of the manager.
    /// If provided, the user must belong to this department (validated in controller).
    /// </summary>
    public string? ManagerId { get; set; }
}

/// <summary>
/// Request model for assigning a manager to a department.
/// </summary>
public class AssignManagerRequest
{
    /// <summary>
    /// User ID of the manager to assign.
    /// The user must belong to this department.
    /// </summary>
    [Required(ErrorMessage = "Manager ID is required.")]
    public string ManagerId { get; set; } = null!;
}
