using Microsoft.EntityFrameworkCore;
using WorkManagement.API.Data;
using WorkManagement.API.DTOs;
using WorkManagement.API.Models;

namespace WorkManagement.API.Services;

public interface IAuthService
{
    Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request);
    Task<ServiceResult<AuthResponse>> RegisterAsync(RegisterRequest request);
    Task<ServiceResult<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request);
    Task<ServiceResult<bool>> LogoutAsync(RefreshTokenRequest request);
}

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IJwtService _jwtService;

    public AuthService(ApplicationDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.Company)
                .Include(u => u.Manager)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return ServiceResult<AuthResponse>.Unauthorized("Invalid email or password");
            }

            // Check if user is soft deleted
            if (user.IsDeleted)
            {
                return ServiceResult<AuthResponse>.Unauthorized("Account is deactivated");
            }

            var token = _jwtService.GenerateToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Store refresh token in database
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            var userDto = new UserDto(
                user.Id, user.Email, user.FirstName, user.LastName,
                user.Phone, user.SystemRole, user.CompanyId, user.Company?.Name,
                user.ManagerId, user.Manager != null ? user.Manager.FirstName + " " + user.Manager.LastName : null
            );

            return ServiceResult<AuthResponse>.Success(new AuthResponse(token, refreshToken, userDto));
        }
        catch (Exception ex)
        {
            return ServiceResult<AuthResponse>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        try
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return ServiceResult<AuthResponse>.BadRequest("Email already exists");
            }

            // Company name is now required
            if (string.IsNullOrWhiteSpace(request.CompanyName))
            {
                return ServiceResult<AuthResponse>.BadRequest("Company name is required");
            }

            // Check if company name already exists
            if (await _context.Companies.AnyAsync(c => c.Name == request.CompanyName))
            {
                return ServiceResult<AuthResponse>.BadRequest("Company name already exists");
            }

            // Create company
            var company = new Company { Name = request.CompanyName };
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            // Registering user is always SuperAdmin of the new company
            var user = new User
            {
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FirstName = request.FirstName,
                LastName = request.LastName,
                SystemRole = SystemRole.SuperAdmin,
                CompanyId = company.Id
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = _jwtService.GenerateToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Store refresh token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            var userDto = new UserDto(
                user.Id, user.Email, user.FirstName, user.LastName,
                user.Phone, user.SystemRole, user.CompanyId, company.Name,
                null, null
            );

            return ServiceResult<AuthResponse>.Success(new AuthResponse(token, refreshToken, userDto));
        }
        catch (Exception ex)
        {
            return ServiceResult<AuthResponse>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return ServiceResult<AuthResponse>.BadRequest("Refresh token is required");
            }

            var user = await _context.Users
                .Include(u => u.Company)
                .Include(u => u.Manager)
                .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

            if (user == null)
            {
                return ServiceResult<AuthResponse>.Unauthorized("Invalid refresh token");
            }

            if (user.RefreshTokenExpiry < DateTime.UtcNow)
            {
                return ServiceResult<AuthResponse>.Unauthorized("Refresh token has expired");
            }

            if (user.IsDeleted)
            {
                return ServiceResult<AuthResponse>.Unauthorized("Account is deactivated");
            }

            // Generate new tokens
            var newToken = _jwtService.GenerateToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            // Update refresh token in database
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            var userDto = new UserDto(
                user.Id, user.Email, user.FirstName, user.LastName,
                user.Phone, user.SystemRole, user.CompanyId, user.Company?.Name,
                user.ManagerId, user.Manager != null ? user.Manager.FirstName + " " + user.Manager.LastName : null
            );

            return ServiceResult<AuthResponse>.Success(new AuthResponse(newToken, newRefreshToken, userDto));
        }
        catch (Exception ex)
        {
            return ServiceResult<AuthResponse>.Failure(ex.Message, ex);
        }
    }

    public async Task<ServiceResult<bool>> LogoutAsync(RefreshTokenRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return ServiceResult<bool>.Success(true);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

            if (user != null)
            {
                // Clear refresh token
                user.RefreshToken = null;
                user.RefreshTokenExpiry = null;
                await _context.SaveChangesAsync();
            }

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Failure(ex.Message, ex);
        }
    }
}
