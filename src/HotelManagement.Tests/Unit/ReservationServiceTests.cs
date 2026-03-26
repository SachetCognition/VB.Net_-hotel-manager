using FluentAssertions;
using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Services;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HotelManagement.Tests.Unit;

public class ReservationServiceTests : IDisposable
{
    private readonly HotelDbContext _context;
    private readonly ReservationService _service;

    public ReservationServiceTests()
    {
        var options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new HotelDbContext(options);
        _service = new ReservationService(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task CreateAsync_ValidReservation_CreatesRecord()
    {
        var request = new CreateReservationRequest(
            "G-123456", "John Doe", "101", "AC", 2000m,
            DateTime.Today.AddDays(1), DateTime.Today.AddDays(3), 2, 0, "INR", "");

        var result = await _service.CreateAsync(request);

        result.Should().NotBeNull();
        result.GuestID.Should().Be("G-123456");
        result.RoomNo.Should().Be("101");
        result.Status.Should().Be("Reserved");
    }

    [Fact]
    public async Task CreateAsync_OverlappingDates_ThrowsInvalidOperation()
    {
        _context.Set<Reservation>().Add(new Reservation
        {
            GuestID = "G-111111", GuestName = "Test", RoomNo = "101",
            DateIN = DateTime.Today, DateOUT = DateTime.Today.AddDays(5),
            Status = "Reserved"
        });
        await _context.SaveChangesAsync();

        var request = new CreateReservationRequest(
            "G-222222", "Jane", "101", "AC", 2000m,
            DateTime.Today.AddDays(1), DateTime.Today.AddDays(3), 2, 0, "INR", "");

        var act = async () => await _service.CreateAsync(request);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already reserved*");
    }

    [Fact]
    public async Task CheckDateOverlapAsync_OverlappingDates_ReturnsTrue()
    {
        _context.Set<Reservation>().Add(new Reservation
        {
            GuestID = "G-111111", GuestName = "Test", RoomNo = "101",
            DateIN = DateTime.Today, DateOUT = DateTime.Today.AddDays(5),
            Status = "Reserved"
        });
        await _context.SaveChangesAsync();

        var result = await _service.CheckDateOverlapAsync("101",
            DateTime.Today.AddDays(1), DateTime.Today.AddDays(3));
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckDateOverlapAsync_NonOverlapping_ReturnsFalse()
    {
        _context.Set<Reservation>().Add(new Reservation
        {
            GuestID = "G-111111", GuestName = "Test", RoomNo = "101",
            DateIN = DateTime.Today, DateOUT = DateTime.Today.AddDays(2),
            Status = "Reserved"
        });
        await _context.SaveChangesAsync();

        var result = await _service.CheckDateOverlapAsync("101",
            DateTime.Today.AddDays(3), DateTime.Today.AddDays(5));
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CheckDateOverlapAsync_DifferentRoom_ReturnsFalse()
    {
        _context.Set<Reservation>().Add(new Reservation
        {
            GuestID = "G-111111", GuestName = "Test", RoomNo = "101",
            DateIN = DateTime.Today, DateOUT = DateTime.Today.AddDays(5),
            Status = "Reserved"
        });
        await _context.SaveChangesAsync();

        var result = await _service.CheckDateOverlapAsync("102",
            DateTime.Today, DateTime.Today.AddDays(3));
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ExistingReservation_DeletesRecord()
    {
        _context.Set<Reservation>().Add(new Reservation
        {
            GuestID = "G-111111", GuestName = "Test", RoomNo = "101",
            DateIN = DateTime.Today, DateOUT = DateTime.Today.AddDays(2),
            Status = "Reserved"
        });
        await _context.SaveChangesAsync();

        var reservation = await _context.Set<Reservation>().FirstAsync();
        await _service.DeleteAsync(reservation.ID);

        var count = await _context.Set<Reservation>().CountAsync();
        count.Should().Be(0);
    }

    [Fact]
    public async Task DeleteAsync_NonExisting_ThrowsKeyNotFound()
    {
        var act = async () => await _service.DeleteAsync(999);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
