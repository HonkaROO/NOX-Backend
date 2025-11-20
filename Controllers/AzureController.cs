using Microsoft.AspNetCore.Mvc;
using NOX_Backend.Services;

namespace NOX_Backend.Controllers.Onboarding;

[ApiController]
[Route("api/onboarding/blob")]
public class AzureBlobController : ControllerBase
{
    private readonly AzureBlobStorageService _blobStorage;

    public AzureBlobController(AzureBlobStorageService blobStorage)
    {
        _blobStorage = blobStorage;
    }

    // GET /api/onboarding/blob/blobs
    [HttpGet("blobs")]
    public async Task<IActionResult> ListBlobs()
    {
        var blobs = await _blobStorage.ListBlobsAsync();
        return Ok(blobs);
    }

    // POST /api/onboarding/blob/upload
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        string blobName = AzureBlobStorageService.GenerateUniqueBlobName(file.FileName);
        string url = await _blobStorage.UploadFileAsync(file, blobName);

        return Ok(new { fileUrl = url });
    }
}
