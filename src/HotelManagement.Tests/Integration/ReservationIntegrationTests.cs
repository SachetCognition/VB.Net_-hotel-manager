using FluentAssertions;
using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Services;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HotelManagement.Tests.Integration;

public class ReservationIntegrationTests : IDisposable
{
    private readonly HotelDbContext _context;
    private readonly ReservationService _reservationService;
    private readonly RoomService _roomService;

    public ReservationIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new HotelDbContext(options);
        _reservationService = new ReservationService(_context);
        _roomService = new RoomService(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task Reservation_BlocksRoomForDateRange()
    {
        _context.Set<Room>().Add(new Room { RoomNo = "101", RoomType = "AC", RoomCharges = 2000m });
        await _context.SaveChangesAsync();

        // Room available before reservation
        var beforeRes = await _roomService.GetAvailableRoomsAsync(
            DateTime.Today.AddDays(1), DateTime.Today.AddDays(5));
        beforeRes.Should().ContainSingle(r => r.RoomNo == "101");

        // Create reservation
        var request = new CreateReservationRequest(
            "G-123456", "John", "101", "AC", 2000m,
            DateTime.Today.AddDays(1), DateTime.Today.AddDays(5), 2, 0, "INR", "");
        await _reservationService.CreateAsync(request);

        // Room no longer available for overlapping dates
        var afterRes = await _roomService.GetAvailableRoomsAsync(
            DateTime.Today.AddDays(2), DateTime.Today.AddDays(4));
        afterRes.Should().NotContain(r => r.RoomNo == "101");
    }

    [Fact]
    public async Task Reservation_DateOverlapPreventsDoubleBooking()
    {
        // First reservation
        var request1 = new CreateReservationRequest(
            "G-111111", "Guest A", "101", "AC", 2000m,
            DateTime.Today, DateTime.Today.AddDays(5), 2, 0, "INR", "");
        await _reservationService.CreateAsync(request1);

        // Second reservation for overlapping dates
        var request2 = new CreateReservationRequest(
            "G-222222", "Guest B", "101", "AC", 2000m,
            DateTime.Today.AddDays(2), DateTime.Today.AddDays(7), 1, 0, "INR", "");

        var act = async () => await _reservationService.CreateAsync(request2);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already reserved*");
    }

    [Fact]
    public async Task Reservation_NonOverlappingDates_Succeeds()
    {
        // First reservation
        var request1 = new CreateReservationRequest(
            "G-111111", "Guest A", "101", "AC", 2000m,
            DateTime.Today, DateTime.Today.AddDays(3), 2, 0, "INR", "");
        await _reservationService.CreateAsync(request1);

        // Non-overlapping reservation
        var request2 = new CreateReservationRequest(
            "G-222222", "Guest B", "101", "AC", 2000m,
            DateTime.Today.AddDays(5), DateTime.Today.AddDays(7), 1, 0, "INR", "");

        var result = await _reservationService.CreateAsync(request2);
        result.Should().NotBeNull();
        result.Status.Should().Be("Reserved");
    }
}
