using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkManagement.API.DTOs;
using WorkManagement.API.Models;
using WorkManagement.API.Services;

namespace WorkManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<ProjectsController> _logger;

    public ProjectsController(IProjectService projectService, ICurrentUserService currentUser, ILogger<ProjectsController> logger)
    {
        _projectService = projectService;
        _currentUser = currentUser;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ProjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProjects()
    {
        var result = await _projectService.GetAllAsync(
            _currentUser.UserId!.Value,
            _currentUser.CompanyId,
            _currentUser.IsSuperAdmin,
            _currentUser.IsAdmin);

        return HandleResult(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProject(int id)
    {
        var result = await _projectService.GetByIdAsync(
            id,
            _currentUser.UserId!.Value,
            _currentUser.CompanyId,
            _currentUser.IsSuperAdmin,
            _currentUser.IsAdmin);

        return HandleResult(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto dto)
    {
        if (!_currentUser.CompanyId.HasValue)
            return Unauthorized(new ErrorResponse { Message = "Company not found" });

        var result = await _projectService.CreateAsync(dto, _currentUser.UserId!.Value, _currentUser.CompanyId.Value);

        if (result.IsSuccess && result.ResultType == ServiceResultType.Created)
            return CreatedAtAction(nameof(GetProject), new { id = result.Data!.Id }, result.Data);

        return HandleResult(result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProject(int id, [FromBody] UpdateProjectDto dto)
    {
        var result = await _projectService.UpdateAsync(
            id,
            dto,
            _currentUser.UserId!.Value,
            _currentUser.CompanyId,
            _currentUser.IsSuperAdmin,
            _currentUser.IsAdmin);

        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteProject(int id)
    {
        var result = await _projectService.DeleteAsync(
            id,
            _currentUser.UserId!.Value,
            _currentUser.CompanyId,
            _currentUser.IsAdmin);

        if (result.IsSuccess)
            return NoContent();

        return HandleResult(result);
    }

    [HttpGet("{projectId}/members")]
    [ProducesResponseType(typeof(List<ProjectMemberDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMembers(int projectId)
    {
        var result = await _projectService.GetMembersAsync(
            projectId,
            _currentUser.UserId!.Value,
            _currentUser.CompanyId,
            _currentUser.IsSuperAdmin,
            _currentUser.IsAdmin);

        return HandleResult(result);
    }

    [HttpGet("{projectId}/available-users")]
    [ProducesResponseType(typeof(List<UserSelectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAvailableUsers(int projectId, [FromQuery] string? search, [FromQuery] bool? unassignedOnly)
    {
        var result = await _projectService.GetAvailableUsersAsync(
            projectId,
            _currentUser.UserId!.Value,
            _currentUser.CompanyId,
            search,
            unassignedOnly);

        return HandleResult(result);
    }

    [HttpPost("{projectId}/members")]
    [ProducesResponseType(typeof(ProjectMemberDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddMember(int projectId, [FromBody] AddProjectMemberDto dto)
    {
        var result = await _projectService.AddMemberAsync(
            projectId,
            dto,
            _currentUser.UserId!.Value,
            _currentUser.CompanyId);

        if (result.IsSuccess && result.ResultType == ServiceResultType.Created)
            return CreatedAtAction(nameof(GetMembers), new { projectId }, result.Data);

        return HandleResult(result);
    }

    [HttpPost("{projectId}/members/bulk")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> BulkAddMembers(int projectId, [FromBody] BulkAddProjectMembersDto dto)
    {
        var result = await _projectService.BulkAddMembersAsync(
            projectId,
            dto,
            _currentUser.UserId!.Value,
            _currentUser.CompanyId);

        if (result.IsSuccess)
            return Ok(new { message = $"Added {result.Data} members to project" });

        return HandleResult(result);
    }

    [HttpPut("{projectId}/members/{memberId}")]
    [ProducesResponseType(typeof(ProjectMemberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateMember(int projectId, int memberId, [FromBody] UpdateProjectMemberDto dto)
    {
        var result = await _projectService.UpdateMemberAsync(projectId, memberId, dto, _currentUser.UserId!.Value);
        return HandleResult(result);
    }

    [HttpDelete("{projectId}/members/{memberId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveMember(int projectId, int memberId)
    {
        var result = await _projectService.RemoveMemberAsync(projectId, memberId, _currentUser.UserId!.Value);

        if (result.IsSuccess)
            return NoContent();

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
