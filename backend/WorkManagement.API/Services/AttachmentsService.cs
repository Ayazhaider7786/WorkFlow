using Microsoft.EntityFrameworkCore;
using WorkManagement.API.Data;
using WorkManagement.API.DTOs;
using WorkManagement.API.Models;

namespace WorkManagement.API.Services;

public interface IAttachmentsService
{
    Task<ServiceResult<IEnumerable<AttachmentDto>>> GetAttachmentsAsync(int projectId, int workItemId, int userId);
    Task<ServiceResult<AttachmentDto>> UploadAttachmentAsync(int projectId, int workItemId, IFormFile file, int userId);
    Task<ServiceResult<bool>> DeleteAttachmentAsync(int projectId, int workItemId, int attachmentId, int userId);
    Task<ServiceResult<FileDownloadDto>> DownloadAttachmentAsync(int projectId, int workItemId, int attachmentId);
}

public class AttachmentsService : IAttachmentsService
{
    private readonly ApplicationDbContext _context;
    private readonly IActivityLogService _activityLog;
    private readonly IWebHostEnvironment _env;
    private readonly ICurrentUserService _currentUserService;

    private const long MaxImageSize = 10 * 1024 * 1024; // 10 MB
    private const long MaxVideoSize = 70 * 1024 * 1024; // 70 MB
    private const long MaxDocSize = 25 * 1024 * 1024; // 25 MB

    public AttachmentsService(ApplicationDbContext context, IActivityLogService activityLog, IWebHostEnvironment env, ICurrentUserService currentUserService)
    {
        _context = context;
        _activityLog = activityLog;
        _env = env;
        _currentUserService = currentUserService;
    }

    public async Task<ServiceResult<IEnumerable<AttachmentDto>>> GetAttachmentsAsync(int projectId, int workItemId, int userId)
    {
        try
        {
            var workItem = await _context.WorkItems
                .Include(w => w.Project)
                .FirstOrDefaultAsync(w => w.Id == workItemId && w.ProjectId == projectId);

            if (workItem == null) return ServiceResult<IEnumerable<AttachmentDto>>.NotFound("Work item not found");

            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null) return ServiceResult<IEnumerable<AttachmentDto>>.Unauthorized("User not found");

            if (currentUser.SystemRole != SystemRole.SuperAdmin && workItem.Project.CompanyId != currentUser.CompanyId)
            {
                return ServiceResult<IEnumerable<AttachmentDto>>.Forbidden("Access denied");
            }

            var attachments = await _context.Attachments
                .Include(a => a.UploadedBy)
                .Where(a => a.WorkItemId == workItemId)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new AttachmentDto(
                    a.Id, a.FileName, a.FilePath, a.ContentType, a.FileSize, a.WorkItemId, a.UploadedById,
                    a.UploadedBy.FirstName + " " + a.UploadedBy.LastName, a.CreatedAt))
                .ToListAsync();

