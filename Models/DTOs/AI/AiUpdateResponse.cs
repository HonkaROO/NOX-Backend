namespace NOX_Backend.Models.DTOs.AI;

/// <summary>
/// Response model from the AI microservice after updating a document.
/// </summary>
public class AiUpdateResponse
{
    /// <summary>
    /// Indicates whether the update was successful.
    /// </summary>
    public required bool Success { get; set; }

    /// <summary>
    /// The number of document chunks deleted from the vector database.
    /// </summary>
    public required int DocumentsDeleted { get; set; }

    /// <summary>
    /// The number of document chunks added to the vector database.
    /// </summary>
    public required int DocumentsAdded { get; set; }

    /// <summary>
    /// The type of file that was uploaded (json, pdf, md, or null if failed).
    /// </summary>
    public string? FileType { get; set; }

    /// <summary>
    /// A descriptive message about the update operation.
    /// </summary>
    public required string Message { get; set; }
}
