using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NOX_Backend.Models;
using NOX_Backend.Models.DTOs.Onboarding;
using NOX_Backend.Models.Onboarding;
using NOX_Backend.Services;

namespace NOX_Backend.Controllers.Onboarding;

/// <summary>
/// Controller for managing onboarding materials.
/// Provides CRUD operations for onboarding materials with file upload to Azure Blob Storage.
/// </summary>
[ApiController]
[Route("api/onboarding/materials")]
public class OnboardingMaterialController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly AzureBlobStorageService _blobStorageService;
    private readonly ILogger<OnboardingMaterialController> _logger;

    /// <summary>
    /// Initializes a new instance of the OnboardingMaterialController.
    /// </summary>
    public OnboardingMaterialController(
        AppDbContext context,
        AzureBlobStorageService blobStorageService,
        ILogger<OnboardingMaterialController> logger)
    {
        _context = context;
        _blobStorageService = blobStorageService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all onboarding materials.
    /// </summary>
    /// <returns>List of all onboarding materials.</returns>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<OnboardingMaterialDto>>> GetMaterials()
    {
        var materials = await _context.OnboardingMaterials
            .OrderBy(m => m.FileName)
            .ToListAsync();

        var materialDtos = materials.Select(MapToOnboardingMaterialDto).ToList();

        return Ok(materialDtos);
    }

    /// <summary>
    /// Gets all onboarding materials for a specific task.
    /// </summary>
    /// <param name="taskId">The task ID.</param>
    /// <returns>List of onboarding materials in the specified task.</returns>
    [HttpGet("task/{taskId}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<OnboardingMaterialDto>>> GetMaterialsByTask(int taskId)
    {
        // Verify task exists
        var taskExists = await _context.OnboardingTasks
            .AnyAsync(t => t.Id == taskId);

        if (!taskExists)
        {
            return NotFound(new { message = "Onboarding task not found." });
        }

        var materials = await _context.OnboardingMaterials
            .Where(m => m.TaskId == taskId)
            .OrderBy(m => m.FileName)
            .ToListAsync();

        var materialDtos = materials.Select(MapToOnboardingMaterialDto).ToList();

        return Ok(materialDtos);
    }

    /// <summary>
    /// Gets a specific onboarding material by ID.
    /// </summary>
    /// <param name="id">The material ID.</param>
    /// <returns>The requested onboarding material with details.</returns>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<OnboardingMaterialDto>> GetMaterial(int id)
    {
        var material = await _context.OnboardingMaterials.FindAsync(id);

        if (material == null)
        {
            return NotFound(new { message = "Onboarding material not found." });
        }

        return Ok(MapToOnboardingMaterialDto(material));
    }

    /// <summary>
    /// Creates a new onboarding material and uploads the file to Azure Blob Storage.
    /// Requires SuperAdmin or Admin role.
    /// </summary>
    /// <param name="request">The material creation request with file.</param>
    /// <returns>The created onboarding material.</returns>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<OnboardingMaterialDto>> CreateMaterial(
        [FromForm] CreateOnboardingMaterialRequest request)
    {
        // File size limit: 50MB
        const long MaxFileSize = 50 * 1024 * 1024;

        // Allowed content types
        var AllowedContentTypes = new[]
        {
            "application/pdf",
            "text/plain",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "image/jpeg",
            "image/png",
            "image/gif"
        };

        // Allowed extensions
        var AllowedExtensions = new[]
        {
            ".pdf", ".txt", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png", ".gif"
        };

        try
        {
            // Validate input
            if (request.File == null || request.File.Length == 0)
            {
                return BadRequest(new { message = "File is required and must not be empty." });
            }

            // Validate file size
            if (request.File.Length > MaxFileSize)
            {
                return BadRequest(new { message = "File size cannot exceed 50MB." });
            }

            // Validate file type
            if (!AllowedContentTypes.Contains(request.File.ContentType))
            {
                return BadRequest(new { message = "File type is not allowed." });
            }

            // Validate file extension
            var fileExtension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(fileExtension))
            {
                return BadRequest(new { message = "File extension is not allowed." });
            }

            if (request.TaskId <= 0)
            {
                return BadRequest(new { message = "Valid TaskId is required." });
            }

            // Verify task exists
            var task = await _context.OnboardingTasks.FindAsync(request.TaskId);
            if (task == null)
            {
                return BadRequest(new { message = "The specified task does not exist." });
            }

            // Generate unique blob name
            string blobName = AzureBlobStorageService.GenerateUniqueBlobName(request.File.FileName);

            // Upload file to Azure Blob Storage
            string fileUrl = await _blobStorageService.UploadFileAsync(request.File, blobName);

            // Create material entity
            var material = new OnboardingMaterial
            {
                FileName = request.File.FileName,
                FileType = request.File.ContentType ?? "application/octet-stream",
                Url = fileUrl,
                TaskId = request.TaskId,
                Task = task,
                CreatedAt = DateTime.UtcNow
            };

            _context.OnboardingMaterials.Add(material);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Material created successfully for task {TaskId} with ID {MaterialId}.",
                request.TaskId,
                material.Id);

            return CreatedAtAction(nameof(GetMaterial), new { id = material.Id }, MapToOnboardingMaterialDto(material));
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "File upload failed for material creation.");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "File upload failed. Please try again." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during material creation.");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An unexpected error occurred during material creation." });
        }
    }

    /// <summary>
    /// Updates an existing onboarding material.
    /// Can update the file by providing a new one, or keep the existing file if no file is provided.
    /// Requires SuperAdmin or Admin role.
    /// </summary>
    /// <param name="id">The material ID to update.</param>
    /// <param name="request">The material update request.</param>
    /// <returns>The updated onboarding material.</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<OnboardingMaterialDto>> UpdateMaterial(
        int id,
        [FromForm] UpdateOnboardingMaterialRequest request)
    {
        try
        {
            var material = await _context.OnboardingMaterials.FindAsync(id);

            if (material == null)
            {
                return NotFound(new { message = "Onboarding material not found." });
            }

            // If a new file is provided, upload it and delete the old one
            if (request.File != null && request.File.Length > 0)
            {
                // Validate file size
                const long MaxFileSize = 50 * 1024 * 1024;
                var AllowedContentTypes = new[]
                {
                    "application/pdf", "text/plain",
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "image/jpeg", "image/png", "image/gif"
                };
                var AllowedExtensions = new[] { ".pdf", ".txt", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png", ".gif" };

                if (request.File.Length > MaxFileSize)
                    return BadRequest(new { message = "File size cannot exceed 50MB." });

                if (!AllowedContentTypes.Contains(request.File.ContentType))
                    return BadRequest(new { message = "File type is not allowed." });

                var fileExtension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(fileExtension))
                    return BadRequest(new { message = "File extension is not allowed." });

                // Generate unique blob name for new file
                string newBlobName = AzureBlobStorageService.GenerateUniqueBlobName(request.File.FileName);

                // Upload new file to Azure Blob Storage
                string newFileUrl = await _blobStorageService.UploadFileAsync(request.File, newBlobName);

                // Update material properties with new file info
                material.FileName = request.File.FileName;
                material.FileType = request.File.ContentType ?? "application/octet-stream";
                material.Url = newFileUrl;
                material.UpdatedAt = DateTime.UtcNow;

                // Save DB changes first to persist new URL
                _context.OnboardingMaterials.Update(material);
                await _context.SaveChangesAsync();

                // Only attempt to delete old blob after DB update succeeds
                try
                {
                    string oldBlobName = ExtractBlobNameFromUrl(material.Url);
                    await _blobStorageService.DeleteBlobAsync(oldBlobName);
                }
                catch (Exception ex)
                {
                    // Log but don't fail if old blob deletion fails
                    _logger.LogWarning(ex, "Failed to delete old blob for material {MaterialId}, but DB update succeeded.", id);
                }
            }
            else
            {
                material.UpdatedAt = DateTime.UtcNow;
                _context.OnboardingMaterials.Update(material);
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("Material {MaterialId} updated successfully.", id);

            return Ok(MapToOnboardingMaterialDto(material));
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "File operation failed during material update.");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "File operation failed. Please try again." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during material update.");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An unexpected error occurred during material update." });
        }
    }

    /// <summary>
    /// Deletes an onboarding material and removes the associated file from Azure Blob Storage.
    /// Requires SuperAdmin or Admin role.
    /// </summary>
    /// <param name="id">The material ID to delete.</param>
    /// <returns>No content on successful deletion.</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> DeleteMaterial(int id)
    {
        try
        {
            var material = await _context.OnboardingMaterials.FindAsync(id);

            if (material == null)
            {
                return NotFound(new { message = "Onboarding material not found." });
            }

            // Delete material from database first
            _context.OnboardingMaterials.Remove(material);
            await _context.SaveChangesAsync();

            // Only attempt to delete blob after DB deletion succeeds
            try
            {
                string blobName = ExtractBlobNameFromUrl(material.Url);
                await _blobStorageService.DeleteBlobAsync(blobName);
            }
            catch (Exception ex)
            {
                // Log but don't fail if blob deletion fails - DB is already updated
                _logger.LogWarning(ex, "Failed to delete blob for material {MaterialId}, but DB deletion succeeded.", id);
            }

            _logger.LogInformation("Material {MaterialId} deleted successfully.", id);

            return NoContent();
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "File deletion failed for material {MaterialId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "File deletion failed. Please try again." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during material deletion.");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An unexpected error occurred during material deletion." });
        }
    }

    /// <summary>
    /// Maps an OnboardingMaterial entity to an OnboardingMaterialDto.
    /// </summary>
    private static OnboardingMaterialDto MapToOnboardingMaterialDto(OnboardingMaterial material)
    {
        return new OnboardingMaterialDto
        {
            Id = material.Id,
            FileName = material.FileName,
            FileType = material.FileType,
            Url = material.Url,
            CreatedAt = material.CreatedAt,
            UpdatedAt = material.UpdatedAt,
            TaskId = material.TaskId
        };
    }

    /// <summary>
    /// Extracts the blob name from an Azure Blob Storage URL.
    /// </summary>
    /// <param name="url">The full URL of the blob.</param>
    /// <returns>The blob name (path and filename).</returns>
    /// <exception cref="ArgumentException">Thrown if URL is null/empty or not a valid Azure Blob Storage URL.</exception>
    private static string ExtractBlobNameFromUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("URL cannot be null or empty.", nameof(url));
        }

        try
        {
            // URL format: https://{account}.blob.core.windows.net/{container}/{blobName}
            var uri = new Uri(url);

            // Validate that this is an Azure Blob Storage URL
            if (!uri.Host.Contains(".blob.core.windows.net"))
            {
                throw new ArgumentException("URL is not a valid Azure Blob Storage URL.", nameof(url));
            }

            var pathSegments = uri.AbsolutePath.TrimStart('/').Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (pathSegments.Length < 2)
            {
                throw new ArgumentException("URL does not contain a valid blob path.", nameof(url));
            }

            // Skip the container name (first segment) and join the rest
            return string.Join("/", pathSegments.Skip(1));
        }
        catch (UriFormatException ex)
        {
            throw new ArgumentException($"URL is not in a valid format: {url}", nameof(url), ex);
        }
    }
}
