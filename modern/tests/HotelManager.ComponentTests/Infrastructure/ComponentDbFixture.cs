using HotelManager.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.ComponentTests.Infrastructure;

/// <summary>
/// In-memory SQLite database usable by component tests that inject a real
/// <see cref="HotelDbContext"/> or <c>IDbContextFactory&lt;HotelDbContext&gt;</c>
/// (Home, CheckInPage, Reservations, report data projections). A master
/// connection is kept open so the schema survives for the fixture lifetime.
/// </summary>
public sealed class ComponentDbFixture : IDisposable
{
    private readonly SqliteConnection _connection;
    public DbContextOptions<HotelDbContext> Options { get; }

    public ComponentDbFixture()
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
