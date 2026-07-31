using FluentAssertions;
using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Services;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace HotelManagement.Tests.Unit;

public class AuthServiceTests : IDisposable
{
    private readonly HotelDbContext _context;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new HotelDbContext(options);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "TestSecretKeyForJWTTokenGeneration2026!@#$%^&*()",
                ["Jwt:Issuer"] = "HotelManagementTest",
                ["Jwt:Audience"] = "HotelManagementTest"
            })
            .Build();

        _service = new AuthService(_context, config);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task RegisterAsync_ValidRequest_ReturnsToken()
    {
        var request = new RegisterRequest("admin", "Password123!", "Admin");
        var result = await _service.RegisterAsync(request);

        result.Should().NotBeNull();
        result.Username.Should().Be("admin");
        result.UserType.Should().Be("Admin");
        result.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RegisterAsync_DuplicateUsername_ThrowsInvalidOperation()
    {
        _context.Set<User>().Add(new User
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("test"),
            UserType = "Admin"
        });
        await _context.SaveChangesAsync();

        var request = new RegisterRequest("admin", "Password123!", "Admin");
        var act = async () => await _service.RegisterAsync(request);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already exists*");
    }

    [Fact]
    public async Task RegisterAsync_InvalidUserType_ThrowsArgumentException()
    {
        var request = new RegisterRequest("admin", "Password123!", "SuperAdmin");
        var act = async () => await _service.RegisterAsync(request);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Admin*User*");
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        _context.Set<User>().Add(new User
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            UserType = "Admin"
        });
        await _context.SaveChangesAsync();

        var request = new LoginRequest("admin", "Password123!", "Admin");
        var result = await _service.LoginAsync(request);

        result.Token.Should().NotBeNullOrEmpty();
        result.Username.Should().Be("admin");
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorized()
    {
        _context.Set<User>().Add(new User
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            UserType = "Admin"
        });
        await _context.SaveChangesAsync();

        var request = new LoginRequest("admin", "WrongPassword", "Admin");
        var act = async () => await _service.LoginAsync(request);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task LoginAsync_NonExistentUser_ThrowsUnauthorized()
    {
        var request = new LoginRequest("nobody", "Password123!", "Admin");
        var act = async () => await _service.LoginAsync(request);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task LoginAsync_CreatesLoginActivity()
    {
        _context.Set<User>().Add(new User
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            UserType = "Admin"
        });
        await _context.SaveChangesAsync();

        await _service.LoginAsync(new LoginRequest("admin", "Password123!", "Admin"));

        var activities = await _context.Set<LoginActivity>().ToListAsync();
        activities.Should().HaveCount(1);
        activities[0].Username.Should().Be("admin");
    }

    [Fact]
    public async Task ChangePasswordAsync_ValidOldPassword_ChangesPassword()
    {
        _context.Set<User>().Add(new User
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("OldPass123!"),
            UserType = "Admin"
        });
        await _context.SaveChangesAsync();

        await _service.ChangePasswordAsync(new ChangePasswordRequest("admin", "OldPass123!", "NewPass456!"));

        var user = await _context.Set<User>().FirstAsync(u => u.Username == "admin");
        BCrypt.Net.BCrypt.Verify("NewPass456!", user.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task ChangePasswordAsync_WrongOldPassword_ThrowsUnauthorized()
    {
        _context.Set<User>().Add(new User
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("OldPass123!"),
            UserType = "Admin"
        });
        await _context.SaveChangesAsync();

        var act = async () => await _service.ChangePasswordAsync(
            new ChangePasswordRequest("admin", "WrongOldPass", "NewPass456!"));
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task ChangePasswordAsync_NonExistentUser_ThrowsKeyNotFound()
    {
        var act = async () => await _service.ChangePasswordAsync(
            new ChangePasswordRequest("nobody", "OldPass", "NewPass"));
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task RegisterAsync_PasswordHashedWithBCrypt()
    {
        var request = new RegisterRequest("testuser", "MyPassword123!", "User");
        await _service.RegisterAsync(request);

        var user = await _context.Set<User>().FirstAsync(u => u.Username == "testuser");
        user.PasswordHash.Should().StartWith("$2");
        BCrypt.Net.BCrypt.Verify("MyPassword123!", user.PasswordHash).Should().BeTrue();
    }
}
