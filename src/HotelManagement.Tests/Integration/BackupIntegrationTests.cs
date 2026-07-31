using FluentAssertions;
using HotelManagement.Core.Entities;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HotelManagement.Tests.Integration;

public class BackupIntegrationTests : IDisposable
{
    private readonly HotelDbContext _context;

    public BackupIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new HotelDbContext(options);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task DatabaseContext_CanSaveAndRetrieveAllEntityTypes()
    {
        // Test that all entity types can be saved and retrieved
        _context.Set<Guest>().Add(new Guest { GuestID = "G-111111", GuestName = "Test Guest", City = "Test" });
        _context.Set<Room>().Add(new Room { RoomNo = "101", RoomType = "AC", RoomCharges = 1000m });
        _context.Set<Employee>().Add(new Employee
        {
            EmployeeID = "E-111111", EmployeeName = "Test Employee",
            Address = "Addr", MobileNo = "1234567890", Salary = 30000m
        });
        _context.Set<Dish>().Add(new Dish { DishName = "Test Dish", Category = "Test", Rate = 100m });
        _context.Set<Beer>().Add(new Beer { BeerName = "Test Beer", Category = "Lager", Rate = 150m, Quantity = 50 });
        _context.Set<Liquor>().Add(new Liquor { LiquorName = "Test Liquor", Category = "Spirit", Rate = 500m, Quantity = 30 });
        _context.Set<User>().Add(new User { Username = "admin", PasswordHash = "hash", UserType = "Admin" });

        await _context.SaveChangesAsync();

        // Verify all records saved
        (await _context.Set<Guest>().CountAsync()).Should().Be(1);
        (await _context.Set<Room>().CountAsync()).Should().Be(1);
        (await _context.Set<Employee>().CountAsync()).Should().Be(1);
        (await _context.Set<Dish>().CountAsync()).Should().Be(1);
        (await _context.Set<Beer>().CountAsync()).Should().Be(1);
        (await _context.Set<Liquor>().CountAsync()).Should().Be(1);
        (await _context.Set<User>().CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task DatabaseContext_PreservesDataIntegrity()
    {
        // Add guest and check-in
        var guest = new Guest { GuestID = "G-123456", GuestName = "Integrity Test", City = "Test" };
        _context.Set<Guest>().Add(guest);

        var checkIn = new CheckInRoom
        {
            GuestID = "G-123456", RoomNo = "101", GuestName = "Integrity Test",
            RoomCharges = 2000m, DateIN = DateTime.Today, DateOUT = DateTime.Today.AddDays(2),
            Status = "Checked In"
        };
        _context.Set<CheckInRoom>().Add(checkIn);
        await _context.SaveChangesAsync();

        // Verify relationship
        var savedCheckIn = await _context.Set<CheckInRoom>().FirstAsync();
        savedCheckIn.GuestID.Should().Be(guest.GuestID);
    }
}
