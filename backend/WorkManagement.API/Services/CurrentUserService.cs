using System.Security.Claims;
using WorkManagement.API.Models;

namespace WorkManagement.API.Services;

public interface ICurrentUserService
{
    int? UserId { get; }
    int? CompanyId { get; }
    string? Email { get; }
    string? SystemRoleString { get; }
    SystemRole SystemRole { get; }
    bool IsSystemAdmin { get; }
    bool IsSuperAdmin { get; }
    bool IsAdmin { get; }
    bool IsManager { get; }
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public int? UserId => int.TryParse(User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    
    public int? CompanyId => int.TryParse(User?.FindFirstValue("CompanyId"), out var id) ? id : null;
    
    public string? Email => User?.FindFirstValue(ClaimTypes.Email);
    
    public string? SystemRoleString => User?.FindFirstValue("SystemRole");

    public SystemRole SystemRole
    {
        get
        {
            return SystemRoleString switch
            {
                "SuperAdmin" => Models.SystemRole.SuperAdmin,
                "Admin" => Models.SystemRole.Admin,
                "Manager" => Models.SystemRole.Manager,
                "QA" => Models.SystemRole.QA,
                _ => Models.SystemRole.Member
            };
        }
    }
    
    public bool IsSystemAdmin => SystemRoleString == "SystemAdmin" || SystemRoleString == "SuperAdmin";
    public bool IsSuperAdmin => SystemRoleString == "SuperAdmin";
    public bool IsAdmin => SystemRoleString == "Admin" || IsSuperAdmin;
    public bool IsManager => SystemRoleString == "Manager" || IsAdmin;
}
