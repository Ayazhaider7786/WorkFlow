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
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ICurrentUserService currentUser, ILogger<UsersController> logger)
    {
        _userService = userService;
        _currentUser = currentUser;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUsers([FromQuery] string? search, [FromQuery] bool? unassigned)
    {
        var result = await _userService.GetAllAsync(
            _currentUser.UserId!.Value,
            _currentUser.CompanyId,
            _currentUser.IsSystemAdmin,
            _currentUser.IsSuperAdmin,
            search,
            unassigned);

        return HandleResult(result);
    }

    [HttpGet("with-projects")]
    [ProducesResponseType(typeof(List<UserWithProjectsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUsersWithProjects([FromQuery] string? search)
    {
        var result = await _userService.GetWithProjectsAsync(_currentUser.CompanyId, search);
        return HandleResult(result);
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMe()
    {
        var result = await _userService.GetMeAsync(_currentUser.UserId!.Value);
        return HandleResult(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserWithProjectsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUser(int id)
    {
        var result = await _userService.GetByIdWithProjectsAsync(
            id,
            _currentUser.UserId!.Value,
            _currentUser.CompanyId,
            _currentUser.IsSystemAdmin,
            _currentUser.IsSuperAdmin);

        return HandleResult(result);
    }

    [HttpGet("team")]
    [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTeamMembers()
    {
        var result = await _userService.GetTeamMembersAsync(_currentUser.UserId!.Value);
        return HandleResult(result);
    }

    [HttpGet("managers")]
    [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetManagers()
    {
        var result = await _userService.GetManagersAsync(_currentUser.CompanyId);
        return HandleResult(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        var result = await _userService.CreateAsync(
            dto,
            _currentUser.UserId!.Value,
            _currentUser.CompanyId,
            _currentUser.SystemRole);

        if (result.IsSuccess && result.ResultType == ServiceResultType.Created)
            return CreatedAtAction(nameof(GetUser), new { id = result.Data!.Id }, result.Data);

        return HandleResult(result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
    {
        var result = await _userService.UpdateAsync(
            id,
            dto,
            _currentUser.UserId!.Value,
            _currentUser.CompanyId,
            _currentUser.IsSystemAdmin,
            _currentUser.IsSuperAdmin,
            _currentUser.SystemRole);

        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var result = await _userService.DeleteAsync(id, _currentUser.UserId!.Value, _currentUser.SystemRole);

        if (result.IsSuccess)
            return NoContent();

        return HandleResult(result);
    }

    [HttpPost("transfer-super-admin")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> TransferSuperAdmin([FromBody] TransferSuperAdminDto dto)
    {
        var result = await _userService.TransferSuperAdminAsync(dto.NewSuperAdminId, _currentUser.UserId!.Value);
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
