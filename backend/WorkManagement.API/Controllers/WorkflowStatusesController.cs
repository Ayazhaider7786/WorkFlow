using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkManagement.API.DTOs;
using WorkManagement.API.Services;

namespace WorkManagement.API.Controllers;

[ApiController]
[Route("api/projects/{projectId}/[controller]")]
[Authorize]
public class WorkflowStatusesController : ControllerBase
{
    private readonly IWorkflowStatusesService _workflowStatusesService;
    private readonly ICurrentUserService _currentUser;

    public WorkflowStatusesController(IWorkflowStatusesService workflowStatusesService, ICurrentUserService currentUser)
    {
        _workflowStatusesService = workflowStatusesService;
        _currentUser = currentUser;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<WorkflowStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<WorkflowStatusDto>>> GetStatuses(int projectId)
    {
        var result = await _workflowStatusesService.GetStatusesAsync(projectId, _currentUser.UserId!.Value);
        
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

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(WorkflowStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<WorkflowStatusDto>> GetStatus(int projectId, int id)
    {
        var result = await _workflowStatusesService.GetStatusAsync(projectId, id, _currentUser.UserId!.Value);
        
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
    [ProducesResponseType(typeof(WorkflowStatusDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<WorkflowStatusDto>> CreateStatus(int projectId, [FromBody] CreateWorkflowStatusDto dto)
    {
        var result = await _workflowStatusesService.CreateStatusAsync(projectId, dto, _currentUser.UserId!.Value);
        
        if (!result.IsSuccess)
        {
            return result.ResultType switch
            {
                ServiceResultType.BadRequest => BadRequest(new { message = result.ErrorMessage }),
                ServiceResultType.NotFound => NotFound(new { message = result.ErrorMessage }),
                ServiceResultType.Forbidden => Forbid(),
                ServiceResultType.Unauthorized => Unauthorized(new { message = result.ErrorMessage }),
                _ => StatusCode(500, new { message = result.ErrorMessage })
            };
        }

        return CreatedAtAction(nameof(GetStatus), new { projectId, id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(WorkflowStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<WorkflowStatusDto>> UpdateStatus(int projectId, int id, [FromBody] UpdateWorkflowStatusDto dto)
    {
        var result = await _workflowStatusesService.UpdateStatusAsync(projectId, id, dto, _currentUser.UserId!.Value);
        
        if (!result.IsSuccess)
        {
            return result.ResultType switch
            {
                ServiceResultType.BadRequest => BadRequest(new { message = result.ErrorMessage }),
                ServiceResultType.NotFound => NotFound(new { message = result.ErrorMessage }),
                ServiceResultType.Forbidden => Forbid(),
                ServiceResultType.Unauthorized => Unauthorized(new { message = result.ErrorMessage }),
                _ => StatusCode(500, new { message = result.ErrorMessage })
            };
        }

        return Ok(result.Data);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteStatus(int projectId, int id)
    {
        var result = await _workflowStatusesService.DeleteStatusAsync(projectId, id, _currentUser.UserId!.Value);
        
        if (!result.IsSuccess)
        {
            return result.ResultType switch
            {
                ServiceResultType.BadRequest => BadRequest(new { message = result.ErrorMessage }),
                ServiceResultType.NotFound => NotFound(new { message = result.ErrorMessage }),
                ServiceResultType.Forbidden => Forbid(),
                ServiceResultType.Unauthorized => Unauthorized(new { message = result.ErrorMessage }),
                _ => StatusCode(500, new { message = result.ErrorMessage })
            };
        }

        return NoContent();
    }

    [HttpPut("reorder")]
    [ProducesResponseType(typeof(IEnumerable<WorkflowStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<WorkflowStatusDto>>> ReorderStatuses(int projectId, [FromBody] List<int> statusIds)
    {
        var result = await _workflowStatusesService.ReorderStatusesAsync(projectId, statusIds, _currentUser.UserId!.Value);
        
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
}
