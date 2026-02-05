namespace WorkManagement.API.Models;

public class FileTicketTransfer : BaseEntity
{
    public int FileTicketId { get; set; }
    public FileTicket FileTicket { get; set; } = null!;
    
    public int FromUserId { get; set; }
    public User FromUser { get; set; } = null!;
    
    public int ToUserId { get; set; }
    public User ToUser { get; set; } = null!;
    
    public DateTime TransferredAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReceivedAt { get; set; }
    public string? Notes { get; set; }
}
