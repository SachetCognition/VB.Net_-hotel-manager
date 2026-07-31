using FluentAssertions;
using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Services;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HotelManagement.Tests.E2E;

/// <summary>
/// E2E-001: Complete guest lifecycle: Register -> Reserve -> Check-In -> Order -> Check-Out -> Invoice
/// </summary>
public class GuestLifecycleTests : IDisposable
{
    private readonly HotelDbContext _context;
    private readonly GuestService _guestService;
    private readonly RoomService _roomService;
    private readonly ReservationService _reservationService;
    private readonly CheckInService _checkInService;
    private readonly CheckOutService _checkOutService;

    public GuestLifecycleTests()
    {
        var options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new HotelDbContext(options);
        _guestService = new GuestService(_context);
        _roomService = new RoomService(_context);
        _reservationService = new ReservationService(_context);
        _checkInService = new CheckInService(_context);
        _checkOutService = new CheckOutService(_context, _checkInService);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task CompleteGuestLifecycle_RegisterReserveCheckInCheckOut()
    {
        // STEP 1: Create room
        var room = await _roomService.CreateAsync(new CreateRoomRequest("Suite-01", "Suite", 8000m));
        room.RoomNo.Should().Be("Suite-01");

        // STEP 2: Register guest
        var guest = await _guestService.CreateAsync(
            new CreateGuestRequest("Amit Sharma", "22 Rajpath", "Delhi", "9876543210", "Passport", "J12345678", "VIP Corporate"));
        guest.GuestID.Should().StartWith("G-");
        guest.GuestName.Should().Be("Amit Sharma");

        // STEP 3: Reserve room
        var reservation = await _reservationService.CreateAsync(
            new CreateReservationRequest(guest.GuestID, guest.GuestName, "Suite-01", "Suite", 8000m,
                DateTime.Today.AddDays(1), DateTime.Today.AddDays(4), 2, 1, "INR", "Corporate booking"));
        reservation.Status.Should().Be("Reserved");
        reservation.RoomNo.Should().Be("Suite-01");

        // STEP 4: Cancel reservation before check-in (legacy flow: check-in consumes reservation)
        await _reservationService.DeleteAsync(reservation.ID);

        // STEP 5: Check-in
        var checkIn = await _checkInService.CheckInAsync(
            new CreateCheckInRequest(guest.GuestID, "Suite-01", 8000m,
                DateTime.Today.AddDays(1), DateTime.Today.AddDays(4), 2, 1,
                guest.GuestName, "22 Rajpath", "Delhi", "9876543210",
                "Passport", "J12345678", 500m, 5m, 14m, 5m, 10000m, "Yes", "INR", "VIP"));
        checkIn.Status.Should().Be("Checked In");
        checkIn.NoOfDays.Should().Be(3);
        // TotalRoomCharges = 8000 * 3 = 24000
        checkIn.TotalRoomCharges.Should().Be(24000m);

        // Verify room is no longer available
        var available = await _roomService.GetAvailableRoomsAsync(
            DateTime.Today.AddDays(1), DateTime.Today.AddDays(4));
        available.Should().NotContain(r => r.RoomNo == "Suite-01");

        // STEP 6: Check-out
        var checkOut = await _checkOutService.CheckOutAsync(
            new CheckOutRequest(checkIn.ID, checkIn.GrandTotal, "INR"));
        checkOut.Status.Should().Be("Checked Out");
        checkOut.BillNo.Should().StartWith("B");
        checkOut.Balance.Should().Be(0m); // Full payment

        // Verify room is available again
        var availableAfter = await _roomService.GetAvailableRoomsAsync(
            DateTime.Today.AddDays(1), DateTime.Today.AddDays(4));
        availableAfter.Should().ContainSingle(r => r.RoomNo == "Suite-01");

        // Verify Tax_Room record created
        var taxRoom = await _context.Set<TaxRoom>().FirstOrDefaultAsync(t => t.BillNo == checkOut.BillNo);
        taxRoom.Should().NotBeNull();
    }

    [Fact]
    public async Task GuestLifecycle_TaxCalculationsMatchLegacy()
    {
        // Setup
        _context.Set<Room>().Add(new Room { RoomNo = "101", RoomType = "AC", RoomCharges = 2000m });
        _context.Set<Guest>().Add(new Guest { GuestID = "G-100001", GuestName = "Tax Test Guest", City = "Test" });
        await _context.SaveChangesAsync();

        // Check-in with known tax parameters
        var checkIn = await _checkInService.CheckInAsync(
            new CreateCheckInRequest("G-100001", "101", 2000m,
                DateTime.Today, DateTime.Today.AddDays(3), 2, 0,
                "Tax Test Guest", "Addr", "City", "1234567890",
                "", "", 500m, 10m, 14m, 5m, 0m, "No", "INR", ""));

        // Verify exact legacy tax calculations:
        // TotalRoomCharges = 2000 * 3 = 6000
        checkIn.TotalRoomCharges.Should().Be(6000m);
        // Discount = (6000 + 500) * 10 / 100 = 650
        checkIn.Discount.Should().Be(650m);
        // SubTotal = 6000 + 500 - 650 = 5850
        checkIn.SubTotal.Should().Be(5850m);
        // ServiceTaxAmount = Round(5850 * 14 / 100, 2) = 819
        checkIn.ServiceTaxAmount.Should().Be(819m);
        // LuxuryTaxAmount = Round((5850 + 819) * 5 / 100, 2) = 333.45
        checkIn.LuxuryTaxAmount.Should().Be(333.45m);
        // GrandTotal = 5850 + 819 + 333.45 = 7002.45
        checkIn.GrandTotal.Should().Be(7002.45m);

        // Check-out and verify education cess
        var checkOut = await _checkOutService.CheckOutAsync(
            new CheckOutRequest(checkIn.ID, 7002.45m, "INR"));

        // Verify education cess from checkout
        var taxRoom = await _context.Set<TaxRoom>().FirstAsync(t => t.BillNo == checkOut.BillNo);
        // EducessTax = (14 * 2) / 100 = 0.28
        taxRoom.EducessTax.Should().Be(0.28m);
        // EducessTaxAmount = (819 * 0.28) / 100 = 2.29
        taxRoom.EducessTaxAmount.Should().Be(2.29m);
        // HEducessTax = (14 * 1) / 100 = 0.14
        taxRoom.HEducessTax.Should().Be(0.14m);
        // HEducessTaxAmount = (819 * 0.14) / 100 = 1.15
        taxRoom.HEducessTaxAmount.Should().Be(1.15m);
    }
}
