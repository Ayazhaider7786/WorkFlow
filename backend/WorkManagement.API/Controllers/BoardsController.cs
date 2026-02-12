using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkManagement.API.DTOs;
using WorkManagement.API.Services;

namespace WorkManagement.API.Controllers;

[ApiController]
[Route("api/projects/{projectId}/[controller]")]
[Authorize]
public class BoardsController : ControllerBase
{
    private readonly IBoardsService _boardsService;
    private readonly ICurrentUserService _currentUser;

    public BoardsController(IBoardsService boardsService, ICurrentUserService currentUser)
    {
        _boardsService = boardsService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BoardDto>>> GetBoards(int projectId)
    {
        var result = await _boardsService.GetProjectBoardsAsync(projectId, _currentUser.UserId!.Value);
        if (!result.IsSuccess) return HandleError(result);
        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BoardDto>> GetBoard(int projectId, int id)
    {
        var result = await _boardsService.GetBoardAsync(projectId, id, _currentUser.UserId!.Value);
        if (!result.IsSuccess) return HandleError(result);
        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<ActionResult<BoardDto>> CreateBoard(int projectId, [FromBody] CreateBoardDto dto)
    {
        var result = await _boardsService.CreateBoardAsync(projectId, dto, _currentUser.UserId!.Value);
        if (!result.IsSuccess) return HandleError(result);
        return CreatedAtAction(nameof(GetBoard), new { projectId, id = result.Data!.Id }, result.Data);
    }

    [HttpPost("{id}/columns")]
    public async Task<ActionResult<BoardDto>> AddColumn(int projectId, int id, [FromBody] AddColumnDto dto)
    {
        var result = await _boardsService.AddColumnAsync(projectId, id, dto.StatusId, _currentUser.UserId!.Value);
        if (!result.IsSuccess) return HandleError(result);
        return Ok(result.Data);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBoard(int projectId, int id)
    {
        var result = await _boardsService.DeleteBoardAsync(projectId, id, _currentUser.UserId!.Value);
        if (!result.IsSuccess) return HandleError(result);
        return NoContent();
    }

    // Helper to handle personal board creation logic if needed via specific endpoint, 
    // or just let frontend call CreateBoard with specific parameters. 
    // For now, adhering to the service interface.

    private ActionResult HandleError<T>(ServiceResult<T> result)
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
}
