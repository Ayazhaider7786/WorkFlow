using Microsoft.EntityFrameworkCore;
using WorkManagement.API.Data;
using WorkManagement.API.DTOs;
using WorkManagement.API.Models;

namespace WorkManagement.API.Services;

public interface IProjectService
{
    Task<ServiceResult<List<ProjectDto>>> GetAllAsync(int userId, int? companyId, bool isSuperAdmin, bool isAdmin);
    Task<ServiceResult<ProjectDto>> GetByIdAsync(int id, int userId, int? companyId, bool isSuperAdmin, bool isAdmin);
    Task<ServiceResult<ProjectDto>> CreateAsync(CreateProjectDto dto, int userId, int companyId);
    Task<ServiceResult<ProjectDto>> UpdateAsync(int id, UpdateProjectDto dto, int userId, int? companyId, bool isSuperAdmin, bool isAdmin);
    Task<ServiceResult<bool>> DeleteAsync(int id, int userId, int? companyId, bool isAdmin);
    Task<ServiceResult<List<ProjectMemberDto>>> GetMembersAsync(int projectId, int userId, int? companyId, bool isSuperAdmin, bool isAdmin);
    Task<ServiceResult<List<UserSelectionDto>>> GetAvailableUsersAsync(int projectId, int userId, int? companyId, string? search, bool? unassignedOnly);
    Task<ServiceResult<ProjectMemberDto>> AddMemberAsync(int projectId, AddProjectMemberDto dto, int userId, int? companyId);
    Task<ServiceResult<int>> BulkAddMembersAsync(int projectId, BulkAddProjectMembersDto dto, int userId, int? companyId);
    Task<ServiceResult<ProjectMemberDto>> UpdateMemberAsync(int projectId, int memberId, UpdateProjectMemberDto dto, int userId);
    Task<ServiceResult<bool>> RemoveMemberAsync(int projectId, int memberId, int userId);
    Task<bool> CanAccessProjectAsync(int projectId, int userId, int? companyId, bool isSuperAdmin, bool isAdmin);
}

