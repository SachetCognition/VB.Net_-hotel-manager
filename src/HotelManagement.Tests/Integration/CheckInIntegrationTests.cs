using FluentAssertions;
using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Services;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HotelManagement.Tests.Integration;

public class CheckInIntegrationTests : IDisposable
{
    private readonly HotelDbContext _context;
    private readonly CheckInService _checkInService;
    private readonly RoomService _roomService;

    public CheckInIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new HotelDbContext(options);
        _checkInService = new CheckInService(_context);
        _roomService = new RoomService(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task CheckIn_CreatesRecordAndAffectsRoomAvailability()
    {
        // Setup: create room and guest
        _context.Set<Room>().Add(new Room { RoomNo = "101", RoomType = "AC", RoomCharges = 2000m });
        _context.Set<Guest>().Add(new Guest { GuestID = "G-123456", GuestName = "John Doe", City = "Mumbai" });
        await _context.SaveChangesAsync();

        // Verify room is available before check-in
        var availableBefore = await _roomService.GetAvailableRoomsAsync(DateTime.Today, DateTime.Today.AddDays(3));
        availableBefore.Should().ContainSingle(r => r.RoomNo == "101");

        // Perform check-in
        var request = new CreateCheckInRequest(
            "G-123456", "101", 2000m,
            DateTime.Today, DateTime.Today.AddDays(3), 2, 0,
            "John Doe", "123 St", "Mumbai", "9876543210",
            "Passport", "P123", 0m, 0m, 10m, 5m, 0m, "No", "INR", "");

        var result = await _checkInService.CheckInAsync(request);

        // Verify check-in record created
        result.Should().NotBeNull();
        result.Status.Should().Be("Checked In");

        // Verify room no longer available for overlapping dates
        var availableAfter = await _roomService.GetAvailableRoomsAsync(DateTime.Today, DateTime.Today.AddDays(3));
        availableAfter.Should().NotContain(r => r.RoomNo == "101");
    }

    [Fact]
    public async Task CheckIn_WithTaxes_CalculatesCorrectGrandTotal()
    {
        _context.Set<Room>().Add(new Room { RoomNo = "201", RoomType = "Deluxe", RoomCharges = 5000m });
        _context.Set<Guest>().Add(new Guest { GuestID = "G-654321", GuestName = "Jane Smith", City = "Delhi" });
        await _context.SaveChangesAsync();

        var request = new CreateCheckInRequest(
            "G-654321", "201", 5000m,
            DateTime.Today, DateTime.Today.AddDays(2), 2, 1,
            "Jane Smith", "456 Ave", "Delhi", "1234567890",
            "Aadhar", "A123", 500m, 10m, 14m, 5m, 0m, "Yes", "INR", "VIP");

        var result = await _checkInService.CheckInAsync(request);

        // Verify tax calculation chain:
        // TotalRoomCharges = 5000 * 2 = 10000
        // Discount = (10000 + 500) * 10 / 100 = 1050
        // SubTotal = 10000 + 500 - 1050 = 9450
        // ServiceTaxAmount = Round(9450 * 14 / 100, 2) = 1323
        // LuxuryTaxAmount = Round((9450 + 1323) * 5 / 100, 2) = 538.65
        // GrandTotal = 9450 + 1323 + 538.65 = 11311.65
        result.NoOfDays.Should().Be(2);
        result.TotalRoomCharges.Should().Be(10000m);
        result.Discount.Should().Be(1050m);
        result.SubTotal.Should().Be(9450m);
        result.ServiceTaxAmount.Should().Be(1323m);
        result.LuxuryTaxAmount.Should().Be(538.65m);
        result.GrandTotal.Should().Be(11311.65m);
    }

    [Fact]
    public async Task CheckIn_PreventDoubleBooking_ThrowsException()
    {
        _context.Set<Room>().Add(new Room { RoomNo = "101", RoomType = "AC", RoomCharges = 2000m });
        _context.Set<Guest>().Add(new Guest { GuestID = "G-111111", GuestName = "Guest A", City = "A" });
        _context.Set<Guest>().Add(new Guest { GuestID = "G-222222", GuestName = "Guest B", City = "B" });
        await _context.SaveChangesAsync();

        // First check-in
        var request1 = new CreateCheckInRequest(
            "G-111111", "101", 2000m,
            DateTime.Today, DateTime.Today.AddDays(5), 2, 0,
            "Guest A", "A", "A", "1111111111", "", "", 0m, 0m, 0m, 0m, 0m, "No", "INR", "");
        await _checkInService.CheckInAsync(request1);

        // Second check-in for same room and overlapping dates
        var request2 = new CreateCheckInRequest(
            "G-222222", "101", 2000m,
            DateTime.Today.AddDays(1), DateTime.Today.AddDays(3), 1, 0,
            "Guest B", "B", "B", "2222222222", "", "", 0m, 0m, 0m, 0m, 0m, "No", "INR", "");

        var act = async () => await _checkInService.CheckInAsync(request2);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*not available*");
    }
}
