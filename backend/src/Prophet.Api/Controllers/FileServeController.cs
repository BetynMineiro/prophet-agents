using Microsoft.AspNetCore.Mvc;
using Prophet.Application.Interfaces.Storage;

namespace Prophet.Api.Controllers;

[ApiController]
[Route("v1/prophet/files")]
public class FileServeController(IStorageService storageService) : ControllerBase
{
    [HttpGet("{*path}")]
    public async Task<IActionResult> GetFile(string path, CancellationToken ct)
    {
        var bytes = await storageService.ReadObjectAsync(path, ct);
        if (bytes is null) return NotFound();
        var contentType = "application/octet-stream";
        var fileName = Path.GetFileName(path);
        return File(bytes, contentType, fileName);
    }
}