public class ProjectService : IProjectService
{
    private readonly ApplicationDbContext _context;
    private readonly IActivityLogService _activityLog;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(ApplicationDbContext context, IActivityLogService activityLog, ILogger<ProjectService> logger)
    {
        _context = context;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<ServiceResult<List<ProjectDto>>> GetAllAsync(int userId, int? companyId, bool isSuperAdmin, bool isAdmin)
    {
        try
        {
            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null)
                return ServiceResult<List<ProjectDto>>.Unauthorized("User not found");

            IQueryable<Project> query;

            if (isSuperAdmin || isAdmin)
            {
                query = _context.Projects.Where(p => p.CompanyId == companyId && !p.IsDeleted);
            }
            else if (currentUser.SystemRole == SystemRole.Manager)
            {
                var managerProjectIds = await _context.ProjectMembers
                    .Where(pm => pm.UserId == userId && !pm.IsDeleted)
                    .Select(pm => pm.ProjectId)
                    .ToListAsync();
                query = _context.Projects.Where(p => managerProjectIds.Contains(p.Id) && !p.IsDeleted);
            }
            else
            {
                if (!currentUser.ManagerId.HasValue)
                {
                    return ServiceResult<List<ProjectDto>>.Success(new List<ProjectDto>());
                }

                var managerProjectIds = await _context.ProjectMembers
                    .Where(pm => pm.UserId == currentUser.ManagerId.Value && !pm.IsDeleted)
                    .Select(pm => pm.ProjectId)
                    .ToListAsync();

                var directProjectIds = await _context.ProjectMembers
                    .Where(pm => pm.UserId == userId && !pm.IsDeleted)
                    .Select(pm => pm.ProjectId)
                    .ToListAsync();

                var allProjectIds = managerProjectIds.Union(directProjectIds).Distinct().ToList();
                query = _context.Projects.Where(p => allProjectIds.Contains(p.Id) && !p.IsDeleted);
            }

            var projects = await query
                .Include(p => p.Members.Where(m => !m.IsDeleted))
                .ThenInclude(m => m.User)
                .Select(p => new ProjectDto(
                    p.Id, p.Name, p.Description, p.Key, p.StartDate, p.EndDate, p.IsActive, p.CompanyId, p.CreatedAt,
                    p.Members.Where(m => !m.IsDeleted && m.User.SystemRole >= SystemRole.Manager)
                        .Select(m => new ProjectManagerDto(m.UserId, m.User.FirstName + " " + m.User.LastName))))
                .ToListAsync();

            return ServiceResult<List<ProjectDto>>.Success(projects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting projects for user {UserId}", userId);
            return ServiceResult<List<ProjectDto>>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<ProjectDto>> GetByIdAsync(int id, int userId, int? companyId, bool isSuperAdmin, bool isAdmin)
    {
        try
        {
            var project = await _context.Projects
                .Include(p => p.Members.Where(m => !m.IsDeleted))
                .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (project == null)
                return ServiceResult<ProjectDto>.NotFound("Project not found");

            if (!await CanAccessProjectAsync(id, userId, companyId, isSuperAdmin, isAdmin))
                return ServiceResult<ProjectDto>.Forbidden("Access denied");

            var dto = new ProjectDto(
                project.Id, project.Name, project.Description, project.Key, project.StartDate, project.EndDate,
                project.IsActive, project.CompanyId, project.CreatedAt,
                project.Members.Where(m => !m.IsDeleted && m.User.SystemRole >= SystemRole.Manager)
                    .Select(m => new ProjectManagerDto(m.UserId, m.User.FirstName + " " + m.User.LastName)));

            return ServiceResult<ProjectDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project {ProjectId}", id);
            return ServiceResult<ProjectDto>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<ProjectDto>> CreateAsync(CreateProjectDto dto, int userId, int companyId)
    {
        try
        {
            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null)
                return ServiceResult<ProjectDto>.Unauthorized("User not found");

            if (currentUser.SystemRole < SystemRole.Admin)
                return ServiceResult<ProjectDto>.Forbidden("Only Admin or SuperAdmin can create projects");

            var manager = await _context.Users.FindAsync(dto.ManagerId);
            if (manager == null || manager.SystemRole < SystemRole.Manager)
                return ServiceResult<ProjectDto>.BadRequest("A Manager or Admin must be assigned to the project");

            if (manager.CompanyId != companyId)
                return ServiceResult<ProjectDto>.BadRequest("Manager must be from the same company");

            if (await _context.Projects.AnyAsync(p => p.CompanyId == companyId && p.Key == dto.Key.ToUpper() && !p.IsDeleted))
                return ServiceResult<ProjectDto>.BadRequest("Project key already exists in this company");

            var project = new Project
            {
                Name = dto.Name,
                Description = dto.Description,
                Key = dto.Key.ToUpper(),
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                CompanyId = companyId,
                IsActive = true,
                IsDeleted = false
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            var projectMember = new ProjectMember
            {
                ProjectId = project.Id,
                UserId = dto.ManagerId,
                Role = ProjectRole.Manager
            };
            _context.ProjectMembers.Add(projectMember);

            var defaultStatuses = new[]
            {
                new WorkflowStatus { Name = "To Do", Order = 1, Color = "#6B7280", IsCore = true, CoreType = CoreStatusType.New, ProjectId = project.Id },
                new WorkflowStatus { Name = "In Progress", Order = 2, Color = "#3B82F6", IsCore = true, CoreType = CoreStatusType.InProgress, ProjectId = project.Id },
                new WorkflowStatus { Name = "Review", Order = 3, Color = "#8B5CF6", IsCore = true, CoreType = CoreStatusType.Review, ProjectId = project.Id },
                new WorkflowStatus { Name = "Done", Order = 4, Color = "#10B981", IsCore = true, CoreType = CoreStatusType.Done, ProjectId = project.Id }
            };
            _context.WorkflowStatuses.AddRange(defaultStatuses);
            await _context.SaveChangesAsync();

            // Create Default Board
            var defaultBoard = new Board
            {
                Name = "Main Board",
                ProjectId = project.Id,
                IsDefault = true,
                OwnerId = null
            };
            _context.Boards.Add(defaultBoard);
            await _context.SaveChangesAsync();

            // Add Columns to Default Board
            var boardColumns = new List<BoardColumn>();
            foreach (var status in defaultStatuses)
            {
               boardColumns.Add(new BoardColumn
               {
                   BoardId = defaultBoard.Id,
                   StatusId = status.Id,
                   Order = status.Order
               });
            }
            _context.BoardColumns.AddRange(boardColumns);
            await _context.SaveChangesAsync();

            await _activityLog.LogAsync(userId, "Created", "Project", project.Id,
                description: $"Project '{project.Name}' created with manager {manager.FirstName} {manager.LastName}",
                projectId: project.Id);

            var resultDto = new ProjectDto(project.Id, project.Name, project.Description, project.Key, project.StartDate,
                project.EndDate, project.IsActive, project.CompanyId, project.CreatedAt,
                new[] { new ProjectManagerDto(manager.Id, manager.FirstName + " " + manager.LastName) });

            return ServiceResult<ProjectDto>.Created(resultDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project");
            return ServiceResult<ProjectDto>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<ProjectDto>> UpdateAsync(int id, UpdateProjectDto dto, int userId, int? companyId, bool isSuperAdmin, bool isAdmin)
    {
        try
        {
            var project = await _context.Projects
                .Include(p => p.Members.Where(m => !m.IsDeleted))
                .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (project == null)
                return ServiceResult<ProjectDto>.NotFound("Project not found");

            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null)
                return ServiceResult<ProjectDto>.Unauthorized("User not found");

            if (currentUser.SystemRole < SystemRole.Manager)
                return ServiceResult<ProjectDto>.Forbidden("Access denied");

            if (currentUser.SystemRole == SystemRole.Manager)
            {
                var isMember = await _context.ProjectMembers
                    .AnyAsync(pm => pm.ProjectId == id && pm.UserId == userId && !pm.IsDeleted);
                if (!isMember)
                    return ServiceResult<ProjectDto>.Forbidden("Access denied");
            }

            if (dto.Name != null) project.Name = dto.Name;
            if (dto.Description != null) project.Description = dto.Description;
            if (dto.StartDate.HasValue) project.StartDate = dto.StartDate;
            if (dto.EndDate.HasValue) project.EndDate = dto.EndDate;
            if (dto.IsActive.HasValue) project.IsActive = dto.IsActive.Value;
            project.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _activityLog.LogAsync(userId, "Updated", "Project", project.Id,
                description: $"Project '{project.Name}' updated", projectId: project.Id);

            var resultDto = new ProjectDto(
                project.Id, project.Name, project.Description, project.Key, project.StartDate, project.EndDate,
                project.IsActive, project.CompanyId, project.CreatedAt,
                project.Members.Where(m => !m.IsDeleted && m.User.SystemRole >= SystemRole.Manager)
                    .Select(m => new ProjectManagerDto(m.UserId, m.User.FirstName + " " + m.User.LastName)));

            return ServiceResult<ProjectDto>.Success(resultDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project {ProjectId}", id);
            return ServiceResult<ProjectDto>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id, int userId, int? companyId, bool isAdmin)
    {
        try
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            if (project == null)
                return ServiceResult<bool>.NotFound("Project not found");

            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null || currentUser.SystemRole < SystemRole.Admin)
                return ServiceResult<bool>.Forbidden("Access denied");

            project.IsDeleted = true;
            project.DeletedAt = DateTime.UtcNow;
            project.DeletedByUserId = userId;
            project.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _activityLog.LogAsync(userId, "Deleted", "Project", project.Id,
                description: $"Project '{project.Name}' deleted (soft delete)", projectId: project.Id);

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting project {ProjectId}", id);
            return ServiceResult<bool>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<List<ProjectMemberDto>>> GetMembersAsync(int projectId, int userId, int? companyId, bool isSuperAdmin, bool isAdmin)
    {
        try
        {
            if (!await CanAccessProjectAsync(projectId, userId, companyId, isSuperAdmin, isAdmin))
                return ServiceResult<List<ProjectMemberDto>>.Forbidden("Access denied");

            var members = await _context.ProjectMembers
                .Include(pm => pm.User)
                .Where(pm => pm.ProjectId == projectId && !pm.IsDeleted)
                .Select(pm => new ProjectMemberDto(pm.Id, pm.ProjectId, pm.UserId,
                    pm.User.FirstName + " " + pm.User.LastName, pm.User.Email, pm.Role))
                .ToListAsync();

            return ServiceResult<List<ProjectMemberDto>>.Success(members);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project members for project {ProjectId}", projectId);
            return ServiceResult<List<ProjectMemberDto>>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<List<UserSelectionDto>>> GetAvailableUsersAsync(int projectId, int userId, int? companyId, string? search, bool? unassignedOnly)
    {
        try
        {
            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null)
                return ServiceResult<List<UserSelectionDto>>.Unauthorized("User not found");

            if (currentUser.SystemRole < SystemRole.Manager)
                return ServiceResult<List<UserSelectionDto>>.Forbidden("Access denied");

            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId && !p.IsDeleted);
            if (project == null)
                return ServiceResult<List<UserSelectionDto>>.NotFound("Project not found");

            var usersQuery = _context.Users
                .Where(u => u.CompanyId == companyId && !u.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                usersQuery = usersQuery.Where(u => u.Email.Contains(search) ||
                    u.FirstName.Contains(search) || u.LastName.Contains(search));
            }

            var users = await usersQuery.ToListAsync();

            var projectMembers = await _context.ProjectMembers
                .Where(pm => pm.ProjectId == projectId && !pm.IsDeleted)
                .ToDictionaryAsync(pm => pm.UserId, pm => pm.Role);

            var result = users.Select(u => new UserSelectionDto(
                u.Id,
                u.Email,
                u.FirstName,
                u.LastName,
                u.SystemRole,
                projectMembers.ContainsKey(u.Id),
                projectMembers.ContainsKey(u.Id) ? projectMembers[u.Id] : null
            )).ToList();

            if (unassignedOnly == true)
            {
                result = result.Where(u => !u.IsAssignedToProject).ToList();
            }

            return ServiceResult<List<UserSelectionDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available users for project {ProjectId}", projectId);
            return ServiceResult<List<UserSelectionDto>>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<ProjectMemberDto>> AddMemberAsync(int projectId, AddProjectMemberDto dto, int userId, int? companyId)
    {
        try
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId && !p.IsDeleted);
            if (project == null)
                return ServiceResult<ProjectMemberDto>.NotFound("Project not found");

            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null)
                return ServiceResult<ProjectMemberDto>.Unauthorized("User not found");

            if (currentUser.SystemRole < SystemRole.Manager)
                return ServiceResult<ProjectMemberDto>.Forbidden("Access denied");

            if (currentUser.SystemRole == SystemRole.Manager)
            {
                var isMember = await _context.ProjectMembers
                    .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId && !pm.IsDeleted);
                if (!isMember)
                    return ServiceResult<ProjectMemberDto>.Forbidden("Access denied");
            }

            var userToAdd = await _context.Users.FindAsync(dto.UserId);
            if (userToAdd == null || userToAdd.IsDeleted)
                return ServiceResult<ProjectMemberDto>.BadRequest("User not found");

            if (userToAdd.CompanyId != companyId)
                return ServiceResult<ProjectMemberDto>.BadRequest("User must be from the same company");

            var existingMember = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == dto.UserId);

            if (existingMember != null)
            {
                if (!existingMember.IsDeleted)
                    return ServiceResult<ProjectMemberDto>.BadRequest("User is already a member of this project");

                existingMember.IsDeleted = false;
                existingMember.DeletedAt = null;
                existingMember.DeletedByUserId = null;
                existingMember.Role = dto.Role;
                existingMember.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                await _activityLog.LogAsync(userId, "Added", "ProjectMember", existingMember.Id,
                    description: $"Added {userToAdd.FirstName} {userToAdd.LastName} to project", projectId: projectId);

                return ServiceResult<ProjectMemberDto>.Success(new ProjectMemberDto(existingMember.Id, existingMember.ProjectId, existingMember.UserId,
                    userToAdd.FirstName + " " + userToAdd.LastName, userToAdd.Email, existingMember.Role));
            }

            var member = new ProjectMember
            {
                ProjectId = projectId,
                UserId = dto.UserId,
                Role = dto.Role
            };

            _context.ProjectMembers.Add(member);
            await _context.SaveChangesAsync();

            await _activityLog.LogAsync(userId, "Added", "ProjectMember", member.Id,
                description: $"Added {userToAdd.FirstName} {userToAdd.LastName} to project", projectId: projectId);

            return ServiceResult<ProjectMemberDto>.Created(new ProjectMemberDto(member.Id, member.ProjectId, member.UserId,
                userToAdd.FirstName + " " + userToAdd.LastName, userToAdd.Email, member.Role));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding member to project {ProjectId}", projectId);
            return ServiceResult<ProjectMemberDto>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<int>> BulkAddMembersAsync(int projectId, BulkAddProjectMembersDto dto, int userId, int? companyId)
    {
        try
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId && !p.IsDeleted);
            if (project == null)
                return ServiceResult<int>.NotFound("Project not found");

            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null || currentUser.SystemRole < SystemRole.Manager)
                return ServiceResult<int>.Forbidden("Access denied");

            var existingMemberIds = await _context.ProjectMembers
                .Where(pm => pm.ProjectId == projectId && !pm.IsDeleted)
                .Select(pm => pm.UserId)
                .ToListAsync();

            var addedCount = 0;
            foreach (var userIdToAdd in dto.UserIds)
            {
                if (existingMemberIds.Contains(userIdToAdd)) continue;

                var user = await _context.Users.FindAsync(userIdToAdd);
                if (user == null || user.IsDeleted || user.CompanyId != companyId) continue;

                var member = new ProjectMember
                {
                    ProjectId = projectId,
                    UserId = userIdToAdd,
                    Role = dto.Role
                };
                _context.ProjectMembers.Add(member);
                addedCount++;
            }

            await _context.SaveChangesAsync();

            await _activityLog.LogAsync(userId, "BulkAdded", "ProjectMember", projectId,
                description: $"Added {addedCount} members to project", projectId: projectId);

            return ServiceResult<int>.Success(addedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk adding members to project {ProjectId}", projectId);
            return ServiceResult<int>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<ProjectMemberDto>> UpdateMemberAsync(int projectId, int memberId, UpdateProjectMemberDto dto, int userId)
    {
        try
        {
            var member = await _context.ProjectMembers
                .Include(pm => pm.User)
                .FirstOrDefaultAsync(pm => pm.Id == memberId && pm.ProjectId == projectId && !pm.IsDeleted);

            if (member == null)
                return ServiceResult<ProjectMemberDto>.NotFound("Project member not found");

            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null || currentUser.SystemRole < SystemRole.Manager)
                return ServiceResult<ProjectMemberDto>.Forbidden("Access denied");

            member.Role = dto.Role;
            member.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _activityLog.LogAsync(userId, "Updated", "ProjectMember", member.Id,
                description: $"Updated role for {member.User.FirstName} {member.User.LastName}", projectId: projectId);

            return ServiceResult<ProjectMemberDto>.Success(new ProjectMemberDto(member.Id, member.ProjectId, member.UserId,
                member.User.FirstName + " " + member.User.LastName, member.User.Email, member.Role));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project member {MemberId}", memberId);
            return ServiceResult<ProjectMemberDto>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<bool>> RemoveMemberAsync(int projectId, int memberId, int userId)
    {
        try
        {
            var member = await _context.ProjectMembers
                .Include(pm => pm.User)
                .FirstOrDefaultAsync(pm => pm.Id == memberId && pm.ProjectId == projectId && !pm.IsDeleted);

            if (member == null)
                return ServiceResult<bool>.NotFound("Project member not found");

            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null || currentUser.SystemRole < SystemRole.Manager)
                return ServiceResult<bool>.Forbidden("Access denied");

            if (member.Role == ProjectRole.Manager || member.User.SystemRole >= SystemRole.Manager)
            {
                var managerCount = await _context.ProjectMembers
                    .Include(pm => pm.User)
                    .CountAsync(pm => pm.ProjectId == projectId && !pm.IsDeleted &&
                        (pm.Role == ProjectRole.Manager || pm.User.SystemRole >= SystemRole.Manager));

                if (managerCount <= 1)
                    return ServiceResult<bool>.BadRequest("Cannot remove the last manager from a project");
            }

            member.IsDeleted = true;
            member.DeletedAt = DateTime.UtcNow;
            member.DeletedByUserId = userId;

            await _context.SaveChangesAsync();

            await _activityLog.LogAsync(userId, "Removed", "ProjectMember", member.Id,
                description: $"Removed {member.User.FirstName} {member.User.LastName} from project", projectId: projectId);

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing project member {MemberId}", memberId);
            return ServiceResult<bool>.Failure(ex.Message, ex);
        }
    }

    public async Task<bool> CanAccessProjectAsync(int projectId, int userId, int? companyId, bool isSuperAdmin, bool isAdmin)
    {
        try
        {
            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null) return false;

            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId && !p.IsDeleted);
            if (project == null) return false;

            if (isSuperAdmin || isAdmin)
            {
                return project.CompanyId == companyId;
            }

            var isMember = await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId && !pm.IsDeleted);
            if (isMember) return true;

            if (currentUser.ManagerId.HasValue)
            {
                return await _context.ProjectMembers
                    .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == currentUser.ManagerId.Value && !pm.IsDeleted);
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}
