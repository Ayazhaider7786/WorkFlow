using Microsoft.EntityFrameworkCore;
using WorkManagement.API.Data;
using WorkManagement.API.DTOs;
using WorkManagement.API.Models;

namespace WorkManagement.API.Services;

public interface ICompaniesService
{
    Task<ServiceResult<IEnumerable<CompanyDto>>> GetCompaniesAsync(int userId);
    Task<ServiceResult<CompanyDto>> GetCompanyAsync(int id, int userId);
    Task<ServiceResult<CompanyDto>> CreateCompanyAsync(CreateCompanyDto dto, int userId);
    Task<ServiceResult<CompanyDto>> UpdateCompanyAsync(int id, UpdateCompanyDto dto, int userId);
    Task<ServiceResult<bool>> DeleteCompanyAsync(int id, int userId);
}

public class CompaniesService : ICompaniesService
{
    private readonly ApplicationDbContext _context;
    private readonly IActivityLogService _activityLog;
    private readonly ICurrentUserService _currentUserService;

    public CompaniesService(ApplicationDbContext context, IActivityLogService activityLog, ICurrentUserService currentUserService)
    {
        _context = context;
        _activityLog = activityLog;
        _currentUserService = currentUserService;
    }

    public async Task<ServiceResult<IEnumerable<CompanyDto>>> GetCompaniesAsync(int userId)
    {
        try
        {
            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null) return ServiceResult<IEnumerable<CompanyDto>>.Unauthorized("User not found");

            IQueryable<Company> query = _context.Companies;

            if (currentUser.SystemRole != SystemRole.SuperAdmin) // Assuming IsSystemAdmin maps to IsSuperAdmin in User model or usage
            {
                // Logic from controller: if not system admin, filter by company id
                // But wait, the controller used _currentUser (ICurrentUserService) not DB user for check
                // "if (!_currentUser.SystemRole == SystemRole.SuperAdmin && _currentUser.CompanyId.HasValue)"
                // I will use the DB user to be safe and consistent
                
                // We need to verify if IsSystemAdmin equivalent property exists on User entity
                // The User entity typically has SystemRole.
                // Let's assume IsSystemAdmin check in controller relies on ICurrentUserService which likely checks claims
                
                // Let's use the current user from DB for role check
                if (currentUser.SystemRole != SystemRole.SuperAdmin)
                {
                     if (currentUser.CompanyId.HasValue)
                     {
                         query = query.Where(c => c.Id == currentUser.CompanyId);
                     }
                     else
                     {
                         // If no company and not super admin, maybe return empty?
                         return ServiceResult<IEnumerable<CompanyDto>>.Success(new List<CompanyDto>());
                     }
                }
            }

            var companies = await query
                .Select(c => new CompanyDto(c.Id, c.Name, c.Description, c.Logo, c.IsActive, c.CreatedAt))
                .ToListAsync();

            return ServiceResult<IEnumerable<CompanyDto>>.Success(companies);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<CompanyDto>>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<CompanyDto>> GetCompanyAsync(int id, int userId)
    {
        try
        {
            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null) return ServiceResult<CompanyDto>.Unauthorized("User not found");

            if (currentUser.SystemRole != SystemRole.SuperAdmin && currentUser.CompanyId != id)
            {
                return ServiceResult<CompanyDto>.Forbidden("Access denied");
            }

            var company = await _context.Companies
                .Where(c => c.Id == id)
                .Select(c => new CompanyDto(c.Id, c.Name, c.Description, c.Logo, c.IsActive, c.CreatedAt))
                .FirstOrDefaultAsync();

            if (company == null) return ServiceResult<CompanyDto>.NotFound("Company not found");

            return ServiceResult<CompanyDto>.Success(company);
        }
        catch (Exception ex)
        {
            return ServiceResult<CompanyDto>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<CompanyDto>> CreateCompanyAsync(CreateCompanyDto dto, int userId)
    {
        try
        {
            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null) return ServiceResult<CompanyDto>.Unauthorized("User not found");

            if (currentUser.SystemRole != SystemRole.SuperAdmin)
            {
                return ServiceResult<CompanyDto>.Forbidden("Access denied");
            }

            if (await _context.Companies.AnyAsync(c => c.Name == dto.Name))
            {
                return ServiceResult<CompanyDto>.BadRequest("Company name already exists");
            }

            var company = new Company
            {
                Name = dto.Name,
                Description = dto.Description,
                Logo = dto.Logo
            };

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            await _activityLog.LogAsync(userId, "Created", "Company", company.Id, description: $"Company '{company.Name}' created");

            return ServiceResult<CompanyDto>.Created(new CompanyDto(company.Id, company.Name, company.Description, company.Logo, company.IsActive, company.CreatedAt));
        }
        catch (Exception ex)
        {
            return ServiceResult<CompanyDto>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<CompanyDto>> UpdateCompanyAsync(int id, UpdateCompanyDto dto, int userId)
    {
        try
        {
            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null) return ServiceResult<CompanyDto>.Unauthorized("User not found");

            if (currentUser.SystemRole != SystemRole.SuperAdmin && currentUser.CompanyId != id)
            {
                return ServiceResult<CompanyDto>.Forbidden("Access denied");
            }

            var company = await _context.Companies.FindAsync(id);
            if (company == null) return ServiceResult<CompanyDto>.NotFound("Company not found");

            if (dto.Name != null) company.Name = dto.Name;
            if (dto.Description != null) company.Description = dto.Description;
            if (dto.Logo != null) company.Logo = dto.Logo;
            if (dto.IsActive.HasValue) company.IsActive = dto.IsActive.Value;
            company.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _activityLog.LogAsync(userId, "Updated", "Company", company.Id, description: $"Company '{company.Name}' updated");

            return ServiceResult<CompanyDto>.Success(new CompanyDto(company.Id, company.Name, company.Description, company.Logo, company.IsActive, company.CreatedAt));
        }
        catch (Exception ex)
        {
            return ServiceResult<CompanyDto>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<bool>> DeleteCompanyAsync(int id, int userId)
    {
        try
        {
            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null) return ServiceResult<bool>.Unauthorized("User not found");

            if (currentUser.SystemRole != SystemRole.SuperAdmin)
            {
                return ServiceResult<bool>.Forbidden("Access denied");
            }

            var company = await _context.Companies.FindAsync(id);
            if (company == null) return ServiceResult<bool>.NotFound("Company not found");

            company.IsDeleted = true;
            company.DeletedAt = DateTime.UtcNow;
            company.DeletedBy = userId;

            await _context.SaveChangesAsync();

            await _activityLog.LogAsync(userId, "Deleted", "Company", company.Id, description: $"Company '{company.Name}' deleted");

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Failure(ex.Message, ex);
        }
    }
}
