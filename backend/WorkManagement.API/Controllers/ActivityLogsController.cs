using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkManagement.API.DTOs;
using WorkManagement.API.Models;
using WorkManagement.API.Services;

namespace WorkManagement.API.Controllers;

[ApiController]
[Authorize]
[Produces("application/json")]
public class ActivityLogsController : ControllerBase
{
    private readonly IActivityLogQueryService _activityLogQueryService;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<ActivityLogsController> _logger;

    public ActivityLogsController(
        IActivityLogQueryService activityLogQueryService,
        ICurrentUserService currentUser,
        ILogger<ActivityLogsController> logger)
    {
        _activityLogQueryService = activityLogQueryService;
        _currentUser = currentUser;
        _logger = logger;
    }

    [HttpGet("api/activitylogs")]
    [ProducesResponseType(typeof(PaginatedResponse<ActivityLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllActivityLogs(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int? userId,
        [FromQuery] int? projectId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _activityLogQueryService.GetAllAsync(
            _currentUser.UserId!.Value,
            _currentUser.CompanyId,
            startDate,
            endDate,
            userId,
            projectId,
            page,
            pageSize);

        return HandleResult(result);
    }

    [HttpGet("api/projects/{projectId}/activitylogs")]
    [ProducesResponseType(typeof(PaginatedResponse<ActivityLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProjectActivityLogs(
        int projectId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int? userId,
        [FromQuery] string? entityType,
        [FromQuery] int? entityId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _activityLogQueryService.GetByProjectAsync(
            projectId,
            _currentUser.UserId!.Value,
            _currentUser.CompanyId,
            _currentUser.IsSystemAdmin,
            startDate,
            endDate,
            userId,
            entityType,
            entityId,
            page,
            pageSize);

        return HandleResult(result);
    }

    [HttpGet("api/projects/{projectId}/activitylogs/workitem/{workItemId}")]
    [ProducesResponseType(typeof(List<ActivityLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetWorkItemLogs(int projectId, int workItemId)
    {
        var result = await _activityLogQueryService.GetByWorkItemAsync(
            projectId,
            workItemId,
            _currentUser.UserId!.Value,
            _currentUser.CompanyId,
            _currentUser.IsSystemAdmin);

        return HandleResult(result);
    }

    [HttpGet("api/projects/{projectId}/activitylogs/fileticket/{fileTicketId}")]
    [ProducesResponseType(typeof(List<ActivityLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFileTicketLogs(int projectId, int fileTicketId)
    {
        var result = await _activityLogQueryService.GetByFileTicketAsync(
            projectId,
            fileTicketId,
            _currentUser.UserId!.Value,
            _currentUser.CompanyId,
            _currentUser.IsSystemAdmin);

        return HandleResult(result);
    }

    [HttpGet("api/activitylogs/user/{userId}")]
    [ProducesResponseType(typeof(PaginatedResponse<ActivityLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserActivityLogs(
        int userId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _activityLogQueryService.GetByUserAsync(
            userId,
            _currentUser.UserId!.Value,
            _currentUser.CompanyId,
            startDate,
            endDate,
            page,
            pageSize);

        return HandleResult(result);
    }

    private IActionResult HandleResult<T>(ServiceResult<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Data);

        var errorResponse = new ErrorResponse
        {
            Message = result.ErrorMessage ?? "An error occurred",
            StackTrace = result.Exception?.StackTrace,
            InnerException = result.Exception?.InnerException?.Message
        };

        return result.ResultType switch
        {
            ServiceResultType.NotFound => NotFound(errorResponse),
            ServiceResultType.BadRequest => BadRequest(errorResponse),
            ServiceResultType.Unauthorized => Unauthorized(errorResponse),
            ServiceResultType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, errorResponse),
            _ => StatusCode(StatusCodes.Status500InternalServerError, errorResponse)
        };
    }
}
