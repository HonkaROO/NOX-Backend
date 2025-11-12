using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NOX_Backend.Models;
using NOX_Backend.Models.DTOs.Onboarding;
using NOX_Backend.Models.Onboarding;

namespace NOX_Backend.Controllers.Onboarding;

/// <summary>
/// Controller for managing onboarding tasks.
/// Provides CRUD operations for onboarding tasks with role-based authorization.
/// </summary>
[ApiController]
[Route("api/onboarding/tasks")]
public class OnboardingTaskController : ControllerBase
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the OnboardingTaskController.
    /// </summary>
    public OnboardingTaskController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets all onboarding tasks.
    /// </summary>
    /// <returns>List of all onboarding tasks.</returns>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<OnboardingTaskDto>>> GetTasks()
    {
        var tasks = await _context.OnboardingTasks
            .Include(t => t.Materials)
            .Include(t => t.Steps)
            .OrderBy(t => t.Title)
            .ToListAsync();

        var taskDtos = tasks.Select(MapToOnboardingTaskDto).ToList();

        return Ok(taskDtos);
    }

    /// <summary>
    /// Gets all onboarding tasks for a specific folder.
    /// </summary>
    /// <param name="folderId">The folder ID.</param>
    /// <returns>List of onboarding tasks in the specified folder.</returns>
    [HttpGet("folder/{folderId}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<OnboardingTaskDto>>> GetTasksByFolder(int folderId)
    {
        // Verify folder exists
        var folderExists = await _context.OnboardingFolders
            .AnyAsync(f => f.Id == folderId);

        if (!folderExists)
        {
            return NotFound(new { message = "Onboarding folder not found." });
        }

        var tasks = await _context.OnboardingTasks
            .Where(t => t.FolderId == folderId)
            .Include(t => t.Materials)
            .Include(t => t.Steps)
            .OrderBy(t => t.Title)
            .ToListAsync();

        var taskDtos = tasks.Select(MapToOnboardingTaskDto).ToList();

        return Ok(taskDtos);
    }

    /// <summary>
    /// Gets a specific onboarding task by ID.
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <returns>The requested onboarding task with details.</returns>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<OnboardingTaskDto>> GetTask(int id)
    {
        var task = await _context.OnboardingTasks
            .Include(t => t.Materials)
            .Include(t => t.Steps)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
        {
            return NotFound(new { message = "Onboarding task not found." });
        }

        return Ok(MapToOnboardingTaskDto(task));
    }

    /// <summary>
    /// Creates a new onboarding task.
    /// Requires SuperAdmin or Admin role.
    /// </summary>
    /// <param name="request">The task creation request.</param>
    /// <returns>The created onboarding task.</returns>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<OnboardingTaskDto>> CreateTask(CreateOnboardingTaskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length > 255)
        {
            return BadRequest(new { message = "Title is required and must be between 1 and 255 characters." });
        }

        if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Length > 1000)
        {
            return BadRequest(new { message = "Description is required and must be between 1 and 1000 characters." });
        }

        // Verify folder exists
        var folderExists = await _context.OnboardingFolders
            .AnyAsync(f => f.Id == request.FolderId);

        if (!folderExists)
        {
            return BadRequest(new { message = "The specified folder does not exist." });
        }

        var task = new OnboardingTask
        {
            Title = request.Title,
            Description = request.Description,
            FolderId = request.FolderId,
            CreatedAt = DateTime.UtcNow
        };

        _context.OnboardingTasks.Add(task);
        await _context.SaveChangesAsync();

        // Reload the task with navigation properties to avoid null reference
        await _context.Entry(task)
            .Collection(t => t.Materials)
            .LoadAsync();
        await _context.Entry(task)
            .Collection(t => t.Steps)
            .LoadAsync();

        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, MapToOnboardingTaskDto(task));
    }

    /// <summary>
    /// Updates an existing onboarding task.
    /// Requires SuperAdmin or Admin role.
    /// </summary>
    /// <param name="id">The task ID to update.</param>
    /// <param name="request">The task update request.</param>
    /// <returns>The updated onboarding task.</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<OnboardingTaskDto>> UpdateTask(int id, UpdateOnboardingTaskRequest request)
    {
        var task = await _context.OnboardingTasks
            .Include(t => t.Materials)
            .Include(t => t.Steps)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
        {
            return NotFound(new { message = "Onboarding task not found." });
        }

        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length > 255)
        {
            return BadRequest(new { message = "Title is required and must be between 1 and 255 characters." });
        }

        if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Length > 1000)
        {
            return BadRequest(new { message = "Description is required and must be between 1 and 1000 characters." });
        }

        task.Title = request.Title;
        task.Description = request.Description;
        task.UpdatedAt = DateTime.UtcNow;

        _context.OnboardingTasks.Update(task);
        await _context.SaveChangesAsync();

        return Ok(MapToOnboardingTaskDto(task));
    }

    /// <summary>
    /// Deletes an onboarding task.
    /// Requires SuperAdmin or Admin role. Cascading delete will remove all associated materials and steps.
    /// </summary>
    /// <param name="id">The task ID to delete.</param>
    /// <returns>No content on successful deletion.</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var task = await _context.OnboardingTasks.FindAsync(id);

        if (task == null)
        {
            return NotFound(new { message = "Onboarding task not found." });
        }

        _context.OnboardingTasks.Remove(task);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Maps an OnboardingTask entity to an OnboardingTaskDto.
    /// </summary>
    private static OnboardingTaskDto MapToOnboardingTaskDto(OnboardingTask task)
    {
        return new OnboardingTaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            FolderId = task.FolderId,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            MaterialCount = task.Materials.Count,
            StepCount = task.Steps.Count
        };
    }
}
