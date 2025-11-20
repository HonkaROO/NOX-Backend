using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace NOX_Backend.Services;

/// <summary>
/// Service for managing Azure Blob Storage operations.
/// Handles uploading and deleting files from Azure Blob Storage.
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
        if (blobServiceClient == null)
        {
            throw new ArgumentNullException(nameof(blobServiceClient));
        }
        if (string.IsNullOrWhiteSpace(containerName))
        {
            throw new ArgumentException("Container name cannot be null or empty.", nameof(containerName));
        }
        if (logger == null)
        {
            throw new ArgumentNullException(nameof(logger));
        }

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
    /// <exception cref="ArgumentException">Thrown if file is empty or blobName is invalid.</exception>
    /// <exception cref="IOException">Thrown if file reading fails.</exception>
    public async Task<string> UploadFileAsync(IFormFile file, string blobName)
    {
        if (file == null)
        {
            throw new ArgumentNullException(nameof(file));
        }
        if (file.Length == 0)
        {
            throw new ArgumentException("File cannot be empty.", nameof(file));
        }
        if (string.IsNullOrWhiteSpace(blobName))
        {
            throw new ArgumentException("Blob name cannot be null or empty.", nameof(blobName));
        }

        try
        {
            BlobClient blobClient = _containerClient.GetBlobClient(blobName);

            using (var stream = file.OpenReadStream())
            {
                var uploadOptions = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = file.ContentType
                    }
                };
                await blobClient.UploadAsync(stream, options: uploadOptions);
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
    /// <exception cref="ArgumentException">Thrown if blobName is null or empty.</exception>
    public async Task DeleteBlobAsync(string blobName)
    {
        if (string.IsNullOrWhiteSpace(blobName))
        {
            throw new ArgumentException("Blob name cannot be null or empty.", nameof(blobName));
        }

        try
        {
            BlobClient blobClient = _containerClient.GetBlobClient(blobName);
            var result = await blobClient.DeleteIfExistsAsync();

            if (result.Value)
            {
                _logger.LogInformation("Blob '{BlobName}' deleted successfully from Azure Blob Storage.", blobName);
            }
            else
            {
                _logger.LogWarning("Blob '{BlobName}' not found in Azure Blob Storage.", blobName);
            }
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
    /// <param name="prefix">The prefix for the blob path (default: "materials").</param>
    /// <returns>A unique blob name.</returns>
    /// <exception cref="ArgumentException">Thrown if fileName is null or empty.</exception>
    public static string GenerateUniqueBlobName(string fileName, string prefix = "materials")
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));
        }

        // Normalize prefix
        if (string.IsNullOrWhiteSpace(prefix))
        {
            prefix = "materials";
        }
        prefix = prefix.Trim('/');

        var fileExtension = Path.GetExtension(fileName);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");
        return $"{prefix}/{fileNameWithoutExtension}_{timestamp}{fileExtension}";
    }
    
    /// <summary>
    /// Lists all blobs in the container.
    /// </summary>
    /// <returns></returns>
    public async Task<List<string>> ListBlobsAsync()
    {
        try
        {
            var blobs = new List<string>();

            await foreach (var blob in _containerClient.GetBlobsAsync())
            {
                blobs.Add(blob.Name);
            }

            _logger.LogInformation("Retrieved {BlobCount} blobs from container.", blobs.Count);

            return blobs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list blobs.");
            throw;
        }
    }

}
