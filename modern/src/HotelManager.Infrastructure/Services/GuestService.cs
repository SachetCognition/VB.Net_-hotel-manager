using System.Security.Cryptography;
using HotelManager.Application.Interfaces;
using HotelManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.Infrastructure.Services;

public class GuestService : IGuestService
{
    public static readonly IReadOnlyList<string> IdTypes =
        ["Keble  ID", "College ID/University ID", "Office ID", "Passport", "Driver Licence", "Any Other"];

    private readonly HotelDbContext _db;

    public GuestService(HotelDbContext db) => _db = db;

    public async Task<IReadOnlyList<Guest>> GetAllAsync(string? search = null)
    {
        var query = _db.Guests.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(g => g.GuestName != null && g.GuestName.StartsWith(search));
        return await query.OrderBy(g => g.GuestName).ToListAsync();
    }

    public async Task<Guest?> GetAsync(string guestId) =>
        await _db.Guests.AsNoTracking().FirstOrDefaultAsync(g => g.GuestID == guestId);

    public async Task<string> GenerateGuestIdAsync()
    {
        string id;
        do
        {
            id = "G-" + GenerateDigits(6);
        } while (await _db.Guests.AnyAsync(g => g.GuestID == id));
        return id;
    }

    private static string GenerateDigits(int length)
    {
        const string chars = "123456789";
        var chars2 = new char[length];
        for (var i = 0; i < length; i++)
            chars2[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
        return new string(chars2);
    }

    public async Task CreateAsync(Guest guest)
    {
        ValidateGuest(guest);
        if (string.IsNullOrWhiteSpace(guest.GuestID))
            guest.GuestID = await GenerateGuestIdAsync();
        _db.Guests.Add(guest);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Guest guest)
    {
        ValidateGuest(guest);
        var existing = await _db.Guests.FirstAsync(g => g.GuestID == guest.GuestID);
        existing.GuestName = guest.GuestName;
        existing.Address = guest.Address;
        existing.City = guest.City;
        existing.ContactNo = guest.ContactNo;
        existing.IDType = guest.IDType;
        existing.IDNumber = guest.IDNumber;
        existing.Notes = guest.Notes;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(string guestId)
    {
        var existing = await _db.Guests.FirstOrDefaultAsync(g => g.GuestID == guestId)
            ?? throw new InvalidOperationException("No record found");
        _db.Guests.Remove(existing);
        await _db.SaveChangesAsync();
    }

    private static void ValidateGuest(Guest guest)
    {
        if (string.IsNullOrWhiteSpace(guest.GuestName))
            throw new ArgumentException("Please enter guest name", nameof(guest));
        if (string.IsNullOrWhiteSpace(guest.Address))
            throw new ArgumentException("Please enter guest address", nameof(guest));
        if (string.IsNullOrWhiteSpace(guest.City))
            throw new ArgumentException("Please enter guest city", nameof(guest));
        if (string.IsNullOrWhiteSpace(guest.ContactNo))
            throw new ArgumentException("Please enter guest contact no.", nameof(guest));
        if (string.IsNullOrWhiteSpace(guest.IDType))
            throw new ArgumentException("Please select id type", nameof(guest));
        if (string.IsNullOrWhiteSpace(guest.IDNumber))
            throw new ArgumentException("Please enter id number", nameof(guest));
    }
}
