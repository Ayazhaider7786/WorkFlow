using Microsoft.EntityFrameworkCore;
using WorkManagement.API.Data;
using WorkManagement.API.DTOs;
using WorkManagement.API.Models;

namespace WorkManagement.API.Services;

public interface IActivityLogQueryService
{
    Task<ServiceResult<PaginatedResponse<ActivityLogDto>>> GetAllAsync(int userId, int? companyId, DateTime? startDate, DateTime? endDate, int? filterUserId, int? projectId, int page, int pageSize);
    Task<ServiceResult<PaginatedResponse<ActivityLogDto>>> GetByProjectAsync(int projectId, int userId, int? companyId, bool isSystemAdmin, DateTime? startDate, DateTime? endDate, int? filterUserId, string? entityType, int? entityId, int page, int pageSize);
    Task<ServiceResult<List<ActivityLogDto>>> GetByWorkItemAsync(int projectId, int workItemId, int userId, int? companyId, bool isSystemAdmin);
    Task<ServiceResult<List<ActivityLogDto>>> GetByFileTicketAsync(int projectId, int fileTicketId, int userId, int? companyId, bool isSystemAdmin);
    Task<ServiceResult<PaginatedResponse<ActivityLogDto>>> GetByUserAsync(int targetUserId, int userId, int? companyId, DateTime? startDate, DateTime? endDate, int page, int pageSize);
}

