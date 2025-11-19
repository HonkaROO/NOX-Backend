namespace NOX_Backend.Models.DTOs.Onboarding;

/// <summary>
/// Data Transfer Object for OnboardingSteps entities.
/// Used in API responses to expose step information without internal relationships.
/// </summary>
public class OnboardingStepsDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the onboarding step.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the description of the onboarding step.
    /// </summary>
    public required string StepDescription { get; set; }

    /// <summary>
    /// Gets or sets the sequence order of the step within its task.
    /// Lower numbers appear first.
    /// </summary>
    public int SequenceOrder { get; set; }

    /// <summary>
    /// Gets or sets the ID of the task this step belongs to.
    /// </summary>
    public int TaskId { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp (UTC), if any.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
