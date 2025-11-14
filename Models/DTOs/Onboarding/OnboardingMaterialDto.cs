namespace NOX_Backend.Models.DTOs.Onboarding;

/// <summary>
/// Response DTO for OnboardingMaterial.
/// </summary>
public class OnboardingMaterialDto
{
    /// <summary>
    /// Gets or sets the material ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the file name.
    /// </summary>
    public required string FileName { get; set; }

    /// <summary>
    /// Gets or sets the file type (MIME type).
    /// </summary>
    public required string FileType { get; set; }

    /// <summary>
    /// Gets or sets the URL to access the file in Azure Blob Storage.
    /// </summary>
    public required string Url { get; set; }

    /// <summary>
    /// Gets or sets the date when the material was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date when the material was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the ID of the task this material belongs to.
    /// </summary>
    public int TaskId { get; set; }
}
