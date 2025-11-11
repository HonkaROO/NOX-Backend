namespace NOX_Backend.Models.Onboarding;

public class OnboardingMaterial
{
    public int Id { get; set; }
    public required string FileName { get; set; }
    public required string FileType { get; set; }
    public required string Url { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Foreign Keys
    public int TaskId { get; set; }

    // Relationships
    public OnboardingTask? Task { get; set; }
}