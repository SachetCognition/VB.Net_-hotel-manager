using FluentAssertions;
using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Services;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HotelManagement.Tests.Integration;

public class GuestIntegrationTests : IDisposable
{
    private readonly HotelDbContext _context;
    private readonly GuestService _guestService;

    public GuestIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new HotelDbContext(options);
        _guestService = new GuestService(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task FullGuestLifecycle_CreateReadUpdateDelete()
    {
        // Create
        var created = await _guestService.CreateAsync(
            new CreateGuestRequest("Rajesh Kumar", "123 MG Road", "Mumbai", "9876543210", "Passport", "P123", "VIP"));
        created.GuestID.Should().StartWith("G-");
        created.GuestName.Should().Be("Rajesh Kumar");

        // Read
        var fetched = await _guestService.GetByIdAsync(created.GuestID);
        fetched.Should().NotBeNull();
        fetched!.GuestName.Should().Be("Rajesh Kumar");

        // Update
        var updated = await _guestService.UpdateAsync(created.GuestID,
            new UpdateGuestRequest("Rajesh K.", "456 MG Road", "Delhi", "8765432109", "Aadhar", "A456", "Updated"));
        updated.GuestName.Should().Be("Rajesh K.");
        updated.City.Should().Be("Delhi");

        // Delete
        await _guestService.DeleteAsync(created.GuestID);
        var deleted = await _guestService.GetByIdAsync(created.GuestID);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task GuestSearch_ByNamePrefix_ReturnsMatchingResults()
    {
        await _guestService.CreateAsync(
            new CreateGuestRequest("Alice Johnson", "A St", "City A", "1111111111", "", "", ""));
        await _guestService.CreateAsync(
            new CreateGuestRequest("Alice Smith", "B St", "City B", "2222222222", "", "", ""));
        await _guestService.CreateAsync(
            new CreateGuestRequest("Bob Wilson", "C St", "City C", "3333333333", "", "", ""));

        var results = await _guestService.GetAllAsync("Alice");
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(g => g.GuestName.Should().StartWith("Alice"));
    }
}
