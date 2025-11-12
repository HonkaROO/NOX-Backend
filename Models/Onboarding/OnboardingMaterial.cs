namespace NOX_Backend.Models.Onboarding;

public class OnboardingMaterial
{
    public int Id { get; set; }
    public required string FileName { get; set; }
    public required string FileType { get; set; }
    public required string Url { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Foreign Keys
    public int TaskId { get; set; }

    // Relationships
    public required OnboardingTask Task { get; set; }
}