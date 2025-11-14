namespace NOX_Backend.Models.DTOs.AI;

/// <summary>
/// Request model for updating a document in the AI microservice.
/// Replaces an old document with a new one in the vector database.
/// </summary>
public class AiUpdateRequest
{
    /// <summary>
    /// The URL of the document to be replaced.
    /// </summary>
    public required string OldUrl { get; set; }

    /// <summary>
    /// The URL of the new document to inject.
    /// Supported formats: .pdf, .json, .md
    /// </summary>
    public required string NewUrl { get; set; }
}
