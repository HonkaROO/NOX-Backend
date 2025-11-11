namespace NOX_Backend.Models.Onboarding;

public class OnboardingSteps
{
    public int Id { get; set; }
    public required string StepDescription { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Foreign Keys
    public int TaskId { get; set; }

    // Relationships
    public OnboardingTask? Task { get; set; }
}