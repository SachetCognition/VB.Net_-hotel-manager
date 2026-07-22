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

    [Fact]
    public async Task GetUsers_ReturnsAllOrderedByName()
    {
        await using var db = _fixture.CreateContext();
        var auth = new AuthService(db);
        await auth.CreateUserAsync(new Registration { UserName = "zeb", UserType = "User" }, "p");
        await auth.CreateUserAsync(new Registration { UserName = "amy", UserType = "Admin" }, "p");

        var users = await auth.GetUsersAsync();
        var names = users.Select(u => u.UserName).ToList();

        Assert.Contains("amy", names);
        Assert.Contains("zeb", names);
        Assert.True(names.IndexOf("amy") < names.IndexOf("zeb"));
    }

    [Fact]
    public async Task DeleteUser_RemovesRegistrationAndLogin()
    {
        await using var db = _fixture.CreateContext();
        var auth = new AuthService(db);
        await auth.CreateUserAsync(new Registration { UserName = "erin", UserType = "User" }, "p");

        await auth.DeleteUserAsync("erin");

        Assert.Null(await auth.GetUserAsync("erin"));
    }

    [Fact]
    public async Task DeleteUser_UnknownUser_NoThrow()
    {
        await using var db = _fixture.CreateContext();
        var auth = new AuthService(db);

        await auth.DeleteUserAsync("ghost");

        Assert.Null(await auth.GetUserAsync("ghost"));
    }

    [Fact]
    public async Task Validate_UserWithNullPassword_Fails()
    {
        await using var db = _fixture.CreateContext();
        db.Registrations.Add(new Registration { UserName = "frank", UserType = "User", User_Password = null });
        await db.SaveChangesAsync();
        var auth = new AuthService(db);

        var result = await auth.ValidateCredentialsAsync("frank", "anything");

        Assert.False(result.Succeeded);
        Assert.Equal("Invalid username or password", result.Error);
    }

    [Fact]
    public void HashPassword_ProducesVerifiableNonPlaintextHash()
    {
        using var db = _fixture.CreateContext();
        var auth = new AuthService(db);

        var hash = auth.HashPassword("secret");

        Assert.NotEqual("secret", hash);
        Assert.NotEmpty(hash);
    }
}
