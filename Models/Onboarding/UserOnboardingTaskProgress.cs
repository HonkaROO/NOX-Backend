namespace NOX_Backend.Models.Onboarding;
public class UserOnboardingTaskProgress
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int TaskId { get; set; }

    // Status: pending | in_progress | completed
    public string Status { get; set; } = "pending";

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ApplicationUser? User { get; set; }
    public OnboardingTask? Task { get; set; }
}
