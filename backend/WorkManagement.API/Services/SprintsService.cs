using Microsoft.EntityFrameworkCore;
using WorkManagement.API.Data;
using WorkManagement.API.DTOs;
using WorkManagement.API.Models;

namespace WorkManagement.API.Services;

public interface ISprintsService
{
    Task<ServiceResult<IEnumerable<SprintDto>>> GetSprintsAsync(int projectId, int userId);
    Task<ServiceResult<SprintDto>> GetSprintAsync(int projectId, int id, int userId);
    Task<ServiceResult<SprintDto>> CreateSprintAsync(int projectId, CreateSprintDto dto, int userId);
    Task<ServiceResult<SprintDto>> UpdateSprintAsync(int projectId, int id, UpdateSprintDto dto, int userId);
    Task<ServiceResult<bool>> DeleteSprintAsync(int projectId, int id, int userId);
    Task<ServiceResult<SprintDto>> StartSprintAsync(int projectId, int id, int userId);
    Task<ServiceResult<SprintDto>> CompleteSprintAsync(int projectId, int id, int userId);
}

public class SprintsService : ISprintsService
{
    private readonly ApplicationDbContext _context;
    private readonly IActivityLogService _activityLog;

    public SprintsService(ApplicationDbContext context, IActivityLogService activityLog)
    {
        _context = context;
        _activityLog = activityLog;
    }

    public async Task<ServiceResult<IEnumerable<SprintDto>>> GetSprintsAsync(int projectId, int userId)
    {
        try
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return ServiceResult<IEnumerable<SprintDto>>.NotFound("Project not found");

            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null) return ServiceResult<IEnumerable<SprintDto>>.Unauthorized("User not found");

            if (currentUser.SystemRole != SystemRole.SuperAdmin && project.CompanyId != currentUser.CompanyId)
            {
                return ServiceResult<IEnumerable<SprintDto>>.Forbidden("Access denied");
            }

            var sprints = await _context.Sprints
                .Include(s => s.WorkItems)
                .Where(s => s.ProjectId == projectId)
                .OrderByDescending(s => s.StartDate)
                .ToListAsync();

            var result = sprints.Select(s => new SprintDto(
                s.Id, s.Name, s.Goal, s.StartDate, s.EndDate, s.Status, s.ProjectId,
                s.WorkItems.Count(w => !w.IsDeleted), s.CreatedAt
            )).ToList();

