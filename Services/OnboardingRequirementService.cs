using Backend.Models.Onboarding;
using Microsoft.EntityFrameworkCore;
using NOX_Backend.Models.DTOs;

namespace NOX_Backend.Services
{
    public class OnboardingRequirementService
    {
        private readonly AppDbContext _db;

        public OnboardingRequirementService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<object>> GetUserChecklist(string userId)
        {
            // TODO: Replace with actual implementation
            await Task.CompletedTask;
            return new List<object>();
        }

        public async Task<List<RequirementDto>> InitializeForUser(string userId)
        {
            var requirements = await _db.Set<Requirement>().Where(r => r.IsActive).ToListAsync();

            foreach (var req in requirements)
            {
                // avoid creating duplicate user requirements
                var exists = await _db.Set<UserRequirement>()
                    .AnyAsync(ur => ur.UserId == userId && ur.RequirementId == req.Id);

                if (!exists)
                {
                    _db.Set<UserRequirement>().Add(new UserRequirement
                    {
                        UserId = userId,
                        RequirementId = req.Id,
                        Requirement = req,
                        Status = "Pending"
                    });
                }
            }

            await _db.SaveChangesAsync();

            return await _db.Set<UserRequirement>()
                .Include(x => x.Requirement)
                .Where(x => x.UserId == userId)
                .Select(x => new RequirementDto
                {
                    Id = x.Id,
                    Category = x.Requirement.Category,
                    Name = x.Requirement.Name,
                    Status = x.Status,
                    FileUrl = x.FileUrl
                })
                .ToListAsync();
        }

        public async Task<bool> Submit(int id, string fileUrl)
        {
            var item = await _db.Set<UserRequirement>().FindAsync(id);
            if (item == null) return false;

            item.Status = "Submitted";
            item.FileUrl = fileUrl;
            item.SubmittedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Review(int id, bool approve, string reviewerId)
        {
            var item = await _db.Set<UserRequirement>().FindAsync(id);
            if (item == null) return false;

            item.Status = approve ? "Approved" : "Rejected";
            item.ReviewedAt = DateTime.UtcNow;
            item.ReviewerId = reviewerId;

            await _db.SaveChangesAsync();
            return true;
        }
    }
}