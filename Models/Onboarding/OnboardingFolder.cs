namespace NOX_Backend.Models.Onboarding;

public class OnboardingFolder
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Relationships
    public ICollection<OnboardingTask> Tasks { get; set; } = [];
}