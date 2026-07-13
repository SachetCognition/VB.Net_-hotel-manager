using HotelManager.Application.Billing;
using HotelManager.Application.Interfaces;
using HotelManager.Domain.Entities;
using HotelManager.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.Web.Services.Reservations;

/// <summary>
/// Room/hall/garden reservation logic reconstructed from the legacy
/// frmCheckIn.vb reservation tabs (Temp_Reservation, Reservation_HallandGarden,
/// Reservation_HallorGarden) and frmRoomsAvailability.vb overlap queries.
/// </summary>
public class ReservationService(HotelDbContext db) : IReservationService
{
    public const string StatusReserved = "Reserved";
    public const string StatusConfirmed = "Confirmed";
    public const string StatusCancelled = "Cancelled";
    public const string StatusCheckedIn = "Checked In";

    public async Task<IReadOnlyList<Reservation>> GetReservationsAsync(string? search = null)
    {
        var query = db.Reservations.Include(r => r.Guest).Include(r => r.Room).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(r => r.ReservationID.Contains(search)
                || (r.Guest != null && r.Guest.GuestName != null && r.Guest.GuestName.Contains(search))
                || (r.RoomNo != null && r.RoomNo.Contains(search)));
        return await query.OrderBy(r => r.Guest!.GuestName).ToListAsync();
    }

    public Task<Reservation?> GetReservationAsync(string reservationId) =>
        db.Reservations.Include(r => r.Guest).Include(r => r.Room)
            .FirstOrDefaultAsync(r => r.ReservationID == reservationId);

    public async Task<string> GenerateReservationIdAsync()
    {
        // Legacy GetUniqueKey(5): random digits 1-9 prefixed with "R-".
        while (true)
        {
            var id = "R-" + string.Concat(Enumerable.Range(0, 5)
                .Select(_ => (char)('1' + Random.Shared.Next(9))));
            var exists = await db.Reservations.AnyAsync(r => r.ReservationID == id)
                || await db.TempReservations.AnyAsync(r => r.ReservationID == id);
            if (!exists) return id;
        }
    }

    public async Task<bool> IsRoomAvailableAsync(string roomNo, DateTime dateIn, DateTime dateOut, string? excludeReservationId = null)
    {
        // Legacy overlap predicate: DateIn < requested DateOut AND DateOut > requested DateIn.
        var reserved = await db.TempReservations.AnyAsync(r =>
            r.RoomNo == roomNo && r.Status == StatusReserved
            && r.ReservationID != excludeReservationId
            && r.DateIN < dateOut && r.DateOUT > dateIn);
        if (reserved) return false;

        var booked = await db.Reservations.AnyAsync(r =>
            r.RoomNo == roomNo && r.Status != StatusCancelled
            && r.ReservationID != excludeReservationId
            && r.DateIN < dateOut && r.DateOUT > dateIn);
        if (booked) return false;

        var checkedIn = await db.CheckIns.AnyAsync(c =>
            c.RoomNo == roomNo && c.Status == StatusCheckedIn
            && c.DateIN < dateOut && c.DateOUT > dateIn);
        return !checkedIn;
    }

    public async Task<IReadOnlyList<RoomAvailability>> GetRoomAvailabilityAsync(DateTime dateIn, DateTime dateOut)
    {
        var rooms = await db.Rooms.OrderBy(r => r.RoomNo).ToListAsync();
        var result = new List<RoomAvailability>(rooms.Count);
        foreach (var room in rooms)
            result.Add(new RoomAvailability(room.RoomNo, room.RoomType, room.RoomCharges,
                await IsRoomAvailableAsync(room.RoomNo, dateIn, dateOut)));
        return result;
    }

    public async Task CreateReservationAsync(Reservation reservation)
    {
        await ValidateRoomAsync(reservation.RoomNo, reservation.DateIN, reservation.DateOUT, null);
        if (string.IsNullOrWhiteSpace(reservation.ReservationID))
            reservation.ReservationID = await GenerateReservationIdAsync();
        reservation.Status ??= StatusConfirmed;
        db.Reservations.Add(reservation);
        await db.SaveChangesAsync();
    }

    public async Task UpdateReservationAsync(Reservation reservation)
    {
        await ValidateRoomAsync(reservation.RoomNo, reservation.DateIN, reservation.DateOUT, reservation.ReservationID);
        var existing = await db.Reservations.FindAsync(reservation.ReservationID)
            ?? throw new InvalidOperationException($"Reservation '{reservation.ReservationID}' not found");
        db.Entry(existing).CurrentValues.SetValues(reservation);
        await db.SaveChangesAsync();
    }

