using FluentAssertions;
using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Services;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HotelManagement.Tests.Unit;

public class CheckInServiceTests : IDisposable
{
    private readonly HotelDbContext _context;
    private readonly CheckInService _service;

    public CheckInServiceTests()
    {
        var options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new HotelDbContext(options);
        _service = new CheckInService(_context);
    }

    public void Dispose() => _context.Dispose();

    // --- CalculateNoOfDays Tests ---

    [Fact]
    public void CalculateNoOfDays_SameDate_Returns1()
    {
        var date = new DateTime(2026, 1, 15);
        _service.CalculateNoOfDays(date, date).Should().Be(1);
    }

    [Fact]
    public void CalculateNoOfDays_OneDayApart_Returns1()
    {
        var dateIn = new DateTime(2026, 1, 15);
        var dateOut = new DateTime(2026, 1, 16);
        _service.CalculateNoOfDays(dateIn, dateOut).Should().Be(1);
    }

    [Fact]
    public void CalculateNoOfDays_MultipleDays_ReturnsCorrectDays()
    {
        var dateIn = new DateTime(2026, 1, 15);
        var dateOut = new DateTime(2026, 1, 20);
        _service.CalculateNoOfDays(dateIn, dateOut).Should().Be(5);
    }

    [Fact]
    public void CalculateNoOfDays_LongStay_ReturnsCorrectDays()
    {
        var dateIn = new DateTime(2026, 1, 1);
        var dateOut = new DateTime(2026, 2, 1);
        _service.CalculateNoOfDays(dateIn, dateOut).Should().Be(31);
    }

    // --- CalculateTaxes Tests ---

    [Fact]
    public void CalculateTaxes_BasicCalculation_MatchesLegacyFormula()
    {
        // RoomCharges=2000, NoOfDays=3, OtherCharges=500, Discount=10%, ServiceTax=14%, LuxuryTax=5%
        var result = _service.CalculateTaxes(2000m, 3, 500m, 10m, 14m, 5m, 0m);

        result.TotalRoomCharges.Should().Be(6000m); // 2000 * 3
        result.Discount.Should().Be(650m); // (6000 + 500) * 10 / 100
        result.SubTotal.Should().Be(5850m); // 6000 + 500 - 650
        result.ServiceTaxAmount.Should().Be(819m); // Round(5850 * 14 / 100, 2)
        result.LuxuryTaxAmount.Should().Be(333.45m); // Round((5850 + 819) * 5 / 100, 2)
        result.GrandTotal.Should().Be(7002.45m); // 5850 + 819 + 333.45
        result.Balance.Should().Be(7002.45m); // GrandTotal - 0
    }

    [Fact]
    public void CalculateTaxes_ZeroDiscount_CalculatesCorrectly()
    {
        var result = _service.CalculateTaxes(1000m, 2, 0m, 0m, 10m, 5m, 0m);

        result.TotalRoomCharges.Should().Be(2000m);
        result.Discount.Should().Be(0m);
        result.SubTotal.Should().Be(2000m);
        result.ServiceTaxAmount.Should().Be(200m);
        result.LuxuryTaxAmount.Should().Be(110m); // Round((2000 + 200) * 5 / 100, 2)
        result.GrandTotal.Should().Be(2310m);
    }

    [Fact]
    public void CalculateTaxes_WithPartialPayment_CalculatesBalance()
    {
        var result = _service.CalculateTaxes(1000m, 1, 0m, 0m, 10m, 5m, 500m);

        result.GrandTotal.Should().Be(1155m); // 1000 + 100 + 55
        result.Balance.Should().Be(655m); // 1155 - 500
    }

    [Fact]
    public void CalculateTaxes_ZeroCharges_ReturnsZeros()
    {
        var result = _service.CalculateTaxes(0m, 1, 0m, 0m, 0m, 0m, 0m);

        result.TotalRoomCharges.Should().Be(0m);
        result.GrandTotal.Should().Be(0m);
        result.Balance.Should().Be(0m);
    }

