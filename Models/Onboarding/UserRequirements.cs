using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Backend.Models;
using NOX_Backend.Models;

namespace Backend.Models.Onboarding
{
    [Table("UserRequirements", Schema = "dbo")]
    public class UserRequirement
    {
        [Key]
        public int Id { get; set; }

        // FK → AspNetUsers.Id (Employee/User)
        [Required]
        public string UserId { get; set; } = default!;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }   // FIXED

        // FK → Requirement.Id
        [Required]
        public int RequirementId { get; set; }

        [ForeignKey(nameof(RequirementId))]
        public Requirement Requirement { get; set; } = default!;

        // Status information
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; 
        // Pending, Submitted, Approved, Rejected

        public string? FileUrl { get; set; }

        public DateTime? SubmittedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }

        // Admin who reviewed the requirement
        public string? ReviewerId { get; set; }

        [ForeignKey(nameof(ReviewerId))]
        public ApplicationUser? Reviewer { get; set; }   
    }
}
