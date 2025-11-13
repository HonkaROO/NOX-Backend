using System.ComponentModel.DataAnnotations;

namespace NOX_Backend.Models.DTOs.Onboarding;

/// <summary>
/// Request DTO for updating an existing OnboardingMaterial.
/// </summary>
public class UpdateOnboardingMaterialRequest
{
    /// <summary>
    /// Gets or sets the optional file to replace the current one.
    /// If null, the existing file is kept.
    /// </summary>
    public IFormFile? File { get; set; }
}
