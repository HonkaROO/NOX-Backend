namespace NOX_Backend.Models.DTOs.Onboarding;

/// <summary>
/// Request DTO for updating an existing OnboardingSteps.
/// </summary>
public class UpdateOnboardingStepsRequest
{
    /// <summary>
    /// Gets or sets the description of the onboarding step.
    /// Must be between 1 and 1000 characters.
    /// </summary>
    public required string StepDescription { get; set; }
}
