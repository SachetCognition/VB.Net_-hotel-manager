using FluentAssertions;
using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Services;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HotelManagement.Tests.Integration;

public class CheckOutIntegrationTests : IDisposable
{
    private readonly HotelDbContext _context;
    private readonly CheckInService _checkInService;
    private readonly CheckOutService _checkOutService;
    private readonly RoomService _roomService;

    public CheckOutIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new HotelDbContext(options);
        _checkInService = new CheckInService(_context);
        _checkOutService = new CheckOutService(_context, _checkInService);
        _roomService = new RoomService(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task CheckOut_CreatesCheckoutRecordAndUpdatesCheckInStatus()
    {
        // Setup check-in
        var checkIn = new CheckInRoom
        {
            RoomNo = "101", GuestID = "G-123456", GuestName = "John Doe",
            RoomCharges = 2000m, DateIN = DateTime.Today, DateOUT = DateTime.Today.AddDays(2),
            NoOfAdults = 2, NoOfKids = 0, NoOfDays = 2,
            TotalRoomCharges = 4000m, OtherCharges = 0m,
            DiscountPer = 0m, Discount = 0m, SubTotal = 4000m,
            ServiceTaxPer = 10m, ServiceTaxAmount = 400m,
            LuxuryTaxPer = 5m, LuxuryTaxAmount = 220m,
            GrandTotal = 4620m, TotalPaid = 2000m, Balance = 2620m,
            ExtraBed = "No", Currency = "INR", Status = "Checked In"
        };
        _context.Set<CheckInRoom>().Add(checkIn);
        await _context.SaveChangesAsync();

        // Perform check-out
        var request = new CheckOutRequest(checkIn.ID, 4620m, "INR");
        var result = await _checkOutService.CheckOutAsync(request);

        // Verify checkout record
        result.Status.Should().Be("Checked Out");
        result.BillNo.Should().StartWith("B");

        // Verify check-in status updated
        var updatedCheckIn = await _context.Set<CheckInRoom>().FindAsync(checkIn.ID);
        updatedCheckIn!.Status.Should().Be("Checked Out");

        // Verify Tax_Room record created
        var taxRoom = await _context.Set<TaxRoom>().FirstOrDefaultAsync(t => t.BillNo == result.BillNo);
        taxRoom.Should().NotBeNull();
        taxRoom!.ServiceTaxAmount.Should().Be(400m);
    }

    [Fact]
    public async Task CheckOut_RoomBecomesAvailableAfterCheckout()
    {
        _context.Set<Room>().Add(new Room { RoomNo = "101", RoomType = "AC", RoomCharges = 2000m });
        var checkIn = new CheckInRoom
        {
            RoomNo = "101", GuestID = "G-123456", GuestName = "John Doe",
            RoomCharges = 2000m, DateIN = DateTime.Today, DateOUT = DateTime.Today.AddDays(3),
            Status = "Checked In"
        };
        _context.Set<CheckInRoom>().Add(checkIn);
        await _context.SaveChangesAsync();

        // Room should NOT be available
        var beforeCheckout = await _roomService.GetAvailableRoomsAsync(DateTime.Today, DateTime.Today.AddDays(2));
        beforeCheckout.Should().NotContain(r => r.RoomNo == "101");

        // Perform check-out
        var request = new CheckOutRequest(checkIn.ID, 0m, "INR");
        await _checkOutService.CheckOutAsync(request);

        // Room SHOULD be available after checkout
        var afterCheckout = await _roomService.GetAvailableRoomsAsync(DateTime.Today, DateTime.Today.AddDays(2));
        afterCheckout.Should().ContainSingle(r => r.RoomNo == "101");
    }
}
