namespace WorkManagement.API.Models;

public class Comment : BaseEntity
{
    public string Content { get; set; } = string.Empty;
    
    public int WorkItemId { get; set; }
    public WorkItem WorkItem { get; set; } = null!;
    
    public int AuthorId { get; set; }
    public User Author { get; set; } = null!;
}
