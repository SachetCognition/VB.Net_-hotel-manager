using HotelManager.Application.Interfaces;
using HotelManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.Infrastructure.Services;

public class RoomService : IRoomService
{
    public static readonly IReadOnlyList<string> RoomTypes =
        ["Excutive Room", "Standard Room", "Twin Room", "Delux Room", "Family Room"];

    private readonly HotelDbContext _db;

    public RoomService(HotelDbContext db) => _db = db;

    public async Task<IReadOnlyList<Room>> GetRoomsAsync(string? search = null)
    {
        var query = _db.Rooms.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(r => r.RoomNo.StartsWith(search)).OrderBy(r => r.RoomType);
        else
            query = query.OrderBy(r => r.RoomNo);
        return await query.ToListAsync();
    }

    public async Task<Room?> GetRoomAsync(string roomNo) =>
        await _db.Rooms.AsNoTracking().FirstOrDefaultAsync(r => r.RoomNo == roomNo);

    public async Task CreateRoomAsync(Room room)
    {
        ValidateRoom(room);
        if (await _db.Rooms.AnyAsync(r => r.RoomNo == room.RoomNo))
            throw new InvalidOperationException("Room Number Already Exists");
        _db.Rooms.Add(room);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateRoomAsync(Room room)
    {
        ValidateRoom(room);
        var existing = await _db.Rooms.FirstAsync(r => r.RoomNo == room.RoomNo);
        existing.RoomType = room.RoomType;
        existing.RoomCharges = room.RoomCharges;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteRoomAsync(string roomNo)
    {
        var existing = await _db.Rooms.FirstOrDefaultAsync(r => r.RoomNo == roomNo)
            ?? throw new InvalidOperationException("No record found");
        _db.Rooms.Remove(existing);
        await _db.SaveChangesAsync();
    }

    private static void ValidateRoom(Room room)
    {
        if (string.IsNullOrWhiteSpace(room.RoomNo))
            throw new ArgumentException("Please enter room no.", nameof(room));
        if (string.IsNullOrWhiteSpace(room.RoomType))
            throw new ArgumentException("Please select room type", nameof(room));
        if (room.RoomCharges is null)
            throw new ArgumentException("Please enter room charges", nameof(room));
    }

    public async Task<IReadOnlyList<Hall>> GetHallsAsync() =>
        await _db.Halls.AsNoTracking().OrderBy(h => h.ID).ToListAsync();

    public async Task<Hall> SaveHallAsync(Hall hall)
    {
        if (hall.Charges is null)
            throw new ArgumentException("Please enter charges", nameof(hall));
        if (hall.ID == 0)
        {
            _db.Halls.Add(hall);
        }
        else
        {
            var existing = await _db.Halls.FirstAsync(h => h.ID == hall.ID);
            existing.Charges = hall.Charges;
            existing.HallName = hall.HallName;
            hall = existing;
        }
        await _db.SaveChangesAsync();
        return hall;
    }

    public async Task DeleteHallAsync(int id)
    {
        var existing = await _db.Halls.FirstOrDefaultAsync(h => h.ID == id)
            ?? throw new InvalidOperationException("No record found");
        _db.Halls.Remove(existing);
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<Garden>> GetGardensAsync() =>
        await _db.Gardens.AsNoTracking().OrderBy(g => g.ID).ToListAsync();

    public async Task<Garden> SaveGardenAsync(Garden garden)
    {
        if (garden.Charges is null)
            throw new ArgumentException("Please enter charges", nameof(garden));
        if (garden.ID == 0)
        {
            _db.Gardens.Add(garden);
        }
        else
        {
            var existing = await _db.Gardens.FirstAsync(g => g.ID == garden.ID);
            existing.Charges = garden.Charges;
            garden = existing;
        }
        await _db.SaveChangesAsync();
        return garden;
    }

    public async Task DeleteGardenAsync(int id)
    {
        var existing = await _db.Gardens.FirstOrDefaultAsync(g => g.ID == id)
            ?? throw new InvalidOperationException("No record found");
        _db.Gardens.Remove(existing);
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<ExtraBed>> GetExtraBedsAsync() =>
        await _db.ExtraBeds.AsNoTracking().OrderBy(b => b.ID).ToListAsync();

    public async Task<ExtraBed> SaveExtraBedAsync(ExtraBed bed)
    {
        if (bed.Charges is null)
            throw new ArgumentException("Please enter charges", nameof(bed));
        if (bed.ID == 0)
        {
            _db.ExtraBeds.Add(bed);
        }
        else
        {
            var existing = await _db.ExtraBeds.FirstAsync(b => b.ID == bed.ID);
            existing.Charges = bed.Charges;
            bed = existing;
        }
        await _db.SaveChangesAsync();
        return bed;
    }

    public async Task DeleteExtraBedAsync(int id)
    {
        var existing = await _db.ExtraBeds.FirstOrDefaultAsync(b => b.ID == id)
            ?? throw new InvalidOperationException("No record found");
        _db.ExtraBeds.Remove(existing);
        await _db.SaveChangesAsync();
    }
}
