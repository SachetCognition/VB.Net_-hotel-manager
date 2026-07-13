using HotelManager.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.IntegrationTests;

/// <summary>
/// Shared in-memory SQLite fixture. Keeps a master connection open so the
/// database lives for the fixture lifetime; each test creates fresh contexts.
/// </summary>
public class SqliteDbFixture : IDisposable
{
    private readonly SqliteConnection _connection;
    public DbContextOptions<HotelDbContext> Options { get; }

    public SqliteDbFixture()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        Options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseSqlite(_connection)
            .Options;
        using var db = new HotelDbContext(Options);
        db.Database.EnsureCreated();
    }

    public HotelDbContext CreateContext() => new(Options);

    public void Dispose() => _connection.Dispose();
}
