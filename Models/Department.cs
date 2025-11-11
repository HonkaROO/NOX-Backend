namespace NOX_Backend.Models;

/// <summary>
/// Represents an organizational department.
/// </summary>
public class Department
{
    /// <summary>
    /// Unique identifier for the department.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Department name (e.g., "Engineering", "HR", "Sales").
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Optional description of the department's purpose or responsibilities.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Foreign key to the ApplicationUser who manages this department.
    /// Must be a user who also belongs to this department.
    /// </summary>
    public string? ManagerId { get; set; }

    /// <summary>
    /// Navigation property: User who manages this department.
    /// </summary>
    public virtual ApplicationUser? Manager { get; set; }

    /// <summary>
    /// Indicates if the department is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Timestamp when the department was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the department was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Navigation property: Users belonging to this department.
    /// </summary>
    public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
}
