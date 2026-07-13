using HotelManager.Application.Billing;
using HotelManager.Application.Interfaces;
using HotelManager.Domain.Entities;
using HotelManager.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.Web.Services.Reservations;

/// <summary>
/// Room check-in logic reconstructed from legacy frmCheckIn.vb: availability
/// validation, extra-bed surcharge into OtherCharges, and stay totals via
/// the shared BillingCalculator.ComputeStay.
/// </summary>
public class CheckInService(HotelDbContext db, IReservationService reservations) : ICheckInService
{
    public const string ExtraBedYes = "Yes";

    public async Task<IReadOnlyList<CheckInRoom>> GetCheckInsAsync(string? status = null, string? search = null)
    {
        var query = db.CheckIns.Include(c => c.Guest).Include(c => c.Room).Include(c => c.Currency).AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(c => c.Status == status);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c =>
                (c.Guest != null && c.Guest.GuestName != null && c.Guest.GuestName.Contains(search))
                || (c.RoomNo != null && c.RoomNo.Contains(search)));
        return await query.OrderBy(c => c.Guest!.GuestName).ToListAsync();
    }

    public Task<CheckInRoom?> GetCheckInAsync(int id) =>
        db.CheckIns.Include(c => c.Guest).Include(c => c.Room).Include(c => c.Currency)
            .FirstOrDefaultAsync(c => c.ID == id);

    public async Task<CheckInRoom> CheckInAsync(CheckInRoom checkIn)
    {
        Validate(checkIn);
        if (!await reservations.IsRoomAvailableAsync(checkIn.RoomNo!, checkIn.DateIN!.Value, checkIn.DateOUT!.Value))
            throw new InvalidOperationException("Selected Room is already reserved/booked");

        if (checkIn.ExtraBed == ExtraBedYes)
        {
            var bedCharges = await db.ExtraBeds.Select(b => b.Charges).FirstOrDefaultAsync();
            checkIn.OtherCharges = (checkIn.OtherCharges ?? 0) + (bedCharges ?? 0);
        }
        checkIn.ExtraBed ??= "No";

        ApplyStayBill(checkIn);
        checkIn.Status = ReservationService.StatusCheckedIn;
        db.CheckIns.Add(checkIn);
        await db.SaveChangesAsync();
        return checkIn;
    }

    public async Task UpdateCheckInAsync(CheckInRoom checkIn)
    {
        Validate(checkIn);
        var existing = await db.CheckIns.FindAsync(checkIn.ID)
            ?? throw new InvalidOperationException($"Check-in {checkIn.ID} not found");
        ApplyStayBill(checkIn);
        db.Entry(existing).CurrentValues.SetValues(checkIn);
        await db.SaveChangesAsync();
    }

    public async Task DeleteCheckInAsync(int id)
    {
        var checkIn = await db.CheckIns.FindAsync(id);
        if (checkIn is null) return;
        db.CheckIns.Remove(checkIn);
        await db.SaveChangesAsync();
    }

    private static void Validate(CheckInRoom checkIn)
    {
        if (string.IsNullOrWhiteSpace(checkIn.GuestID))
            throw new InvalidOperationException("Please retrieve guest id");
        if (string.IsNullOrWhiteSpace(checkIn.RoomNo))
            throw new InvalidOperationException("Please select room no.");
        if (checkIn.DateIN is null || checkIn.DateOUT is null || checkIn.DateOUT < checkIn.DateIN)
            throw new InvalidOperationException("Selected date out must be greater than date in");
    }

    private static void ApplyStayBill(CheckInRoom checkIn)
    {
        var bill = BillingCalculator.ComputeStay(
            checkIn.DateIN!.Value, checkIn.DateOUT!.Value,
            checkIn.RoomCharges ?? 0, checkIn.OtherCharges ?? 0,
            checkIn.DiscountPer ?? 0, checkIn.ServiceTaxPer ?? 0,
            checkIn.LuxuryTaxPer ?? 0, checkIn.TotalPaid ?? 0);
        if ((checkIn.TotalPaid ?? 0) > bill.GrandTotal)
            throw new InvalidOperationException("Total paid can not be more than grand total");
        checkIn.NoOfDays = bill.NoOfDays;
        checkIn.TotalRoomCharges = bill.TotalCharges;
        checkIn.Discount = bill.Discount;
        checkIn.SubTotal = bill.SubTotal;
        checkIn.ServiceTaxAmount = bill.ServiceTaxAmount;
        checkIn.LuxuryTaxAmount = bill.LuxuryTaxAmount;
        checkIn.GrandTotal = bill.GrandTotal;
        checkIn.Balance = bill.Balance;
    }
}
