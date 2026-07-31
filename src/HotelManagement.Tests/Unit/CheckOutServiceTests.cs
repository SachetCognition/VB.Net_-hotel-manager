using FluentAssertions;
using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Services;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HotelManagement.Tests.Unit;

public class CheckOutServiceTests : IDisposable
{
    private readonly HotelDbContext _context;
    private readonly CheckInService _checkInService;
    private readonly CheckOutService _service;

    public CheckOutServiceTests()
    {
        var options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new HotelDbContext(options);
        _checkInService = new CheckInService(_context);
        _service = new CheckOutService(_context, _checkInService);
    }

    public void Dispose() => _context.Dispose();

    // --- CalculateEducationCess Tests ---

    [Fact]
    public void CalculateEducationCess_StandardValues_MatchesLegacyFormula()
    {
        // ServiceTaxPer=14%, ServiceTaxAmount=819
        var result = _service.CalculateEducationCess(14m, 819m);

        // EducessTax = (14 * 2) / 100 = 0.28
        result.EducessTax.Should().Be(0.28m);
        // EducessTaxAmount = (819 * 0.28) / 100 = 2.29 (rounded to 2 decimals)
        result.EducessTaxAmount.Should().Be(2.29m);
        // HEducessTax = (14 * 1) / 100 = 0.14
        result.HEducessTax.Should().Be(0.14m);
        // HEducessTaxAmount = (819 * 0.14) / 100 = 1.15 (rounded to 2 decimals)
        result.HEducessTaxAmount.Should().Be(1.15m);
    }

    [Fact]
    public void CalculateEducationCess_ZeroServiceTax_ReturnsZeros()
    {
        var result = _service.CalculateEducationCess(0m, 0m);

        result.EducessTax.Should().Be(0m);
        result.EducessTaxAmount.Should().Be(0m);
        result.HEducessTax.Should().Be(0m);
        result.HEducessTaxAmount.Should().Be(0m);
    }

    [Fact]
    public void CalculateEducationCess_HighServiceTax_CalculatesCorrectly()
    {
        var result = _service.CalculateEducationCess(18m, 1800m);

        // EducessTax = (18 * 2) / 100 = 0.36
        result.EducessTax.Should().Be(0.36m);
        // EducessTaxAmount = (1800 * 0.36) / 100 = 6.48
        result.EducessTaxAmount.Should().Be(6.48m);
        // HEducessTax = (18 * 1) / 100 = 0.18
        result.HEducessTax.Should().Be(0.18m);
        // HEducessTaxAmount = (1800 * 0.18) / 100 = 3.24
        result.HEducessTaxAmount.Should().Be(3.24m);
    }

    [Fact]
    public void CalculateEducationCess_SmallValues_RoundsCorrectly()
    {
        var result = _service.CalculateEducationCess(5m, 50m);

        // EducessTax = (5 * 2) / 100 = 0.10
        result.EducessTax.Should().Be(0.10m);
        // EducessTaxAmount = (50 * 0.10) / 100 = 0.05
        result.EducessTaxAmount.Should().Be(0.05m);
        // HEducessTax = (5 * 1) / 100 = 0.05
        result.HEducessTax.Should().Be(0.05m);
        // HEducessTaxAmount = (50 * 0.05) / 100 = 0.025 -> Round to 0.02
        result.HEducessTaxAmount.Should().Be(0.02m);
    }

    // --- CheckOutAsync Tests ---

    [Fact]
    public async Task CheckOutAsync_ValidCheckIn_CreatesCheckOut()
    {
        var checkIn = new CheckInRoom
        {
            RoomNo = "101", GuestID = "G-123456", GuestName = "John Doe",
            RoomCharges = 1000m, DateIN = DateTime.Today, DateOUT = DateTime.Today.AddDays(2),
            NoOfAdults = 2, NoOfKids = 0, NoOfDays = 2,
            TotalRoomCharges = 2000m, OtherCharges = 0m,
            DiscountPer = 0m, Discount = 0m, SubTotal = 2000m,
            ServiceTaxPer = 10m, ServiceTaxAmount = 200m,
            LuxuryTaxPer = 5m, LuxuryTaxAmount = 110m,
            GrandTotal = 2310m, TotalPaid = 1000m, Balance = 1310m,
            ExtraBed = "No", Currency = "INR", Status = "Checked In"
        };
        _context.Set<CheckInRoom>().Add(checkIn);
        await _context.SaveChangesAsync();

        var request = new CheckOutRequest(checkIn.ID, 2310m, "INR");
        var result = await _service.CheckOutAsync(request);

        result.Should().NotBeNull();
        result.Status.Should().Be("Checked Out");
        result.GuestID.Should().Be("G-123456");
        result.BillNo.Should().StartWith("B");
    }

    [Fact]
    public async Task CheckOutAsync_NotCheckedIn_ThrowsInvalidOperation()
    {
        var checkIn = new CheckInRoom
        {
            RoomNo = "101", GuestID = "G-123456", GuestName = "John Doe",
            RoomCharges = 1000m, DateIN = DateTime.Today, DateOUT = DateTime.Today.AddDays(2),
            Status = "Checked Out"
        };
        _context.Set<CheckInRoom>().Add(checkIn);
        await _context.SaveChangesAsync();

        var request = new CheckOutRequest(checkIn.ID, 1000m, "INR");
        var act = async () => await _service.CheckOutAsync(request);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*not currently checked in*");
    }

    [Fact]
    public async Task CheckOutAsync_NonExistentCheckIn_ThrowsKeyNotFound()
    {
        var request = new CheckOutRequest(999, 1000m, "INR");
        var act = async () => await _service.CheckOutAsync(request);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task CheckOutAsync_EmptyCurrency_ThrowsArgumentException()
    {
        var checkIn = new CheckInRoom
        {
            RoomNo = "101", GuestID = "G-123456", GuestName = "John Doe",
            RoomCharges = 1000m, DateIN = DateTime.Today, DateOUT = DateTime.Today.AddDays(2),
            Status = "Checked In"
        };
        _context.Set<CheckInRoom>().Add(checkIn);
        await _context.SaveChangesAsync();

        var request = new CheckOutRequest(checkIn.ID, 1000m, "");
        var act = async () => await _service.CheckOutAsync(request);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*currency*");
    }

    [Fact]
    public async Task CheckOutAsync_UpdatesCheckInStatus()
    {
        var checkIn = new CheckInRoom
        {
            RoomNo = "101", GuestID = "G-123456", GuestName = "John Doe",
            RoomCharges = 1000m, DateIN = DateTime.Today, DateOUT = DateTime.Today.AddDays(1),
            Status = "Checked In"
        };
        _context.Set<CheckInRoom>().Add(checkIn);
        await _context.SaveChangesAsync();

        var request = new CheckOutRequest(checkIn.ID, 1000m, "INR");
        await _service.CheckOutAsync(request);

        var updated = await _context.Set<CheckInRoom>().FindAsync(checkIn.ID);
        updated!.Status.Should().Be("Checked Out");
    }
}
