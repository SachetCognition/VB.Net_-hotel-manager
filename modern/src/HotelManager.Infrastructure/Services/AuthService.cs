using HotelManager.Application.Interfaces;
using HotelManager.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly HotelDbContext _db;
    private readonly PasswordHasher<Registration> _hasher = new();

    public AuthService(HotelDbContext db) => _db = db;

    public async Task<AuthResult> ValidateCredentialsAsync(string userName, string password)
    {
        var user = await _db.Registrations.AsNoTracking()
            .FirstOrDefaultAsync(r => r.UserName == userName);
        if (user is null || user.User_Password is null)
            return new AuthResult(false, null, null, "Invalid username or password");
        var result = _hasher.VerifyHashedPassword(user, user.User_Password, password);
        if (result == PasswordVerificationResult.Failed)
            return new AuthResult(false, null, null, "Invalid username or password");
        return new AuthResult(true, user.UserName, user.UserType, null);
    }

    public Task<Registration?> GetUserAsync(string userName) =>
        _db.Registrations.AsNoTracking().FirstOrDefaultAsync(r => r.UserName == userName);

    public async Task<IReadOnlyList<Registration>> GetUsersAsync() =>
        await _db.Registrations.AsNoTracking().OrderBy(r => r.UserName).ToListAsync();

    public async Task CreateUserAsync(Registration user, string password)
    {
        user.User_Password = _hasher.HashPassword(user, password);
        _db.Registrations.Add(user);
        _db.Users.Add(new User { Username = user.UserName });
        await _db.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(string userName, string newPassword)
    {
        var user = await _db.Registrations.FirstAsync(r => r.UserName == userName);
        user.User_Password = _hasher.HashPassword(user, newPassword);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(string userName)
    {
        var login = await _db.Users.FindAsync(userName);
        if (login is not null) _db.Users.Remove(login);
        var user = await _db.Registrations.FindAsync(userName);
        if (user is not null) _db.Registrations.Remove(user);
        await _db.SaveChangesAsync();
    }

    public string HashPassword(string password) =>
        _hasher.HashPassword(new Registration { UserName = "_" }, password);
}
