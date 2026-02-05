using Microsoft.EntityFrameworkCore;
using WorkManagement.API.Data;
using WorkManagement.API.DTOs;
using WorkManagement.API.Models;

namespace WorkManagement.API.Services;

public interface IUserService
{
    Task<ServiceResult<List<UserDto>>> GetAllAsync(int userId, int? companyId, bool isSystemAdmin, bool isSuperAdmin, string? search, bool? unassigned);
    Task<ServiceResult<List<UserWithProjectsDto>>> GetWithProjectsAsync(int? companyId, string? search);
    Task<ServiceResult<UserDto>> GetByIdAsync(int id, int userId, int? companyId, bool isSystemAdmin, bool isSuperAdmin);
    Task<ServiceResult<UserWithProjectsDto>> GetByIdWithProjectsAsync(int id, int userId, int? companyId, bool isSystemAdmin, bool isSuperAdmin);
    Task<ServiceResult<UserDto>> GetMeAsync(int userId);
    Task<ServiceResult<List<UserDto>>> GetTeamMembersAsync(int userId);
    Task<ServiceResult<List<UserDto>>> GetManagersAsync(int? companyId);
    Task<ServiceResult<UserDto>> CreateAsync(CreateUserDto dto, int userId, int? companyId, SystemRole creatorRole);
    Task<ServiceResult<UserDto>> UpdateAsync(int id, UpdateUserDto dto, int userId, int? companyId, bool isSystemAdmin, bool isSuperAdmin, SystemRole updaterRole);
    Task<ServiceResult<bool>> DeleteAsync(int id, int userId, SystemRole deleterRole);
    Task<ServiceResult<UserDto>> TransferSuperAdminAsync(int newSuperAdminId, int userId);
}

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(ApplicationDbContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ServiceResult<List<UserDto>>> GetAllAsync(int userId, int? companyId, bool isSystemAdmin, bool isSuperAdmin, string? search, bool? unassigned)
    {
        try
        {
            var query = _context.Users
                .Include(u => u.Company)
                .Include(u => u.Manager)
                .Where(u => !u.IsDeleted)
                .AsQueryable();

            if (!isSystemAdmin && !isSuperAdmin)
            {
                query = query.Where(u => u.CompanyId == companyId);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u => u.Email.Contains(search) ||
                    u.FirstName.Contains(search) || u.LastName.Contains(search));
            }

            if (unassigned == true)
            {
                var assignedUserIds = await _context.ProjectMembers
                    .Where(pm => !pm.IsDeleted)
                    .Select(pm => pm.UserId)
                    .Distinct()
                    .ToListAsync();
                query = query.Where(u => !assignedUserIds.Contains(u.Id));
            }

            var users = await query
                .Select(u => new UserDto(
                    u.Id, u.Email, u.FirstName, u.LastName, u.Phone, u.SystemRole,
                    u.CompanyId, u.Company != null ? u.Company.Name : null,
                    u.ManagerId, u.Manager != null ? u.Manager.FirstName + " " + u.Manager.LastName : null))
                .ToListAsync();

            return ServiceResult<List<UserDto>>.Success(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return ServiceResult<List<UserDto>>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<List<UserWithProjectsDto>>> GetWithProjectsAsync(int? companyId, string? search)
    {
        try
        {
            var query = _context.Users
                .Include(u => u.Company)
                .Include(u => u.Manager)
                .Where(u => !u.IsDeleted && u.CompanyId == companyId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u => u.Email.Contains(search) ||
                    u.FirstName.Contains(search) || u.LastName.Contains(search));
            }

            var users = await query.ToListAsync();
            var userIds = users.Select(u => u.Id).ToList();

            var projectAssignments = await _context.ProjectMembers
                .Include(pm => pm.Project)
                .Where(pm => userIds.Contains(pm.UserId) && !pm.IsDeleted && !pm.Project.IsDeleted)
                .ToListAsync();

            var result = users.Select(u => new UserWithProjectsDto(
                u.Id, u.Email, u.FirstName, u.LastName, u.Phone, u.SystemRole,
                u.CompanyId, u.Company?.Name,
                u.ManagerId, u.Manager != null ? u.Manager.FirstName + " " + u.Manager.LastName : null,
                projectAssignments.Where(pa => pa.UserId == u.Id)
                    .Select(pa => new UserProjectAssignmentDto(pa.ProjectId, pa.Project.Name, pa.Project.Key, pa.Role))
            )).ToList();

            return ServiceResult<List<UserWithProjectsDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users with projects");
            return ServiceResult<List<UserWithProjectsDto>>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<UserDto>> GetByIdAsync(int id, int userId, int? companyId, bool isSystemAdmin, bool isSuperAdmin)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.Company)
                .Include(u => u.Manager)
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

            if (user == null)
                return ServiceResult<UserDto>.NotFound("User not found");

            if (!isSystemAdmin && !isSuperAdmin && user.CompanyId != companyId)
                return ServiceResult<UserDto>.Forbidden("Access denied");

            return ServiceResult<UserDto>.Success(new UserDto(
                user.Id, user.Email, user.FirstName, user.LastName, user.Phone, user.SystemRole,
                user.CompanyId, user.Company?.Name,
                user.ManagerId, user.Manager != null ? user.Manager.FirstName + " " + user.Manager.LastName : null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId}", id);
            return ServiceResult<UserDto>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<UserWithProjectsDto>> GetByIdWithProjectsAsync(int id, int userId, int? companyId, bool isSystemAdmin, bool isSuperAdmin)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.Company)
                .Include(u => u.Manager)
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

            if (user == null)
                return ServiceResult<UserWithProjectsDto>.NotFound("User not found");

            if (!isSystemAdmin && !isSuperAdmin && user.CompanyId != companyId)
                return ServiceResult<UserWithProjectsDto>.Forbidden("Access denied");

            var projectAssignments = await _context.ProjectMembers
                .Include(pm => pm.Project)
                .Where(pm => pm.UserId == id && !pm.IsDeleted && !pm.Project.IsDeleted)
                .Select(pm => new UserProjectAssignmentDto(pm.ProjectId, pm.Project.Name, pm.Project.Key, pm.Role))
                .ToListAsync();

            return ServiceResult<UserWithProjectsDto>.Success(new UserWithProjectsDto(
                user.Id, user.Email, user.FirstName, user.LastName, user.Phone, user.SystemRole,
                user.CompanyId, user.Company?.Name,
                user.ManagerId, user.Manager != null ? user.Manager.FirstName + " " + user.Manager.LastName : null,
                projectAssignments));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user with projects {UserId}", id);
            return ServiceResult<UserWithProjectsDto>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<UserDto>> GetMeAsync(int userId)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.Company)
                .Include(u => u.Manager)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return ServiceResult<UserDto>.NotFound("User not found");

            return ServiceResult<UserDto>.Success(new UserDto(
                user.Id, user.Email, user.FirstName, user.LastName, user.Phone, user.SystemRole,
                user.CompanyId, user.Company?.Name,
                user.ManagerId, user.Manager != null ? user.Manager.FirstName + " " + user.Manager.LastName : null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return ServiceResult<UserDto>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<List<UserDto>>> GetTeamMembersAsync(int userId)
    {
        try
        {
            var currentUserRole = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.SystemRole)
                .FirstOrDefaultAsync();

            if (currentUserRole < SystemRole.Manager)
            {
                return ServiceResult<List<UserDto>>.Success(new List<UserDto>());
            }

            var teamMembers = await _context.Users
                .Include(u => u.Company)
                .Where(u => u.ManagerId == userId && !u.IsDeleted)
                .Select(u => new UserDto(
                    u.Id, u.Email, u.FirstName, u.LastName, u.Phone, u.SystemRole,
                    u.CompanyId, u.Company != null ? u.Company.Name : null,
                    u.ManagerId, null))
                .ToListAsync();

            return ServiceResult<List<UserDto>>.Success(teamMembers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting team members");
            return ServiceResult<List<UserDto>>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<List<UserDto>>> GetManagersAsync(int? companyId)
    {
        try
        {
            var managers = await _context.Users
                .Include(u => u.Company)
                .Where(u => u.CompanyId == companyId && u.SystemRole >= SystemRole.Manager && !u.IsDeleted)
                .Select(u => new UserDto(
                    u.Id, u.Email, u.FirstName, u.LastName, u.Phone, u.SystemRole,
                    u.CompanyId, u.Company != null ? u.Company.Name : null,
                    null, null))
                .ToListAsync();

            return ServiceResult<List<UserDto>>.Success(managers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting managers");
            return ServiceResult<List<UserDto>>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<UserDto>> CreateAsync(CreateUserDto dto, int userId, int? companyId, SystemRole creatorRole)
    {
        try
        {
            var currentUser = await _context.Users.Include(u => u.Company).FirstOrDefaultAsync(u => u.Id == userId);
            if (currentUser == null)
                return ServiceResult<UserDto>.Unauthorized("User not found");

            var targetRole = dto.Role ?? SystemRole.Member;

            if (!CanCreateRole(creatorRole, targetRole))
                return ServiceResult<UserDto>.BadRequest($"You don't have permission to create {targetRole} users");

            User? assignedManager = null;
            if (dto.ManagerId.HasValue)
            {
                assignedManager = await _context.Users.FindAsync(dto.ManagerId.Value);
                if (assignedManager == null || assignedManager.SystemRole < SystemRole.Manager)
                    return ServiceResult<UserDto>.BadRequest("Invalid manager specified");
            }

            if ((targetRole == SystemRole.Member || targetRole == SystemRole.QA) && !dto.ManagerId.HasValue)
                return ServiceResult<UserDto>.BadRequest("Members and QA users must be assigned to a manager");

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return ServiceResult<UserDto>.BadRequest("Email already exists");

            var user = new User
            {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Phone = dto.Phone,
                SystemRole = targetRole,
                CompanyId = companyId,
                ManagerId = dto.ManagerId
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return ServiceResult<UserDto>.Created(new UserDto(user.Id, user.Email, user.FirstName, user.LastName, user.Phone, user.SystemRole,
                user.CompanyId, currentUser.Company?.Name,
                user.ManagerId, assignedManager != null ? assignedManager.FirstName + " " + assignedManager.LastName : null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return ServiceResult<UserDto>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<UserDto>> UpdateAsync(int id, UpdateUserDto dto, int userId, int? companyId, bool isSystemAdmin, bool isSuperAdmin, SystemRole updaterRole)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.Company)
                .Include(u => u.Manager)
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

            if (user == null)
                return ServiceResult<UserDto>.NotFound("User not found");

            if (id == userId)
            {
                if (dto.FirstName != null) user.FirstName = dto.FirstName;
                if (dto.LastName != null) user.LastName = dto.LastName;
                if (dto.Phone != null) user.Phone = dto.Phone;
            }
            else
            {
                if (!isSystemAdmin && !isSuperAdmin && user.CompanyId != companyId)
                    return ServiceResult<UserDto>.Forbidden("Access denied");

                if (dto.Role.HasValue && dto.Role.Value != user.SystemRole)
                {
                    if (!CanCreateRole(updaterRole, dto.Role.Value))
                        return ServiceResult<UserDto>.BadRequest($"You don't have permission to assign {dto.Role.Value} role");
                    user.SystemRole = dto.Role.Value;
                }

                if (dto.ManagerId.HasValue && dto.ManagerId.Value != user.ManagerId)
                {
                    var newManager = await _context.Users.FindAsync(dto.ManagerId.Value);
                    if (newManager == null || newManager.SystemRole < SystemRole.Manager)
                        return ServiceResult<UserDto>.BadRequest("Invalid manager specified");
                    user.ManagerId = dto.ManagerId.Value;
                    user.Manager = newManager;
                }

                if (dto.FirstName != null) user.FirstName = dto.FirstName;
                if (dto.LastName != null) user.LastName = dto.LastName;
                if (dto.Phone != null) user.Phone = dto.Phone;
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ServiceResult<UserDto>.Success(new UserDto(
                user.Id, user.Email, user.FirstName, user.LastName, user.Phone, user.SystemRole,
                user.CompanyId, user.Company?.Name,
                user.ManagerId, user.Manager != null ? user.Manager.FirstName + " " + user.Manager.LastName : null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return ServiceResult<UserDto>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id, int userId, SystemRole deleterRole)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
            if (user == null)
                return ServiceResult<bool>.NotFound("User not found");

            if (user.SystemRole == SystemRole.SuperAdmin)
                return ServiceResult<bool>.BadRequest("Super Admin cannot be deleted");

            if (!CanDeleteUser(deleterRole, user.SystemRole))
                return ServiceResult<bool>.BadRequest("You don't have permission to delete this user");

            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            user.DeletedBy = userId;

            await _context.SaveChangesAsync();
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return ServiceResult<bool>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<UserDto>> TransferSuperAdminAsync(int newSuperAdminId, int userId)
    {
        try
        {
            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null || currentUser.SystemRole != SystemRole.SuperAdmin)
                return ServiceResult<UserDto>.Forbidden("Access denied");

            var newSuperAdmin = await _context.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Id == newSuperAdminId && !u.IsDeleted);

            if (newSuperAdmin == null)
                return ServiceResult<UserDto>.NotFound("User not found");

            if (newSuperAdmin.SystemRole != SystemRole.Admin)
                return ServiceResult<UserDto>.BadRequest("Super Admin can only be transferred to an Admin");

            if (newSuperAdmin.CompanyId != currentUser.CompanyId)
                return ServiceResult<UserDto>.BadRequest("User must be in the same company");

            newSuperAdmin.SystemRole = SystemRole.SuperAdmin;
            currentUser.SystemRole = SystemRole.Admin;

            await _context.SaveChangesAsync();

            return ServiceResult<UserDto>.Success(new UserDto(
                newSuperAdmin.Id, newSuperAdmin.Email, newSuperAdmin.FirstName, newSuperAdmin.LastName,
                newSuperAdmin.Phone, newSuperAdmin.SystemRole, newSuperAdmin.CompanyId, newSuperAdmin.Company?.Name,
                newSuperAdmin.ManagerId, null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transferring super admin");
            return ServiceResult<UserDto>.Failure(ex.Message, ex);
        }
    }

    private bool CanCreateRole(SystemRole creatorRole, SystemRole targetRole)
    {
        return creatorRole switch
        {
            SystemRole.SuperAdmin => targetRole != SystemRole.SuperAdmin,
            SystemRole.Admin => targetRole <= SystemRole.Manager,
            SystemRole.Manager => targetRole <= SystemRole.QA,
            _ => false
        };
    }

    private bool CanDeleteUser(SystemRole deleterRole, SystemRole targetRole)
    {
        if (targetRole == SystemRole.SuperAdmin) return false;

        return deleterRole switch
        {
            SystemRole.SuperAdmin => true,
            SystemRole.Admin => targetRole < SystemRole.Admin,
            SystemRole.Manager => targetRole < SystemRole.Manager,
            _ => false
        };
    }
}
