using HotelManager.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.IntegrationTests.Hr;

/// <summary>
/// In-memory SQLite fixture for HR &amp; payroll tests (copied from the shared
/// SqliteDbFixture pattern, owned by the HR workstream).
/// </summary>
public class HrSqliteDbFixture : IDisposable
{
    private readonly SqliteConnection _connection;
    public DbContextOptions<HotelDbContext> Options { get; }

    public HrSqliteDbFixture()
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