public class ActivityLogQueryService : IActivityLogQueryService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ActivityLogQueryService> _logger;

    public ActivityLogQueryService(ApplicationDbContext context, ILogger<ActivityLogQueryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ServiceResult<PaginatedResponse<ActivityLogDto>>> GetAllAsync(int userId, int? companyId, DateTime? startDate, DateTime? endDate, int? filterUserId, int? projectId, int page, int pageSize)
    {
        try
        {
            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null)
                return ServiceResult<PaginatedResponse<ActivityLogDto>>.Unauthorized("User not found");

            var query = _context.ActivityLogs
                .Include(a => a.User)
                .Include(a => a.Project)
                .Where(a => a.User.CompanyId == companyId)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(a => a.Timestamp >= startDate.Value);

            if (endDate.HasValue)
            {
                var endOfDay = endDate.Value.Date.AddDays(1);
                query = query.Where(a => a.Timestamp < endOfDay);
            }

            if (filterUserId.HasValue)
                query = query.Where(a => a.UserId == filterUserId.Value);

            if (projectId.HasValue)
                query = query.Where(a => a.ProjectId == projectId.Value);

            var totalCount = await query.CountAsync();

            var logs = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new ActivityLogDto(
                    a.Id, a.Action, a.EntityType, a.EntityId,
                    a.OldValue, a.NewValue, a.Description, a.Timestamp,
                    a.UserId, a.User.FirstName + " " + a.User.LastName,
                    a.ProjectId, a.Project != null ? a.Project.Name : null
                ))
                .ToListAsync();

            var response = new PaginatedResponse<ActivityLogDto>
            {
                Data = logs,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return ServiceResult<PaginatedResponse<ActivityLogDto>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all activity logs");
            return ServiceResult<PaginatedResponse<ActivityLogDto>>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<PaginatedResponse<ActivityLogDto>>> GetByProjectAsync(int projectId, int userId, int? companyId, bool isSystemAdmin, DateTime? startDate, DateTime? endDate, int? filterUserId, string? entityType, int? entityId, int page, int pageSize)
    {
        try
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId && !p.IsDeleted);
            if (project == null)
                return ServiceResult<PaginatedResponse<ActivityLogDto>>.NotFound("Project not found");

            if (!isSystemAdmin && project.CompanyId != companyId)
                return ServiceResult<PaginatedResponse<ActivityLogDto>>.Forbidden("Access denied");

            var query = _context.ActivityLogs
                .Include(a => a.User)
                .Include(a => a.Project)
                .Where(a => a.ProjectId == projectId)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(a => a.Timestamp >= startDate.Value);

            if (endDate.HasValue)
            {
                var endOfDay = endDate.Value.Date.AddDays(1);
                query = query.Where(a => a.Timestamp < endOfDay);
            }

            if (filterUserId.HasValue)
                query = query.Where(a => a.UserId == filterUserId.Value);

            if (!string.IsNullOrEmpty(entityType))
                query = query.Where(a => a.EntityType == entityType);

            if (entityId.HasValue)
                query = query.Where(a => a.EntityId == entityId.Value);

            var totalCount = await query.CountAsync();

            var logs = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new ActivityLogDto(
                    a.Id, a.Action, a.EntityType, a.EntityId,
                    a.OldValue, a.NewValue, a.Description, a.Timestamp,
                    a.UserId, a.User.FirstName + " " + a.User.LastName,
                    a.ProjectId, a.Project != null ? a.Project.Name : null
                ))
                .ToListAsync();

            var response = new PaginatedResponse<ActivityLogDto>
            {
                Data = logs,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return ServiceResult<PaginatedResponse<ActivityLogDto>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project activity logs for project {ProjectId}", projectId);
            return ServiceResult<PaginatedResponse<ActivityLogDto>>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<List<ActivityLogDto>>> GetByWorkItemAsync(int projectId, int workItemId, int userId, int? companyId, bool isSystemAdmin)
    {
        try
        {
            var workItem = await _context.WorkItems
                .Include(w => w.Project)
                .FirstOrDefaultAsync(w => w.Id == workItemId && w.ProjectId == projectId);

            if (workItem == null)
                return ServiceResult<List<ActivityLogDto>>.NotFound("Work item not found");

            if (!isSystemAdmin && workItem.Project.CompanyId != companyId)
                return ServiceResult<List<ActivityLogDto>>.Forbidden("Access denied");

            var logs = await _context.ActivityLogs
                .Include(a => a.User)
                .Include(a => a.Project)
                .Where(a => a.WorkItemId == workItemId)
                .OrderByDescending(a => a.Timestamp)
                .Select(a => new ActivityLogDto(
                    a.Id, a.Action, a.EntityType, a.EntityId,
                    a.OldValue, a.NewValue, a.Description, a.Timestamp,
                    a.UserId, a.User.FirstName + " " + a.User.LastName,
                    a.ProjectId, a.Project != null ? a.Project.Name : null
                ))
                .ToListAsync();

            return ServiceResult<List<ActivityLogDto>>.Success(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting work item logs for work item {WorkItemId}", workItemId);
            return ServiceResult<List<ActivityLogDto>>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<List<ActivityLogDto>>> GetByFileTicketAsync(int projectId, int fileTicketId, int userId, int? companyId, bool isSystemAdmin)
    {
        try
        {
            var fileTicket = await _context.FileTickets
                .Include(f => f.Project)
                .FirstOrDefaultAsync(f => f.Id == fileTicketId && f.ProjectId == projectId);

            if (fileTicket == null)
                return ServiceResult<List<ActivityLogDto>>.NotFound("File ticket not found");

            if (!isSystemAdmin && fileTicket.Project.CompanyId != companyId)
                return ServiceResult<List<ActivityLogDto>>.Forbidden("Access denied");

            var logs = await _context.ActivityLogs
                .Include(a => a.User)
                .Include(a => a.Project)
                .Where(a => a.FileTicketId == fileTicketId)
                .OrderByDescending(a => a.Timestamp)
                .Select(a => new ActivityLogDto(
                    a.Id, a.Action, a.EntityType, a.EntityId,
                    a.OldValue, a.NewValue, a.Description, a.Timestamp,
                    a.UserId, a.User.FirstName + " " + a.User.LastName,
                    a.ProjectId, a.Project != null ? a.Project.Name : null
                ))
                .ToListAsync();

            return ServiceResult<List<ActivityLogDto>>.Success(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file ticket logs for file ticket {FileTicketId}", fileTicketId);
            return ServiceResult<List<ActivityLogDto>>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<PaginatedResponse<ActivityLogDto>>> GetByUserAsync(int targetUserId, int userId, int? companyId, DateTime? startDate, DateTime? endDate, int page, int pageSize)
    {
        try
        {
            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null)
                return ServiceResult<PaginatedResponse<ActivityLogDto>>.Unauthorized("User not found");

            var targetUser = await _context.Users.FindAsync(targetUserId);
            if (targetUser == null || targetUser.CompanyId != companyId)
                return ServiceResult<PaginatedResponse<ActivityLogDto>>.NotFound("User not found");

            var query = _context.ActivityLogs
                .Include(a => a.User)
                .Include(a => a.Project)
                .Where(a => a.UserId == targetUserId)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(a => a.Timestamp >= startDate.Value);

            if (endDate.HasValue)
            {
                var endOfDay = endDate.Value.Date.AddDays(1);
                query = query.Where(a => a.Timestamp < endOfDay);
            }

            var totalCount = await query.CountAsync();

            var logs = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new ActivityLogDto(
                    a.Id, a.Action, a.EntityType, a.EntityId,
                    a.OldValue, a.NewValue, a.Description, a.Timestamp,
                    a.UserId, a.User.FirstName + " " + a.User.LastName,
                    a.ProjectId, a.Project != null ? a.Project.Name : null
                ))
                .ToListAsync();

            var response = new PaginatedResponse<ActivityLogDto>
            {
                Data = logs,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return ServiceResult<PaginatedResponse<ActivityLogDto>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user activity logs for user {UserId}", targetUserId);
            return ServiceResult<PaginatedResponse<ActivityLogDto>>.Failure(ex.Message, ex);
        }
    }
}