            return ServiceResult<IEnumerable<AttachmentDto>>.Success(attachments);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<AttachmentDto>>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<AttachmentDto>> UploadAttachmentAsync(int projectId, int workItemId, IFormFile file, int userId)
    {
        try
        {
            var workItem = await _context.WorkItems
                .Include(w => w.Project)
                .FirstOrDefaultAsync(w => w.Id == workItemId && w.ProjectId == projectId);

            if (workItem == null) return ServiceResult<AttachmentDto>.NotFound("Work item not found");

            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null) return ServiceResult<AttachmentDto>.Unauthorized("User not found");

            if (currentUser.SystemRole != SystemRole.SuperAdmin && workItem.Project.CompanyId != currentUser.CompanyId)
            {
                return ServiceResult<AttachmentDto>.Forbidden("Access denied");
            }

            if (file == null || file.Length == 0)
            {
                return ServiceResult<AttachmentDto>.BadRequest("No file uploaded");
            }

            var isImage = file.ContentType.StartsWith("image/");
            var isVideo = file.ContentType.StartsWith("video/");
            var isDoc = file.ContentType.Contains("pdf") || 
                        file.ContentType.Contains("word") || 
                        file.ContentType.Contains("document") ||
                        file.ContentType.Contains("excel") ||
                        file.ContentType.Contains("spreadsheet") ||
                        file.ContentType == "application/msword" ||
                        file.ContentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document" ||
                        file.ContentType == "application/vnd.ms-excel" ||
                        file.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            if (isImage && file.Length > MaxImageSize)
            {
                return ServiceResult<AttachmentDto>.BadRequest("Image files must be less than 10 MB");
            }

            if (isVideo && file.Length > MaxVideoSize)
            {
                return ServiceResult<AttachmentDto>.BadRequest("Video files must be less than 70 MB");
            }

            if (isDoc && file.Length > MaxDocSize)
            {
                return ServiceResult<AttachmentDto>.BadRequest("Document files must be less than 25 MB");
            }

            // Create uploads directory
            var uploadsPath = Path.Combine(_env.ContentRootPath, "uploads", projectId.ToString());
            Directory.CreateDirectory(uploadsPath);

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var attachment = new Attachment
            {
                FileName = file.FileName,
                FilePath = $"/uploads/{projectId}/{fileName}",
                ContentType = file.ContentType,
                FileSize = file.Length,
                WorkItemId = workItemId,
                UploadedById = userId
            };

            _context.Attachments.Add(attachment);
            await _context.SaveChangesAsync();

            await _activityLog.LogAsync(userId, "Attached", "WorkItem", workItemId,
                workItemId: workItemId, description: $"Attached file '{file.FileName}'");

            return ServiceResult<AttachmentDto>.Created(new AttachmentDto(
                attachment.Id, attachment.FileName, attachment.FilePath, attachment.ContentType,
                attachment.FileSize, attachment.WorkItemId, attachment.UploadedById,
                currentUser.FirstName + " " + currentUser.LastName, attachment.CreatedAt));
        }
        catch (Exception ex)
        {
            return ServiceResult<AttachmentDto>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<bool>> DeleteAttachmentAsync(int projectId, int workItemId, int attachmentId, int userId)
    {
        try
        {
            var attachment = await _context.Attachments
                .Include(a => a.WorkItem)
                .ThenInclude(w => w.Project)
                .FirstOrDefaultAsync(a => a.Id == attachmentId && a.WorkItemId == workItemId);

            if (attachment == null) return ServiceResult<bool>.NotFound("Attachment not found");

            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null) return ServiceResult<bool>.Unauthorized("User not found");

            if (attachment.UploadedById != userId && currentUser.SystemRole != SystemRole.SuperAdmin)
            {
                var membership = await _context.ProjectMembers
                    .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
                
                if (membership == null || membership.Role < ProjectRole.Manager)
                {
                    return ServiceResult<bool>.Forbidden("Access denied");
                }
            }

            // Delete physical file
            var fullPath = Path.Combine(_env.ContentRootPath, attachment.FilePath.TrimStart('/'));
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            var fileName = attachment.FileName;
            attachment.IsDeleted = true;
            attachment.DeletedAt = DateTime.UtcNow;
            attachment.DeletedBy = userId;

            await _context.SaveChangesAsync();

            await _activityLog.LogAsync(userId, "Removed", "WorkItem", workItemId,
                workItemId: workItemId, description: $"Removed attachment '{fileName}'");

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<FileDownloadDto>> DownloadAttachmentAsync(int projectId, int workItemId, int attachmentId)
    {
        try
        {
            var attachment = await _context.Attachments
                .Include(a => a.WorkItem)
                .ThenInclude(w => w.Project)
                .FirstOrDefaultAsync(a => a.Id == attachmentId && a.WorkItemId == workItemId);

            if (attachment == null) return ServiceResult<FileDownloadDto>.NotFound("Attachment not found");

            var fullPath = Path.Combine(_env.ContentRootPath, attachment.FilePath.TrimStart('/'));
            if (!File.Exists(fullPath))
            {
                return ServiceResult<FileDownloadDto>.NotFound("File not found");
            }

            var bytes = await File.ReadAllBytesAsync(fullPath);
            return ServiceResult<FileDownloadDto>.Success(new FileDownloadDto(bytes, attachment.ContentType, attachment.FileName));
        }
        catch (Exception ex)
        {
            return ServiceResult<FileDownloadDto>.Failure(ex.Message, ex);
        }
    }
}
