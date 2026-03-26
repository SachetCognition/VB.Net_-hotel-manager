using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Core.Services;

public class ReservationService : IReservationService
{
    private readonly DbContext _context;

    public ReservationService(DbContext context)
    {
        _context = context;
    }

    public async Task<ReservationResponse> CreateAsync(CreateReservationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RoomNo))
            throw new ArgumentException("Please select room");
        if (string.IsNullOrWhiteSpace(request.GuestID))
            throw new ArgumentException("Please select guest");
        if (request.DateOUT < request.DateIN)
            throw new ArgumentException("Check-out date cannot be before check-in date");

        // Check for date overlap in Temp_Reservation
        if (await CheckDateOverlapAsync(request.RoomNo, request.DateIN, request.DateOUT))
            throw new InvalidOperationException("Room is already reserved for the selected date range");

        var reservation = new Reservation
        {
            GuestID = request.GuestID,
            GuestName = request.GuestName,
            RoomNo = request.RoomNo,
            RoomType = request.RoomType,
            RoomCharges = request.RoomCharges,
            DateIN = request.DateIN,
            DateOUT = request.DateOUT,
            NoOfAdults = request.NoOfAdults,
            NoOfKids = request.NoOfKids,
            Status = "Reserved",
            Currency = request.Currency ?? string.Empty,
            Notes = request.Notes ?? string.Empty
        };

        _context.Set<Reservation>().Add(reservation);
        await _context.SaveChangesAsync();
        return MapToResponse(reservation);
    }

    public async Task<ReservationResponse?> GetByIdAsync(int id)
    {
        var reservation = await _context.Set<Reservation>().FindAsync(id);
        return reservation == null ? null : MapToResponse(reservation);
    }

    public async Task<IEnumerable<ReservationResponse>> GetAllAsync(
        string? roomNo = null, string? guestName = null,
        DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Set<Reservation>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(roomNo))
            query = query.Where(r => r.RoomNo == roomNo);
        if (!string.IsNullOrWhiteSpace(guestName))
            query = query.Where(r => r.GuestName.StartsWith(guestName));
        if (fromDate.HasValue)
            query = query.Where(r => r.DateIN >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(r => r.DateOUT <= toDate.Value);

        var results = await query.OrderByDescending(r => r.ID).ToListAsync();
        return results.Select(MapToResponse);
    }

    public async Task<ReservationResponse> UpdateAsync(int id, CreateReservationRequest request)
    {
        var reservation = await _context.Set<Reservation>().FindAsync(id)
            ?? throw new KeyNotFoundException($"Reservation {id} not found");

        if (await CheckDateOverlapAsync(request.RoomNo, request.DateIN, request.DateOUT, id))
            throw new InvalidOperationException("Room is already reserved for the selected date range");

        reservation.GuestID = request.GuestID;
        reservation.GuestName = request.GuestName;
        reservation.RoomNo = request.RoomNo;
        reservation.RoomType = request.RoomType;
        reservation.RoomCharges = request.RoomCharges;
        reservation.DateIN = request.DateIN;
        reservation.DateOUT = request.DateOUT;
        reservation.NoOfAdults = request.NoOfAdults;
        reservation.NoOfKids = request.NoOfKids;
        reservation.Currency = request.Currency ?? string.Empty;
        reservation.Notes = request.Notes ?? string.Empty;

        await _context.SaveChangesAsync();
        return MapToResponse(reservation);
    }

    public async Task DeleteAsync(int id)
    {
        var reservation = await _context.Set<Reservation>().FindAsync(id)
            ?? throw new KeyNotFoundException($"Reservation {id} not found");

        _context.Set<Reservation>().Remove(reservation);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Date overlap detection matching legacy Temp_Reservation check.
    /// Two date ranges overlap if: Start1 &lt; End2 AND Start2 &lt; End1
    /// </summary>
    public async Task<bool> CheckDateOverlapAsync(string roomNo, DateTime dateIn, DateTime dateOut, int? excludeId = null)
    {
        return await _context.Set<Reservation>()
            .AnyAsync(r => r.RoomNo == roomNo
                && r.Status == "Reserved"
                && r.DateIN < dateOut
                && r.DateOUT > dateIn
                && (excludeId == null || r.ID != excludeId));
    }

    private static ReservationResponse MapToResponse(Reservation r) =>
        new(r.ID, r.GuestID, r.GuestName, r.RoomNo, r.RoomType,
            r.RoomCharges, r.DateIN, r.DateOUT,
            r.NoOfAdults, r.NoOfKids, r.Status, r.Currency, r.Notes);
}
