using System.ComponentModel.DataAnnotations;

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
    [Required]
    [StringLength(1000, MinimumLength = 1)]
    public required string StepDescription { get; set; }

    /// <summary>
    /// Gets or sets the sequence order of the step within its task.
    /// If specified, will reorder the step within the task.
    /// </summary>
    public int? SequenceOrder { get; set; }
}
