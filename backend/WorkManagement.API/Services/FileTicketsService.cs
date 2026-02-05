using Microsoft.EntityFrameworkCore;
using WorkManagement.API.Data;
using WorkManagement.API.DTOs;
using WorkManagement.API.Models;

namespace WorkManagement.API.Services;

public interface IFileTicketsService
{
    Task<ServiceResult<IEnumerable<FileTicketDto>>> GetFileTicketsAsync(int projectId, int? currentHolderId, FileTicketStatus? status, int userId);
    Task<ServiceResult<FileTicketDto>> GetFileTicketAsync(int projectId, int id, int userId);
    Task<ServiceResult<FileTicketDto>> CreateFileTicketAsync(int projectId, CreateFileTicketDto dto, int userId);
    Task<ServiceResult<FileTicketDto>> UpdateFileTicketAsync(int projectId, int id, UpdateFileTicketDto dto, int userId);
    Task<ServiceResult<FileTicketDto>> TransferFileTicketAsync(int projectId, int id, TransferFileTicketDto dto, int userId);
    Task<ServiceResult<FileTicketDto>> ReceiveFileTicketAsync(int projectId, int id, int userId);
    Task<ServiceResult<IEnumerable<FileTicketTransferDto>>> GetTransfersAsync(int projectId, int id, int userId);
    Task<ServiceResult<bool>> DeleteFileTicketAsync(int projectId, int id, int userId);
}

public class FileTicketsService : IFileTicketsService
{
    private readonly ApplicationDbContext _context;
    private readonly IActivityLogService _activityLog;

    public FileTicketsService(ApplicationDbContext context, IActivityLogService activityLog)
    {
        _context = context;
        _activityLog = activityLog;
    }

    public async Task<ServiceResult<IEnumerable<FileTicketDto>>> GetFileTicketsAsync(int projectId, int? currentHolderId, FileTicketStatus? status, int userId)
    {
        try
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return ServiceResult<IEnumerable<FileTicketDto>>.NotFound("Project not found");

            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null) return ServiceResult<IEnumerable<FileTicketDto>>.Unauthorized("User not found");

            if (currentUser.SystemRole != SystemRole.SuperAdmin && project.CompanyId != currentUser.CompanyId)
            {
                return ServiceResult<IEnumerable<FileTicketDto>>.Forbidden("Access denied");
            }

            IQueryable<FileTicket> query = _context.FileTickets
                .Include(f => f.CreatedBy)
                .Include(f => f.CurrentHolder)
                .Where(f => f.ProjectId == projectId);

            if (currentHolderId.HasValue)
            {
                query = query.Where(f => f.CurrentHolderId == currentHolderId);
            }

            if (status.HasValue)
            {
                query = query.Where(f => f.Status == status);
            }

