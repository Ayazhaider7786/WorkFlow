using WorkManagement.API.Models;

namespace WorkManagement.API.DTOs;

// Auth DTOs
public record LoginRequest(string Email, string Password);
public record RegisterRequest(string Email, string Password, string FirstName, string LastName, string? CompanyName);
public record AuthResponse(string Token, string RefreshToken, UserDto User);
public record RefreshTokenRequest(string RefreshToken);

// User DTOs
public record UserDto(int Id, string Email, string FirstName, string LastName, string? Phone, SystemRole SystemRole, int? CompanyId, string? CompanyName, int? ManagerId, string? ManagerName);
public record UserWithProjectsDto(int Id, string Email, string FirstName, string LastName, string? Phone, SystemRole SystemRole, int? CompanyId, string? CompanyName, int? ManagerId, string? ManagerName, IEnumerable<UserProjectAssignmentDto> ProjectAssignments);
public record UserProjectAssignmentDto(int ProjectId, string ProjectName, string ProjectKey, ProjectRole Role);
public record CreateUserDto(string Email, string Password, string FirstName, string LastName, string? Phone, SystemRole? Role, int? ManagerId);
public record UpdateUserDto(string? FirstName, string? LastName, string? Phone, SystemRole? Role, int? ManagerId);
public record TransferSuperAdminDto(int NewSuperAdminId);

// Company DTOs
public record CompanyDto(int Id, string Name, string? Description, string? Logo, bool IsActive, DateTime CreatedAt);
public record CreateCompanyDto(string Name, string? Description, string? Logo);
public record UpdateCompanyDto(string? Name, string? Description, string? Logo, bool? IsActive);

// Project DTOs
public record ProjectDto(int Id, string Name, string? Description, string Key, DateTime? StartDate, DateTime? EndDate, bool IsActive, int CompanyId, DateTime CreatedAt, IEnumerable<ProjectManagerDto>? Managers);
public record ProjectManagerDto(int UserId, string UserName);
public record CreateProjectDto(string Name, string? Description, string Key, DateTime? StartDate, DateTime? EndDate, int ManagerId); // Manager required
public record UpdateProjectDto(string? Name, string? Description, DateTime? StartDate, DateTime? EndDate, bool? IsActive);

// Project Member DTOs
public record ProjectMemberDto(int Id, int ProjectId, int UserId, string UserName, string UserEmail, ProjectRole Role);
public record AddProjectMemberDto(int UserId, ProjectRole Role);
public record UpdateProjectMemberDto(ProjectRole Role);
public record BulkAddProjectMembersDto(IEnumerable<int> UserIds, ProjectRole Role);

// User Selection DTO for project assignment
public record UserSelectionDto(int Id, string Email, string FirstName, string LastName, SystemRole SystemRole, bool IsAssignedToProject, ProjectRole? ProjectRole);

// Workflow Status DTOs
public record WorkflowStatusDto(int Id, string Name, string? Description, int Order, string Color, bool IsCore, CoreStatusType? CoreType, int ProjectId);
public record CreateWorkflowStatusDto(string Name, string? Description, int Order, string? Color);
public record UpdateWorkflowStatusDto(string? Name, string? Description, int? Order, string? Color);

// Work Item DTOs
public record WorkItemDto(
    int Id, string Title, string? Description, WorkItemType Type, Priority Priority, DateTime? DueDate,
    decimal? EstimatedHours, decimal? ActualHours, int ItemNumber, string ItemKey,
    int ProjectId, int StatusId, string StatusName, string StatusColor,
    int? AssignedToId, string? AssignedToName, int? SprintId, string? SprintName,
    bool IsInBacklog, int? QueueOrder, int CreatedById, string CreatedByName, DateTime CreatedAt,
    int CommentCount, int AttachmentCount, int? ParentId, string? ParentKey, int ChildCount);
public record CreateWorkItemDto(string Title, string? Description, WorkItemType? Type, Priority? Priority, DateTime? DueDate, decimal? EstimatedHours, int? AssignedToId, int? SprintId, int? ParentId);
public record UpdateWorkItemDto(string? Title, string? Description, WorkItemType? Type, Priority? Priority, DateTime? DueDate, decimal? EstimatedHours, decimal? ActualHours, int? StatusId, int? AssignedToId, int? SprintId, bool? IsInBacklog, int? QueueOrder, int? ParentId);

// Sprint DTOs
public record SprintDto(int Id, string Name, string? Goal, DateTime StartDate, DateTime EndDate, SprintStatus Status, int ProjectId, int WorkItemCount, DateTime CreatedAt);
public record CreateSprintDto(string Name, string? Goal, DateTime StartDate, DateTime EndDate);
public record UpdateSprintDto(string? Name, string? Goal, DateTime? StartDate, DateTime? EndDate, SprintStatus? Status);

// File Ticket DTOs
public record FileTicketDto(
    int Id, string TicketNumber, string Title, string? Description, FileTicketType Type,
    FileTicketStatus Status, DateTime? DueDate, int ProjectId, int CreatedById,
    string CreatedByName, int? CurrentHolderId, string? CurrentHolderName, DateTime CreatedAt);
public record CreateFileTicketDto(string Title, string? Description, FileTicketType? Type, DateTime? DueDate, int? CurrentHolderId);
public record UpdateFileTicketDto(string? Title, string? Description, FileTicketStatus? Status, DateTime? DueDate);
public record TransferFileTicketDto(int ToUserId, string? Notes);

// File Ticket Transfer DTOs
public record FileTicketTransferDto(int Id, int FileTicketId, int FromUserId, string FromUserName, int ToUserId, string ToUserName, DateTime TransferredAt, DateTime? ReceivedAt, string? Notes);

// Activity Log DTOs
public record ActivityLogDto(int Id, string Action, string EntityType, int EntityId, string? OldValue, string? NewValue, string? Description, DateTime Timestamp, int UserId, string UserName, int? ProjectId, string? ProjectName);
public record ActivityLogQueryDto(DateTime? StartDate, DateTime? EndDate, int? UserId, int? ProjectId, int Page = 1, int PageSize = 20);

// Dashboard DTOs
public record DashboardDto(
    int TotalWorkItems, int CompletedWorkItems, int InProgressWorkItems, int BlockedWorkItems,
    int TotalSprints, int ActiveSprints,
    int TotalFileTickets, int PendingFileTickets,
    IEnumerable<WorkItemsByStatusDto> WorkItemsByStatus,
    IEnumerable<WorkItemsByPriorityDto> WorkItemsByPriority,
    IEnumerable<UserWorkloadDto> UserWorkload);
public record WorkItemsByStatusDto(string Status, string Color, int Count);
public record WorkItemsByPriorityDto(string Priority, int Count);
public record UserWorkloadDto(int UserId, string UserName, int AssignedItems, int CompletedItems);

// Queue DTOs
public record UserQueueDto(int UserId, string UserName, IEnumerable<WorkItemDto> QueueItems);

// Comment DTOs
public record CommentDto(int Id, string Content, int WorkItemId, int AuthorId, string AuthorName, DateTime CreatedAt);
public record CreateCommentDto(string Content);

// Attachment DTOs
public record AttachmentDto(int Id, string FileName, string FilePath, string ContentType, long FileSize, int WorkItemId, int UploadedById, string UploadedByName, DateTime CreatedAt);

// Pagination DTOs
public record PaginationParams(int Page = 1, int PageSize = 20);
public record DateRangeParams(DateTime? StartDate, DateTime? EndDate);
public record FileDownloadDto(byte[] FileBytes, string ContentType, string FileName);
