using NOX_Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace NOX_Backend.Controllers.Onboarding;
/// <summary>
/// Controller for managing onboarding requirements.
/// </summary>
[ApiController]
[Route("api/onboarding/requirements")]
public class OnboardingRequirementsController : ControllerBase
{
    private readonly OnboardingRequirementService _service;

    public OnboardingRequirementsController(OnboardingRequirementService service)
    {
        _service = service;
    }

    [HttpGet("{userId}/checklist")]
    public async Task<IActionResult> Checklist(string userId)
    {
        var list = await _service.GetUserChecklist(userId);
        return Ok(list);
    }

    [HttpPost("submit/{id}")]
    public async Task<IActionResult> Submit(int id, [FromBody] string fileUrl)
    {
        var ok = await _service.Submit(id, fileUrl);
        return ok ? Ok() : NotFound();
    }

    [HttpPut("review/{id}")]
    public async Task<IActionResult> Review(int id, bool approve)
    {
        var reviewerId = User.FindFirst("usr_Id")?.Value;
        if (reviewerId == null)
        {
            return Unauthorized("Reviewer ID not found.");
        }
        var ok = await _service.Review(id, approve, reviewerId);
        return ok ? Ok() : NotFound();
    }
}

