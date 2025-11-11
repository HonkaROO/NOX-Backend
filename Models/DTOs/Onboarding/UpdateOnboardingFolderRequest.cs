namespace NOX_Backend.Models.DTOs.Onboarding;

/// <summary>
/// Request DTO for updating an existing OnboardingFolder.
/// </summary>
public class UpdateOnboardingFolderRequest
{
    /// <summary>
    /// Gets or sets the title of the onboarding folder.
    /// Must be between 1 and 255 characters.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Gets or sets the description of the onboarding folder.
    /// Must be between 1 and 1000 characters.
    /// </summary>
    public required string Description { get; set; }
}
