using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkManagement.API.DTOs;
using WorkManagement.API.Services;

namespace WorkManagement.API.Controllers;

[ApiController]
[Route("api/projects/{projectId}/workitems/{workItemId}/[controller]")]
[Authorize]
public class AttachmentsController : ControllerBase
{
    private readonly IAttachmentsService _attachmentsService;
    private readonly ICurrentUserService _currentUser;

    public AttachmentsController(IAttachmentsService attachmentsService, ICurrentUserService currentUser)
    {
        _attachmentsService = attachmentsService;
        _currentUser = currentUser;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AttachmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<AttachmentDto>>> GetAttachments(int projectId, int workItemId)
    {
        var result = await _attachmentsService.GetAttachmentsAsync(projectId, workItemId, _currentUser.UserId!.Value);
        
        if (!result.IsSuccess)
        {
            return result.ResultType switch
            {
                ServiceResultType.NotFound => NotFound(new { message = result.ErrorMessage }),
                ServiceResultType.Forbidden => Forbid(),
                ServiceResultType.Unauthorized => Unauthorized(new { message = result.ErrorMessage }),
                _ => StatusCode(500, new { message = result.ErrorMessage })
            };
        }

        return Ok(result.Data);
    }

    [HttpPost]
    [RequestSizeLimit(100 * 1024 * 1024)]
    [ProducesResponseType(typeof(AttachmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AttachmentDto>> UploadAttachment(int projectId, int workItemId, IFormFile file)
    {
        var result = await _attachmentsService.UploadAttachmentAsync(projectId, workItemId, file, _currentUser.UserId!.Value);

        if (!result.IsSuccess)
        {
            return result.ResultType switch
            {
                ServiceResultType.NotFound => NotFound(new { message = result.ErrorMessage }),
                ServiceResultType.Forbidden => Forbid(),
                ServiceResultType.Unauthorized => Unauthorized(new { message = result.ErrorMessage }),
                ServiceResultType.BadRequest => BadRequest(new { message = result.ErrorMessage }),
                _ => StatusCode(500, new { message = result.ErrorMessage })
            };
        }

        return CreatedAtAction(nameof(GetAttachments), new { projectId, workItemId }, result.Data);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteAttachment(int projectId, int workItemId, int id)
    {
        var result = await _attachmentsService.DeleteAttachmentAsync(projectId, workItemId, id, _currentUser.UserId!.Value);

        if (!result.IsSuccess)
        {
            return result.ResultType switch
            {
                ServiceResultType.NotFound => NotFound(new { message = result.ErrorMessage }),
                ServiceResultType.Forbidden => Forbid(),
                ServiceResultType.Unauthorized => Unauthorized(new { message = result.ErrorMessage }),
                _ => StatusCode(500, new { message = result.ErrorMessage })
            };
        }

        return NoContent();
    }

    [HttpGet("{id}/download")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadAttachment(int projectId, int workItemId, int id)
    {
        var result = await _attachmentsService.DownloadAttachmentAsync(projectId, workItemId, id);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        return File(result.Data!.FileBytes, result.Data.ContentType, result.Data.FileName);
    }
}
