using HotelManagement.Core.Services;
using Xunit;

namespace HotelManagement.Tests.Security;

/// <summary>
/// Category 9: Security Tests (~20 tests)
/// Tests SQL injection prevention and password security.
/// </summary>
public class SecurityTests
{
    private readonly ValidationService _validation = new();
    private readonly PasswordService _password = new();

    // TC-SEC-001: SQL injection in guest name
    [Fact]
    public void SqlInjection_GuestName_SingleQuote()
    {
        Assert.True(_validation.ContainsSqlInjection("'; DROP TABLE Guest;--"));
    }

    // TC-SEC-002: SQL injection with UNION SELECT
    [Fact]
    public void SqlInjection_UnionSelect()
    {
        Assert.True(_validation.ContainsSqlInjection("' UNION SELECT * FROM Users--"));
    }

    // TC-SEC-003: SQL injection with semicolon
    [Fact]
    public void SqlInjection_Semicolon()
    {
        Assert.True(_validation.ContainsSqlInjection("test; DELETE FROM Guest"));
    }

    // TC-SEC-004: SQL injection with double dash comment
    [Fact]
    public void SqlInjection_DoubleDash()
    {
        Assert.True(_validation.ContainsSqlInjection("admin'--"));
    }

    // TC-SEC-005: SQL injection with DROP TABLE
    [Fact]
    public void SqlInjection_DropTable()
    {
        Assert.True(_validation.ContainsSqlInjection("'; drop table users;"));
    }

    // TC-SEC-006: SQL injection with INSERT INTO
    [Fact]
    public void SqlInjection_InsertInto()
    {
        Assert.True(_validation.ContainsSqlInjection("'; insert into users values('hacker','pass')"));
    }

    // TC-SEC-007: SQL injection with UPDATE
    [Fact]
    public void SqlInjection_Update()
    {
        Assert.True(_validation.ContainsSqlInjection("'; update users set role='admin'"));
    }

    // TC-SEC-008: SQL injection with EXEC
    [Fact]
    public void SqlInjection_Exec()
    {
        Assert.True(_validation.ContainsSqlInjection("'; exec xp_cmdshell 'dir'"));
    }

    // TC-SEC-009: SQL injection with block comment
    [Fact]
    public void SqlInjection_BlockComment()
    {
        Assert.True(_validation.ContainsSqlInjection("admin /* comment */ --"));
    }

    // TC-SEC-010: Clean input should not trigger
    [Fact]
    public void SqlInjection_CleanInput_ReturnsFalse()
    {
        Assert.False(_validation.ContainsSqlInjection("John Doe"));
    }

    // TC-SEC-011: Empty input should not trigger
    [Fact]
    public void SqlInjection_EmptyInput_ReturnsFalse()
    {
        Assert.False(_validation.ContainsSqlInjection(""));
    }

    // TC-SEC-012: Null input should not trigger
    [Fact]
    public void SqlInjection_NullInput_ReturnsFalse()
    {
        Assert.False(_validation.ContainsSqlInjection(null!));
    }

    // TC-SEC-013: SQL injection in search fields
    [Fact]
    public void SqlInjection_SearchField()
    {
        Assert.True(_validation.ContainsSqlInjection("' OR '1'='1"));
    }

    // TC-SEC-014: SQL injection in room number
    [Fact]
    public void SqlInjection_RoomNumber()
    {
        Assert.True(_validation.ContainsSqlInjection("101'; DELETE FROM Room;--"));
    }

    // TC-SEC-015: BCrypt password hash and verify
    [Fact]
    public void Password_HashAndVerify_ShouldMatch()
    {
        string hash = _password.HashPassword("MySecureP@ss");
        Assert.True(_password.VerifyPassword("MySecureP@ss", hash));
    }

    // TC-SEC-016: BCrypt different passwords should not match
    [Fact]
    public void Password_DifferentPasswords_ShouldNotMatch()
    {
        string hash = _password.HashPassword("Password1");
        Assert.False(_password.VerifyPassword("Password2", hash));
    }

    // TC-SEC-017: BCrypt hash should be different each time (salt)
    [Fact]
    public void Password_SamePassword_DifferentHashes()
    {
        string hash1 = _password.HashPassword("Same");
        string hash2 = _password.HashPassword("Same");
        Assert.NotEqual(hash1, hash2);
    }

    // TC-SEC-018: BCrypt hash should not be plain text
    [Fact]
    public void Password_Hash_ShouldNotBePlainText()
    {
        string hash = _password.HashPassword("test123");
        Assert.NotEqual("test123", hash);
        Assert.StartsWith("$2", hash); // BCrypt prefix
    }

    // TC-SEC-019: SQL injection xp_ stored procedures
    [Fact]
    public void SqlInjection_XpStoredProc()
    {
        Assert.True(_validation.ContainsSqlInjection("'; xp_cmdshell('dir')"));
    }

    // TC-SEC-020: SQL injection sp_ stored procedures
    [Fact]
    public void SqlInjection_SpStoredProc()
    {
        Assert.True(_validation.ContainsSqlInjection("'; sp_executesql('select 1')"));
    }
}
