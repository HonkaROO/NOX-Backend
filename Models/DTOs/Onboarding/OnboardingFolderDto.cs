namespace NOX_Backend.Models.DTOs.Onboarding;

/// <summary>
/// Data Transfer Object for OnboardingFolder entities.
/// Used in API responses to expose folder information without internal relationships.
/// </summary>
public class OnboardingFolderDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the onboarding folder.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the title of the onboarding folder.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Gets or sets the description of the onboarding folder.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp (UTC), if any.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the number of tasks in this folder.
    /// </summary>
    public int TaskCount { get; set; }
}
