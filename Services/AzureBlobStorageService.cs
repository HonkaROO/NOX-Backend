using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace NOX_Backend.Services;

/// <summary>
/// Service for managing Azure Blob Storage operations.
/// Handles uploading, downloading, and deleting files from Azure Blob Storage.
/// </summary>
public class AzureBlobStorageService
{
    private readonly BlobContainerClient _containerClient;
    private readonly ILogger<AzureBlobStorageService> _logger;

    /// <summary>
    /// Initializes a new instance of the AzureBlobStorageService.
    /// </summary>
    /// <param name="blobServiceClient">The BlobServiceClient for authentication and connection.</param>
    /// <param name="containerName">The name of the Blob Storage container.</param>
    /// <param name="logger">The logger instance.</param>
    public AzureBlobStorageService(
        BlobServiceClient blobServiceClient,
        string containerName,
        ILogger<AzureBlobStorageService> logger)
    {
        _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        _logger = logger;
    }

    /// <summary>
    /// Uploads a file to Azure Blob Storage.
    /// </summary>
    /// <param name="file">The file to upload.</param>
    /// <param name="blobName">The name to give the blob in storage.</param>
    /// <returns>The URL of the uploaded blob.</returns>
    /// <exception cref="ArgumentNullException">Thrown if file is null.</exception>
    /// <exception cref="IOException">Thrown if file reading fails.</exception>
    public async Task<string> UploadFileAsync(IFormFile file, string blobName)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentNullException(nameof(file), "File cannot be null or empty.");
        }

        try
        {
            BlobClient blobClient = _containerClient.GetBlobClient(blobName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            _logger.LogInformation("File '{BlobName}' uploaded successfully to Azure Blob Storage.", blobName);

            return blobClient.Uri.ToString();
        }
        catch (Azure.RequestFailedException ex)
        {
            _logger.LogError(ex, "Azure Blob Storage error while uploading file '{BlobName}'.", blobName);
            throw new IOException($"Failed to upload file to Azure Blob Storage: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while uploading file '{BlobName}' to Azure Blob Storage.", blobName);
            throw;
        }
    }

    /// <summary>
    /// Deletes a blob from Azure Blob Storage.
    /// </summary>
    /// <param name="blobName">The name of the blob to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteBlobAsync(string blobName)
    {
        try
        {
            BlobClient blobClient = _containerClient.GetBlobClient(blobName);
            await blobClient.DeleteAsync();

            _logger.LogInformation("Blob '{BlobName}' deleted successfully from Azure Blob Storage.", blobName);
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Blob '{BlobName}' not found in Azure Blob Storage.", blobName);
            // Don't throw for 404 as blob may have already been deleted
        }
        catch (Azure.RequestFailedException ex)
        {
            _logger.LogError(ex, "Azure Blob Storage error while deleting blob '{BlobName}'.", blobName);
            throw new IOException($"Failed to delete blob from Azure Blob Storage: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting blob '{BlobName}' from Azure Blob Storage.", blobName);
            throw;
        }
    }

    /// <summary>
    /// Generates a unique blob name using the file name and a timestamp.
    /// </summary>
    /// <param name="fileName">The original file name.</param>
    /// <returns>A unique blob name.</returns>
    public static string GenerateUniqueBlobName(string fileName)
    {
        var fileExtension = Path.GetExtension(fileName);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");
        return $"materials/{fileNameWithoutExtension}_{timestamp}{fileExtension}";
    }

}
