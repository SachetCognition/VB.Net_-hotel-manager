using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Core.Services;

/// <summary>
/// Check-In Service implementing exact legacy tax calculation formulas from frmCheckIn.vb::Compute()
/// </summary>
public class CheckInService : ICheckInService
{
    private readonly DbContext _context;

    public CheckInService(DbContext context)
    {
        _context = context;
    }

    public async Task<CheckInResponse> CheckInAsync(CreateCheckInRequest request)
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(request.RoomNo))
            throw new ArgumentException("Please select room no.");
        if (string.IsNullOrWhiteSpace(request.GuestID))
            throw new ArgumentException("Please select guest");

        // Validate dates
        if (request.DateOUT < request.DateIN)
            throw new ArgumentException("Check-out date cannot be before check-in date");

        // Check room availability (checks both Temp_Reservation AND CheckIn_Room)
        if (!await IsRoomAvailableAsync(request.RoomNo, request.DateIN, request.DateOUT))
            throw new InvalidOperationException("Room is not available for the selected dates");

        // Validate TotalPaid
        var noOfDays = CalculateNoOfDays(request.DateIN, request.DateOUT);
        var taxResult = CalculateTaxes(request.RoomCharges, noOfDays, request.OtherCharges,
            request.DiscountPer, request.ServiceTaxPer, request.LuxuryTaxPer, request.TotalPaid);

        if (request.TotalPaid > taxResult.GrandTotal)
            throw new ArgumentException("Total paid can not be more than grand total");

        var checkIn = new CheckInRoom
        {
            GuestID = request.GuestID,
            RoomNo = request.RoomNo,
            RoomCharges = request.RoomCharges,
            DateIN = request.DateIN,
            DateOUT = request.DateOUT,
            NoOfAdults = request.NoOfAdults,
            NoOfKids = request.NoOfKids,
            GuestName = request.GuestName,
            Address = request.Address,
            City = request.City,
            ContactNo = request.ContactNo,
            IDType = request.IDType,
            IDNumber = request.IDNumber,
            NoOfDays = taxResult.NoOfDays,
            TotalRoomCharges = taxResult.TotalRoomCharges,
            OtherCharges = request.OtherCharges,
            DiscountPer = request.DiscountPer,
            Discount = taxResult.Discount,
            SubTotal = taxResult.SubTotal,
            ServiceTaxPer = request.ServiceTaxPer,
            ServiceTaxAmount = taxResult.ServiceTaxAmount,
            LuxuryTaxPer = request.LuxuryTaxPer,
            LuxuryTaxAmount = taxResult.LuxuryTaxAmount,
            GrandTotal = taxResult.GrandTotal,
            TotalPaid = request.TotalPaid,
            Balance = taxResult.Balance,
            ExtraBed = request.ExtraBed ?? "No",
            Currency = request.Currency,
            Status = "Checked In",
            Notes = request.Notes ?? string.Empty
        };

        _context.Set<CheckInRoom>().Add(checkIn);
        await _context.SaveChangesAsync();
        return MapToResponse(checkIn);
    }

    public async Task<CheckInResponse?> GetByIdAsync(int id)
    {
        var checkIn = await _context.Set<CheckInRoom>().FindAsync(id);
        return checkIn == null ? null : MapToResponse(checkIn);
    }

    public async Task<IEnumerable<CheckInResponse>> GetAllAsync(
        string? roomNo = null, string? guestName = null,
        DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Set<CheckInRoom>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(roomNo))
            query = query.Where(c => c.RoomNo == roomNo);
        if (!string.IsNullOrWhiteSpace(guestName))
            query = query.Where(c => c.GuestName.StartsWith(guestName));
        if (fromDate.HasValue)
            query = query.Where(c => c.DateIN >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(c => c.DateOUT <= toDate.Value);

        var results = await query.OrderByDescending(c => c.ID).ToListAsync();
        return results.Select(MapToResponse);
    }

    public async Task DeleteAsync(int id)
    {
        var checkIn = await _context.Set<CheckInRoom>().FindAsync(id)
            ?? throw new KeyNotFoundException($"Check-in record {id} not found");

        _context.Set<CheckInRoom>().Remove(checkIn);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Calculates NoOfDays exactly as legacy: if DateOUT == DateIN then 1 else (DateOUT - DateIN).Days
    /// From frmCheckIn.vb Compute(): 
    ///   If DateTimePicker1.Text = DateTimePicker2.Text Then NoOfDays.Text = 1
    ///   Else NoOfDays.Text = Val(ts.Days)
    /// </summary>
    public int CalculateNoOfDays(DateTime dateIn, DateTime dateOut)
    {
        if (dateOut.Date == dateIn.Date)
            return 1;
        return (dateOut.Date - dateIn.Date).Days;
    }

    /// <summary>
    /// Implements exact tax calculation formulas from frmCheckIn.vb::Compute()
    /// 
    /// TotalRoomCharges = RoomCharges * NoOfDays
    /// Discount = ((TotalRoomCharges + OtherCharges) * DiscountPer) / 100
    /// SubTotal = TotalRoomCharges + OtherCharges - Discount
    /// ServiceTaxAmount = Round((SubTotal * ServiceTaxPer) / 100, 2)
    /// LuxuryTaxAmount = Round(((SubTotal + ServiceTaxAmount) * LuxuryTaxPer) / 100, 2)
    /// GrandTotal = SubTotal + ServiceTaxAmount + LuxuryTaxAmount
    /// Balance = GrandTotal - TotalPaid
    /// </summary>
    public TaxCalculationResult CalculateTaxes(
        decimal roomCharges, int noOfDays, decimal otherCharges,
        decimal discountPer, decimal serviceTaxPer, decimal luxuryTaxPer, decimal totalPaid)
    {
        var totalRoomCharges = roomCharges * noOfDays;
        var discount = ((totalRoomCharges + otherCharges) * discountPer) / 100m;
        var subTotal = totalRoomCharges + otherCharges - discount;
        var serviceTaxAmount = Math.Round((subTotal * serviceTaxPer) / 100m, 2);
        var luxuryTaxAmount = Math.Round(((subTotal + serviceTaxAmount) * luxuryTaxPer) / 100m, 2);
        var grandTotal = subTotal + serviceTaxAmount + luxuryTaxAmount;
        var balance = grandTotal - totalPaid;

        return new TaxCalculationResult(
            noOfDays, totalRoomCharges, discount, subTotal,
            serviceTaxAmount, luxuryTaxAmount, grandTotal, balance);
    }

    /// <summary>
    /// Checks room availability by verifying both Temp_Reservation AND CheckIn_Room tables for date overlaps.
    /// From frmCheckIn.vb: checks both tables before allowing check-in.
    /// </summary>
    public async Task<bool> IsRoomAvailableAsync(string roomNo, DateTime dateIn, DateTime dateOut, int? excludeId = null)
    {
        // Check CheckIn_Room for active check-ins with overlapping dates
        var hasActiveCheckIn = await _context.Set<CheckInRoom>()
            .AnyAsync(c => c.RoomNo == roomNo
                && c.Status == "Checked In"
                && c.DateIN < dateOut
                && c.DateOUT > dateIn
                && (excludeId == null || c.ID != excludeId));

        if (hasActiveCheckIn)
            return false;

        // Check Temp_Reservation for reservations with overlapping dates
        var hasReservation = await _context.Set<Reservation>()
            .AnyAsync(r => r.RoomNo == roomNo
                && r.Status == "Reserved"
                && r.DateIN < dateOut
                && r.DateOUT > dateIn);

        return !hasReservation;
    }

    private static CheckInResponse MapToResponse(CheckInRoom c) =>
        new(c.ID, c.GuestID, c.RoomNo, c.RoomCharges,
            c.DateIN, c.DateOUT, c.NoOfAdults, c.NoOfKids,
            c.GuestName, c.Address, c.City, c.ContactNo,
            c.IDType, c.IDNumber, c.NoOfDays,
            c.TotalRoomCharges, c.OtherCharges,
            c.DiscountPer, c.Discount, c.SubTotal,
            c.ServiceTaxPer, c.ServiceTaxAmount,
            c.LuxuryTaxPer, c.LuxuryTaxAmount,
            c.GrandTotal, c.TotalPaid, c.Balance,
            c.ExtraBed, c.Currency, c.Status, c.Notes);
}
