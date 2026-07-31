using FluentAssertions;
using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Services;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HotelManagement.Tests.Unit;

public class RoomServiceTests : IDisposable
{
    private readonly HotelDbContext _context;
    private readonly RoomService _service;

    public RoomServiceTests()
    {
        var options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new HotelDbContext(options);
        _service = new RoomService(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task CreateAsync_ValidRoom_ReturnsResponse()
    {
        var request = new CreateRoomRequest("101", "AC", 2000m);
        var result = await _service.CreateAsync(request);

        result.RoomNo.Should().Be("101");
        result.RoomType.Should().Be("AC");
        result.RoomCharges.Should().Be(2000m);
    }

    [Fact]
    public async Task CreateAsync_DuplicateRoomNo_ThrowsInvalidOperation()
    {
        _context.Set<Room>().Add(new Room { RoomNo = "101", RoomType = "AC", RoomCharges = 1000m });
        await _context.SaveChangesAsync();

        var request = new CreateRoomRequest("101", "Deluxe", 3000m);
        var act = async () => await _service.CreateAsync(request);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already exists*");
    }

    [Fact]
    public async Task CreateAsync_EmptyRoomNo_ThrowsArgumentException()
    {
        var request = new CreateRoomRequest("", "AC", 2000m);
        var act = async () => await _service.CreateAsync(request);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*room number*");
    }

    [Fact]
    public async Task CreateAsync_InvalidRoomType_ThrowsArgumentException()
    {
        var request = new CreateRoomRequest("101", "Invalid", 2000m);
        var act = async () => await _service.CreateAsync(request);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Room type*");
    }

    [Theory]
    [InlineData("AC")]
    [InlineData("Non-AC")]
    [InlineData("Deluxe")]
    [InlineData("Suite")]
    public async Task CreateAsync_ValidRoomTypes_Succeed(string roomType)
    {
        var request = new CreateRoomRequest($"R-{roomType}", roomType, 1000m);
        var result = await _service.CreateAsync(request);
        result.RoomType.Should().Be(roomType);
    }

    [Fact]
    public async Task CreateAsync_ZeroCharges_ThrowsArgumentException()
    {
        var request = new CreateRoomRequest("101", "AC", 0m);
        var act = async () => await _service.CreateAsync(request);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*positive*");
    }

    [Fact]
    public async Task CreateAsync_NegativeCharges_ThrowsArgumentException()
    {
        var request = new CreateRoomRequest("101", "AC", -100m);
        var act = async () => await _service.CreateAsync(request);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*positive*");
    }

    [Fact]
    public async Task GetAvailableRoomsAsync_ExcludesCheckedInRooms()
    {
        _context.Set<Room>().Add(new Room { RoomNo = "101", RoomType = "AC", RoomCharges = 1000m });
        _context.Set<Room>().Add(new Room { RoomNo = "102", RoomType = "AC", RoomCharges = 1000m });
        _context.Set<CheckInRoom>().Add(new CheckInRoom
        {
            RoomNo = "101", GuestID = "G-111111", GuestName = "Test",
            DateIN = DateTime.Today, DateOUT = DateTime.Today.AddDays(3),
            Status = "Checked In"
        });
        await _context.SaveChangesAsync();

        var available = await _service.GetAvailableRoomsAsync(DateTime.Today, DateTime.Today.AddDays(2));
        available.Should().HaveCount(1);
        available.First().RoomNo.Should().Be("102");
    }

    [Fact]
    public async Task GetAvailableRoomsAsync_ExcludesReservedRooms()
    {
        _context.Set<Room>().Add(new Room { RoomNo = "101", RoomType = "AC", RoomCharges = 1000m });
        _context.Set<Room>().Add(new Room { RoomNo = "102", RoomType = "AC", RoomCharges = 1000m });
        _context.Set<Reservation>().Add(new Reservation
        {
            RoomNo = "101", GuestID = "G-111111", GuestName = "Test",
            DateIN = DateTime.Today, DateOUT = DateTime.Today.AddDays(3),
            Status = "Reserved"
        });
        await _context.SaveChangesAsync();

        var available = await _service.GetAvailableRoomsAsync(DateTime.Today, DateTime.Today.AddDays(2));
        available.Should().HaveCount(1);
        available.First().RoomNo.Should().Be("102");
    }

    [Fact]
    public async Task DeleteAsync_NonExisting_ThrowsKeyNotFound()
    {
        var act = async () => await _service.DeleteAsync("999");
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_UpdatesRoom()
    {
        _context.Set<Room>().Add(new Room { RoomNo = "101", RoomType = "AC", RoomCharges = 1000m });
        await _context.SaveChangesAsync();

        var request = new UpdateRoomRequest("Deluxe", 5000m);
        var result = await _service.UpdateAsync("101", request);

        result.RoomType.Should().Be("Deluxe");
        result.RoomCharges.Should().Be(5000m);
    }
}
