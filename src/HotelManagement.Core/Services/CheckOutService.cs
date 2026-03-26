using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace HotelManagement.Core.Services;

/// <summary>
/// Check-Out Service implementing exact legacy formulas from frmCheckOut.vb::Calculate()
/// Including education cess and higher education cess calculations.
/// </summary>
public class CheckOutService : ICheckOutService
{
    private readonly DbContext _context;
    private readonly ICheckInService _checkInService;

    public CheckOutService(DbContext context, ICheckInService checkInService)
    {
        _context = context;
        _checkInService = checkInService;
    }

    public async Task<CheckOutResponse> CheckOutAsync(CheckOutRequest request)
    {
        var checkIn = await _context.Set<CheckInRoom>().FindAsync(request.CheckInId)
            ?? throw new KeyNotFoundException($"Check-in record {request.CheckInId} not found");

        if (checkIn.Status != "Checked In")
            throw new InvalidOperationException("Guest is not currently checked in");

        if (string.IsNullOrWhiteSpace(request.Currency))
            throw new ArgumentException("Please select currency");

        // Recalculate taxes using exact legacy formulas
        var noOfDays = _checkInService.CalculateNoOfDays(checkIn.DateIN, checkIn.DateOUT);
        var taxResult = _checkInService.CalculateTaxes(
            checkIn.RoomCharges, noOfDays, checkIn.OtherCharges,
            checkIn.DiscountPer, checkIn.ServiceTaxPer, checkIn.LuxuryTaxPer, request.TotalPaid);

        // Calculate education cess (from frmCheckOut.vb::Calculate())
        var cessResult = CalculateEducationCess(checkIn.ServiceTaxPer, taxResult.ServiceTaxAmount);

        // First save checkout to get database-generated ID, then derive BillNo from it
        var checkout = new CheckoutRoom
        {
            BillNo = "TEMP",  // Placeholder — will be updated after SaveChanges gives us the ID
            GuestID = checkIn.GuestID,
            RoomNo = checkIn.RoomNo,
            RoomCharges = checkIn.RoomCharges,
            DateIN = checkIn.DateIN,
            DateOUT = checkIn.DateOUT,
            NoOfAdults = checkIn.NoOfAdults,
            NoOfKids = checkIn.NoOfKids,
            GuestName = checkIn.GuestName,
            Address = checkIn.Address,
            City = checkIn.City,
            ContactNo = checkIn.ContactNo,
            NoOfDays = taxResult.NoOfDays,
            TotalRoomCharges = taxResult.TotalRoomCharges,
            OtherCharges = checkIn.OtherCharges,
            DiscountPer = checkIn.DiscountPer,
            Discount = taxResult.Discount,
            SubTotal = taxResult.SubTotal,
            ServiceTaxPer = checkIn.ServiceTaxPer,
            ServiceTaxAmount = taxResult.ServiceTaxAmount,
            LuxuryTaxPer = checkIn.LuxuryTaxPer,
            LuxuryTaxAmount = taxResult.LuxuryTaxAmount,
            EducessTax = cessResult.EducessTax,
            EducessTaxAmount = cessResult.EducessTaxAmount,
            HEducessTax = cessResult.HEducessTax,
            HEducessTaxAmount = cessResult.HEducessTaxAmount,
            GrandTotal = taxResult.GrandTotal,
            TotalPaid = request.TotalPaid,
            Balance = taxResult.Balance,
            ExtraBed = checkIn.ExtraBed,
            Currency = request.Currency,
            Status = "Checked Out",
            Notes = checkIn.Notes,
            CheckOutDate = DateTime.UtcNow
        };

        _context.Set<CheckoutRoom>().Add(checkout);

        // Update check-in status
        checkIn.Status = "Checked Out";

        // Use an explicit transaction to ensure atomicity across both saves.
        // InMemoryDatabase does not support transactions, so we check first.
        IDbContextTransaction? transaction = null;
        if (_context.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
            transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            await _context.SaveChangesAsync();

            // Now we have the database-generated ID — derive BillNo from it (concurrency-safe)
            var billNo = $"B{checkout.ID}";
            checkout.BillNo = billNo;

            // Create Tax_Room record with the real BillNo
            var taxRoom = new TaxRoom
            {
                BillNo = billNo,
                RoomNo = checkIn.RoomNo,
                ServiceTaxPer = checkIn.ServiceTaxPer,
                ServiceTaxAmount = taxResult.ServiceTaxAmount,
                LuxuryTaxPer = checkIn.LuxuryTaxPer,
                LuxuryTaxAmount = taxResult.LuxuryTaxAmount,
                EducessTax = cessResult.EducessTax,
                EducessTaxAmount = cessResult.EducessTaxAmount,
                HEducessTax = cessResult.HEducessTax,
                HEducessTaxAmount = cessResult.HEducessTaxAmount
            };

            _context.Set<TaxRoom>().Add(taxRoom);
            await _context.SaveChangesAsync();

            if (transaction != null)
                await transaction.CommitAsync();
        }
        catch
        {
            if (transaction != null)
                await transaction.RollbackAsync();
            throw;
        }
        finally
        {
            transaction?.Dispose();
        }

        return MapToResponse(checkout);
    }

