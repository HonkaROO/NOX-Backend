using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NOX_Backend.Services;

namespace NOX_Backend.Controllers;

/// <summary>
/// Controller for testing Azure Blob Storage integration.
/// All endpoints require authentication.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AzureBlobStorageController : ControllerBase
{
    private readonly AzureBlobStorageService? _blobStorageService;
    private readonly ILogger<AzureBlobStorageController> _logger;

    public AzureBlobStorageController(
        AzureBlobStorageService? blobStorageService,
        ILogger<AzureBlobStorageController> logger)
    {
        _blobStorageService = blobStorageService;
        _logger = logger;
    }

    /// <summary>
    /// Helper method to check if Azure Blob Storage service is configured.
    /// Returns true if configured, false otherwise.
    /// </summary>
    private bool IsAzureServiceConfigured()
    {
        return _blobStorageService != null;
    }

    /// <summary>
    /// Helper method to return Azure service not configured error response.
    /// </summary>
    private ActionResult<object> AzureServiceNotConfiguredError()
    {
        return StatusCode(503, new
        {
            message = "Azure Blob Storage service is not configured",
            details = "Azure credentials (AZURE_TENANT_ID, AZURE_CLIENT_ID, AZURE_CLIENT_SECRET, AZURE_STORAGE_ACCOUNT_NAME) are missing or invalid. Check the debug/env endpoint for details."
        });
    }

    /// <summary>
    /// Tests the connection to Azure Blob Storage.
    /// GET /api/azureblobstorage/test-connection
    /// </summary>
    [HttpGet("test-connection")]
    public async Task<ActionResult<object>> TestConnection()
    {
        try
        {
            if (!IsAzureServiceConfigured())
            {
                return AzureServiceNotConfiguredError();
            }

            var isConnected = await _blobStorageService!.TestConnectionAsync();

            if (isConnected)
            {
                return Ok(new { message = "Successfully connected to Azure Blob Storage", isConnected = true });
            }

            return StatusCode(500, new { message = "Failed to connect to Azure Blob Storage", isConnected = false });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing Azure Blob Storage connection");
            return StatusCode(500, new { message = "An error occurred while testing the connection", error = ex.Message });
        }
    }

    /// <summary>
    /// Debug endpoint to verify environment variables are loaded correctly.
    /// This helps diagnose connection issues.
    /// GET /api/azureblobstorage/debug/env
    /// WARNING: This endpoint exposes partial credential information for debugging purposes only.
    /// Remove this endpoint in production!
    /// </summary>
    [HttpGet("debug/env")]
    [AllowAnonymous]
    public IActionResult CheckEnvironmentVariables()
    {
        var tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
        var clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
        var clientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");
        var storageAccountName = Environment.GetEnvironmentVariable("AZURE_STORAGE_ACCOUNT_NAME");

        var response = new
        {
            message = "Environment variables status",
            variables = new
            {
                AZURE_TENANT_ID = string.IsNullOrEmpty(tenantId) ? "NOT SET" : $"✓ Set ({tenantId.Substring(0, 8)}...)",
                AZURE_CLIENT_ID = string.IsNullOrEmpty(clientId) ? "NOT SET" : $"✓ Set ({clientId.Substring(0, 8)}...)",
                AZURE_CLIENT_SECRET = string.IsNullOrEmpty(clientSecret) ? "NOT SET" : "✓ Set (hidden for security)",
                AZURE_STORAGE_ACCOUNT_NAME = string.IsNullOrEmpty(storageAccountName) ? "NOT SET" : $"✓ Set ({storageAccountName})"
            },
            allSet = !string.IsNullOrEmpty(tenantId) &&
                     !string.IsNullOrEmpty(clientId) &&
                     !string.IsNullOrEmpty(clientSecret) &&
                     !string.IsNullOrEmpty(storageAccountName)
        };

        return Ok(response);
    }

    /// <summary>
    /// Lists all blob containers in the storage account.
    /// GET /api/azureblobstorage/containers
    /// </summary>
    [HttpGet("containers")]
    public async Task<ActionResult<object>> ListContainers()
    {
        try
        {
            if (!IsAzureServiceConfigured())
            {
                return AzureServiceNotConfiguredError();
            }

            var containers = await _blobStorageService!.ListContainersAsync();

            return Ok(new
            {
                message = "Successfully retrieved containers",
                containerCount = containers.Count,
                containers = containers
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing containers");
            return StatusCode(500, new { message = "An error occurred while listing containers", error = ex.Message });
        }
    }

    /// <summary>
    /// Creates a new blob container.
    /// POST /api/azureblobstorage/containers
    /// </summary>
    [HttpPost("containers")]
    public async Task<ActionResult<object>> CreateContainer([FromBody] CreateContainerRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            if (!IsAzureServiceConfigured())
            {
                return AzureServiceNotConfiguredError();
            }

            var created = await _blobStorageService!.CreateContainerAsync(request.ContainerName);

            return Created(string.Empty, new
            {
                message = created ? "Container created successfully" : "Container already exists",
                containerName = request.ContainerName,
                created = created
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating container");
            return StatusCode(500, new { message = "An error occurred while creating the container", error = ex.Message });
        }
    }

    /// <summary>
    /// Uploads a file to a blob container.
    /// POST /api/azureblobstorage/upload
    /// </summary>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<object>> UploadBlob([FromForm] UploadBlobRequest request)
    {
        try
        {
            // Check if Azure Blob Storage service is configured
            if (!IsAzureServiceConfigured())
            {
                return AzureServiceNotConfiguredError();
            }

            if (string.IsNullOrEmpty(request.ContainerName))
            {
                return BadRequest(new { message = "containerName is required" });
            }

            if (string.IsNullOrEmpty(request.BlobName))
            {
                return BadRequest(new { message = "blobName is required" });
            }

            if (request.File == null || request.File.Length == 0)
            {
                return BadRequest(new { message = "A file must be provided" });
            }

            // Create container if it doesn't exist
            await _blobStorageService!.CreateContainerAsync(request.ContainerName);

            // Upload the file
            using var stream = request.File.OpenReadStream();
            var blobUri = await _blobStorageService!.UploadBlobAsync(request.ContainerName, request.BlobName, stream);

            return Ok(new
            {
                message = "File uploaded successfully",
                containerName = request.ContainerName,
                blobName = request.BlobName,
                blobUri = blobUri,
                fileSize = request.File.Length
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading blob");
            return StatusCode(500, new { message = "An error occurred while uploading the file", error = ex.Message });
        }
    }

    /// <summary>
    /// Downloads a blob from a container.
    /// GET /api/azureblobstorage/download
    /// </summary>
    [HttpGet("download")]
    public async Task<IActionResult> DownloadBlob([FromQuery] string containerName, [FromQuery] string blobName)
    {
        if (string.IsNullOrEmpty(containerName) || string.IsNullOrEmpty(blobName))
        {
            return BadRequest(new { message = "containerName and blobName are required" });
        }

        if (!IsAzureServiceConfigured())
        {
            return StatusCode(503, new
            {
                message = "Azure Blob Storage service is not configured",
                details = "Azure credentials (AZURE_TENANT_ID, AZURE_CLIENT_ID, AZURE_CLIENT_SECRET, AZURE_STORAGE_ACCOUNT_NAME) are missing or invalid. Check the debug/env endpoint for details."
            });
        }

        try
        {
            var stream = await _blobStorageService!.DownloadBlobAsync(containerName, blobName);

            return File(stream, "application/octet-stream", blobName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading blob");
            return StatusCode(500, new { message = "An error occurred while downloading the file", error = ex.Message });
        }
    }

    /// <summary>
    /// Lists all blobs in a container.
    /// GET /api/azureblobstorage/containers/{containerName}/blobs
    /// </summary>
    [HttpGet("containers/{containerName}/blobs")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> ListBlobs(string containerName)
    {
        if (!IsAzureServiceConfigured())
        {
            return AzureServiceNotConfiguredError();
        }

        try
        {
            var blobs = await _blobStorageService!.ListBlobsAsync(containerName);

            return Ok(new
            {
                message = "Successfully retrieved blobs",
                containerName = containerName,
                blobCount = blobs.Count,
                blobs = blobs
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing blobs");
            return StatusCode(500, new { message = "An error occurred while listing blobs", error = ex.Message });
        }
    }

    /// <summary>
    /// Gets properties of a specific blob.
    /// GET /api/azureblobstorage/containers/{containerName}/blobs/{blobName}/properties
    /// </summary>
    [HttpGet("containers/{containerName}/blobs/{blobName}/properties")]
    public async Task<ActionResult<object>> GetBlobProperties(string containerName, string blobName)
    {
        if (!IsAzureServiceConfigured())
        {
            return AzureServiceNotConfiguredError();
        }

        try
        {
            var properties = await _blobStorageService!.GetBlobPropertiesAsync(containerName, blobName);

            return Ok(new
            {
                message = "Successfully retrieved blob properties",
                properties = properties
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting blob properties");
            return StatusCode(500, new { message = "An error occurred while getting blob properties", error = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a blob from a container.
    /// DELETE /api/azureblobstorage/containers/{containerName}/blobs/{blobName}
    /// </summary>
    [HttpDelete("containers/{containerName}/blobs/{blobName}")]
    public async Task<ActionResult<object>> DeleteBlob(string containerName, string blobName)
    {
        if (!IsAzureServiceConfigured())
        {
            return AzureServiceNotConfiguredError();
        }

        try
        {
            var deleted = await _blobStorageService!.DeleteBlobAsync(containerName, blobName);

            if (deleted)
            {
                return Ok(new
                {
                    message = "Blob deleted successfully",
                    containerName = containerName,
                    blobName = blobName,
                    deleted = true
                });
            }

            return NotFound(new
            {
                message = "Blob not found",
                containerName = containerName,
                blobName = blobName,
                deleted = false
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting blob");
            return StatusCode(500, new { message = "An error occurred while deleting the blob", error = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a blob container.
    /// DELETE /api/azureblobstorage/containers/{containerName}
    /// </summary>
    [HttpDelete("containers/{containerName}")]
    public async Task<ActionResult<object>> DeleteContainer(string containerName)
    {
        if (!IsAzureServiceConfigured())
        {
            return AzureServiceNotConfiguredError();
        }

        try
        {
            var deleted = await _blobStorageService!.DeleteContainerAsync(containerName);

            if (deleted)
            {
                return Ok(new
                {
                    message = "Container deleted successfully",
                    containerName = containerName,
                    deleted = true
                });
            }

            return NotFound(new
            {
                message = "Container not found",
                containerName = containerName,
                deleted = false
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting container");
            return StatusCode(500, new { message = "An error occurred while deleting the container", error = ex.Message });
        }
    }
}

/// <summary>
/// Request model for creating a container.
/// </summary>
public class CreateContainerRequest
{
    public required string ContainerName { get; set; }
}

/// <summary>
/// Request model for uploading a blob to a container.
/// </summary>
public class UploadBlobRequest
{
    /// <summary>
    /// Name of the blob container to upload to.
    /// </summary>
    public required string ContainerName { get; set; }

    /// <summary>
    /// Name of the blob file (e.g., "document.pdf").
    /// </summary>
    public required string BlobName { get; set; }

    /// <summary>
    /// The file to upload.
    /// </summary>
    public required IFormFile File { get; set; }
}
