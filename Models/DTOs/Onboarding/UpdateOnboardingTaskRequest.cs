using System.ComponentModel.DataAnnotations;

namespace NOX_Backend.Models.DTOs.Onboarding;

/// <summary>
/// Request DTO for updating an existing OnboardingTask.
/// </summary>
public class UpdateOnboardingTaskRequest
{
    /// <summary>
    /// Gets or sets the title of the onboarding task.
    /// Must be between 1 and 255 characters.
    /// </summary>
    [Required]
    [StringLength(255, MinimumLength = 1)]
    public required string Title { get; set; }

    /// <summary>
    /// Gets or sets the description of the onboarding task.
    /// Must be between 1 and 1000 characters.
    /// </summary>
    [Required]
    [StringLength(1000, MinimumLength = 1)]
    public required string Description { get; set; }
}
