using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkManagement.API.DTOs;
using WorkManagement.API.Models;
using WorkManagement.API.Services;

namespace WorkManagement.API.Controllers;

[ApiController]
[Route("api/projects/{projectId}/[controller]")]
[Authorize]
public class FileTicketsController : ControllerBase
{
    private readonly IFileTicketsService _fileTicketsService;
    private readonly ICurrentUserService _currentUser;

    public FileTicketsController(IFileTicketsService fileTicketsService, ICurrentUserService currentUser)
    {
        _fileTicketsService = fileTicketsService;
        _currentUser = currentUser;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FileTicketDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<FileTicketDto>>> GetFileTickets(int projectId, [FromQuery] int? currentHolderId, [FromQuery] FileTicketStatus? status)
    {
        var result = await _fileTicketsService.GetFileTicketsAsync(projectId, currentHolderId, status, _currentUser.UserId!.Value);
        
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
    [ProducesResponseType(typeof(FileTicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<FileTicketDto>> GetFileTicket(int projectId, int id)
    {
        var result = await _fileTicketsService.GetFileTicketAsync(projectId, id, _currentUser.UserId!.Value);
        
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
    [ProducesResponseType(typeof(FileTicketDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<FileTicketDto>> CreateFileTicket(int projectId, [FromBody] CreateFileTicketDto dto)
    {
        var result = await _fileTicketsService.CreateFileTicketAsync(projectId, dto, _currentUser.UserId!.Value);
        
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

        return CreatedAtAction(nameof(GetFileTicket), new { projectId, id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(FileTicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<FileTicketDto>> UpdateFileTicket(int projectId, int id, [FromBody] UpdateFileTicketDto dto)
    {
        var result = await _fileTicketsService.UpdateFileTicketAsync(projectId, id, dto, _currentUser.UserId!.Value);
        
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

    [HttpPost("{id}/transfer")]
    [ProducesResponseType(typeof(FileTicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<FileTicketDto>> TransferFileTicket(int projectId, int id, [FromBody] TransferFileTicketDto dto)
    {
        var result = await _fileTicketsService.TransferFileTicketAsync(projectId, id, dto, _currentUser.UserId!.Value);
        
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

    [HttpPost("{id}/receive")]
    [ProducesResponseType(typeof(FileTicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<FileTicketDto>> ReceiveFileTicket(int projectId, int id)
    {
        var result = await _fileTicketsService.ReceiveFileTicketAsync(projectId, id, _currentUser.UserId!.Value);
        
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

    [HttpGet("{id}/transfers")]
    [ProducesResponseType(typeof(IEnumerable<FileTicketTransferDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<FileTicketTransferDto>>> GetTransfers(int projectId, int id)
    {
        var result = await _fileTicketsService.GetTransfersAsync(projectId, id, _currentUser.UserId!.Value);
        
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
    public async Task<IActionResult> DeleteFileTicket(int projectId, int id)
    {
        var result = await _fileTicketsService.DeleteFileTicketAsync(projectId, id, _currentUser.UserId!.Value);
        
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
}
