using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkManagement.API.DTOs;
using WorkManagement.API.Services;

namespace WorkManagement.API.Controllers;

[ApiController]
[Route("api/projects/{projectId}/[controller]")]
[Authorize]
public class SprintsController : ControllerBase
{
    private readonly ISprintsService _sprintsService;
    private readonly ICurrentUserService _currentUser;

    public SprintsController(ISprintsService sprintsService, ICurrentUserService currentUser)
    {
        _sprintsService = sprintsService;
        _currentUser = currentUser;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SprintDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<SprintDto>>> GetSprints(int projectId)
    {
        var result = await _sprintsService.GetSprintsAsync(projectId, _currentUser.UserId!.Value);

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
    [ProducesResponseType(typeof(SprintDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SprintDto>> GetSprint(int projectId, int id)
    {
        var result = await _sprintsService.GetSprintAsync(projectId, id, _currentUser.UserId!.Value);

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
    [ProducesResponseType(typeof(SprintDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SprintDto>> CreateSprint(int projectId, [FromBody] CreateSprintDto dto)
    {
        var result = await _sprintsService.CreateSprintAsync(projectId, dto, _currentUser.UserId!.Value);

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

        return CreatedAtAction(nameof(GetSprint), new { projectId, id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(SprintDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SprintDto>> UpdateSprint(int projectId, int id, [FromBody] UpdateSprintDto dto)
    {
        var result = await _sprintsService.UpdateSprintAsync(projectId, id, dto, _currentUser.UserId!.Value);

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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteSprint(int projectId, int id)
    {
        var result = await _sprintsService.DeleteSprintAsync(projectId, id, _currentUser.UserId!.Value);

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

    [HttpPost("{id}/start")]
    [ProducesResponseType(typeof(SprintDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SprintDto>> StartSprint(int projectId, int id)
    {
        var result = await _sprintsService.StartSprintAsync(projectId, id, _currentUser.UserId!.Value);

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

    [HttpPost("{id}/complete")]
    [ProducesResponseType(typeof(SprintDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SprintDto>> CompleteSprint(int projectId, int id)
    {
        var result = await _sprintsService.CompleteSprintAsync(projectId, id, _currentUser.UserId!.Value);

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
}
