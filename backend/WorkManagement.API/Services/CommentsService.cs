using Microsoft.EntityFrameworkCore;
using WorkManagement.API.Data;
using WorkManagement.API.DTOs;
using WorkManagement.API.Models;

namespace WorkManagement.API.Services;

public interface ICommentsService
{
    Task<ServiceResult<IEnumerable<CommentDto>>> GetCommentsAsync(int projectId, int workItemId, int userId);
    Task<ServiceResult<CommentDto>> CreateCommentAsync(int projectId, int workItemId, CreateCommentDto dto, int userId);
    Task<ServiceResult<bool>> DeleteCommentAsync(int projectId, int workItemId, int commentId, int userId);
}

public class CommentsService : ICommentsService
{
    private readonly ApplicationDbContext _context;
    private readonly IActivityLogService _activityLog;

    public CommentsService(ApplicationDbContext context, IActivityLogService activityLog)
    {
        _context = context;
        _activityLog = activityLog;
    }

    public async Task<ServiceResult<IEnumerable<CommentDto>>> GetCommentsAsync(int projectId, int workItemId, int userId)
    {
        try
        {
            var workItem = await _context.WorkItems
                .Include(w => w.Project)
                .FirstOrDefaultAsync(w => w.Id == workItemId && w.ProjectId == projectId);

            if (workItem == null) return ServiceResult<IEnumerable<CommentDto>>.NotFound("Work item not found");

            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null) return ServiceResult<IEnumerable<CommentDto>>.Unauthorized("User not found");

            if (currentUser.SystemRole != SystemRole.SuperAdmin && workItem.Project.CompanyId != currentUser.CompanyId)
            {
                return ServiceResult<IEnumerable<CommentDto>>.Forbidden("Access denied");
            }

            var comments = await _context.Comments
                .Include(c => c.Author)
                .Where(c => c.WorkItemId == workItemId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CommentDto(
                    c.Id, c.Content, c.WorkItemId, c.AuthorId,
                    c.Author.FirstName + " " + c.Author.LastName, c.CreatedAt))
                .ToListAsync();

            return ServiceResult<IEnumerable<CommentDto>>.Success(comments);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<CommentDto>>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<CommentDto>> CreateCommentAsync(int projectId, int workItemId, CreateCommentDto dto, int userId)
    {
        try
        {
            var workItem = await _context.WorkItems
                .Include(w => w.Project)
                .FirstOrDefaultAsync(w => w.Id == workItemId && w.ProjectId == projectId);

            if (workItem == null) return ServiceResult<CommentDto>.NotFound("Work item not found");

            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null) return ServiceResult<CommentDto>.Unauthorized("User not found");

            if (currentUser.SystemRole != SystemRole.SuperAdmin && workItem.Project.CompanyId != currentUser.CompanyId)
            {
                return ServiceResult<CommentDto>.Forbidden("Access denied");
            }

            var comment = new Comment
            {
                Content = dto.Content,
                WorkItemId = workItemId,
                AuthorId = userId
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            await _activityLog.LogAsync(userId, "Commented", "WorkItem", workItemId,
                workItemId: workItemId, description: "Added a comment");

            return ServiceResult<CommentDto>.Created(new CommentDto(
                comment.Id, comment.Content, comment.WorkItemId, comment.AuthorId,
                currentUser.FirstName + " " + currentUser.LastName, comment.CreatedAt));
        }
        catch (Exception ex)
        {
            return ServiceResult<CommentDto>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<bool>> DeleteCommentAsync(int projectId, int workItemId, int commentId, int userId)
    {
        try
        {
            var comment = await _context.Comments
                .Include(c => c.WorkItem)
                .ThenInclude(w => w.Project)
                .FirstOrDefaultAsync(c => c.Id == commentId && c.WorkItemId == workItemId);

            if (comment == null) return ServiceResult<bool>.NotFound("Comment not found");

            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null) return ServiceResult<bool>.Unauthorized("User not found");

            // Only author or admins can delete
            if (comment.AuthorId != userId && currentUser.SystemRole != SystemRole.SuperAdmin)
            {
                var membership = await _context.ProjectMembers
                    .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
                
                if (membership == null || membership.Role < ProjectRole.Manager)
                {
                    return ServiceResult<bool>.Forbidden("Access denied");
                }
            }

            comment.IsDeleted = true;
            comment.DeletedAt = DateTime.UtcNow;
            comment.DeletedBy = userId;

            await _context.SaveChangesAsync();

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Failure(ex.Message, ex);
        }
    }
}
