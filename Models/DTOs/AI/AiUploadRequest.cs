namespace NOX_Backend.Models.DTOs.AI;

/// <summary>
/// Request model for uploading a document to the AI microservice.
/// </summary>
public class AiUploadRequest
{
    /// <summary>
    /// The publicly accessible URL of the document file.
    /// Supported formats: .pdf, .json, .md
    /// </summary>
    public required string Url { get; set; }
}
