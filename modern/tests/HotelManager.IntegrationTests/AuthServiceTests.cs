using HotelManager.Domain.Entities;
using HotelManager.Infrastructure.Services;

namespace HotelManager.IntegrationTests;

public class AuthServiceTests : IClassFixture<SqliteDbFixture>
{
    private readonly SqliteDbFixture _fixture;

    public AuthServiceTests(SqliteDbFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task CreateUser_ThenValidate_Succeeds()
    {
        await using var db = _fixture.CreateContext();
        var auth = new AuthService(db);
        await auth.CreateUserAsync(new Registration { UserName = "alice", UserType = "User" }, "S3cret!");

        var result = await auth.ValidateCredentialsAsync("alice", "S3cret!");
        Assert.True(result.Succeeded);
        Assert.Equal("alice", result.UserName);
        Assert.Equal("User", result.UserType);
    }

    [Fact]
    public async Task Validate_WrongPassword_Fails()
    {
        await using var db = _fixture.CreateContext();
        var auth = new AuthService(db);
        await auth.CreateUserAsync(new Registration { UserName = "bob", UserType = "Admin" }, "right");

        var result = await auth.ValidateCredentialsAsync("bob", "wrong");
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Validate_UnknownUser_Fails()
    {
        await using var db = _fixture.CreateContext();
        var auth = new AuthService(db);
        var result = await auth.ValidateCredentialsAsync("nobody", "x");
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task PasswordsAreHashed_NotStoredInPlaintext()
    {
        await using var db = _fixture.CreateContext();
        var auth = new AuthService(db);
        await auth.CreateUserAsync(new Registration { UserName = "carol", UserType = "User" }, "plaintext");

        var user = await auth.GetUserAsync("carol");
        Assert.NotNull(user);
        Assert.NotEqual("plaintext", user!.User_Password);
    }

    [Fact]
    public async Task ChangePassword_InvalidatesOldPassword()
    {
        await using var db = _fixture.CreateContext();
        var auth = new AuthService(db);
        await auth.CreateUserAsync(new Registration { UserName = "dave", UserType = "User" }, "old");
        await auth.ChangePasswordAsync("dave", "new");

        Assert.False((await auth.ValidateCredentialsAsync("dave", "old")).Succeeded);
        Assert.True((await auth.ValidateCredentialsAsync("dave", "new")).Succeeded);
    }
}
