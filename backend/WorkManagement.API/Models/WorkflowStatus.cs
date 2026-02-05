namespace WorkManagement.API.Models;

public class WorkflowStatus : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Order { get; set; }
    public string Color { get; set; } = "#6B7280";
    public bool IsCore { get; set; } = false; // Core statuses cannot be deleted
    public CoreStatusType? CoreType { get; set; }
    
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    
    public ICollection<WorkItem> WorkItems { get; set; } = new List<WorkItem>();
}

public enum CoreStatusType
{
    New = 0,
    InProgress = 1,
    Review = 2,
    Done = 3,
    Blocked = 4
}
