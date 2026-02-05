using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkManagement.API.DTOs;
using WorkManagement.API.Services;

namespace WorkManagement.API.Controllers;

[ApiController]
[Route("api/projects/{projectId}/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ICurrentUserService _currentUser;

    public DashboardController(IDashboardService dashboardService, ICurrentUserService currentUser)
    {
        _dashboardService = dashboardService;
        _currentUser = currentUser;
    }

    [HttpGet]
    [ProducesResponseType(typeof(DashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<DashboardDto>> GetDashboard(int projectId)
    {
        var result = await _dashboardService.GetDashboardAsync(projectId, _currentUser.UserId!.Value);

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
