namespace NOX_Backend.Models.DTOs.AI;

/// <summary>
/// Response model from the AI microservice after uploading a document.
/// </summary>
public class AiUploadResponse
{
    /// <summary>
    /// Indicates whether the upload was successful.
    /// </summary>
    public required bool Success { get; set; }

    /// <summary>
    /// The number of document chunks added to the vector database.
    /// </summary>
    public required int DocumentsAdded { get; set; }

    /// <summary>
    /// The type of file that was uploaded (json, pdf, md, or null if failed).
    /// </summary>
    public string? FileType { get; set; }

    /// <summary>
    /// A descriptive message about the upload operation.
    /// </summary>
    public required string Message { get; set; }
}
