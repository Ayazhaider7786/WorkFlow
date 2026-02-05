namespace WorkManagement.API.Models;

public class FileTicket : BaseEntity
{
    public string TicketNumber { get; set; } = string.Empty; // Globally unique ID
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public FileTicketType Type { get; set; } = FileTicketType.Physical;
    public FileTicketStatus Status { get; set; } = FileTicketStatus.Created;
    public DateTime? DueDate { get; set; }
    
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    
    public int CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;
    
    public int? CurrentHolderId { get; set; }
    public User? CurrentHolder { get; set; }
    
    public ICollection<FileTicketTransfer> Transfers { get; set; } = new List<FileTicketTransfer>();
    public ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
}

public enum FileTicketType
{
    Physical = 0,
    Digital = 1
}

public enum FileTicketStatus
{
    Created = 0,
    InTransit = 1,
    Received = 2,
    Processing = 3,
    Approved = 4,
    Rejected = 5,
    Completed = 6,
    Lost = 7
}
