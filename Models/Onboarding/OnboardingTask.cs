namespace NOX_Backend.Models.Onboarding;

public class OnboardingTask
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Foreign Keys
    public int FolderId { get; set; }

    // Relationships
    public OnboardingFolder? Folder { get; set; }
    public ICollection<OnboardingMaterial> Materials { get; set; } = [];
    public ICollection<OnboardingSteps> Steps { get; set; } = [];
}