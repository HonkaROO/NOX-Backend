using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NOX_Backend.Models;
using NOX_Backend.Models.DTOs.Onboarding;
using NOX_Backend.Models.Onboarding;

namespace NOX_Backend.Controllers.Onboarding;

/// <summary>
/// Controller for managing onboarding folders.
/// Provides CRUD operations for onboarding folders with role-based authorization.
/// </summary>
[ApiController]
[Route("api/onboarding/folders")]
public class OnboardingFolderController : ControllerBase
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the OnboardingFolderController.
    /// </summary>
    public OnboardingFolderController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets all onboarding folders.
    /// </summary>
    /// <returns>List of all onboarding folders.</returns>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<OnboardingFolderDto>>> GetFolders()
    {
        var folders = await _context.OnboardingFolders
            .Include(f => f.Tasks)
            .OrderBy(f => f.Title)
            .ToListAsync();

        var folderDtos = folders.Select(MapToOnboardingFolderDto).ToList();

        return Ok(folderDtos);
    }

    /// <summary>
    /// Gets a specific onboarding folder by ID.
    /// </summary>
    /// <param name="id">The folder ID.</param>
    /// <returns>The requested onboarding folder with details.</returns>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<OnboardingFolderDto>> GetFolder(int id)
    {
        var folder = await _context.OnboardingFolders
            .Include(f => f.Tasks)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (folder == null)
        {
            return NotFound(new { message = "Onboarding folder not found." });
        }

        return Ok(MapToOnboardingFolderDto(folder));
    }

    /// <summary>
    /// Creates a new onboarding folder.
    /// Requires SuperAdmin or Admin role.
    /// </summary>
    /// <param name="request">The folder creation request.</param>
    /// <returns>The created onboarding folder.</returns>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<OnboardingFolderDto>> CreateFolder(CreateOnboardingFolderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length > 255)
        {
            return BadRequest(new { message = "Title is required and must be between 1 and 255 characters." });
        }

        if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Length > 1000)
        {
            return BadRequest(new { message = "Description is required and must be between 1 and 1000 characters." });
        }

        // Check if folder with same title already exists
        var existingFolder = await _context.OnboardingFolders
            .FirstOrDefaultAsync(f => f.Title == request.Title);

        if (existingFolder != null)
        {
            return Conflict(new { message = "A folder with this title already exists." });
        }

        var folder = new OnboardingFolder
        {
            Title = request.Title,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };

        _context.OnboardingFolders.Add(folder);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetFolder), new { id = folder.Id }, MapToOnboardingFolderDto(folder));
    }

    /// <summary>
    /// Updates an existing onboarding folder.
    /// Requires SuperAdmin or Admin role.
    /// </summary>
    /// <param name="id">The folder ID to update.</param>
    /// <param name="request">The folder update request.</param>
    /// <returns>The updated onboarding folder.</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<OnboardingFolderDto>> UpdateFolder(int id, UpdateOnboardingFolderRequest request)
    {
        var folder = await _context.OnboardingFolders
            .Include(f => f.Tasks)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (folder == null)
        {
            return NotFound(new { message = "Onboarding folder not found." });
        }

        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length > 255)
        {
            return BadRequest(new { message = "Title is required and must be between 1 and 255 characters." });
        }

        if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Length > 1000)
        {
            return BadRequest(new { message = "Description is required and must be between 1 and 1000 characters." });
        }

        // Check if another folder with the same title already exists
        var existingFolder = await _context.OnboardingFolders
            .FirstOrDefaultAsync(f => f.Title == request.Title && f.Id != id);

        if (existingFolder != null)
        {
            return Conflict(new { message = "A folder with this title already exists." });
        }

        folder.Title = request.Title;
        folder.Description = request.Description;
        folder.UpdatedAt = DateTime.UtcNow;

        _context.OnboardingFolders.Update(folder);
        await _context.SaveChangesAsync();

        return Ok(MapToOnboardingFolderDto(folder));
    }

    /// <summary>
    /// Deletes an onboarding folder.
    /// Requires SuperAdmin or Admin role. Cascading delete will remove all associated tasks.
    /// </summary>
    /// <param name="id">The folder ID to delete.</param>
    /// <returns>No content on successful deletion.</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> DeleteFolder(int id)
    {
        var folder = await _context.OnboardingFolders.FindAsync(id);

        if (folder == null)
        {
            return NotFound(new { message = "Onboarding folder not found." });
        }

        _context.OnboardingFolders.Remove(folder);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Maps an OnboardingFolder entity to an OnboardingFolderDto.
    /// </summary>
    private static OnboardingFolderDto MapToOnboardingFolderDto(OnboardingFolder folder)
    {
        return new OnboardingFolderDto
        {
            Id = folder.Id,
            Title = folder.Title,
            Description = folder.Description,
            CreatedAt = folder.CreatedAt,
            UpdatedAt = folder.UpdatedAt,
            TaskCount = folder.Tasks.Count
        };
    }
}
