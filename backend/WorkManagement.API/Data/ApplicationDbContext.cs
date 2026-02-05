using Microsoft.EntityFrameworkCore;
using WorkManagement.API.Models;

namespace WorkManagement.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<WorkflowStatus> WorkflowStatuses => Set<WorkflowStatus>();
    public DbSet<WorkItem> WorkItems => Set<WorkItem>();
    public DbSet<Sprint> Sprints => Set<Sprint>();
    public DbSet<FileTicket> FileTickets => Set<FileTicket>();
    public DbSet<FileTicketTransfer> FileTicketTransfers => Set<FileTicketTransfer>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Attachment> Attachments => Set<Attachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Company
        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasOne(e => e.Company)
                  .WithMany(c => c.Users)
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);
            // Manager relationship (for Members and QA reporting to Manager)
            // Use NoAction to avoid cascade cycles with self-referencing FK
            entity.HasOne(e => e.Manager)
                  .WithMany(m => m.TeamMembers)
                  .HasForeignKey(e => e.ManagerId)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        // Project
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasIndex(e => new { e.CompanyId, e.Key }).IsUnique();
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasOne(e => e.Company)
                  .WithMany(c => c.Projects)
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ProjectMember
        modelBuilder.Entity<ProjectMember>(entity =>
        {
            entity.HasIndex(e => new { e.ProjectId, e.UserId }).IsUnique();
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasOne(e => e.Project)
                  .WithMany(p => p.Members)
                  .HasForeignKey(e => e.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User)
                  .WithMany(u => u.ProjectMemberships)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // WorkflowStatus
        modelBuilder.Entity<WorkflowStatus>(entity =>
        {
            entity.HasIndex(e => new { e.ProjectId, e.Name }).IsUnique();
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasOne(e => e.Project)
                  .WithMany(p => p.WorkflowStatuses)
                  .HasForeignKey(e => e.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // WorkItem - FIX: Added precision for decimal properties
        modelBuilder.Entity<WorkItem>(entity =>
        {
            entity.HasIndex(e => new { e.ProjectId, e.ItemNumber }).IsUnique();
            entity.HasQueryFilter(e => !e.IsDeleted);
            
            // FIX: Specify precision for decimal columns
            entity.Property(e => e.EstimatedHours).HasPrecision(10, 2);
            entity.Property(e => e.ActualHours).HasPrecision(10, 2);
            
            entity.HasOne(e => e.Project)
                  .WithMany(p => p.WorkItems)
                  .HasForeignKey(e => e.ProjectId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Status)
                  .WithMany(s => s.WorkItems)
                  .HasForeignKey(e => e.StatusId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.AssignedTo)
                  .WithMany(u => u.AssignedWorkItems)
                  .HasForeignKey(e => e.AssignedToId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.CreatedBy)
                  .WithMany()
                  .HasForeignKey(e => e.CreatedById)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Sprint)
                  .WithMany(s => s.WorkItems)
                  .HasForeignKey(e => e.SprintId)
                  .OnDelete(DeleteBehavior.SetNull);
            // Parent-Child relationship for hierarchical work items
            entity.HasOne(e => e.Parent)
                  .WithMany(e => e.Children)
                  .HasForeignKey(e => e.ParentId)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        // Sprint
        modelBuilder.Entity<Sprint>(entity =>
        {
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasOne(e => e.Project)
                  .WithMany(p => p.Sprints)
                  .HasForeignKey(e => e.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // FileTicket
        modelBuilder.Entity<FileTicket>(entity =>
        {
            entity.HasIndex(e => e.TicketNumber).IsUnique();
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasOne(e => e.Project)
                  .WithMany(p => p.FileTickets)
                  .HasForeignKey(e => e.ProjectId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.CreatedBy)
                  .WithMany(u => u.FileTicketsCreated)
                  .HasForeignKey(e => e.CreatedById)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.CurrentHolder)
                  .WithMany(u => u.FileTicketsHeld)
                  .HasForeignKey(e => e.CurrentHolderId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // FileTicketTransfer - FIX: Added matching query filter
        modelBuilder.Entity<FileTicketTransfer>(entity =>
        {
            // FIX: Add query filter to match FileTicket's filter
            entity.HasQueryFilter(e => !e.IsDeleted);
            
            entity.HasOne(e => e.FileTicket)
                  .WithMany(f => f.Transfers)
                  .HasForeignKey(e => e.FileTicketId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.FromUser)
                  .WithMany()
                  .HasForeignKey(e => e.FromUserId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.ToUser)
                  .WithMany()
                  .HasForeignKey(e => e.ToUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ActivityLog - FIX: Made User navigation optional
        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            
            // FIX: Make User relationship optional to avoid query filter issues
            entity.HasOne(e => e.User)
                  .WithMany(u => u.ActivityLogs)
                  .HasForeignKey(e => e.UserId)
                  .IsRequired(false)  // Make optional
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.WorkItem)
                  .WithMany(w => w.ActivityLogs)
                  .HasForeignKey(e => e.WorkItemId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.FileTicket)
                  .WithMany(f => f.ActivityLogs)
                  .HasForeignKey(e => e.FileTicketId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Comment
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasOne(e => e.WorkItem)
                  .WithMany(w => w.Comments)
                  .HasForeignKey(e => e.WorkItemId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Author)
                  .WithMany()
                  .HasForeignKey(e => e.AuthorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Attachment
        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasOne(e => e.WorkItem)
                  .WithMany(w => w.Attachments)
                  .HasForeignKey(e => e.WorkItemId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.UploadedBy)
                  .WithMany()
                  .HasForeignKey(e => e.UploadedById)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
