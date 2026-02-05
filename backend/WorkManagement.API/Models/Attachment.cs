namespace WorkManagement.API.Models;

public class Attachment : BaseEntity
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    
    public int WorkItemId { get; set; }
    public WorkItem WorkItem { get; set; } = null!;
    
    public int UploadedById { get; set; }
    public User UploadedBy { get; set; } = null!;
}
