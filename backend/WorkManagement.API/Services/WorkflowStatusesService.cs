using Microsoft.EntityFrameworkCore;
using WorkManagement.API.Data;
using WorkManagement.API.DTOs;
using WorkManagement.API.Models;

namespace WorkManagement.API.Services;

public interface IWorkflowStatusesService
{
    Task<ServiceResult<IEnumerable<WorkflowStatusDto>>> GetStatusesAsync(int projectId, int userId);
    Task<ServiceResult<WorkflowStatusDto>> GetStatusAsync(int projectId, int id, int userId);
    Task<ServiceResult<WorkflowStatusDto>> CreateStatusAsync(int projectId, CreateWorkflowStatusDto dto, int userId);
    Task<ServiceResult<WorkflowStatusDto>> UpdateStatusAsync(int projectId, int id, UpdateWorkflowStatusDto dto, int userId);
    Task<ServiceResult<bool>> DeleteStatusAsync(int projectId, int id, int userId);
    Task<ServiceResult<IEnumerable<WorkflowStatusDto>>> ReorderStatusesAsync(int projectId, List<int> statusIds, int userId);
}

public class WorkflowStatusesService : IWorkflowStatusesService
{
    private readonly ApplicationDbContext _context;
    private readonly IActivityLogService _activityLog;

    public WorkflowStatusesService(ApplicationDbContext context, IActivityLogService activityLog)
    {
        _context = context;
        _activityLog = activityLog;
    }

    public async Task<ServiceResult<IEnumerable<WorkflowStatusDto>>> GetStatusesAsync(int projectId, int userId)
    {
        try
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return ServiceResult<IEnumerable<WorkflowStatusDto>>.NotFound("Project not found");

            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null) return ServiceResult<IEnumerable<WorkflowStatusDto>>.Unauthorized("User not found");

            if (currentUser.SystemRole != SystemRole.SuperAdmin && project.CompanyId != currentUser.CompanyId)
            {
                return ServiceResult<IEnumerable<WorkflowStatusDto>>.Forbidden("Access denied");
            }

            var statuses = await _context.WorkflowStatuses
                .Where(s => s.ProjectId == projectId)
                .OrderBy(s => s.Order)
                .Select(s => new WorkflowStatusDto(s.Id, s.Name, s.Description, s.Order, s.Color, s.IsCore, s.CoreType, s.ProjectId))
                .ToListAsync();

