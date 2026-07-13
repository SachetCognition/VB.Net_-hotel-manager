using HotelManager.Domain.Entities;
using HotelManager.Infrastructure;
using HotelManager.Web.Services.Reservations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.IntegrationTests.Reservations;

/// <summary>
/// In-memory SQLite fixture for the Child B reservation workstream, following
/// the shared SqliteDbFixture pattern, pre-seeded with master data.
/// </summary>
public class ReservationsFixture : IDisposable
{
    private readonly SqliteConnection _connection;
    public DbContextOptions<HotelDbContext> Options { get; }

    public ReservationsFixture()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        Options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseSqlite(_connection)
            .Options;
        using var db = new HotelDbContext(Options);
        db.Database.EnsureCreated();
        Seed(db);
    }

    private static void Seed(HotelDbContext db)
    {
        db.Guests.Add(new Guest { GuestID = "G-1", GuestName = "Alice" });
        db.Guests.Add(new Guest { GuestID = "G-2", GuestName = "Bob" });
        for (var i = 101; i <= 110; i++)
            db.Rooms.Add(new Room { RoomNo = i.ToString(), RoomType = "Deluxe", RoomCharges = 1000 });
        db.ExtraBeds.Add(new ExtraBed { Charges = 250 });
        db.Halls.Add(new Hall { Charges = 5000, HallName = "Main Hall" });
        db.Gardens.Add(new Garden { Charges = 3000 });
        db.Currencies.Add(new CurrencySet { CS_Currency = "USD" });
        db.SaveChanges();
    }

    public HotelDbContext CreateContext() => new(Options);

    public ReservationService CreateReservationService(HotelDbContext db) => new(db);

    public CheckInService CreateCheckInService(HotelDbContext db) =>
        new(db, new ReservationService(db));

    public CheckOutService CreateCheckOutService(HotelDbContext db) => new(db);

    public void Dispose() => _connection.Dispose();
}
