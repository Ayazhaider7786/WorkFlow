namespace WorkManagement.API.Models;

public class Sprint : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Goal { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public SprintStatus Status { get; set; } = SprintStatus.Planning;
    
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    
    public ICollection<WorkItem> WorkItems { get; set; } = new List<WorkItem>();
}

public enum SprintStatus
{
    Planning = 0,
    Active = 1,
    Completed = 2
}
