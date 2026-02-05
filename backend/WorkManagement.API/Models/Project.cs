namespace WorkManagement.API.Models;

public class Project : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Key { get; set; } = string.Empty; // e.g., "PRJ" for PRJ-001
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Soft delete support
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserId { get; set; }
    
    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    
    public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
    public ICollection<WorkItem> WorkItems { get; set; } = new List<WorkItem>();
    public ICollection<Sprint> Sprints { get; set; } = new List<Sprint>();
    public ICollection<WorkflowStatus> WorkflowStatuses { get; set; } = new List<WorkflowStatus>();
    public ICollection<FileTicket> FileTickets { get; set; } = new List<FileTicket>();
}
