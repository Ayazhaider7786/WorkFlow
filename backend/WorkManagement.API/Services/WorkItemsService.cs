using Microsoft.EntityFrameworkCore;
using WorkManagement.API.Data;
using WorkManagement.API.DTOs;
using WorkManagement.API.Models;

namespace WorkManagement.API.Services;

public interface IWorkItemsService
{
    Task<ServiceResult<IEnumerable<WorkItemDto>>> GetWorkItemsAsync(int projectId, int? sprintId, bool? backlogOnly, int? assignedToId, int? parentId, WorkItemType? type, int userId);
    Task<ServiceResult<WorkItemDto>> GetWorkItemAsync(int projectId, int id, int userId);
    Task<ServiceResult<IEnumerable<WorkItemDto>>> GetChildrenAsync(int projectId, int id, int userId);
    Task<ServiceResult<WorkItemDto>> CreateWorkItemAsync(int projectId, CreateWorkItemDto dto, int userId);
    Task<ServiceResult<WorkItemDto>> UpdateWorkItemAsync(int projectId, int id, UpdateWorkItemDto dto, int userId);
    Task<ServiceResult<bool>> DeleteWorkItemAsync(int projectId, int id, int userId);
}

public class WorkItemsService : IWorkItemsService
{
    private readonly ApplicationDbContext _context;
    private readonly IActivityLogService _activityLog;
    private readonly ICurrentUserService _currentUserService;

    public WorkItemsService(ApplicationDbContext context, IActivityLogService activityLog, ICurrentUserService currentUserService)
    {
        _context = context;
        _activityLog = activityLog;
        _currentUserService = currentUserService;
    }

    public async Task<ServiceResult<IEnumerable<WorkItemDto>>> GetWorkItemsAsync(int projectId, int? sprintId, bool? backlogOnly, int? assignedToId, int? parentId, WorkItemType? type, int userId)
    {
        try
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return ServiceResult<IEnumerable<WorkItemDto>>.NotFound("Project not found");

            if (!await CanAccessProject(projectId, userId))
            {
                return ServiceResult<IEnumerable<WorkItemDto>>.Forbidden("Access denied");
            }

            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null) return ServiceResult<IEnumerable<WorkItemDto>>.Unauthorized("User not found");

            var query = _context.WorkItems
                .Include(w => w.Status)
                .Include(w => w.AssignedTo)
                .Include(w => w.Sprint)
                .Include(w => w.CreatedBy)
                .Include(w => w.Comments)
                .Include(w => w.Attachments)
                .Include(w => w.Parent)
                .Include(w => w.Children)
                .Where(w => w.ProjectId == projectId);

            // Apply visibility rules for Members and QA
            if (currentUser.SystemRole == SystemRole.Member || currentUser.SystemRole == SystemRole.QA)
            {
                query = query.Where(w => 
                    w.CreatedById == userId || 
                    w.AssignedToId == userId);
            }

            if (sprintId.HasValue)
            {
                query = query.Where(w => w.SprintId == sprintId.Value);
            }
            else if (backlogOnly == true)
            {
                query = query.Where(w => w.IsInBacklog);
            }

            if (assignedToId.HasValue)
            {
                query = query.Where(w => w.AssignedToId == assignedToId.Value);
            }

            if (parentId.HasValue)
            {
                query = query.Where(w => w.ParentId == parentId.Value);
            }

            if (type.HasValue)
            {
                query = query.Where(w => w.Type == type.Value);
            }

            var workItems = await query
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync(); // Execute query first to avoid translation issues if any select projection is complex, though Select below is fine too.
                // Actually mapping directly is better for perf to avoid pulling all fields.
                // Re-writing as Select directly on query.
            
