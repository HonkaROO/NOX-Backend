namespace NOX_Backend.Models.DTOs.AI;

/// <summary>
/// Response model from the AI microservice after deleting a document.
/// </summary>
public class AiDeleteResponse
{
    /// <summary>
    /// Indicates whether the deletion was successful.
    /// </summary>
    public required bool Success { get; set; }

    /// <summary>
    /// The number of document chunks deleted from the vector database.
    /// </summary>
    public required int DocumentsDeleted { get; set; }

    /// <summary>
    /// A descriptive message about the deletion operation.
    /// </summary>
    public required string Message { get; set; }
}
