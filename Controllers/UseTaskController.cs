using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NOX_Backend.Models;
using NOX_Backend.Models.DTOs;

namespace NOX_Backend.Controllers.Onboarding
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
            var tasks = await _context.UserTaskProgress
                .Where(p => p.UserId == userId)
                .Include(p => p.Task)
                .Select(p => new UserTaskProgressDto
                {
                    TaskId = p.TaskId,
                    Status = p.Status,
                    UpdatedAt = p.UpdatedAt,
                    TaskTitle = p.Task.Title,
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
            var progress = await _context.UserTaskProgress
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
                TaskTitle = progress.Task.Title,
                TaskDescription = progress.Task.Description
            });
        }
    }

    public class UpdateTaskStatusRequest
    {
        public string Status { get; set; } = "pending";
    }
}
