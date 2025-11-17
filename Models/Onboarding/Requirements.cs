using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Onboarding
{
    [Table("Requirements", Schema = "dbo")]
    public class Requirement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [MaxLength(300)]
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<UserRequirement> UserRequirements { get; set; } = new List<UserRequirement>();
    }
}
