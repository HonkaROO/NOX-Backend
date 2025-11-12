using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NOX_Backend.Models;
using NOX_Backend.Models.DTOs.Onboarding;
using NOX_Backend.Models.Onboarding;

namespace NOX_Backend.Controllers.Onboarding;

/// <summary>
/// Controller for managing onboarding steps.
/// Provides CRUD operations for onboarding steps with role-based authorization.
/// </summary>
[ApiController]
[Route("api/onboarding/steps")]
public class OnboardingStepsController : ControllerBase
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the OnboardingStepsController.
    /// </summary>
    public OnboardingStepsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets all onboarding steps.
    /// </summary>
    /// <returns>List of all onboarding steps.</returns>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<OnboardingStepsDto>>> GetSteps()
    {
        var steps = await _context.OnboardingSteps
            .OrderBy(s => s.Id)
            .ToListAsync();

        var stepDtos = steps.Select(MapToOnboardingStepsDto).ToList();

        return Ok(stepDtos);
    }

    /// <summary>
    /// Gets all onboarding steps for a specific task.
    /// </summary>
    /// <param name="taskId">The task ID.</param>
    /// <returns>List of onboarding steps in the specified task.</returns>
    [HttpGet("task/{taskId}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<OnboardingStepsDto>>> GetStepsByTask(int taskId)
    {
        // Verify task exists
        var taskExists = await _context.OnboardingTasks
            .AnyAsync(t => t.Id == taskId);

        if (!taskExists)
        {
            return NotFound(new { message = "Onboarding task not found." });
        }

        var steps = await _context.OnboardingSteps
            .Where(s => s.TaskId == taskId)
            .OrderBy(s => s.Id)
            .ToListAsync();

        var stepDtos = steps.Select(MapToOnboardingStepsDto).ToList();

        return Ok(stepDtos);
    }

    /// <summary>
    /// Gets a specific onboarding step by ID.
    /// </summary>
    /// <param name="id">The step ID.</param>
    /// <returns>The requested onboarding step with details.</returns>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<OnboardingStepsDto>> GetStep(int id)
    {
        var step = await _context.OnboardingSteps
            .FirstOrDefaultAsync(s => s.Id == id);

        if (step == null)
        {
            return NotFound(new { message = "Onboarding step not found." });
        }

        return Ok(MapToOnboardingStepsDto(step));
    }

    /// <summary>
    /// Creates a new onboarding step.
    /// Requires SuperAdmin or Admin role.
    /// </summary>
    /// <param name="request">The step creation request.</param>
    /// <returns>The created onboarding step.</returns>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<OnboardingStepsDto>> CreateStep(CreateOnboardingStepsRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.StepDescription) || request.StepDescription.Length > 1000)
        {
            return BadRequest(new { message = "StepDescription is required and must be between 1 and 1000 characters." });
        }

        // Verify task exists
        var taskExists = await _context.OnboardingTasks
            .AnyAsync(t => t.Id == request.TaskId);

        if (!taskExists)
        {
            return BadRequest(new { message = "The specified task does not exist." });
        }

        var step = new OnboardingSteps
        {
            StepDescription = request.StepDescription,
            TaskId = request.TaskId,
            CreatedAt = DateTime.UtcNow
        };

        _context.OnboardingSteps.Add(step);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetStep), new { id = step.Id }, MapToOnboardingStepsDto(step));
    }

    /// <summary>
    /// Updates an existing onboarding step.
    /// Requires SuperAdmin or Admin role.
    /// </summary>
    /// <param name="id">The step ID to update.</param>
    /// <param name="request">The step update request.</param>
    /// <returns>The updated onboarding step.</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<OnboardingStepsDto>> UpdateStep(int id, UpdateOnboardingStepsRequest request)
    {
        var step = await _context.OnboardingSteps
            .FirstOrDefaultAsync(s => s.Id == id);

        if (step == null)
        {
            return NotFound(new { message = "Onboarding step not found." });
        }

        if (string.IsNullOrWhiteSpace(request.StepDescription) || request.StepDescription.Length > 1000)
        {
            return BadRequest(new { message = "StepDescription is required and must be between 1 and 1000 characters." });
        }

        step.StepDescription = request.StepDescription;
        step.UpdatedAt = DateTime.UtcNow;

        _context.OnboardingSteps.Update(step);
        await _context.SaveChangesAsync();

        return Ok(MapToOnboardingStepsDto(step));
    }

    /// <summary>
    /// Deletes an onboarding step.
    /// Requires SuperAdmin or Admin role.
    /// </summary>
    /// <param name="id">The step ID to delete.</param>
    /// <returns>No content on successful deletion.</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> DeleteStep(int id)
    {
        var step = await _context.OnboardingSteps.FindAsync(id);

        if (step == null)
        {
            return NotFound(new { message = "Onboarding step not found." });
        }

        _context.OnboardingSteps.Remove(step);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Maps an OnboardingSteps entity to an OnboardingStepsDto.
    /// </summary>
    private static OnboardingStepsDto MapToOnboardingStepsDto(OnboardingSteps step)
    {
        return new OnboardingStepsDto
        {
            Id = step.Id,
            StepDescription = step.StepDescription,
            TaskId = step.TaskId,
            CreatedAt = step.CreatedAt,
            UpdatedAt = step.UpdatedAt
        };
    }
}
