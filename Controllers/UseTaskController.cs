using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NOX_Backend.Models;
using NOX_Backend.Models.DTOs;
using NOX_Backend.Models.Onboarding;

namespace NOX_Backend.Controllers
{
    [ApiController]
    [Route("api/onboarding/user-tasks")]
    public class UserTaskProgressController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserTaskProgressController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Get all progress for a specific user
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserTaskProgress(string userId)
        {
            var tasks = await _context.UserOnboardingTaskProgress
                .Where(p => p.UserId == userId)
                .Include(p => p.Task)
                .Select(p => new UserTaskProgressDto
                {
                    TaskId = p.TaskId,
                    Status = p.Status,
                    UpdatedAt = p.UpdatedAt,
                    TaskTitle = p.Task!.Title,
                    TaskDescription = p.Task.Description
                })
                .ToListAsync();

            return Ok(tasks);
        }

        // PUT: update task status
        [HttpPut("{userId}/{taskId}")]
        public async Task<IActionResult> UpdateTaskStatus(string userId, int taskId,
            [FromBody] UpdateTaskStatusRequest request)
        {
            var progress = await _context.UserOnboardingTaskProgress
                .Include(p => p.Task)
                .FirstOrDefaultAsync(p => p.TaskId == taskId && p.UserId == userId);

            if (progress == null)
                return NotFound();

            progress.Status = request.Status;
            progress.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new UserTaskProgressDto
            {
                TaskId = progress.TaskId,
                Status = progress.Status,
                UpdatedAt = progress.UpdatedAt,
                TaskTitle = progress.Task!.Title,
                TaskDescription = progress.Task.Description
            });
        }

        // POST: Create a task progress entry for a user
        [HttpPost("{userId}/{taskId}")]
        public async Task<IActionResult> CreateUserTaskProgress(string userId, int taskId)
        {
            // Check if task exists
            var task = await _context.OnboardingTasks.FirstOrDefaultAsync(t => t.Id == taskId);
            if (task == null)
                return NotFound(new { message = "Task not found." });

            // Check if entry already exists
            var existing = await _context.UserOnboardingTaskProgress
                .FirstOrDefaultAsync(p => p.UserId == userId && p.TaskId == taskId);

            if (existing != null)
                return BadRequest(new { message = "Progress record already exists for this user and task." });

            // Create new record (ENTITY MODEL, NOT DTO)
            var progress = new UserOnboardingTaskProgress
            {
                UserId = userId,
                TaskId = taskId,
                Status = "pending",
                UpdatedAt = DateTime.UtcNow
            };

            _context.UserOnboardingTaskProgress.Add(progress);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserTaskProgress), new { userId = userId }, new UserTaskProgressDto
            {
                TaskId = taskId,
                Status = progress.Status,
                UpdatedAt = progress.UpdatedAt,
                TaskTitle = task.Title,
                TaskDescription = task.Description
            });
        }
    }

    public class UpdateTaskStatusRequest
    {
        public string Status { get; set; } = "pending";
    }
}
