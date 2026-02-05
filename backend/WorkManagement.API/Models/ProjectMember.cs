namespace WorkManagement.API.Models;

public class ProjectMember : BaseEntity
{
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public ProjectRole Role { get; set; } = ProjectRole.Member;
    
    // Soft delete support
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserId { get; set; }
}

public enum ProjectRole
{
    Viewer = 0,
    Member = 1,
    Manager = 2,
    Admin = 3
}