    public async Task CancelReservationAsync(string reservationId)
    {
        var reservation = await db.Reservations.FindAsync(reservationId)
            ?? throw new InvalidOperationException($"Reservation '{reservationId}' not found");
        reservation.Status = StatusCancelled;
        await db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<TempReservation>> GetTempReservationsAsync() =>
        await db.TempReservations.Include(r => r.Guest).Include(r => r.Room)
            .Where(r => r.Status == StatusReserved)
            .OrderBy(r => r.Guest!.GuestName).ToListAsync();

    public async Task SaveTempReservationAsync(TempReservation reservation)
    {
        var existing = string.IsNullOrWhiteSpace(reservation.ReservationID)
            ? null
            : await db.TempReservations.FindAsync(reservation.ReservationID);
        await ValidateRoomAsync(reservation.RoomNo, reservation.DateIN, reservation.DateOUT,
            existing?.ReservationID);
        if (existing is null)
        {
            if (string.IsNullOrWhiteSpace(reservation.ReservationID))
                reservation.ReservationID = await GenerateReservationIdAsync();
            reservation.Status ??= StatusReserved;
            db.TempReservations.Add(reservation);
        }
        else
        {
            db.Entry(existing).CurrentValues.SetValues(reservation);
        }
        await db.SaveChangesAsync();
    }

    public async Task DeleteTempReservationAsync(string reservationId)
    {
        var temp = await db.TempReservations.FindAsync(reservationId);
        if (temp is null) return;
        db.TempReservations.Remove(temp);
        await db.SaveChangesAsync();
    }

    public async Task ConfirmTempReservationAsync(string reservationId)
    {
        var temp = await db.TempReservations.FindAsync(reservationId)
            ?? throw new InvalidOperationException($"Reservation '{reservationId}' not found");
        db.Reservations.Add(new Reservation
        {
            ReservationID = temp.ReservationID,
            GuestID = temp.GuestID,
            RoomNo = temp.RoomNo,
            DateIN = temp.DateIN,
            DateOUT = temp.DateOUT,
            Status = StatusConfirmed,
            Notes = temp.Notes,
        });
        db.TempReservations.Remove(temp);
        await db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<ReservationHallAndGarden>> GetHallAndGardenReservationsAsync() =>
        await db.ReservationsHallAndGarden.Include(r => r.Guest).Include(r => r.Currency)
            .OrderBy(r => r.Guest!.GuestName).ToListAsync();

    public async Task SaveHallAndGardenReservationAsync(ReservationHallAndGarden reservation, TaxReservationHallAndGarden? tax = null)
    {
        var bill = BillingCalculator.ComputeHallAndGarden(
            reservation.DateFrom_Hall ?? DateTime.Today, reservation.DateTo_Hall ?? DateTime.Today,
            reservation.Rate_Hall ?? 0,
            reservation.DateFrom_Garden ?? DateTime.Today, reservation.DateTo_Garden ?? DateTime.Today,
            reservation.Rate_Garden ?? 0,
            reservation.OtherCharges ?? 0, reservation.DiscountPer ?? 0,
            reservation.ServiceTaxPer ?? 0, reservation.LuxuryTaxPer ?? 0,
            reservation.TotalPaid ?? 0);
        reservation.Days_Hall = bill.DaysHall;
        reservation.TotalCharges_Hall = bill.TotalChargesHall;
        reservation.Days_Garden = bill.DaysGarden;
        reservation.TotalCharges_Garden = bill.TotalChargesGarden;
        ApplyBill(bill.Bill,
            v => reservation.Discount = v, v => reservation.SubTotal = v,
            v => reservation.ServiceTaxAmount = v, v => reservation.LuxuryTaxAmount = v,
            v => reservation.GrandTotal = v, v => reservation.Balance = v);

        var existing = string.IsNullOrWhiteSpace(reservation.ID)
            ? null
            : await db.ReservationsHallAndGarden.FindAsync(reservation.ID);
        if (existing is null)
        {
            if (string.IsNullOrWhiteSpace(reservation.ID))
                reservation.ID = await GenerateReservationIdAsync();
            db.ReservationsHallAndGarden.Add(reservation);
        }
        else
        {
            db.Entry(existing).CurrentValues.SetValues(reservation);
        }

        var taxRow = await db.TaxReservationsHallAndGarden
            .FirstOrDefaultAsync(t => t.ReservationID == reservation.ID);
        taxRow ??= db.TaxReservationsHallAndGarden.Add(new TaxReservationHallAndGarden
        {
            ReservationID = reservation.ID,
        }).Entity;
        taxRow.EducationalTax = tax?.EducationalTax ?? bill.Bill.EducessTax;
        taxRow.EducationalTaxAmount = tax?.EducationalTaxAmount ?? bill.Bill.EducessTaxAmount;
        taxRow.HEduTax = tax?.HEduTax ?? bill.Bill.HEduCessTax;
        taxRow.HEduTaxAmount = tax?.HEduTaxAmount ?? bill.Bill.HEduCessTaxAmount;
        await db.SaveChangesAsync();
    }

    public async Task DeleteHallAndGardenReservationAsync(string id)
    {
        var taxes = await db.TaxReservationsHallAndGarden.Where(t => t.ReservationID == id).ToListAsync();
        db.TaxReservationsHallAndGarden.RemoveRange(taxes);
        var reservation = await db.ReservationsHallAndGarden.FindAsync(id);
        if (reservation is not null) db.ReservationsHallAndGarden.Remove(reservation);
        await db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<ReservationHallOrGarden>> GetHallOrGardenReservationsAsync() =>
        await db.ReservationsHallOrGarden.Include(r => r.Guest).Include(r => r.Currency)
            .OrderBy(r => r.Guest!.GuestName).ToListAsync();

    public async Task SaveHallOrGardenReservationAsync(ReservationHallOrGarden reservation, TaxReservationHallOrGarden? tax = null)
    {
        var bill = BillingCalculator.ComputeHallOrGarden(
            reservation.DateFrom ?? DateTime.Today, reservation.DateTo ?? DateTime.Today,
            reservation.Rate ?? 0, reservation.OtherCharges ?? 0,
            reservation.DiscountPer ?? 0, reservation.ServiceTaxPer ?? 0,
            reservation.LuxuryTaxPer ?? 0, reservation.TotalPaid ?? 0);
        reservation.Days = bill.NoOfDays;
        reservation.TotalCharges = bill.TotalCharges;
        ApplyBill(bill,
            v => reservation.Discount = v, v => reservation.SubTotal = v,
            v => reservation.ServiceTaxAmount = v, v => reservation.LuxuryTaxAmount = v,
            v => reservation.GrandTotal = v, v => reservation.Balance = v);

        var existing = string.IsNullOrWhiteSpace(reservation.ID)
            ? null
            : await db.ReservationsHallOrGarden.FindAsync(reservation.ID);
        if (existing is null)
        {
            if (string.IsNullOrWhiteSpace(reservation.ID))
                reservation.ID = await GenerateReservationIdAsync();
            db.ReservationsHallOrGarden.Add(reservation);
        }
        else
        {
            db.Entry(existing).CurrentValues.SetValues(reservation);
        }

        var taxRow = await db.TaxReservationsHallOrGarden
            .FirstOrDefaultAsync(t => t.ReservationID == reservation.ID);
        taxRow ??= db.TaxReservationsHallOrGarden.Add(new TaxReservationHallOrGarden
        {
            ReservationID = reservation.ID,
        }).Entity;
        taxRow.EducationalTax = tax?.EducationalTax ?? bill.EducessTax;
        taxRow.EducationalTaxAmount = tax?.EducationalTaxAmount ?? bill.EducessTaxAmount;
        taxRow.HEduTax = tax?.HEduTax ?? bill.HEduCessTax;
        taxRow.HEduTaxAmount = tax?.HEduTaxAmount ?? bill.HEduCessTaxAmount;
        await db.SaveChangesAsync();
    }

    public async Task DeleteHallOrGardenReservationAsync(string id)
    {
        var taxes = await db.TaxReservationsHallOrGarden.Where(t => t.ReservationID == id).ToListAsync();
        db.TaxReservationsHallOrGarden.RemoveRange(taxes);
        var reservation = await db.ReservationsHallOrGarden.FindAsync(id);
        if (reservation is not null) db.ReservationsHallOrGarden.Remove(reservation);
        await db.SaveChangesAsync();
    }

    private async Task ValidateRoomAsync(string? roomNo, DateTime? dateIn, DateTime? dateOut, string? excludeReservationId)
    {
        if (string.IsNullOrWhiteSpace(roomNo))
            throw new InvalidOperationException("Please select room no.");
        if (dateIn is null || dateOut is null || dateOut < dateIn)
            throw new InvalidOperationException("Selected date out must be greater than date in");
        if (!await IsRoomAvailableAsync(roomNo, dateIn.Value, dateOut.Value, excludeReservationId))
            throw new InvalidOperationException("Selected Room is already reserved/booked");
    }

    private static void ApplyBill(StayBill bill,
        Action<double> discount, Action<double> subTotal, Action<double> serviceTax,
        Action<double> luxuryTax, Action<double> grandTotal, Action<double> balance)
    {
        discount(bill.Discount);
        subTotal(bill.SubTotal);
        serviceTax(bill.ServiceTaxAmount);
        luxuryTax(bill.LuxuryTaxAmount);
        grandTotal(bill.GrandTotal);
        balance(bill.Balance);
    }
}
