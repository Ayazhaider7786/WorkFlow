using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkManagement.API.DTOs;
using WorkManagement.API.Models;
using WorkManagement.API.Services;

namespace WorkManagement.API.Controllers;

[ApiController]
[Route("api/projects/{projectId}/[controller]")]
[Authorize]
public class WorkItemsController : ControllerBase
{
    private readonly IWorkItemsService _workItemsService;
    private readonly ICurrentUserService _currentUser;

    public WorkItemsController(IWorkItemsService workItemsService, ICurrentUserService currentUser)
    {
        _workItemsService = workItemsService;
        _currentUser = currentUser;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<WorkItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<WorkItemDto>>> GetWorkItems(
        int projectId,
        [FromQuery] int? sprintId = null,
        [FromQuery] bool? backlogOnly = null,
        [FromQuery] int? assignedToId = null,
        [FromQuery] int? parentId = null,
        [FromQuery] WorkItemType? type = null)
    {
        var result = await _workItemsService.GetWorkItemsAsync(projectId, sprintId, backlogOnly, assignedToId, parentId, type, _currentUser.UserId!.Value);
        
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
    [ProducesResponseType(typeof(WorkItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<WorkItemDto>> GetWorkItem(int projectId, int id)
    {
        var result = await _workItemsService.GetWorkItemAsync(projectId, id, _currentUser.UserId!.Value);
        
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

    [HttpGet("{id}/children")]
    [ProducesResponseType(typeof(IEnumerable<WorkItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<WorkItemDto>>> GetChildren(int projectId, int id)
    {
        var result = await _workItemsService.GetChildrenAsync(projectId, id, _currentUser.UserId!.Value);
        
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
    [ProducesResponseType(typeof(WorkItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<WorkItemDto>> CreateWorkItem(int projectId, [FromBody] CreateWorkItemDto dto)
    {
        var result = await _workItemsService.CreateWorkItemAsync(projectId, dto, _currentUser.UserId!.Value);
        
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

        return CreatedAtAction(nameof(GetWorkItem), new { projectId, id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(WorkItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<WorkItemDto>> UpdateWorkItem(int projectId, int id, [FromBody] UpdateWorkItemDto dto)
    {
        var result = await _workItemsService.UpdateWorkItemAsync(projectId, id, dto, _currentUser.UserId!.Value);
        
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

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteWorkItem(int projectId, int id)
    {
        var result = await _workItemsService.DeleteWorkItemAsync(projectId, id, _currentUser.UserId!.Value);
        
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
}
