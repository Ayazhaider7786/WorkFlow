using Microsoft.EntityFrameworkCore;
using WorkManagement.API.Data;
using WorkManagement.API.DTOs;
using WorkManagement.API.Models;

namespace WorkManagement.API.Services;

public interface IDashboardService
{
    Task<ServiceResult<DashboardDto>> GetDashboardAsync(int projectId, int userId);
}

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DashboardService(ApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ServiceResult<DashboardDto>> GetDashboardAsync(int projectId, int userId)
    {
        try
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return ServiceResult<DashboardDto>.NotFound("Project not found");

            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null) return ServiceResult<DashboardDto>.Unauthorized("User not found");

            if (currentUser.SystemRole != SystemRole.SuperAdmin && project.CompanyId != currentUser.CompanyId)
            {
                return ServiceResult<DashboardDto>.Forbidden("Access denied");
            }

            var workItems = await _context.WorkItems
                .Include(w => w.Status)
                .Where(w => w.ProjectId == projectId)
                .ToListAsync();

            var sprints = await _context.Sprints
                .Where(s => s.ProjectId == projectId)
                .ToListAsync();

            var fileTickets = await _context.FileTickets
                .Where(f => f.ProjectId == projectId)
                .ToListAsync();

            var doneStatus = await _context.WorkflowStatuses
                .FirstOrDefaultAsync(s => s.ProjectId == projectId && s.CoreType == CoreStatusType.Done);

            var inProgressStatus = await _context.WorkflowStatuses
                .FirstOrDefaultAsync(s => s.ProjectId == projectId && s.CoreType == CoreStatusType.InProgress);

            var blockedStatus = await _context.WorkflowStatuses
                .FirstOrDefaultAsync(s => s.ProjectId == projectId && s.CoreType == CoreStatusType.Blocked);

            // Work items by status
            var statuses = await _context.WorkflowStatuses
                .Where(s => s.ProjectId == projectId)
                .OrderBy(s => s.Order)
                .ToListAsync();

            var workItemsByStatus = statuses.Select(s => new WorkItemsByStatusDto(
                s.Name, s.Color, workItems.Count(w => w.StatusId == s.Id)
            )).ToList();

            // Work items by priority
            var workItemsByPriority = Enum.GetValues<Priority>()
                .Select(p => new WorkItemsByPriorityDto(p.ToString(), workItems.Count(w => w.Priority == p)))
                .ToList();

            // User workload
            var members = await _context.ProjectMembers
                .Include(pm => pm.User)
                .Where(pm => pm.ProjectId == projectId)
                .ToListAsync();

            var userWorkload = members.Select(m => new UserWorkloadDto(
                m.UserId,
                $"{m.User.FirstName} {m.User.LastName}",
                workItems.Count(w => w.AssignedToId == m.UserId),
                workItems.Count(w => w.AssignedToId == m.UserId && w.StatusId == doneStatus?.Id)
            )).ToList();

            var dashboard = new DashboardDto(
                TotalWorkItems: workItems.Count,
                CompletedWorkItems: workItems.Count(w => w.StatusId == doneStatus?.Id),
                InProgressWorkItems: workItems.Count(w => w.StatusId == inProgressStatus?.Id),
                BlockedWorkItems: workItems.Count(w => w.StatusId == blockedStatus?.Id),
                TotalSprints: sprints.Count,
                ActiveSprints: sprints.Count(s => s.Status == SprintStatus.Active),
                TotalFileTickets: fileTickets.Count,
                PendingFileTickets: fileTickets.Count(f => f.Status != FileTicketStatus.Completed && f.Status != FileTicketStatus.Rejected),
                WorkItemsByStatus: workItemsByStatus,
                WorkItemsByPriority: workItemsByPriority,
                UserWorkload: userWorkload
            );

            return ServiceResult<DashboardDto>.Success(dashboard);
        }
        catch (Exception ex)
        {
            return ServiceResult<DashboardDto>.Failure(ex.Message, ex);
        }
    }
}