            return ServiceResult<IEnumerable<WorkflowStatusDto>>.Success(statuses);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<WorkflowStatusDto>>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<WorkflowStatusDto>> GetStatusAsync(int projectId, int id, int userId)
    {
        try
        {
            var status = await _context.WorkflowStatuses
                .Include(s => s.Project)
                .FirstOrDefaultAsync(s => s.Id == id && s.ProjectId == projectId);

            if (status == null) return ServiceResult<WorkflowStatusDto>.NotFound("Status not found");

            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null) return ServiceResult<WorkflowStatusDto>.Unauthorized("User not found");

            if (currentUser.SystemRole != SystemRole.SuperAdmin && status.Project.CompanyId != currentUser.CompanyId)
            {
                return ServiceResult<WorkflowStatusDto>.Forbidden("Access denied");
            }

            return ServiceResult<WorkflowStatusDto>.Success(new WorkflowStatusDto(status.Id, status.Name, status.Description, status.Order, status.Color, status.IsCore, status.CoreType, status.ProjectId));
        }
        catch (Exception ex)
        {
            return ServiceResult<WorkflowStatusDto>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<WorkflowStatusDto>> CreateStatusAsync(int projectId, CreateWorkflowStatusDto dto, int userId)
    {
        try
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return ServiceResult<WorkflowStatusDto>.NotFound("Project not found");

            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null) return ServiceResult<WorkflowStatusDto>.Unauthorized("User not found");

            if (currentUser.SystemRole != SystemRole.SuperAdmin && project.CompanyId != currentUser.CompanyId)
            {
                return ServiceResult<WorkflowStatusDto>.Forbidden("Access denied");
            }

            // Allow System Admin and Super Admin to bypass membership check
            if (currentUser.SystemRole != SystemRole.SuperAdmin && currentUser.SystemRole != SystemRole.Admin)
            {
                var membership = await _context.ProjectMembers
                    .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
                
                if (membership == null || (membership.Role != ProjectRole.Manager && membership.Role != ProjectRole.Admin))
                {
                    return ServiceResult<WorkflowStatusDto>.Forbidden("Access denied");
                }
            }

            if (await _context.WorkflowStatuses.AnyAsync(s => s.ProjectId == projectId && s.Name == dto.Name))
            {
                return ServiceResult<WorkflowStatusDto>.BadRequest("Status name already exists in this project");
            }

            var status = new WorkflowStatus
            {
                Name = dto.Name,
                Description = dto.Description,
                Order = dto.Order,
                Color = dto.Color ?? "#6B7280",
                IsCore = false,
                ProjectId = projectId
            };

            _context.WorkflowStatuses.Add(status);
            await _context.SaveChangesAsync();

            await _activityLog.LogAsync(userId, "Created", "WorkflowStatus", status.Id,
                description: $"Workflow status '{status.Name}' created");

            return ServiceResult<WorkflowStatusDto>.Created(new WorkflowStatusDto(status.Id, status.Name, status.Description, status.Order, status.Color, status.IsCore, status.CoreType, status.ProjectId));
        }
        catch (Exception ex)
        {
            return ServiceResult<WorkflowStatusDto>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<WorkflowStatusDto>> UpdateStatusAsync(int projectId, int id, UpdateWorkflowStatusDto dto, int userId)
    {
        try
        {
            var status = await _context.WorkflowStatuses
                .Include(s => s.Project)
                .FirstOrDefaultAsync(s => s.Id == id && s.ProjectId == projectId);

            if (status == null) return ServiceResult<WorkflowStatusDto>.NotFound("Status not found");

            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null) return ServiceResult<WorkflowStatusDto>.Unauthorized("User not found");

            if (currentUser.SystemRole != SystemRole.SuperAdmin && status.Project.CompanyId != currentUser.CompanyId)
            {
                return ServiceResult<WorkflowStatusDto>.Forbidden("Access denied");
            }

            // Allow System Admin and Super Admin to bypass membership check
            if (currentUser.SystemRole != SystemRole.SuperAdmin && currentUser.SystemRole != SystemRole.Admin)
            {
                var membership = await _context.ProjectMembers
                    .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
                
                if (membership == null || (membership.Role != ProjectRole.Manager && membership.Role != ProjectRole.Admin))
                {
                    return ServiceResult<WorkflowStatusDto>.Forbidden("Access denied");
                }
            }

            if (status.IsCore && dto.Name != null && dto.Name != status.Name)
            {
                return ServiceResult<WorkflowStatusDto>.BadRequest("Cannot rename core statuses");
            }

            if (dto.Name != null) status.Name = dto.Name;
            if (dto.Description != null) status.Description = dto.Description;
            if (dto.Order.HasValue) status.Order = dto.Order.Value;
            if (dto.Color != null) status.Color = dto.Color;
            status.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ServiceResult<WorkflowStatusDto>.Success(new WorkflowStatusDto(status.Id, status.Name, status.Description, status.Order, status.Color, status.IsCore, status.CoreType, status.ProjectId));
        }
        catch (Exception ex)
        {
            return ServiceResult<WorkflowStatusDto>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<bool>> DeleteStatusAsync(int projectId, int id, int userId)
    {
        try
        {
            var status = await _context.WorkflowStatuses
                .Include(s => s.Project)
                .FirstOrDefaultAsync(s => s.Id == id && s.ProjectId == projectId);

            if (status == null) return ServiceResult<bool>.NotFound("Status not found");

            if (status.IsCore)
            {
                return ServiceResult<bool>.BadRequest("Cannot delete core statuses");
            }

            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null) return ServiceResult<bool>.Unauthorized("User not found");

            if (currentUser.SystemRole != SystemRole.SuperAdmin && status.Project.CompanyId != currentUser.CompanyId)
            {
                return ServiceResult<bool>.Forbidden("Access denied");
            }

            // Allow System Admin and Super Admin to bypass membership check
            if (currentUser.SystemRole != SystemRole.SuperAdmin && currentUser.SystemRole != SystemRole.Admin)
            {
                var membership = await _context.ProjectMembers
                    .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
                
                if (membership == null || (membership.Role != ProjectRole.Manager && membership.Role != ProjectRole.Admin))
                {
                    return ServiceResult<bool>.Forbidden("Access denied");
                }
            }

            var hasWorkItems = await _context.WorkItems.AnyAsync(w => w.StatusId == id);
            if (hasWorkItems)
            {
                return ServiceResult<bool>.BadRequest("Cannot delete status with existing work items. Move items first.");
            }

            status.IsDeleted = true;
            status.DeletedAt = DateTime.UtcNow;
            status.DeletedBy = userId;

            await _context.SaveChangesAsync();

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<IEnumerable<WorkflowStatusDto>>> ReorderStatusesAsync(int projectId, List<int> statusIds, int userId)
    {
        try
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return ServiceResult<IEnumerable<WorkflowStatusDto>>.NotFound("Project not found");

            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null) return ServiceResult<IEnumerable<WorkflowStatusDto>>.Unauthorized("User not found");

            if (currentUser.SystemRole != SystemRole.SuperAdmin && project.CompanyId != currentUser.CompanyId)
            {
                return ServiceResult<IEnumerable<WorkflowStatusDto>>.Forbidden("Access denied");
            }

            // Allow System Admin and Super Admin to bypass membership check
            if (currentUser.SystemRole != SystemRole.SuperAdmin && currentUser.SystemRole != SystemRole.Admin)
            {
                var membership = await _context.ProjectMembers
                    .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
                
                if (membership == null || (membership.Role != ProjectRole.Manager && membership.Role != ProjectRole.Admin))
                {
                    return ServiceResult<IEnumerable<WorkflowStatusDto>>.Forbidden("Access denied");
                }
            }

            var statuses = await _context.WorkflowStatuses
                .Where(s => s.ProjectId == projectId && statusIds.Contains(s.Id))
                .ToListAsync();

            for (int i = 0; i < statusIds.Count; i++)
            {
                var status = statuses.FirstOrDefault(s => s.Id == statusIds[i]);
                if (status != null)
                {
                    status.Order = i + 1;
                }
            }

            await _context.SaveChangesAsync();

            var result = await _context.WorkflowStatuses
                .Where(s => s.ProjectId == projectId)
                .OrderBy(s => s.Order)
                .Select(s => new WorkflowStatusDto(s.Id, s.Name, s.Description, s.Order, s.Color, s.IsCore, s.CoreType, s.ProjectId))
                .ToListAsync();

            return ServiceResult<IEnumerable<WorkflowStatusDto>>.Success(result);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<WorkflowStatusDto>>.Failure(ex.Message, ex);
        }
    }
}
