using HotelManager.Application.Interfaces;
using HotelManager.Domain.Entities;
using HotelManager.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.Web.Services.Reservations;

/// <summary>
/// Room check-out logic reconstructed from legacy frmCheckOut.vb: sequential
/// "B{n}" bill numbers, Checkout_Room + Tax_Room rows, and flipping the
/// check-in status to "Checked Out". Double checkout is rejected.
/// </summary>
public class CheckOutService(HotelDbContext db) : ICheckOutService
{
    public const string StatusCheckedOut = "Checked Out";

    public async Task<IReadOnlyList<CheckoutRoom>> GetCheckoutsAsync(string? search = null)
    {
        var query = db.Checkouts
            .Include(c => c.CheckIn)!.ThenInclude(ci => ci!.Guest)
            .Include(c => c.CheckIn)!.ThenInclude(ci => ci!.Room)
            .Include(c => c.Currency)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c =>
                (c.BillNo != null && c.BillNo.Contains(search))
                || (c.CheckIn != null && c.CheckIn.Guest != null
                    && c.CheckIn.Guest.GuestName != null && c.CheckIn.Guest.GuestName.Contains(search)));
        return await query.OrderBy(c => c.CheckIn!.Guest!.GuestName).ToListAsync();
    }

    public Task<CheckoutRoom?> GetCheckoutAsync(int id) =>
        db.Checkouts
            .Include(c => c.CheckIn)!.ThenInclude(ci => ci!.Guest)
            .Include(c => c.Taxes)
            .Include(c => c.Currency)
            .Include(c => c.Hotel)
            .FirstOrDefaultAsync(c => c.ID == id);

    public async Task<string> GenerateBillNoAsync()
    {
        // Legacy auto(): BillNo = "B" + (max Checkout_Room.ID + 1).
        var maxId = await db.Checkouts.MaxAsync(c => (int?)c.ID) ?? 0;
        return "B" + (maxId + 1);
    }

    public async Task<CheckoutRoom> CheckOutAsync(int checkInId, CheckoutRoom checkout, TaxRoom? tax = null)
    {
        var checkIn = await db.CheckIns.FindAsync(checkInId)
            ?? throw new InvalidOperationException($"Check-in {checkInId} not found");
        if (checkIn.Status == StatusCheckedOut
            || await db.Checkouts.AnyAsync(c => c.CheckInID == checkInId))
            throw new InvalidOperationException("Guest has already been checked out");

        checkout.CheckInID = checkInId;
        checkout.BillNo ??= await GenerateBillNoAsync();
        checkout.CheckOutDate ??= DateTime.Now;
        db.Checkouts.Add(checkout);
        checkIn.Status = StatusCheckedOut;

        if (tax is not null)
        {
            tax.Bill = checkout;
            db.TaxRooms.Add(tax);
        }
        await db.SaveChangesAsync();
        return checkout;
    }
}
