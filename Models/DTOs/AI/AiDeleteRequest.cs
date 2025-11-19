namespace NOX_Backend.Models.DTOs.AI;

/// <summary>
/// Request model for deleting a document from the AI microservice.
/// </summary>
public class AiDeleteRequest
{
    /// <summary>
    /// The URL of the document to delete from the vector database.
    /// </summary>
    public required string Url { get; set; }
}
