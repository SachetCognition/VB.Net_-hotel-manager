using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Core.Services;

public class RoomService : IRoomService
{
    private readonly DbContext _context;

    public RoomService(DbContext context)
    {
        _context = context;
    }

    public async Task<RoomResponse> CreateAsync(CreateRoomRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RoomNo))
            throw new ArgumentException("Please enter room number");

        var existing = await _context.Set<Room>().FindAsync(request.RoomNo);
        if (existing != null)
            throw new InvalidOperationException($"Room number '{request.RoomNo}' already exists");

        ValidateRoomType(request.RoomType);
        if (request.RoomCharges <= 0)
            throw new ArgumentException("Room charges must be positive");

        var room = new Room
        {
            RoomNo = request.RoomNo,
            RoomType = request.RoomType,
            RoomCharges = request.RoomCharges
        };

        _context.Set<Room>().Add(room);
        await _context.SaveChangesAsync();
        return MapToResponse(room);
    }

    public async Task<RoomResponse?> GetByIdAsync(string roomNo)
    {
        var room = await _context.Set<Room>().FindAsync(roomNo);
        return room == null ? null : MapToResponse(room);
    }

    public async Task<IEnumerable<RoomResponse>> GetAllAsync(string? search = null)
    {
        var query = _context.Set<Room>().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(r => r.RoomNo.StartsWith(search));

        var rooms = await query.OrderBy(r => r.RoomNo).ToListAsync();
        return rooms.Select(MapToResponse);
    }

    public async Task<RoomResponse> UpdateAsync(string roomNo, UpdateRoomRequest request)
    {
        var room = await _context.Set<Room>().FindAsync(roomNo)
            ?? throw new KeyNotFoundException($"Room {roomNo} not found");

        ValidateRoomType(request.RoomType);
        if (request.RoomCharges <= 0)
            throw new ArgumentException("Room charges must be positive");

        room.RoomType = request.RoomType;
        room.RoomCharges = request.RoomCharges;
        await _context.SaveChangesAsync();
        return MapToResponse(room);
    }

    public async Task DeleteAsync(string roomNo)
    {
        var room = await _context.Set<Room>().FindAsync(roomNo)
            ?? throw new KeyNotFoundException($"Room {roomNo} not found");

        _context.Set<Room>().Remove(room);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<RoomResponse>> GetAvailableRoomsAsync(DateTime dateIn, DateTime dateOut)
    {
        var allRooms = await _context.Set<Room>().ToListAsync();
        var checkedInRooms = await _context.Set<CheckInRoom>()
            .Where(c => c.Status == "Checked In" && c.DateIN < dateOut && c.DateOUT > dateIn)
            .Select(c => c.RoomNo)
            .ToListAsync();
        var reservedRooms = await _context.Set<Reservation>()
            .Where(r => r.Status == "Reserved" && r.DateIN < dateOut && r.DateOUT > dateIn)
            .Select(r => r.RoomNo)
            .ToListAsync();

        var unavailable = checkedInRooms.Union(reservedRooms).ToHashSet();
        return allRooms.Where(r => !unavailable.Contains(r.RoomNo)).Select(MapToResponse);
    }

    private static void ValidateRoomType(string roomType)
    {
        var validTypes = new[] { "AC", "Non-AC", "Deluxe", "Suite" };
        if (!validTypes.Contains(roomType))
            throw new ArgumentException($"Room type must be one of: {string.Join(", ", validTypes)}");
    }

    private static RoomResponse MapToResponse(Room r) =>
        new(r.RoomNo, r.RoomType, r.RoomCharges);
}
