using System.Text;
using System.Text.Json;
using NOX_Backend.Models.DTOs.AI;

namespace NOX_Backend.Services;

/// <summary>
/// Service for communicating with the AI microservice to manage document uploads, updates, and deletions.
/// Handles synchronization of onboarding materials with the AI vector database.
/// </summary>
public class AiDocumentService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AiDocumentService> _logger;
    private readonly string _aiServiceUrl;

    /// <summary>
    /// Initializes a new instance of the AiDocumentService.
    /// </summary>
    public AiDocumentService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<AiDocumentService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _aiServiceUrl = configuration["AiService:Url"] ?? "http://127.0.0.1:8000";
    }

    /// <summary>
    /// Uploads a document to the AI microservice for vector database injection.
    /// Supported file types: .pdf, .json, .md
    /// </summary>
    /// <param name="url">The publicly accessible URL of the document file.</param>
    /// <returns>The upload response containing success status and document count.</returns>
    /// <exception cref="HttpRequestException">Thrown if the AI service request fails.</exception>
    public async Task<AiUploadResponse> UploadDocumentAsync(string url)
    {
        try
        {
            var request = new AiUploadRequest { Url = url };
            var jsonContent = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"{_aiServiceUrl}/upload-document",
                content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "AI service upload failed with status {StatusCode}: {ErrorContent}",
                    response.StatusCode,
                    errorContent);
                throw new HttpRequestException(
                    $"AI service upload failed: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var uploadResponse = JsonSerializer.Deserialize<AiUploadResponse>(responseContent);

            if (uploadResponse == null)
            {
                throw new HttpRequestException("Failed to deserialize AI service response");
            }

            _logger.LogInformation(
                "Document uploaded to AI service: {FileType}, {DocumentsAdded} chunks added",
                uploadResponse.FileType,
                uploadResponse.DocumentsAdded);

            return uploadResponse;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during document upload to AI service");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during document upload to AI service");
            throw;
        }
    }

    /// <summary>
    /// Updates a document in the AI microservice by replacing the old document with a new one.
    /// Performs deletion of old document and injection of new document atomically.
    /// </summary>
    /// <param name="oldUrl">The URL of the document to be replaced.</param>
    /// <param name="newUrl">The URL of the new document.</param>
    /// <returns>The update response containing success status and document counts.</returns>
    /// <exception cref="HttpRequestException">Thrown if the AI service request fails.</exception>
    public async Task<AiUpdateResponse> UpdateDocumentAsync(string oldUrl, string newUrl)
    {
        try
        {
            var request = new AiUpdateRequest { OldUrl = oldUrl, NewUrl = newUrl };
            var jsonContent = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"{_aiServiceUrl}/update-document",
                content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "AI service update failed with status {StatusCode}: {ErrorContent}",
                    response.StatusCode,
                    errorContent);
                throw new HttpRequestException(
                    $"AI service update failed: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var updateResponse = JsonSerializer.Deserialize<AiUpdateResponse>(responseContent);

            if (updateResponse == null)
            {
                throw new HttpRequestException("Failed to deserialize AI service response");
            }

            _logger.LogInformation(
                "Document updated in AI service: {FileType}, {DocumentsDeleted} chunks deleted, {DocumentsAdded} chunks added",
                updateResponse.FileType,
                updateResponse.DocumentsDeleted,
                updateResponse.DocumentsAdded);

            return updateResponse;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during document update in AI service");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during document update in AI service");
            throw;
        }
    }

    /// <summary>
    /// Deletes a document from the AI microservice's vector database.
    /// </summary>
    /// <param name="url">The URL of the document to delete.</param>
    /// <returns>The delete response containing success status and deleted document count.</returns>
    /// <exception cref="HttpRequestException">Thrown if the AI service request fails.</exception>
    public async Task<AiDeleteResponse> DeleteDocumentAsync(string url)
    {
        try
        {
            var request = new AiDeleteRequest { Url = url };
            var jsonContent = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"{_aiServiceUrl}/delete-document",
                content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "AI service delete failed with status {StatusCode}: {ErrorContent}",
                    response.StatusCode,
                    errorContent);
                throw new HttpRequestException(
                    $"AI service delete failed: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var deleteResponse = JsonSerializer.Deserialize<AiDeleteResponse>(responseContent);

            if (deleteResponse == null)
            {
                throw new HttpRequestException("Failed to deserialize AI service response");
            }

            _logger.LogInformation(
                "Document deleted from AI service: {DocumentsDeleted} chunks removed",
                deleteResponse.DocumentsDeleted);

            return deleteResponse;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during document deletion from AI service");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during document deletion from AI service");
            throw;
        }
    }

    /// <summary>
    /// Determines if a file should be synced to the AI service based on its extension.
    /// </summary>
    /// <param name="fileName">The name of the file.</param>
    /// <returns>True if the file type is supported by the AI service; otherwise, false.</returns>
    public static bool IsSupportedFileType(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return false;
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension is ".pdf" or ".json" or ".md";
    }
}