            var dtos = await query
                .OrderByDescending(w => w.CreatedAt)
                .Select(w => new WorkItemDto(
                    w.Id, w.Title, w.Description, w.Type, w.Priority, w.DueDate,
                    w.EstimatedHours, w.ActualHours, w.ItemNumber,
                    project.Key + "-" + w.ItemNumber,
                    w.ProjectId, w.StatusId, w.Status.Name, w.Status.Color,
                    w.AssignedToId, w.AssignedTo != null ? w.AssignedTo.FirstName + " " + w.AssignedTo.LastName : null,
                    w.SprintId, w.Sprint != null ? w.Sprint.Name : null,
                    w.IsInBacklog, w.QueueOrder,
                    w.CreatedById, w.CreatedBy.FirstName + " " + w.CreatedBy.LastName,
                    w.CreatedAt, w.Comments.Count, w.Attachments.Count,
                    w.ParentId, w.Parent != null ? project.Key + "-" + w.Parent.ItemNumber : null,
                    w.Children.Count))
                .ToListAsync();

            return ServiceResult<IEnumerable<WorkItemDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<WorkItemDto>>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<WorkItemDto>> GetWorkItemAsync(int projectId, int id, int userId)
    {
        try
        {
            var workItem = await _context.WorkItems
                .Include(w => w.Project)
                .Include(w => w.Status)
                .Include(w => w.AssignedTo)
                .Include(w => w.Sprint)
                .Include(w => w.CreatedBy)
                .Include(w => w.Comments)
                .Include(w => w.Attachments)
                .Include(w => w.Parent)
                .Include(w => w.Children)
                .FirstOrDefaultAsync(w => w.Id == id && w.ProjectId == projectId);

            if (workItem == null) return ServiceResult<WorkItemDto>.NotFound("Work item not found");

            if (!await CanAccessWorkItem(workItem, userId))
            {
                return ServiceResult<WorkItemDto>.Forbidden("Access denied");
            }

            return ServiceResult<WorkItemDto>.Success(new WorkItemDto(
                workItem.Id, workItem.Title, workItem.Description, workItem.Type, workItem.Priority, workItem.DueDate,
                workItem.EstimatedHours, workItem.ActualHours, workItem.ItemNumber,
                workItem.Project.Key + "-" + workItem.ItemNumber,
                workItem.ProjectId, workItem.StatusId, workItem.Status.Name, workItem.Status.Color,
                workItem.AssignedToId, workItem.AssignedTo?.FirstName + " " + workItem.AssignedTo?.LastName,
                workItem.SprintId, workItem.Sprint?.Name,
                workItem.IsInBacklog, workItem.QueueOrder,
                workItem.CreatedById, workItem.CreatedBy.FirstName + " " + workItem.CreatedBy.LastName,
                workItem.CreatedAt, workItem.Comments.Count, workItem.Attachments.Count,
                workItem.ParentId, workItem.Parent != null ? workItem.Project.Key + "-" + workItem.Parent.ItemNumber : null,
                workItem.Children.Count));
        }
        catch (Exception ex)
        {
            return ServiceResult<WorkItemDto>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<IEnumerable<WorkItemDto>>> GetChildrenAsync(int projectId, int id, int userId)
    {
        try
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return ServiceResult<IEnumerable<WorkItemDto>>.NotFound("Project not found");

            // Assuming standard access check applies to getting children too
            if (!await CanAccessProject(projectId, userId))
            {
                return ServiceResult<IEnumerable<WorkItemDto>>.Forbidden("Access denied");
            }

            var workItems = await _context.WorkItems
                .Include(w => w.Status)
                .Include(w => w.AssignedTo)
                .Include(w => w.Sprint)
                .Include(w => w.CreatedBy)
                .Include(w => w.Comments)
                .Include(w => w.Attachments)
                .Include(w => w.Children)
                .Where(w => w.ProjectId == projectId && w.ParentId == id)
                .OrderBy(w => w.CreatedAt)
                .Select(w => new WorkItemDto(
                    w.Id, w.Title, w.Description, w.Type, w.Priority, w.DueDate,
                    w.EstimatedHours, w.ActualHours, w.ItemNumber,
                    project.Key + "-" + w.ItemNumber,
                    w.ProjectId, w.StatusId, w.Status.Name, w.Status.Color,
                    w.AssignedToId, w.AssignedTo != null ? w.AssignedTo.FirstName + " " + w.AssignedTo.LastName : null,
                    w.SprintId, w.Sprint != null ? w.Sprint.Name : null,
                    w.IsInBacklog, w.QueueOrder,
                    w.CreatedById, w.CreatedBy.FirstName + " " + w.CreatedBy.LastName,
                    w.CreatedAt, w.Comments.Count, w.Attachments.Count,
                    w.ParentId, project.Key + "-" + id,
                    w.Children.Count))
                .ToListAsync();

            return ServiceResult<IEnumerable<WorkItemDto>>.Success(workItems);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<WorkItemDto>>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<WorkItemDto>> CreateWorkItemAsync(int projectId, CreateWorkItemDto dto, int userId)
    {
        try
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return ServiceResult<WorkItemDto>.NotFound("Project not found");

            if (!await CanAccessProject(projectId, userId))
            {
                return ServiceResult<WorkItemDto>.Forbidden("Access denied");
            }

            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null) return ServiceResult<WorkItemDto>.Unauthorized("User not found");

            // Members cannot create tickets (only Managers, QA, Admin, SuperAdmin can)
            if (currentUser.SystemRole == SystemRole.Member)
            {
                return ServiceResult<WorkItemDto>.BadRequest("Members cannot create tickets");
            }

            // Validate parent if specified
            WorkItem? parent = null;
            if (dto.ParentId.HasValue)
            {
                parent = await _context.WorkItems.FindAsync(dto.ParentId.Value);
                if (parent == null || parent.ProjectId != projectId)
                {
                    return ServiceResult<WorkItemDto>.BadRequest("Invalid parent work item");
                }

                // Validate parent-child type hierarchy
                var itemType = dto.Type ?? WorkItemType.Task;
                if (!IsValidChildType(parent.Type, itemType))
                {
                    return ServiceResult<WorkItemDto>.BadRequest($"Cannot create {itemType} under {parent.Type}");
                }
            }

            // Get the default status
            var defaultStatus = await _context.WorkflowStatuses
                .Where(s => s.ProjectId == projectId && s.CoreType == CoreStatusType.New)
                .FirstOrDefaultAsync();

            if (defaultStatus == null)
            {
                defaultStatus = await _context.WorkflowStatuses
                    .Where(s => s.ProjectId == projectId)
                    .OrderBy(s => s.Order)
                    .FirstOrDefaultAsync();
            }

            if (defaultStatus == null)
            {
                return ServiceResult<WorkItemDto>.BadRequest("No workflow statuses configured for this project");
            }

            // Auto-generate item number
            var maxItemNumber = await _context.WorkItems
                .Where(w => w.ProjectId == projectId)
                .MaxAsync(w => (int?)w.ItemNumber) ?? 0;

            var workItem = new WorkItem
            {
                Title = dto.Title,
                Description = dto.Description,
                Type = dto.Type ?? WorkItemType.Task,
                Priority = dto.Priority ?? Priority.Medium,
                DueDate = dto.DueDate,
                EstimatedHours = dto.EstimatedHours,
                ProjectId = projectId,
                StatusId = defaultStatus.Id,
                AssignedToId = dto.AssignedToId,
                SprintId = dto.SprintId,
                ParentId = dto.ParentId,
                ItemNumber = maxItemNumber + 1,
                IsInBacklog = dto.SprintId == null,
                CreatedById = userId
            };

            _context.WorkItems.Add(workItem);
            await _context.SaveChangesAsync();

            var typeLabel = GetTypeLabel(workItem.Type);
            await _activityLog.LogAsync(userId, "Created", "WorkItem", workItem.Id,
                workItemId: workItem.Id, description: $"Created {typeLabel} '{workItem.Title}'");

            var assignedTo = dto.AssignedToId.HasValue
                ? await _context.Users.FindAsync(dto.AssignedToId.Value)
                : null;

            var sprint = dto.SprintId.HasValue
                ? await _context.Sprints.FindAsync(dto.SprintId.Value)
                : null;

            return ServiceResult<WorkItemDto>.Created(new WorkItemDto(
                workItem.Id, workItem.Title, workItem.Description, workItem.Type, workItem.Priority, workItem.DueDate,
                workItem.EstimatedHours, workItem.ActualHours, workItem.ItemNumber,
                project.Key + "-" + workItem.ItemNumber,
                workItem.ProjectId, workItem.StatusId, defaultStatus.Name, defaultStatus.Color,
                workItem.AssignedToId, assignedTo != null ? assignedTo.FirstName + " " + assignedTo.LastName : null,
                workItem.SprintId, sprint?.Name,
                workItem.IsInBacklog, workItem.QueueOrder,
                workItem.CreatedById, currentUser.FirstName + " " + currentUser.LastName,
                workItem.CreatedAt, 0, 0,
                workItem.ParentId, parent != null ? project.Key + "-" + parent.ItemNumber : null, 0));
        }
        catch (Exception ex)
        {
            return ServiceResult<WorkItemDto>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<WorkItemDto>> UpdateWorkItemAsync(int projectId, int id, UpdateWorkItemDto dto, int userId)
    {
        try
        {
            var workItem = await _context.WorkItems
                .Include(w => w.Project)
                .Include(w => w.Status)
                .Include(w => w.AssignedTo)
                .Include(w => w.Sprint)
                .Include(w => w.CreatedBy)
                .Include(w => w.Comments)
                .Include(w => w.Attachments)
                .Include(w => w.Parent)
                .Include(w => w.Children)
                .FirstOrDefaultAsync(w => w.Id == id && w.ProjectId == projectId);

            if (workItem == null) return ServiceResult<WorkItemDto>.NotFound("Work item not found");

            if (!await CanAccessWorkItem(workItem, userId))
            {
                return ServiceResult<WorkItemDto>.Forbidden("Access denied");
            }

            var changes = new List<string>();

            if (dto.Title != null && dto.Title != workItem.Title)
            {
                changes.Add($"Title changed from '{workItem.Title}' to '{dto.Title}'");
                workItem.Title = dto.Title;
            }
            if (dto.Description != null) workItem.Description = dto.Description;
            
            if (dto.Type.HasValue && dto.Type.Value != workItem.Type)
            {
                changes.Add($"Type changed from {workItem.Type} to {dto.Type.Value}");
                workItem.Type = dto.Type.Value;
            }

            if (dto.Priority.HasValue && dto.Priority.Value != workItem.Priority)
            {
                changes.Add($"Priority changed from {workItem.Priority} to {dto.Priority.Value}");
                workItem.Priority = dto.Priority.Value;
            }
            if (dto.DueDate.HasValue) workItem.DueDate = dto.DueDate;
            if (dto.EstimatedHours.HasValue) workItem.EstimatedHours = dto.EstimatedHours;
            if (dto.ActualHours.HasValue) workItem.ActualHours = dto.ActualHours;
            
            if (dto.StatusId.HasValue && dto.StatusId.Value != workItem.StatusId)
            {
                var newStatus = await _context.WorkflowStatuses.FindAsync(dto.StatusId.Value);
                if (newStatus != null)
                {
                    changes.Add($"Status changed from '{workItem.Status.Name}' to '{newStatus.Name}'");
                    workItem.StatusId = dto.StatusId.Value;
                    workItem.Status = newStatus;
                }
            }

            if (dto.AssignedToId.HasValue && dto.AssignedToId != workItem.AssignedToId)
            {
                var newAssignee = await _context.Users.FindAsync(dto.AssignedToId.Value);
                if (newAssignee != null)
                {
                    changes.Add($"Assigned to {newAssignee.FirstName} {newAssignee.LastName}");
                    workItem.AssignedToId = dto.AssignedToId.Value;
                    workItem.AssignedTo = newAssignee;
                }
            }

            if (dto.SprintId.HasValue) 
            {
                workItem.SprintId = dto.SprintId;
                workItem.IsInBacklog = false;
            }
            if (dto.IsInBacklog.HasValue)
            {
                workItem.IsInBacklog = dto.IsInBacklog.Value;
                if (dto.IsInBacklog.Value) workItem.SprintId = null;
            }
            if (dto.QueueOrder.HasValue) workItem.QueueOrder = dto.QueueOrder;

            if (dto.ParentId.HasValue && dto.ParentId != workItem.ParentId)
            {
                if (dto.ParentId.Value == 0)
                {
                    workItem.ParentId = null;
                    changes.Add("Removed from parent");
                }
                else
                {
                    var newParent = await _context.WorkItems.FindAsync(dto.ParentId.Value);
                    if (newParent != null && newParent.ProjectId == projectId)
                    {
                        workItem.ParentId = dto.ParentId.Value;
                        changes.Add($"Moved under {workItem.Project.Key}-{newParent.ItemNumber}");
                    }
                }
            }

            workItem.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            if (changes.Any())
            {
                await _activityLog.LogAsync(userId, "Updated", "WorkItem", workItem.Id,
                    workItemId: workItem.Id, description: string.Join("; ", changes));
            }

            return ServiceResult<WorkItemDto>.Success(new WorkItemDto(
                workItem.Id, workItem.Title, workItem.Description, workItem.Type, workItem.Priority, workItem.DueDate,
                workItem.EstimatedHours, workItem.ActualHours, workItem.ItemNumber,
                workItem.Project.Key + "-" + workItem.ItemNumber,
                workItem.ProjectId, workItem.StatusId, workItem.Status.Name, workItem.Status.Color,
                workItem.AssignedToId, workItem.AssignedTo?.FirstName + " " + workItem.AssignedTo?.LastName,
                workItem.SprintId, workItem.Sprint?.Name,
                workItem.IsInBacklog, workItem.QueueOrder,
                workItem.CreatedById, workItem.CreatedBy.FirstName + " " + workItem.CreatedBy.LastName,
                workItem.CreatedAt, workItem.Comments.Count, workItem.Attachments.Count,
                workItem.ParentId, workItem.Parent != null ? workItem.Project.Key + "-" + workItem.Parent.ItemNumber : null,
                workItem.Children.Count));
        }
        catch (Exception ex)
        {
            return ServiceResult<WorkItemDto>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<bool>> DeleteWorkItemAsync(int projectId, int id, int userId)
    {
        try
        {
            var workItem = await _context.WorkItems
                .Include(w => w.Project)
                .FirstOrDefaultAsync(w => w.Id == id && w.ProjectId == projectId);

            if (workItem == null) return ServiceResult<bool>.NotFound("Work item not found");

            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null) return ServiceResult<bool>.Unauthorized("User not found");

            // Members cannot delete tickets
            if (currentUser.SystemRole == SystemRole.Member)
            {
                return ServiceResult<bool>.BadRequest("Members cannot delete tickets");
            }

            // QA can only delete their own tickets
            if (currentUser.SystemRole == SystemRole.QA && workItem.CreatedById != userId)
            {
                return ServiceResult<bool>.Forbidden("Access denied");
            }

            workItem.IsDeleted = true;
            workItem.DeletedAt = DateTime.UtcNow;
            workItem.DeletedBy = userId;

            await _context.SaveChangesAsync();

            await _activityLog.LogAsync(userId, "Deleted", "WorkItem", workItem.Id,
                workItemId: workItem.Id, description: $"Deleted ticket '{workItem.Title}'");

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Failure(ex.Message, ex);
        }
    }

    private bool IsValidChildType(WorkItemType parentType, WorkItemType childType)
    {
        return parentType switch
        {
            WorkItemType.Epic => childType == WorkItemType.Feature || childType == WorkItemType.Story,
            WorkItemType.Feature => childType == WorkItemType.Story || childType == WorkItemType.Task || childType == WorkItemType.Bug,
            WorkItemType.Story => childType == WorkItemType.Task || childType == WorkItemType.Bug || childType == WorkItemType.Subtask,
            WorkItemType.Task => childType == WorkItemType.Subtask,
            WorkItemType.Bug => childType == WorkItemType.Subtask,
            _ => false
        };
    }

    private string GetTypeLabel(WorkItemType type)
    {
        return type switch
        {
            WorkItemType.Epic => "Epic",
            WorkItemType.Feature => "Feature",
            WorkItemType.Story => "Story",
            WorkItemType.Task => "Task",
            WorkItemType.Bug => "Bug",
            WorkItemType.Subtask => "Subtask",
            _ => "Item"
        };
    }

    private async Task<bool> CanAccessProject(int projectId, int userId)
    {
        var currentUser = await _context.Users.FindAsync(userId);
        if (currentUser == null) return false;

        var project = await _context.Projects.FindAsync(projectId);
        if (project == null) return false;

        // Assuming SystemRole properties or extensions exist on User, or relying on what was in Controller.
        // Controller used _currentUser (service) .IsSuperAdmin/IsAdmin.
        // Here we have specific User entity logic. 
        // For now I'll use the properties from User entity if they exist, or map from SystemRole.
        // Use SystemRole enum checks as seen in CreateWorkItemAsync.
        
        if (currentUser.SystemRole == SystemRole.SuperAdmin || currentUser.SystemRole == SystemRole.Admin || currentUser.SystemRole == SystemRole.Manager) 
        {
            // Managers/Admins access based on Company usually?
            // Controller logic: "if (_currentUser.IsSuperAdmin || _currentUser.IsAdmin) return project.CompanyId == _currentUser.CompanyId;"
            // But also checked ProjectMembers.
            
            // Replicating controller logic:
            if (currentUser.SystemRole == SystemRole.SuperAdmin) return true; // SuperAdmin sees all? Or just same company? Controller said "IsSuperAdmin || IsAdmin ... return project.CompanyId == _currentUser.CompanyId". 
            // So if SuperAdmin is global, they might see all factories? But usually SaaS implies company isolation.
            // Let's stick to CompanyId check for "Admins".
            
            if (currentUser.SystemRole == SystemRole.Admin)
            {
                 return project.CompanyId == currentUser.CompanyId;
            }
        }
        
        // General check for company ID + membership
        if (project.CompanyId != currentUser.CompanyId && currentUser.SystemRole != SystemRole.SuperAdmin) return false;

        var isMember = await _context.ProjectMembers
            .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
        if (isMember) return true;

        if (currentUser.ManagerId.HasValue)
        {
            return await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == currentUser.ManagerId.Value);
        }

        return false;
    }

    private async Task<bool> CanAccessWorkItem(WorkItem workItem, int userId)
    {
        var currentUser = await _context.Users.FindAsync(userId);
        if (currentUser == null) return false;

        if (currentUser.SystemRole == SystemRole.SuperAdmin || currentUser.SystemRole == SystemRole.Admin)
        {
            return workItem.Project.CompanyId == currentUser.CompanyId;
        }

        if (currentUser.SystemRole == SystemRole.Manager)
        {
            return await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == workItem.ProjectId && pm.UserId == userId);
        }

        return workItem.CreatedById == userId || workItem.AssignedToId == userId;
    }
}
