using FluentAssertions;
using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Services;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HotelManagement.Tests.Unit;

public class GuestServiceTests : IDisposable
{
    private readonly HotelDbContext _context;
    private readonly GuestService _service;

    public GuestServiceTests()
    {
        var options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new HotelDbContext(options);
        _service = new GuestService(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public void GenerateGuestId_ReturnsCorrectFormat()
    {
        var id = _service.GenerateGuestId();
        id.Should().StartWith("G-");
        id.Length.Should().Be(8); // "G-" + 6 digits
        id[2..].Should().MatchRegex("^[1-9]{6}$");
    }

    [Fact]
    public void GenerateGuestId_ProducesUniqueIds()
    {
        var ids = Enumerable.Range(0, 100).Select(_ => _service.GenerateGuestId()).ToList();
        ids.Distinct().Count().Should().Be(100);
    }

    [Fact]
    public async Task CreateAsync_ValidGuest_ReturnsResponse()
    {
        var request = new CreateGuestRequest("John Doe", "123 Main St", "Mumbai", "9876543210", "Passport", "P123456", "VIP Guest");
        var result = await _service.CreateAsync(request);

        result.GuestID.Should().StartWith("G-");
        result.GuestName.Should().Be("John Doe");
        result.City.Should().Be("Mumbai");
        result.ContactNo.Should().Be("9876543210");
    }

    [Fact]
    public async Task CreateAsync_EmptyName_ThrowsArgumentException()
    {
        var request = new CreateGuestRequest("", "123 Main St", "Mumbai", "9876543210", "Passport", "P123", "");
        var act = async () => await _service.CreateAsync(request);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*guest name*");
    }

    [Fact]
    public async Task CreateAsync_EmptyAddress_ThrowsArgumentException()
    {
        var request = new CreateGuestRequest("John", "", "Mumbai", "9876543210", "Passport", "P123", "");
        var act = async () => await _service.CreateAsync(request);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*address*");
    }

    [Fact]
    public async Task CreateAsync_EmptyCity_ThrowsArgumentException()
    {
        var request = new CreateGuestRequest("John", "123 St", "", "9876543210", "Passport", "P123", "");
        var act = async () => await _service.CreateAsync(request);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*city*");
    }

    [Fact]
    public async Task CreateAsync_NonNumericContact_ThrowsArgumentException()
    {
        var request = new CreateGuestRequest("John", "123 St", "Mumbai", "abc123", "Passport", "P123", "");
        var act = async () => await _service.CreateAsync(request);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*numeric*");
    }

    [Fact]
    public async Task GetByIdAsync_ExistingGuest_ReturnsGuest()
    {
        _context.Set<Guest>().Add(new Guest { GuestID = "G-111111", GuestName = "Test Guest", City = "Delhi" });
        await _context.SaveChangesAsync();

        var result = await _service.GetByIdAsync("G-111111");
        result.Should().NotBeNull();
        result!.GuestName.Should().Be("Test Guest");
    }

    [Fact]
    public async Task GetByIdAsync_NonExisting_ReturnsNull()
    {
        var result = await _service.GetByIdAsync("G-999999");
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ExistingGuest_RemovesGuest()
    {
        _context.Set<Guest>().Add(new Guest { GuestID = "G-111111", GuestName = "Test Guest" });
        await _context.SaveChangesAsync();

        await _service.DeleteAsync("G-111111");

        var guest = await _context.Set<Guest>().FindAsync("G-111111");
        guest.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_NonExisting_ThrowsKeyNotFound()
    {
        var act = async () => await _service.DeleteAsync("G-999999");
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetAllAsync_WithSearch_FiltersResults()
    {
        _context.Set<Guest>().Add(new Guest { GuestID = "G-111111", GuestName = "Alice Smith" });
        _context.Set<Guest>().Add(new Guest { GuestID = "G-222222", GuestName = "Bob Jones" });
        await _context.SaveChangesAsync();

        var results = await _service.GetAllAsync("Alice");
        results.Should().HaveCount(1);
        results.First().GuestName.Should().Be("Alice Smith");
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_UpdatesGuest()
    {
        _context.Set<Guest>().Add(new Guest
        {
            GuestID = "G-111111", GuestName = "John",
            Address = "Old", City = "Old", ContactNo = "1234567890"
        });
        await _context.SaveChangesAsync();

        var request = new UpdateGuestRequest("John Updated", "New Address", "New City", "9876543210", "ID Card", "ID123", "Updated");
        var result = await _service.UpdateAsync("G-111111", request);

        result.GuestName.Should().Be("John Updated");
        result.City.Should().Be("New City");
    }
}
