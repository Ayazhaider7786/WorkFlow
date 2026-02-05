namespace WorkManagement.API.Models;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    
    public SystemRole SystemRole { get; set; } = SystemRole.Member;
    
    public int? CompanyId { get; set; }
    public Company? Company { get; set; }
    
    // Manager that this user reports to (for Members and QA)
    public int? ManagerId { get; set; }
    public User? Manager { get; set; }
    
    // Users that report to this manager
    public ICollection<User> TeamMembers { get; set; } = new List<User>();
    
    public ICollection<ProjectMember> ProjectMemberships { get; set; } = new List<ProjectMember>();
    public ICollection<WorkItem> AssignedWorkItems { get; set; } = new List<WorkItem>();
    public ICollection<FileTicket> FileTicketsCreated { get; set; } = new List<FileTicket>();
    public ICollection<FileTicket> FileTicketsHeld { get; set; } = new List<FileTicket>();
    public ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
}

public enum SystemRole
{
    Member = 0,
    QA = 1,
    Manager = 2,
    Admin = 3,
    SuperAdmin = 4
}
