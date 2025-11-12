using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace NOX_Backend.Services;

/// <summary>
/// Service for interacting with Azure Blob Storage.
/// Uses ClientSecretCredential for Azure AD authentication via dependency injection from Program.cs.
/// The BlobServiceClient is registered in Program.cs with credentials from environment variables:
/// AZURE_TENANT_ID, AZURE_CLIENT_ID, AZURE_CLIENT_SECRET, AZURE_STORAGE_ACCOUNT_NAME
/// </summary>
public class AzureBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<AzureBlobStorageService> _logger;

    public AzureBlobStorageService(BlobServiceClient blobServiceClient, ILogger<AzureBlobStorageService> logger)
    {
        _blobServiceClient = blobServiceClient ?? throw new ArgumentNullException(nameof(blobServiceClient),
            "BlobServiceClient is null. Ensure Azure credentials (AZURE_TENANT_ID, AZURE_CLIENT_ID, AZURE_CLIENT_SECRET, AZURE_STORAGE_ACCOUNT_NAME) are properly configured in environment variables.");
        _logger = logger;

        _logger.LogInformation("Azure Blob Storage service initialized successfully");
    }

    /// <summary>
    /// Tests the connection to Azure Blob Storage by listing containers.
    /// </summary>
    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            var enumerator = _blobServiceClient.GetBlobContainersAsync().GetAsyncEnumerator();
            var hasContainers = await enumerator.MoveNextAsync();
            await enumerator.DisposeAsync();

            _logger.LogInformation("Azure Blob Storage connection test successful");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure Blob Storage connection test failed");
            return false;
        }
    }

    /// <summary>
    /// Gets a list of all blob containers.
    /// </summary>
    public async Task<List<string>> ListContainersAsync()
    {
        try
        {
            var containers = new List<string>();
            await foreach (var container in _blobServiceClient.GetBlobContainersAsync())
            {
                containers.Add(container.Name);
            }

            _logger.LogInformation("Retrieved {ContainerCount} containers", containers.Count);
            return containers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list containers");
            throw;
        }
    }

    /// <summary>
    /// Creates a blob container if it doesn't exist.
    /// </summary>
    public async Task<bool> CreateContainerAsync(string containerName)
    {
        try
        {
            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentException("Container name cannot be null or empty", nameof(containerName));
            }

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var response = await containerClient.CreateIfNotExistsAsync();

            // Check if the response was successful (HTTP 201 Created) or already exists (HTTP 409 Conflict)
            if (response?.GetRawResponse()?.Status == 201)
            {
                _logger.LogInformation("Created blob container: {ContainerName}", containerName);
                return true;
            }

            _logger.LogInformation("Blob container already exists or creation was not successful: {ContainerName}", containerName);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create container: {ContainerName}", containerName);
            throw;
        }
    }

    /// <summary>
    /// Uploads a file to a blob container.
    /// </summary>
    public async Task<string> UploadBlobAsync(string containerName, string blobName, Stream stream, bool overwrite = true)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            // Ensure container exists
            await containerClient.CreateIfNotExistsAsync();

            // Upload with overwrite option
            await blobClient.UploadAsync(stream, overwrite);

            _logger.LogInformation("Successfully uploaded blob: {BlobName} to container: {ContainerName}", blobName, containerName);
            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload blob: {BlobName} to container: {ContainerName}", blobName, containerName);
            throw;
        }
    }

    /// <summary>
    /// Downloads a blob from a container.
    /// </summary>
    public async Task<Stream> DownloadBlobAsync(string containerName, string blobName)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var download = await blobClient.DownloadAsync();

            _logger.LogInformation("Successfully downloaded blob: {BlobName} from container: {ContainerName}", blobName, containerName);
            return download.Value.Content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download blob: {BlobName} from container: {ContainerName}", blobName, containerName);
            throw;
        }
    }

    /// <summary>
    /// Lists all blobs in a container.
    /// </summary>
    public async Task<List<string>> ListBlobsAsync(string containerName)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobs = new List<string>();

            await foreach (var blob in containerClient.GetBlobsAsync())
            {
                blobs.Add(blob.Name);
            }

            _logger.LogInformation("Retrieved {BlobCount} blobs from container: {ContainerName}", blobs.Count, containerName);
            return blobs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list blobs in container: {ContainerName}", containerName);
            throw;
        }
    }

    /// <summary>
    /// Deletes a blob from a container.
    /// </summary>
    public async Task<bool> DeleteBlobAsync(string containerName, string blobName)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var response = await blobClient.DeleteIfExistsAsync();

            if (response.Value)
            {
                _logger.LogInformation("Successfully deleted blob: {BlobName} from container: {ContainerName}", blobName, containerName);
            }
            else
            {
                _logger.LogInformation("Blob does not exist: {BlobName} in container: {ContainerName}", blobName, containerName);
            }

            return response.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete blob: {BlobName} from container: {ContainerName}", blobName, containerName);
            throw;
        }
    }

    /// <summary>
    /// Deletes a blob container.
    /// </summary>
    public async Task<bool> DeleteContainerAsync(string containerName)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var response = await containerClient.DeleteIfExistsAsync();

            if (response.Value)
            {
                _logger.LogInformation("Successfully deleted container: {ContainerName}", containerName);
            }
            else
            {
                _logger.LogInformation("Container does not exist: {ContainerName}", containerName);
            }

            return response.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete container: {ContainerName}", containerName);
            throw;
        }
    }

    /// <summary>
    /// Gets blob properties (metadata, size, etc.).
    /// </summary>
    public async Task<Dictionary<string, string>> GetBlobPropertiesAsync(string containerName, string blobName)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var properties = await blobClient.GetPropertiesAsync();

            var result = new Dictionary<string, string>
            {
                { "BlobName", blobName },
                { "ContainerName", containerName },
                { "ContentType", properties.Value.ContentType ?? "unknown" },
                { "ContentLength", properties.Value.ContentLength.ToString() },
                { "LastModified", properties.Value.LastModified.ToString() },
                { "ETag", properties.Value.ETag.ToString() }
            };

            _logger.LogInformation("Retrieved properties for blob: {BlobName}", blobName);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get properties for blob: {BlobName}", blobName);
            throw;
        }
    }
}