            return ServiceResult<IEnumerable<SprintDto>>.Success(result);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<SprintDto>>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<SprintDto>> GetSprintAsync(int projectId, int id, int userId)
    {
        try
        {
            var sprint = await _context.Sprints
                .Include(s => s.Project)
                .Include(s => s.WorkItems)
                .FirstOrDefaultAsync(s => s.Id == id && s.ProjectId == projectId);

            if (sprint == null) return ServiceResult<SprintDto>.NotFound("Sprint not found");

            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null) return ServiceResult<SprintDto>.Unauthorized("User not found");

            if (currentUser.SystemRole != SystemRole.SuperAdmin && sprint.Project.CompanyId != currentUser.CompanyId)
            {
                return ServiceResult<SprintDto>.Forbidden("Access denied");
            }

            return ServiceResult<SprintDto>.Success(new SprintDto(
                sprint.Id, sprint.Name, sprint.Goal, sprint.StartDate, sprint.EndDate, sprint.Status, sprint.ProjectId,
                sprint.WorkItems.Count(w => !w.IsDeleted), sprint.CreatedAt
            ));
        }
        catch (Exception ex)
        {
            return ServiceResult<SprintDto>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<SprintDto>> CreateSprintAsync(int projectId, CreateSprintDto dto, int userId)
    {
        try
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return ServiceResult<SprintDto>.NotFound("Project not found");

            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null) return ServiceResult<SprintDto>.Unauthorized("User not found");

            if (currentUser.SystemRole != SystemRole.SuperAdmin && project.CompanyId != currentUser.CompanyId)
            {
                return ServiceResult<SprintDto>.Forbidden("Access denied");
            }

            var membership = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
            
            if (membership == null || (membership.Role != ProjectRole.Manager && membership.Role != ProjectRole.Admin))
            {
                return ServiceResult<SprintDto>.Forbidden("Access denied");
            }

            var sprint = new Sprint
            {
                Name = dto.Name,
                Goal = dto.Goal,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                ProjectId = projectId,
                Status = SprintStatus.Planning
            };

            _context.Sprints.Add(sprint);
            await _context.SaveChangesAsync();

            await _activityLog.LogAsync(userId, "Created", "Sprint", sprint.Id,
                description: $"Sprint '{sprint.Name}' created");

            return ServiceResult<SprintDto>.Created(new SprintDto(sprint.Id, sprint.Name, sprint.Goal, sprint.StartDate, sprint.EndDate, sprint.Status, sprint.ProjectId, 0, sprint.CreatedAt));
        }
        catch (Exception ex)
        {
            return ServiceResult<SprintDto>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<SprintDto>> UpdateSprintAsync(int projectId, int id, UpdateSprintDto dto, int userId)
    {
        try
        {
            var sprint = await _context.Sprints
                .Include(s => s.Project)
                .Include(s => s.WorkItems)
                .FirstOrDefaultAsync(s => s.Id == id && s.ProjectId == projectId);

            if (sprint == null) return ServiceResult<SprintDto>.NotFound("Sprint not found");

            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null) return ServiceResult<SprintDto>.Unauthorized("User not found");

            if (currentUser.SystemRole != SystemRole.SuperAdmin && sprint.Project.CompanyId != currentUser.CompanyId)
            {
                return ServiceResult<SprintDto>.Forbidden("Access denied");
            }

            var membership = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
            
            if (membership == null || (membership.Role != ProjectRole.Manager && membership.Role != ProjectRole.Admin))
            {
                return ServiceResult<SprintDto>.Forbidden("Access denied");
            }

            if (dto.Name != null) sprint.Name = dto.Name;
            if (dto.Goal != null) sprint.Goal = dto.Goal;
            if (dto.StartDate.HasValue) sprint.StartDate = dto.StartDate.Value;
            if (dto.EndDate.HasValue) sprint.EndDate = dto.EndDate.Value;
            if (dto.Status.HasValue) sprint.Status = dto.Status.Value;
            sprint.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _activityLog.LogAsync(userId, "Updated", "Sprint", sprint.Id,
                description: $"Sprint '{sprint.Name}' updated");

            return ServiceResult<SprintDto>.Success(new SprintDto(
                sprint.Id, sprint.Name, sprint.Goal, sprint.StartDate, sprint.EndDate, sprint.Status, sprint.ProjectId,
                sprint.WorkItems.Count(w => !w.IsDeleted), sprint.CreatedAt
            ));
        }
        catch (Exception ex)
        {
            return ServiceResult<SprintDto>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<bool>> DeleteSprintAsync(int projectId, int id, int userId)
    {
        try
        {
            var sprint = await _context.Sprints
                .Include(s => s.Project)
                .FirstOrDefaultAsync(s => s.Id == id && s.ProjectId == projectId);

            if (sprint == null) return ServiceResult<bool>.NotFound("Sprint not found");

            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null) return ServiceResult<bool>.Unauthorized("User not found");

            if (currentUser.SystemRole != SystemRole.SuperAdmin && sprint.Project.CompanyId != currentUser.CompanyId)
            {
                return ServiceResult<bool>.Forbidden("Access denied");
            }

            var membership = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
            
            if (membership == null || (membership.Role != ProjectRole.Manager && membership.Role != ProjectRole.Admin))
            {
                return ServiceResult<bool>.Forbidden("Access denied");
            }

            sprint.IsDeleted = true;
            sprint.DeletedAt = DateTime.UtcNow;
            sprint.DeletedBy = userId;

            var workItems = await _context.WorkItems.Where(w => w.SprintId == id).ToListAsync();
            foreach (var item in workItems)
            {
                item.SprintId = null;
                item.IsInBacklog = true;
            }

            await _context.SaveChangesAsync();

            await _activityLog.LogAsync(userId, "Deleted", "Sprint", sprint.Id,
                description: $"Sprint '{sprint.Name}' deleted");

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<SprintDto>> StartSprintAsync(int projectId, int id, int userId)
    {
        try
        {
            var sprint = await _context.Sprints
                .Include(s => s.Project)
                .Include(s => s.WorkItems)
                .FirstOrDefaultAsync(s => s.Id == id && s.ProjectId == projectId);

            if (sprint == null) return ServiceResult<SprintDto>.NotFound("Sprint not found");
            
            // Check permissions here as well if needed? (implied by manager role check in other methods, should probably check here too for consistency but keeping it same as controller logic which didn't strictly check role for start/complete beyond general auth maybe? Controller code did not check role explicitly in Start/Complete, just general access? Wait, let me check controller again.)
            // Controller StartSprint:
            // checks system admin / company ID.
            // DOES NOT check ProjectRole like Create/Update/Delete did. This might be an oversight in original controller but strictly creating service to match controller logic.
            // Actually, I should probably add the role check if it makes sense, but adhering to "refactor" usually implies keeping consistent logic unless bug fix is intended. I will keep it consistent with controller for now but maybe add comment.
            // Actually, controller has NO role check for Start/Complete. Just company check.

            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null) return ServiceResult<SprintDto>.Unauthorized("User not found");

            if (currentUser.SystemRole != SystemRole.SuperAdmin && sprint.Project.CompanyId != currentUser.CompanyId)
            {
                return ServiceResult<SprintDto>.Forbidden("Access denied");
            }

            if (sprint.Status != SprintStatus.Planning)
            {
                return ServiceResult<SprintDto>.BadRequest("Only planning sprints can be started");
            }

            sprint.Status = SprintStatus.Active;
            sprint.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _activityLog.LogAsync(userId, "Started", "Sprint", sprint.Id,
                description: $"Sprint '{sprint.Name}' started");

            return ServiceResult<SprintDto>.Success(new SprintDto(
                sprint.Id, sprint.Name, sprint.Goal, sprint.StartDate, sprint.EndDate, sprint.Status, sprint.ProjectId,
                sprint.WorkItems.Count(w => !w.IsDeleted), sprint.CreatedAt
            ));
        }
        catch (Exception ex)
        {
            return ServiceResult<SprintDto>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<SprintDto>> CompleteSprintAsync(int projectId, int id, int userId)
    {
        try
        {
            var sprint = await _context.Sprints
                .Include(s => s.Project)
                .Include(s => s.WorkItems)
                .FirstOrDefaultAsync(s => s.Id == id && s.ProjectId == projectId);

            if (sprint == null) return ServiceResult<SprintDto>.NotFound("Sprint not found");

            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null) return ServiceResult<SprintDto>.Unauthorized("User not found");

            if (currentUser.SystemRole != SystemRole.SuperAdmin && sprint.Project.CompanyId != currentUser.CompanyId)
            {
                return ServiceResult<SprintDto>.Forbidden("Access denied");
            }

            if (sprint.Status != SprintStatus.Active)
            {
                return ServiceResult<SprintDto>.BadRequest("Only active sprints can be completed");
            }

            sprint.Status = SprintStatus.Completed;
            sprint.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _activityLog.LogAsync(userId, "Completed", "Sprint", sprint.Id,
                description: $"Sprint '{sprint.Name}' completed");

            return ServiceResult<SprintDto>.Success(new SprintDto(
                sprint.Id, sprint.Name, sprint.Goal, sprint.StartDate, sprint.EndDate, sprint.Status, sprint.ProjectId,
                sprint.WorkItems.Count(w => !w.IsDeleted), sprint.CreatedAt
            ));
        }
        catch (Exception ex)
        {
            return ServiceResult<SprintDto>.Failure(ex.Message, ex);
        }
    }
}
