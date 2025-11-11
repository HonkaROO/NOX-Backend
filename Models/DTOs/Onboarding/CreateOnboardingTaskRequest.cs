namespace NOX_Backend.Models.DTOs.Onboarding;

/// <summary>
/// Request DTO for creating a new OnboardingTask.
/// </summary>
public class CreateOnboardingTaskRequest
{
    /// <summary>
    /// Gets or sets the title of the onboarding task.
    /// Must be between 1 and 255 characters.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Gets or sets the description of the onboarding task.
    /// Must be between 1 and 1000 characters.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Gets or sets the ID of the folder this task belongs to.
    /// </summary>
    public int FolderId { get; set; }
}
