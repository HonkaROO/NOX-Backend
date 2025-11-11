namespace NOX_Backend.Models.DTOs.Onboarding;

/// <summary>
/// Data Transfer Object for OnboardingTask entities.
/// Used in API responses to expose task information without internal relationships.
/// </summary>
public class OnboardingTaskDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the onboarding task.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the title of the onboarding task.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Gets or sets the description of the onboarding task.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Gets or sets the ID of the folder this task belongs to.
    /// </summary>
    public int FolderId { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp (UTC), if any.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the number of materials associated with this task.
    /// </summary>
    public int MaterialCount { get; set; }

    /// <summary>
    /// Gets or sets the number of steps associated with this task.
    /// </summary>
    public int StepCount { get; set; }
}
