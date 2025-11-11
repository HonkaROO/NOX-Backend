namespace NOX_Backend.Models.DTOs.Onboarding;

/// <summary>
/// Request DTO for creating a new OnboardingSteps.
/// </summary>
public class CreateOnboardingStepsRequest
{
    /// <summary>
    /// Gets or sets the description of the onboarding step.
    /// Must be between 1 and 1000 characters.
    /// </summary>
    public required string StepDescription { get; set; }

    /// <summary>
    /// Gets or sets the ID of the task this step belongs to.
    /// </summary>
    public int TaskId { get; set; }
}