    [Fact]
    public void CalculateTaxes_FullPayment_ZeroBalance()
    {
        var result = _service.CalculateTaxes(1000m, 1, 0m, 0m, 0m, 0m, 1000m);

        result.GrandTotal.Should().Be(1000m);
        result.Balance.Should().Be(0m);
    }

    [Fact]
    public void CalculateTaxes_OnlyOtherCharges_CalculatesCorrectly()
    {
        var result = _service.CalculateTaxes(0m, 1, 500m, 0m, 10m, 5m, 0m);

        result.TotalRoomCharges.Should().Be(0m);
        result.SubTotal.Should().Be(500m);
        result.ServiceTaxAmount.Should().Be(50m);
        result.LuxuryTaxAmount.Should().Be(27.50m); // Round((500 + 50) * 5 / 100, 2)
        result.GrandTotal.Should().Be(577.50m);
    }

    [Fact]
    public void CalculateTaxes_100PercentDiscount_ZeroSubtotal()
    {
        var result = _service.CalculateTaxes(1000m, 1, 0m, 100m, 10m, 5m, 0m);

        result.Discount.Should().Be(1000m);
        result.SubTotal.Should().Be(0m);
        result.GrandTotal.Should().Be(0m);
    }

    // --- CheckInAsync Tests ---

    [Fact]
    public async Task CheckInAsync_ValidRequest_CreatesCheckIn()
    {
        _context.Set<Room>().Add(new Room { RoomNo = "101", RoomType = "AC", RoomCharges = 1000m });
        _context.Set<Guest>().Add(new Guest { GuestID = "G-123456", GuestName = "John Doe" });
        await _context.SaveChangesAsync();

        var request = new CreateCheckInRequest(
            "G-123456", "101", 1000m,
            DateTime.Today, DateTime.Today.AddDays(2), 2, 0,
            "John Doe", "123 St", "City", "1234567890",
            "Passport", "P123", 0m, 0m, 0m, 0m, 0m, "No", "INR", "");

        var result = await _service.CheckInAsync(request);

        result.Should().NotBeNull();
        result.GuestID.Should().Be("G-123456");
        result.RoomNo.Should().Be("101");
        result.Status.Should().Be("Checked In");
        result.NoOfDays.Should().Be(2);
    }

    [Fact]
    public async Task CheckInAsync_EmptyRoomNo_ThrowsArgumentException()
    {
        var request = new CreateCheckInRequest(
            "G-123456", "", 1000m,
            DateTime.Today, DateTime.Today.AddDays(1), 2, 0,
            "John Doe", "123 St", "City", "1234567890",
            "Passport", "P123", 0m, 0m, 0m, 0m, 0m, "No", "INR", "");

        var act = async () => await _service.CheckInAsync(request);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*room no*");
    }

    [Fact]
    public async Task CheckInAsync_EmptyGuestID_ThrowsArgumentException()
    {
        var request = new CreateCheckInRequest(
            "", "101", 1000m,
            DateTime.Today, DateTime.Today.AddDays(1), 2, 0,
            "John Doe", "123 St", "City", "1234567890",
            "Passport", "P123", 0m, 0m, 0m, 0m, 0m, "No", "INR", "");

        var act = async () => await _service.CheckInAsync(request);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*guest*");
    }

    [Fact]
    public async Task CheckInAsync_DateOutBeforeDateIn_ThrowsArgumentException()
    {
        var request = new CreateCheckInRequest(
            "G-123456", "101", 1000m,
            DateTime.Today.AddDays(2), DateTime.Today, 2, 0,
            "John Doe", "123 St", "City", "1234567890",
            "Passport", "P123", 0m, 0m, 0m, 0m, 0m, "No", "INR", "");

        var act = async () => await _service.CheckInAsync(request);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*date*");
    }

