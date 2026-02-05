using WorkManagement.API.Data;
using WorkManagement.API.Models;

namespace WorkManagement.API.Services;

public interface IActivityLogService
{
    Task LogAsync(int userId, string action, string entityType, int entityId, string? oldValue = null, string? newValue = null, string? description = null, int? workItemId = null, int? fileTicketId = null, int? projectId = null);
}

public class ActivityLogService : IActivityLogService
{
    private readonly ApplicationDbContext _context;

    public ActivityLogService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(int userId, string action, string entityType, int entityId, string? oldValue = null, string? newValue = null, string? description = null, int? workItemId = null, int? fileTicketId = null, int? projectId = null)
    {
        var log = new ActivityLog
        {
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValue = oldValue,
            NewValue = newValue,
            Description = description,
            WorkItemId = workItemId,
            FileTicketId = fileTicketId,
            ProjectId = projectId,
            Timestamp = DateTime.UtcNow
        };

        _context.ActivityLogs.Add(log);
        await _context.SaveChangesAsync();
    }
}