            var tickets = await query
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => new FileTicketDto(
                    f.Id, f.TicketNumber, f.Title, f.Description, f.Type, f.Status, f.DueDate,
                    f.ProjectId, f.CreatedById, $"{f.CreatedBy.FirstName} {f.CreatedBy.LastName}",
                    f.CurrentHolderId, f.CurrentHolder != null ? $"{f.CurrentHolder.FirstName} {f.CurrentHolder.LastName}" : null,
                    f.CreatedAt
                ))
                .ToListAsync();

            return ServiceResult<IEnumerable<FileTicketDto>>.Success(tickets);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<FileTicketDto>>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<FileTicketDto>> GetFileTicketAsync(int projectId, int id, int userId)
    {
        try
        {
            var ticket = await _context.FileTickets
                .Include(f => f.Project)
                .Include(f => f.CreatedBy)
                .Include(f => f.CurrentHolder)
                .FirstOrDefaultAsync(f => f.Id == id && f.ProjectId == projectId);

            if (ticket == null) return ServiceResult<FileTicketDto>.NotFound("File ticket not found");

            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null) return ServiceResult<FileTicketDto>.Unauthorized("User not found");

            if (currentUser.SystemRole != SystemRole.SuperAdmin && ticket.Project.CompanyId != currentUser.CompanyId)
            {
                return ServiceResult<FileTicketDto>.Forbidden("Access denied");
            }

            return ServiceResult<FileTicketDto>.Success(new FileTicketDto(
                ticket.Id, ticket.TicketNumber, ticket.Title, ticket.Description, ticket.Type, ticket.Status, ticket.DueDate,
                ticket.ProjectId, ticket.CreatedById, $"{ticket.CreatedBy.FirstName} {ticket.CreatedBy.LastName}",
                ticket.CurrentHolderId, ticket.CurrentHolder != null ? $"{ticket.CurrentHolder.FirstName} {ticket.CurrentHolder.LastName}" : null,
                ticket.CreatedAt
            ));
        }
        catch (Exception ex)
        {
            return ServiceResult<FileTicketDto>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<FileTicketDto>> CreateFileTicketAsync(int projectId, CreateFileTicketDto dto, int userId)
    {
        try
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return ServiceResult<FileTicketDto>.NotFound("Project not found");

            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null) return ServiceResult<FileTicketDto>.Unauthorized("User not found");

            if (currentUser.SystemRole != SystemRole.SuperAdmin && project.CompanyId != currentUser.CompanyId)
            {
                return ServiceResult<FileTicketDto>.Forbidden("Access denied");
            }

            // Generate unique ticket number
            var year = DateTime.UtcNow.Year;
            var lastTicket = await _context.FileTickets
                .Where(f => f.TicketNumber.StartsWith($"FT-{year}"))
                .OrderByDescending(f => f.TicketNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastTicket != null)
            {
                var parts = lastTicket.TicketNumber.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out var num))
                {
                    nextNumber = num + 1;
                }
            }

            var ticket = new FileTicket
            {
                TicketNumber = $"FT-{year}-{nextNumber:D4}",
                Title = dto.Title,
                Description = dto.Description,
                Type = dto.Type ?? FileTicketType.Physical,
                Status = FileTicketStatus.Created,
                DueDate = dto.DueDate,
                ProjectId = projectId,
                CreatedById = userId,
                CurrentHolderId = dto.CurrentHolderId ?? userId
            };

            _context.FileTickets.Add(ticket);
            await _context.SaveChangesAsync();

            await _activityLog.LogAsync(userId, "Created", "FileTicket", ticket.Id,
                description: $"File ticket '{ticket.TicketNumber}' created", fileTicketId: ticket.Id);

            await _context.Entry(ticket).Reference(f => f.CreatedBy).LoadAsync();
            await _context.Entry(ticket).Reference(f => f.CurrentHolder).LoadAsync();

            return ServiceResult<FileTicketDto>.Created(new FileTicketDto(
                ticket.Id, ticket.TicketNumber, ticket.Title, ticket.Description, ticket.Type, ticket.Status, ticket.DueDate,
                ticket.ProjectId, ticket.CreatedById, $"{ticket.CreatedBy.FirstName} {ticket.CreatedBy.LastName}",
                ticket.CurrentHolderId, ticket.CurrentHolder != null ? $"{ticket.CurrentHolder.FirstName} {ticket.CurrentHolder.LastName}" : null,
                ticket.CreatedAt
            ));
        }
        catch (Exception ex)
        {
            return ServiceResult<FileTicketDto>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<FileTicketDto>> UpdateFileTicketAsync(int projectId, int id, UpdateFileTicketDto dto, int userId)
    {
        try
        {
            var ticket = await _context.FileTickets
                .Include(f => f.Project)
                .Include(f => f.CreatedBy)
                .Include(f => f.CurrentHolder)
                .FirstOrDefaultAsync(f => f.Id == id && f.ProjectId == projectId);

            if (ticket == null) return ServiceResult<FileTicketDto>.NotFound("File ticket not found");

            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null) return ServiceResult<FileTicketDto>.Unauthorized("User not found");

            if (currentUser.SystemRole != SystemRole.SuperAdmin && ticket.Project.CompanyId != currentUser.CompanyId)
            {
                return ServiceResult<FileTicketDto>.Forbidden("Access denied");
            }

            var oldStatus = ticket.Status;

            if (dto.Title != null) ticket.Title = dto.Title;
            if (dto.Description != null) ticket.Description = dto.Description;
            if (dto.Status.HasValue) ticket.Status = dto.Status.Value;
            if (dto.DueDate.HasValue) ticket.DueDate = dto.DueDate;
            ticket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            if (dto.Status.HasValue && dto.Status.Value != oldStatus)
            {
                await _activityLog.LogAsync(userId, "StatusChanged", "FileTicket", ticket.Id,
                    oldValue: oldStatus.ToString(), newValue: dto.Status.Value.ToString(),
                    description: $"Status changed from {oldStatus} to {dto.Status.Value}",
                    fileTicketId: ticket.Id);
            }

            return ServiceResult<FileTicketDto>.Success(new FileTicketDto(
                ticket.Id, ticket.TicketNumber, ticket.Title, ticket.Description, ticket.Type, ticket.Status, ticket.DueDate,
                ticket.ProjectId, ticket.CreatedById, $"{ticket.CreatedBy.FirstName} {ticket.CreatedBy.LastName}",
                ticket.CurrentHolderId, ticket.CurrentHolder != null ? $"{ticket.CurrentHolder.FirstName} {ticket.CurrentHolder.LastName}" : null,
                ticket.CreatedAt
            ));
        }
        catch (Exception ex)
        {
            return ServiceResult<FileTicketDto>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<FileTicketDto>> TransferFileTicketAsync(int projectId, int id, TransferFileTicketDto dto, int userId)
    {
        try
        {
            var ticket = await _context.FileTickets
                .Include(f => f.Project)
                .Include(f => f.CreatedBy)
                .Include(f => f.CurrentHolder)
                .FirstOrDefaultAsync(f => f.Id == id && f.ProjectId == projectId);

            if (ticket == null) return ServiceResult<FileTicketDto>.NotFound("File ticket not found");

            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null) return ServiceResult<FileTicketDto>.Unauthorized("User not found");

            if (currentUser.SystemRole != SystemRole.SuperAdmin && ticket.Project.CompanyId != currentUser.CompanyId)
            {
                return ServiceResult<FileTicketDto>.Forbidden("Access denied");
            }

            var toUser = await _context.Users.FindAsync(dto.ToUserId);
            if (toUser == null || toUser.CompanyId != ticket.Project.CompanyId)
            {
                return ServiceResult<FileTicketDto>.BadRequest("Target user not found or not in same company");
            }

            var transfer = new FileTicketTransfer
            {
                FileTicketId = ticket.Id,
                FromUserId = ticket.CurrentHolderId ?? userId,
                ToUserId = dto.ToUserId,
                Notes = dto.Notes,
                TransferredAt = DateTime.UtcNow
            };

            _context.FileTicketTransfers.Add(transfer);

            var oldHolder = ticket.CurrentHolder != null ? $"{ticket.CurrentHolder.FirstName} {ticket.CurrentHolder.LastName}" : "Unknown";
            ticket.CurrentHolderId = dto.ToUserId;
            ticket.Status = FileTicketStatus.InTransit;
            ticket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _activityLog.LogAsync(userId, "Transferred", "FileTicket", ticket.Id,
                oldValue: oldHolder, newValue: $"{toUser.FirstName} {toUser.LastName}",
                description: $"File transferred from {oldHolder} to {toUser.FirstName} {toUser.LastName}",
                fileTicketId: ticket.Id);

            await _context.Entry(ticket).Reference(f => f.CurrentHolder).LoadAsync();

            return ServiceResult<FileTicketDto>.Success(new FileTicketDto(
                ticket.Id, ticket.TicketNumber, ticket.Title, ticket.Description, ticket.Type, ticket.Status, ticket.DueDate,
                ticket.ProjectId, ticket.CreatedById, $"{ticket.CreatedBy.FirstName} {ticket.CreatedBy.LastName}",
                ticket.CurrentHolderId, ticket.CurrentHolder != null ? $"{ticket.CurrentHolder.FirstName} {ticket.CurrentHolder.LastName}" : null,
                ticket.CreatedAt
            ));
        }
        catch (Exception ex)
        {
            return ServiceResult<FileTicketDto>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<FileTicketDto>> ReceiveFileTicketAsync(int projectId, int id, int userId)
    {
        try
        {
            var ticket = await _context.FileTickets
                .Include(f => f.Project)
                .Include(f => f.CreatedBy)
                .Include(f => f.CurrentHolder)
                .FirstOrDefaultAsync(f => f.Id == id && f.ProjectId == projectId);

            if (ticket == null) return ServiceResult<FileTicketDto>.NotFound("File ticket not found");

            if (ticket.CurrentHolderId != userId)
            {
                return ServiceResult<FileTicketDto>.Forbidden("Access denied");
            }

            var lastTransfer = await _context.FileTicketTransfers
                .Where(t => t.FileTicketId == ticket.Id && t.ToUserId == userId && t.ReceivedAt == null)
                .OrderByDescending(t => t.TransferredAt)
                .FirstOrDefaultAsync();

            if (lastTransfer != null)
            {
                lastTransfer.ReceivedAt = DateTime.UtcNow;
            }

            ticket.Status = FileTicketStatus.Received;
            ticket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _activityLog.LogAsync(userId, "Received", "FileTicket", ticket.Id,
                description: $"File ticket received", fileTicketId: ticket.Id);

            return ServiceResult<FileTicketDto>.Success(new FileTicketDto(
                ticket.Id, ticket.TicketNumber, ticket.Title, ticket.Description, ticket.Type, ticket.Status, ticket.DueDate,
                ticket.ProjectId, ticket.CreatedById, $"{ticket.CreatedBy.FirstName} {ticket.CreatedBy.LastName}",
                ticket.CurrentHolderId, ticket.CurrentHolder != null ? $"{ticket.CurrentHolder.FirstName} {ticket.CurrentHolder.LastName}" : null,
                ticket.CreatedAt
            ));
        }
        catch (Exception ex)
        {
            return ServiceResult<FileTicketDto>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<IEnumerable<FileTicketTransferDto>>> GetTransfersAsync(int projectId, int id, int userId)
    {
        try
        {
            var ticket = await _context.FileTickets
                .Include(f => f.Project)
                .FirstOrDefaultAsync(f => f.Id == id && f.ProjectId == projectId);

            if (ticket == null) return ServiceResult<IEnumerable<FileTicketTransferDto>>.NotFound("File ticket not found");

            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null) return ServiceResult<IEnumerable<FileTicketTransferDto>>.Unauthorized("User not found");

            if (currentUser.SystemRole != SystemRole.SuperAdmin && ticket.Project.CompanyId != currentUser.CompanyId)
            {
                return ServiceResult<IEnumerable<FileTicketTransferDto>>.Forbidden("Access denied");
            }

            var transfers = await _context.FileTicketTransfers
                .Include(t => t.FromUser)
                .Include(t => t.ToUser)
                .Where(t => t.FileTicketId == id)
                .OrderByDescending(t => t.TransferredAt)
                .Select(t => new FileTicketTransferDto(
                    t.Id, t.FileTicketId,
                    t.FromUserId, $"{t.FromUser.FirstName} {t.FromUser.LastName}",
                    t.ToUserId, $"{t.ToUser.FirstName} {t.ToUser.LastName}",
                    t.TransferredAt, t.ReceivedAt, t.Notes
                ))
                .ToListAsync();

            return ServiceResult<IEnumerable<FileTicketTransferDto>>.Success(transfers);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<FileTicketTransferDto>>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<bool>> DeleteFileTicketAsync(int projectId, int id, int userId)
    {
        try
        {
            var ticket = await _context.FileTickets
                .Include(f => f.Project)
                .FirstOrDefaultAsync(f => f.Id == id && f.ProjectId == projectId);

            if (ticket == null) return ServiceResult<bool>.NotFound("File ticket not found");

            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null) return ServiceResult<bool>.Unauthorized("User not found");

            if (currentUser.SystemRole != SystemRole.SuperAdmin && ticket.Project.CompanyId != currentUser.CompanyId)
            {
                return ServiceResult<bool>.Forbidden("Access denied");
            }

            ticket.IsDeleted = true;
            ticket.DeletedAt = DateTime.UtcNow;
            ticket.DeletedBy = userId;

            await _context.SaveChangesAsync();

            await _activityLog.LogAsync(userId, "Deleted", "FileTicket", ticket.Id,
                description: $"File ticket '{ticket.TicketNumber}' deleted", fileTicketId: ticket.Id);

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Failure(ex.Message, ex);
        }
    }
}
