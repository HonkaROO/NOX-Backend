using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace NOX_Backend.Models.DTOs.Onboarding;

/// <summary>
/// Request DTO for injecting knowledge files directly to Azure Blob Storage for AI indexing.
/// </summary>
public class InjectKnowledgeRequest
{
    /// <summary>
    /// Gets or sets the file to upload (PDF, JSON, or Markdown only).
    /// </summary>
    [Required]
    public required IFormFile File { get; set; }
}
