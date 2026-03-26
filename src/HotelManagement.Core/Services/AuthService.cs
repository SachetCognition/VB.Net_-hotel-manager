using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace HotelManagement.Core.Services;

public class AuthService : IAuthService
{
    private readonly DbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(DbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
    {
        var users = _context.Set<User>();

        var existing = await users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (existing != null)
            throw new InvalidOperationException("Username already exists");

        if (request.UserType != "Admin" && request.UserType != "User")
            throw new ArgumentException("UserType must be 'Admin' or 'User'");

        var user = new User
        {
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            UserType = request.UserType
        };

        users.Add(user);
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(user);
        return new LoginResponse(user.ID, user.Username, user.UserType, token);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var users = _context.Set<User>();

        var user = await users.FirstOrDefaultAsync(u =>
            u.Username == request.Username && u.UserType == request.UserType);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");

        // Record login activity
        var activity = new LoginActivity
        {
            Username = user.Username,
            UserType = user.UserType,
            LoginTime = DateTime.UtcNow,
            SessionInfo = Guid.NewGuid().ToString()
        };
        _context.Set<LoginActivity>().Add(activity);
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(user);
        return new LoginResponse(user.ID, user.Username, user.UserType, token);
    }

    public async Task ChangePasswordAsync(ChangePasswordRequest request)
    {
        var users = _context.Set<User>();
        var user = await users.FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null)
            throw new KeyNotFoundException("User not found");

        if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Old password is incorrect");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<object>> GetLoginActivityAsync()
    {
        return await _context.Set<LoginActivity>()
            .OrderByDescending(l => l.LoginTime)
            .Select(l => new { l.ID, l.Username, l.UserType, l.LoginTime, l.SessionInfo })
            .ToListAsync();
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "HotelManagementSystemDefaultSecretKey2026!@#$%"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.UserType)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "HotelManagement",
            audience: _configuration["Jwt:Audience"] ?? "HotelManagement",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
