using Microsoft.EntityFrameworkCore;
using WestDesk.Application.DTOs.Auth;
using WestDesk.Application.Interfaces;
using WestDesk.Domain.Entities;
using WestDesk.Infrastructure.Persistence;

namespace WestDesk.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly WestDeskDbContext _context;
    private readonly ITokenService _tokenService;

    public AuthService(WestDeskDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var emailExists = await _context.Users.AnyAsync(u => u.Email == request.Email);
        if (emailExists)
        {
            throw new InvalidOperationException("Bu email adresi zaten kayıtlı.");
        }

        var tenant = new Tenant
        {
            Name = request.CompanyName,
            Slug = request.CompanyName.ToLower().Replace(" ", "-"),
            Plan = SubscriptionPlan.Free
        };
        _context.Tenants.Add(tenant);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            TenantId = tenant.Id,
            Tenant = tenant,
            Email = request.Email,
            FullName = request.FullName,
            PasswordHash = passwordHash,
            Role = UserRole.Admin
        };
        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        var token = _tokenService.GenerateToken(user);

        return new AuthResponse
        {
            Token = token,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role.ToString(),
            TenantId = user.TenantId
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null)
        {
            throw new UnauthorizedAccessException("Email veya şifre hatalı.");
        }

        var passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!passwordValid)
        {
            throw new UnauthorizedAccessException("Email veya şifre hatalı.");
        }

        var token = _tokenService.GenerateToken(user);

        return new AuthResponse
        {
            Token = token,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role.ToString(),
            TenantId = user.TenantId
        };
    }
}