namespace WorkManagement.API.Models;

public class ActivityLog
{
    public int Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? Description { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public int? ProjectId { get; set; }
    public Project? Project { get; set; }
    
    public int? WorkItemId { get; set; }
    public WorkItem? WorkItem { get; set; }
    
    public int? FileTicketId { get; set; }
    public FileTicket? FileTicket { get; set; }
}