    public async Task<CheckOutResponse?> GetByIdAsync(int id)
    {
        var checkout = await _context.Set<CheckoutRoom>().FindAsync(id);
        return checkout == null ? null : MapToResponse(checkout);
    }

    public async Task<IEnumerable<CheckOutResponse>> GetAllAsync(
        string? roomNo = null, string? guestName = null,
        DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Set<CheckoutRoom>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(roomNo))
            query = query.Where(c => c.RoomNo == roomNo);
        if (!string.IsNullOrWhiteSpace(guestName))
            query = query.Where(c => c.GuestName.StartsWith(guestName));
        if (fromDate.HasValue)
            query = query.Where(c => c.CheckOutDate >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(c => c.CheckOutDate <= toDate.Value);

        var results = await query.OrderByDescending(c => c.ID).ToListAsync();
        return results.Select(MapToResponse);
    }

    public async Task DeleteAsync(int id)
    {
        var checkout = await _context.Set<CheckoutRoom>().FindAsync(id)
            ?? throw new KeyNotFoundException($"Check-out record {id} not found");

        // Delete associated Tax_Room record
        var taxRoom = await _context.Set<TaxRoom>().FirstOrDefaultAsync(t => t.BillNo == checkout.BillNo);
        if (taxRoom != null)
            _context.Set<TaxRoom>().Remove(taxRoom);

        _context.Set<CheckoutRoom>().Remove(checkout);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Calculates education cess exactly as legacy frmCheckOut.vb::Calculate()
    /// 
    /// EducessTax = (ServiceTaxPer * 2) / 100
    /// EducessTaxAmount = (ServiceTaxAmount * EducessTax) / 100
    /// HEducessTax = (ServiceTaxPer * 1) / 100
    /// HEducessTaxAmount = (ServiceTaxAmount * HEducessTax) / 100
    /// </summary>
    public CheckOutTaxResult CalculateEducationCess(decimal serviceTaxPer, decimal serviceTaxAmount)
    {
        var educessTax = Math.Round((serviceTaxPer * 2m) / 100m, 2);
        var educessTaxAmount = Math.Round((serviceTaxAmount * educessTax) / 100m, 2);
        var hEducessTax = Math.Round((serviceTaxPer * 1m) / 100m, 2);
        var hEducessTaxAmount = Math.Round((serviceTaxAmount * hEducessTax) / 100m, 2);

        return new CheckOutTaxResult(educessTax, educessTaxAmount, hEducessTax, hEducessTaxAmount);
    }

    private static CheckOutResponse MapToResponse(CheckoutRoom c) =>
        new(c.ID, c.BillNo, c.GuestID, c.RoomNo,
            c.RoomCharges, c.DateIN, c.DateOUT,
            c.NoOfDays, c.TotalRoomCharges, c.OtherCharges,
            c.DiscountPer, c.Discount, c.SubTotal,
            c.ServiceTaxPer, c.ServiceTaxAmount,
            c.LuxuryTaxPer, c.LuxuryTaxAmount,
            c.EducessTax, c.EducessTaxAmount,
            c.HEducessTax, c.HEducessTaxAmount,
            c.GrandTotal, c.TotalPaid, c.Balance,
            c.ExtraBed, c.Currency, c.Status, c.CheckOutDate);
}
