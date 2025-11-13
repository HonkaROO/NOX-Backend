using System.ComponentModel.DataAnnotations;

namespace NOX_Backend.Models.DTOs.Onboarding;

/// <summary>
/// Request DTO for creating a new OnboardingMaterial with file upload.
/// </summary>
public class CreateOnboardingMaterialRequest
{
    /// <summary>
    /// Gets or sets the file to upload.
    /// </summary>
    [Required]
    public required IFormFile File { get; set; }

    /// <summary>
    /// Gets or sets the ID of the task this material belongs to.
    /// </summary>
    public int TaskId { get; set; }
}
