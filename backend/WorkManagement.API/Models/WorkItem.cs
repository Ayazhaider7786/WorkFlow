namespace WorkManagement.API.Models;

public class WorkItem : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public WorkItemType Type { get; set; } = WorkItemType.Task;
    public Priority Priority { get; set; } = Priority.Medium;
    public DateTime? DueDate { get; set; }
    public decimal? EstimatedHours { get; set; }
    public decimal? ActualHours { get; set; }
    public int ItemNumber { get; set; } // Auto-generated within project
    
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    
    public int StatusId { get; set; }
    public WorkflowStatus Status { get; set; } = null!;
    
    public int? AssignedToId { get; set; }
    public User? AssignedTo { get; set; }
    
    public int CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;
    
    public int? SprintId { get; set; }
    public Sprint? Sprint { get; set; }
    
    // Parent-Child relationship
    public int? ParentId { get; set; }
    public WorkItem? Parent { get; set; }
    public ICollection<WorkItem> Children { get; set; } = new List<WorkItem>();
    
    public bool IsInBacklog { get; set; } = true;
    public int? QueueOrder { get; set; } // For user's daily queue
    
    public ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
}

public enum WorkItemType
{
    Epic = 0,       // Largest work item, contains Features/Stories
    Feature = 1,    // Major functionality, contains Stories
    Story = 2,      // User story, contains Tasks
    Task = 3,       // Individual work item
    Bug = 4,        // Defect to fix
    Subtask = 5     // Small piece of work under Task/Bug
}

public enum Priority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}
