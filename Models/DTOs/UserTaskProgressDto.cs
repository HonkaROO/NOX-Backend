using System.ComponentModel.DataAnnotations;


namespace NOX_Backend.Models.DTOs;
public class UserTaskProgressDto
{
    public int TaskId { get; set; }
    public string Status { get; set; } = null!;
    public DateTime UpdatedAt { get; set; }

    public string TaskTitle { get; set; } = null!;
    public string TaskDescription { get; set; } = null!;
}