    [Fact]
    public async Task CheckInAsync_TotalPaidExceedsGrandTotal_ThrowsArgumentException()
    {
        _context.Set<Room>().Add(new Room { RoomNo = "101", RoomType = "AC", RoomCharges = 1000m });
        _context.Set<Guest>().Add(new Guest { GuestID = "G-123456", GuestName = "John Doe" });
        await _context.SaveChangesAsync();

        var request = new CreateCheckInRequest(
            "G-123456", "101", 1000m,
            DateTime.Today, DateTime.Today.AddDays(1), 2, 0,
            "John Doe", "123 St", "City", "1234567890",
            "Passport", "P123", 0m, 0m, 0m, 0m, 999999m, "No", "INR", "");

        var act = async () => await _service.CheckInAsync(request);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*grand total*");
    }

    // --- IsRoomAvailableAsync Tests ---

    [Fact]
    public async Task IsRoomAvailableAsync_NoExistingBookings_ReturnsTrue()
    {
        var result = await _service.IsRoomAvailableAsync("101", DateTime.Today, DateTime.Today.AddDays(2));
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsRoomAvailableAsync_OverlappingCheckIn_ReturnsFalse()
    {
        _context.Set<CheckInRoom>().Add(new CheckInRoom
        {
            RoomNo = "101", GuestID = "G-111111", GuestName = "Test",
            DateIN = DateTime.Today, DateOUT = DateTime.Today.AddDays(5),
            Status = "Checked In"
        });
        await _context.SaveChangesAsync();

        var result = await _service.IsRoomAvailableAsync("101", DateTime.Today.AddDays(1), DateTime.Today.AddDays(3));
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsRoomAvailableAsync_OverlappingReservation_ReturnsFalse()
    {
        _context.Set<Reservation>().Add(new Reservation
        {
            RoomNo = "101", GuestID = "G-111111", GuestName = "Test",
            DateIN = DateTime.Today, DateOUT = DateTime.Today.AddDays(5),
            Status = "Reserved"
        });
        await _context.SaveChangesAsync();

        var result = await _service.IsRoomAvailableAsync("101", DateTime.Today.AddDays(1), DateTime.Today.AddDays(3));
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsRoomAvailableAsync_NonOverlappingDates_ReturnsTrue()
    {
        _context.Set<CheckInRoom>().Add(new CheckInRoom
        {
            RoomNo = "101", GuestID = "G-111111", GuestName = "Test",
            DateIN = DateTime.Today, DateOUT = DateTime.Today.AddDays(2),
            Status = "Checked In"
        });
        await _context.SaveChangesAsync();

        var result = await _service.IsRoomAvailableAsync("101", DateTime.Today.AddDays(3), DateTime.Today.AddDays(5));
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsRoomAvailableAsync_CheckedOutRoom_ReturnsTrue()
    {
        _context.Set<CheckInRoom>().Add(new CheckInRoom
        {
            RoomNo = "101", GuestID = "G-111111", GuestName = "Test",
            DateIN = DateTime.Today, DateOUT = DateTime.Today.AddDays(5),
            Status = "Checked Out"
        });
        await _context.SaveChangesAsync();

        var result = await _service.IsRoomAvailableAsync("101", DateTime.Today.AddDays(1), DateTime.Today.AddDays(3));
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsRoomAvailableAsync_DifferentRoom_ReturnsTrue()
    {
        _context.Set<CheckInRoom>().Add(new CheckInRoom
        {
            RoomNo = "102", GuestID = "G-111111", GuestName = "Test",
            DateIN = DateTime.Today, DateOUT = DateTime.Today.AddDays(5),
            Status = "Checked In"
        });
        await _context.SaveChangesAsync();

        var result = await _service.IsRoomAvailableAsync("101", DateTime.Today, DateTime.Today.AddDays(3));
        result.Should().BeTrue();
    }
}
