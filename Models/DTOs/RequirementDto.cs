namespace NOX_Backend.Models.DTOs
{
    public class RequirementDto
    {
        /// <summary>
        /// Unique identifier for the requirement.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Category of the requirement.
        /// </summary>
        public string Category { get; set; } = null!;
        /// <summary>
        /// Name of the requirement.
        /// </summary>
        public string Name { get; set; } = null!;
        /// <summary>
        /// Status of the requirement for the user.
        /// Active/Pending/Submitted/Approved/Rejected
        /// </summary>
        public string Status { get; set; } = null!; 
        /// <summary>
        /// URL to the submitted file for the requirement, if any.
        /// </summary>
        public string? FileUrl { get; set; }
    }
}
