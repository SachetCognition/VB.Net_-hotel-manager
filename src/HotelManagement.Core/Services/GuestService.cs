using System.Security.Cryptography;
using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Core.Services;

public class GuestService : IGuestService
{
    private readonly DbContext _context;

    public GuestService(DbContext context)
    {
        _context = context;
    }

    public async Task<GuestResponse> CreateAsync(CreateGuestRequest request)
    {
        ValidateGuest(request.GuestName, request.Address, request.City, request.ContactNo);

        var guest = new Guest
        {
            GuestID = GenerateGuestId(),
            GuestName = request.GuestName,
            Address = request.Address,
            City = request.City,
            ContactNo = request.ContactNo,
            IDType = request.IDType,
            IDNumber = request.IDNumber,
            Notes = request.Notes ?? string.Empty
        };

        _context.Set<Guest>().Add(guest);
        await _context.SaveChangesAsync();
        return MapToResponse(guest);
    }

    public async Task<GuestResponse?> GetByIdAsync(string guestId)
    {
        var guest = await _context.Set<Guest>().FindAsync(guestId);
        return guest == null ? null : MapToResponse(guest);
    }

    public async Task<IEnumerable<GuestResponse>> GetAllAsync(string? search = null)
    {
        var query = _context.Set<Guest>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(g => g.GuestName.StartsWith(search));

        var guests = await query.OrderBy(g => g.GuestName).ToListAsync();
        return guests.Select(MapToResponse);
    }

    public async Task<GuestResponse> UpdateAsync(string guestId, UpdateGuestRequest request)
    {
        var guest = await _context.Set<Guest>().FindAsync(guestId)
            ?? throw new KeyNotFoundException($"Guest {guestId} not found");

        ValidateGuest(request.GuestName, request.Address, request.City, request.ContactNo);

        guest.GuestName = request.GuestName;
        guest.Address = request.Address;
        guest.City = request.City;
        guest.ContactNo = request.ContactNo;
        guest.IDType = request.IDType;
        guest.IDNumber = request.IDNumber;
        guest.Notes = request.Notes ?? string.Empty;

        await _context.SaveChangesAsync();
        return MapToResponse(guest);
    }

    public async Task DeleteAsync(string guestId)
    {
        var guest = await _context.Set<Guest>().FindAsync(guestId)
            ?? throw new KeyNotFoundException($"Guest {guestId} not found");

        _context.Set<Guest>().Remove(guest);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Generates a unique Guest ID in the format "G-" + 6 random digits (1-9).
    /// Mirrors legacy: txtGuestID.Text = "G-" &amp; GetUniqueKey(6) using RNGCryptoServiceProvider with chars "123456789"
    /// </summary>
    public string GenerateGuestId()
    {
        return "G-" + GetUniqueKey(6);
    }

    private static string GetUniqueKey(int size)
    {
        const string chars = "123456789";
        var data = new byte[size];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(data);
        var result = new char[size];
        for (int i = 0; i < size; i++)
        {
            result[i] = chars[data[i] % chars.Length];
        }
        return new string(result);
    }

    private static void ValidateGuest(string name, string address, string city, string contactNo)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Please enter guest name");
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Please enter guest address");
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("Please enter guest city");
        if (string.IsNullOrWhiteSpace(contactNo))
            throw new ArgumentException("Please enter contact number");
        if (!contactNo.All(char.IsDigit))
            throw new ArgumentException("Contact number must be numeric only");
    }

    private static GuestResponse MapToResponse(Guest g) =>
        new(g.GuestID, g.GuestName, g.Address, g.City,
            g.ContactNo, g.IDType, g.IDNumber, g.Notes);
}
