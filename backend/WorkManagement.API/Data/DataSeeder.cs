using Microsoft.EntityFrameworkCore;
using WorkManagement.API.Models;

namespace WorkManagement.API.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Check if data already exists
        if (await context.Companies.AnyAsync()) return;

        // Create demo company
        var company = new Company
        {
            Name = "TechCorp Solutions",
            Description = "A modern technology company",
            IsActive = true
        };
        context.Companies.Add(company);
        await context.SaveChangesAsync();

        // Create users with role hierarchy
        var superAdmin = new User
        {
            Email = "superadmin@techcorp.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("SuperAdmin@123"),
            FirstName = "Super",
            LastName = "Admin",
            SystemRole = SystemRole.SuperAdmin,
            CompanyId = company.Id
        };

        var admin = new User
        {
            Email = "admin@techcorp.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            FirstName = "John",
            LastName = "Admin",
            SystemRole = SystemRole.Admin,
            CompanyId = company.Id
        };

        var manager1 = new User
        {
            Email = "manager@techcorp.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager@123"),
            FirstName = "Sarah",
            LastName = "Manager",
            SystemRole = SystemRole.Manager,
            CompanyId = company.Id
        };

        var manager2 = new User
        {
            Email = "manager2@techcorp.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager@123"),
            FirstName = "Mike",
            LastName = "Manager",
            SystemRole = SystemRole.Manager,
            CompanyId = company.Id
        };

        context.Users.AddRange(superAdmin, admin, manager1, manager2);
        await context.SaveChangesAsync();

        // Create QA and Members under Manager1
        var qa1 = new User
        {
            Email = "qa@techcorp.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("QA@123"),
            FirstName = "Alex",
            LastName = "QA",
            SystemRole = SystemRole.QA,
            CompanyId = company.Id,
            ManagerId = manager1.Id
        };

        var member1 = new User
        {
            Email = "member1@techcorp.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Member@123"),
            FirstName = "Tom",
            LastName = "Developer",
            SystemRole = SystemRole.Member,
            CompanyId = company.Id,
            ManagerId = manager1.Id
        };

        var member2 = new User
        {
            Email = "member2@techcorp.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Member@123"),
            FirstName = "Jane",
            LastName = "Developer",
            SystemRole = SystemRole.Member,
            CompanyId = company.Id,
            ManagerId = manager1.Id
        };

        // Members under Manager2
        var member3 = new User
        {
            Email = "member3@techcorp.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Member@123"),
            FirstName = "Bob",
            LastName = "Developer",
            SystemRole = SystemRole.Member,
            CompanyId = company.Id,
            ManagerId = manager2.Id
        };

        context.Users.AddRange(qa1, member1, member2, member3);
        await context.SaveChangesAsync();

        // Create projects
        var project1 = new Project
        {
            Name = "E-Commerce Platform",
            Description = "Building a modern e-commerce platform",
            Key = "ECOM",
            CompanyId = company.Id,
            IsActive = true,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(90)
        };

        var project2 = new Project
        {
            Name = "Mobile App",
            Description = "Cross-platform mobile application",
            Key = "MOB",
            CompanyId = company.Id,
            IsActive = true,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(120)
        };

        context.Projects.AddRange(project1, project2);
        await context.SaveChangesAsync();

        // Add managers to projects
        var projectMember1 = new ProjectMember { ProjectId = project1.Id, UserId = manager1.Id, Role = ProjectRole.Manager };
        var projectMember2 = new ProjectMember { ProjectId = project1.Id, UserId = manager2.Id, Role = ProjectRole.Manager };
        var projectMember3 = new ProjectMember { ProjectId = project2.Id, UserId = manager1.Id, Role = ProjectRole.Manager };

        // Add team members to project1
        var projectMember4 = new ProjectMember { ProjectId = project1.Id, UserId = qa1.Id, Role = ProjectRole.Member };
        var projectMember5 = new ProjectMember { ProjectId = project1.Id, UserId = member1.Id, Role = ProjectRole.Member };
        var projectMember6 = new ProjectMember { ProjectId = project1.Id, UserId = member2.Id, Role = ProjectRole.Member };

        context.ProjectMembers.AddRange(projectMember1, projectMember2, projectMember3, projectMember4, projectMember5, projectMember6);
        await context.SaveChangesAsync();

        // Create workflow statuses for projects
        foreach (var project in new[] { project1, project2 })
        {
            var statuses = new[]
            {
                new WorkflowStatus { Name = "To Do", Order = 1, Color = "#6B7280", IsCore = true, CoreType = CoreStatusType.New, ProjectId = project.Id },
                new WorkflowStatus { Name = "In Progress", Order = 2, Color = "#3B82F6", IsCore = true, CoreType = CoreStatusType.InProgress, ProjectId = project.Id },
                new WorkflowStatus { Name = "In Review", Order = 3, Color = "#8B5CF6", IsCore = true, CoreType = CoreStatusType.Review, ProjectId = project.Id },
                new WorkflowStatus { Name = "Done", Order = 4, Color = "#10B981", IsCore = true, CoreType = CoreStatusType.Done, ProjectId = project.Id },
                new WorkflowStatus { Name = "Blocked", Order = 5, Color = "#EF4444", IsCore = true, CoreType = CoreStatusType.Blocked, ProjectId = project.Id }
            };
            context.WorkflowStatuses.AddRange(statuses);
        }
        await context.SaveChangesAsync();

        // Get statuses for project1
        var todoStatus = await context.WorkflowStatuses.FirstAsync(s => s.ProjectId == project1.Id && s.CoreType == CoreStatusType.New);
        var inProgressStatus = await context.WorkflowStatuses.FirstAsync(s => s.ProjectId == project1.Id && s.CoreType == CoreStatusType.InProgress);

        // Create sprints
        var sprint1 = new Sprint
        {
            Name = "Sprint 1",
            Goal = "Setup and initial features",
            StartDate = DateTime.UtcNow.AddDays(-14),
            EndDate = DateTime.UtcNow,
            Status = SprintStatus.Completed,
            ProjectId = project1.Id
        };

        var sprint2 = new Sprint
        {
            Name = "Sprint 2",
            Goal = "Core functionality",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(14),
            Status = SprintStatus.Active,
            ProjectId = project1.Id
        };

        context.Sprints.AddRange(sprint1, sprint2);
        await context.SaveChangesAsync();

        // Create work items
        var workItems = new[]
        {
            new WorkItem
            {
                Title = "Setup project structure",
                Description = "Initialize the project with proper folder structure and configurations",
                Priority = Priority.High,
                ItemNumber = 1,
                ProjectId = project1.Id,
                StatusId = inProgressStatus.Id,
                AssignedToId = member1.Id,
                CreatedById = manager1.Id,
                SprintId = sprint2.Id,
                IsInBacklog = false,
                EstimatedHours = 8
            },
            new WorkItem
            {
                Title = "Design database schema",
                Description = "Create database schema for users, products, and orders",
                Priority = Priority.High,
                ItemNumber = 2,
                ProjectId = project1.Id,
                StatusId = todoStatus.Id,
                AssignedToId = member2.Id,
                CreatedById = qa1.Id,
                SprintId = sprint2.Id,
                IsInBacklog = false,
                EstimatedHours = 16
            },
            new WorkItem
            {
                Title = "Implement authentication",
                Description = "User login, registration, and JWT implementation",
                Priority = Priority.Critical,
                ItemNumber = 3,
                ProjectId = project1.Id,
                StatusId = todoStatus.Id,
                CreatedById = manager1.Id,
                IsInBacklog = true,
                EstimatedHours = 24
            }
        };

        context.WorkItems.AddRange(workItems);
        await context.SaveChangesAsync();

        // Create activity logs
        foreach (var workItem in workItems)
        {
            var log = new ActivityLog
            {
                Action = "Created",
                EntityType = "WorkItem",
                EntityId = workItem.Id,
                Description = $"Created ticket '{workItem.Title}'",
                UserId = workItem.CreatedById,
                WorkItemId = workItem.Id,
                Timestamp = DateTime.UtcNow.AddHours(-1)
            };
            context.ActivityLogs.Add(log);
        }
        await context.SaveChangesAsync();
    }
}
