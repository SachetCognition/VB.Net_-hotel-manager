using HotelManager.Application.Interfaces;
using HotelManager.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.Web.Services.Reports;

/// <summary>
/// Data projections for every report. Kept separate from PDF layout so the
/// query logic can be unit-tested without rendering.
/// </summary>
public class ReportDataService
{
    private readonly IDbContextFactory<HotelDbContext> _factory;

    public ReportDataService(IDbContextFactory<HotelDbContext> factory) => _factory = factory;

    private static HotelHeaderRow ToHeader(Domain.Entities.HotelInfo? h) => new(
        h?.HotelName, h?.Address, h?.ContactNo, h?.ContactNo1, h?.Email, h?.TIN, h?.STNo);

    public async Task<HotelHeaderRow> GetHotelHeaderAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        return ToHeader(await db.HotelInfos.AsNoTracking().FirstOrDefaultAsync());
    }

    public async Task<RoomInvoiceRow?> GetRoomInvoiceAsync(int checkoutId)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var hotel = ToHeader(await db.HotelInfos.AsNoTracking().FirstOrDefaultAsync());
        var checkout = await db.Checkouts.AsNoTracking()
            .Include(c => c.CheckIn).ThenInclude(ci => ci!.Guest)
            .Include(c => c.Currency)
            .FirstOrDefaultAsync(c => c.ID == checkoutId);
        if (checkout?.CheckIn is null) return null;
        var ci = checkout.CheckIn;
        var g = ci.Guest;
        return new RoomInvoiceRow(
            checkout.ID, checkout.BillNo, checkout.CheckOutDate, checkout.Currency?.CS_Currency,
            g?.GuestID, g?.GuestName, g?.Address, g?.City, g?.ContactNo, g?.IDType, g?.IDNumber,
            ci.RoomNo, ci.RoomCharges, ci.DateIN, ci.DateOUT, ci.NoOfAdults, ci.NoOfKids, ci.NoOfDays,
            ci.ExtraBed, ci.TotalRoomCharges, ci.OtherCharges, ci.SubTotal,
            ci.ServiceTaxPer, ci.ServiceTaxAmount, ci.LuxuryTaxPer, ci.LuxuryTaxAmount,
            ci.DiscountPer, ci.Discount, ci.GrandTotal, ci.TotalPaid, ci.Balance,
            ci.Status, checkout.Notes, hotel);
    }

    public async Task<IReadOnlyList<InvoicePickRow>> GetRoomInvoicePicksAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Checkouts.AsNoTracking()
            .OrderByDescending(c => c.CheckOutDate)
            .Select(c => new InvoicePickRow(
                c.ID.ToString(), c.BillNo,
                c.CheckIn != null && c.CheckIn.Guest != null ? c.CheckIn.Guest.GuestName : null,
                c.CheckOutDate))
            .ToListAsync();
    }

    public async Task<HallAndGardenInvoiceRow?> GetHallAndGardenInvoiceAsync(string reservationId)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var hotel = ToHeader(await db.HotelInfos.AsNoTracking().FirstOrDefaultAsync());
        var r = await db.ReservationsHallAndGarden.AsNoTracking()
            .Include(x => x.Guest).Include(x => x.Currency)
            .FirstOrDefaultAsync(x => x.ID == reservationId);
        if (r is null) return null;
        return new HallAndGardenInvoiceRow(
            r.ID, r.Currency?.CS_Currency,
            r.Guest?.GuestID, r.Guest?.GuestName, r.Guest?.Address, r.Guest?.City, r.Guest?.ContactNo,
            r.Hall, r.DateFrom_Hall, r.DateTo_Hall, r.Days_Hall, r.Rate_Hall, r.TotalCharges_Hall,
            r.Garden, r.DateFrom_Garden, r.DateTo_Garden, r.Days_Garden, r.Rate_Garden, r.TotalCharges_Garden,
            r.OtherCharges, r.SubTotal, r.ServiceTaxPer, r.ServiceTaxAmount,
            r.LuxuryTaxPer, r.LuxuryTaxAmount, r.DiscountPer, r.Discount,
            r.GrandTotal, r.TotalPaid, r.Balance, r.Notes, hotel);
    }

    public async Task<IReadOnlyList<InvoicePickRow>> GetHallAndGardenInvoicePicksAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.ReservationsHallAndGarden.AsNoTracking()
            .OrderBy(r => r.ID)
            .Select(r => new InvoicePickRow(r.ID, r.ID, r.Guest != null ? r.Guest.GuestName : null, r.DateFrom_Hall))
            .ToListAsync();
    }

    public async Task<HallOrGardenInvoiceRow?> GetHallOrGardenInvoiceAsync(string reservationId)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var hotel = ToHeader(await db.HotelInfos.AsNoTracking().FirstOrDefaultAsync());
        var r = await db.ReservationsHallOrGarden.AsNoTracking()
            .Include(x => x.Guest).Include(x => x.Currency)
            .FirstOrDefaultAsync(x => x.ID == reservationId);
        if (r is null) return null;
        return new HallOrGardenInvoiceRow(
            r.ID, r.Currency?.CS_Currency,
            r.Guest?.GuestID, r.Guest?.GuestName, r.Guest?.Address, r.Guest?.City, r.Guest?.ContactNo,
            r.Type, r.DateFrom, r.DateTo, r.Days, r.Rate, r.TotalCharges,
            r.OtherCharges, r.SubTotal, r.ServiceTaxPer, r.ServiceTaxAmount,
            r.LuxuryTaxPer, r.LuxuryTaxAmount, r.DiscountPer, r.Discount,
            r.GrandTotal, r.TotalPaid, r.Balance, r.Notes, hotel);
    }

    public async Task<IReadOnlyList<InvoicePickRow>> GetHallOrGardenInvoicePicksAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.ReservationsHallOrGarden.AsNoTracking()
            .OrderBy(r => r.ID)
            .Select(r => new InvoicePickRow(r.ID, r.ID, r.Guest != null ? r.Guest.GuestName : null, r.DateFrom))
            .ToListAsync();
    }

    public async Task<OrderInvoiceRow?> GetOrderInvoiceAsync(int orderId)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var hotel = ToHeader(await db.HotelInfos.AsNoTracking().FirstOrDefaultAsync());
        var o = await db.Orders.AsNoTracking()
            .Include(x => x.Products)
            .Include(x => x.Currency)
            .Include(x => x.CheckIn).ThenInclude(ci => ci!.Guest)
            .FirstOrDefaultAsync(x => x.ID == orderId);
        if (o is null) return null;
        var g = o.CheckIn?.Guest;
        var lines = o.Products
            .OrderBy(p => p.ID)
            .Select(p => new OrderLineRow(p.ProductID, p.ProductName, p.Volume, p.Rate, p.Quantity, p.Amount))
            .ToList();
        return new OrderInvoiceRow(
            o.ID, o.OrderNo, o.OrderDate, o.Currency?.CS_Currency,
            o.CheckIn?.RoomNo, g?.GuestID, g?.GuestName, g?.Address, g?.ContactNo,
            o.SubTotal, o.VATPer, o.VATAmount, o.STPer, o.STAmount,
            o.GrandTotal, o.TotalPayment, o.PaymentDue, lines, hotel);
    }

    public async Task<IReadOnlyList<InvoicePickRow>> GetOrderInvoicePicksAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Orders.AsNoTracking()
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new InvoicePickRow(
                o.ID.ToString(), o.OrderNo,
                o.CheckIn != null ? o.CheckIn.RoomNo : null, o.OrderDate))
            .ToListAsync();
    }

    public async Task<RestaurantReceiptRow?> GetRestaurantReceiptAsync(int orderId)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var hotel = ToHeader(await db.HotelInfos.AsNoTracking().FirstOrDefaultAsync());
        var o = await db.RestaurantOrders.AsNoTracking()
            .Include(x => x.Products)
            .Include(x => x.Currency)
            .FirstOrDefaultAsync(x => x.ID == orderId);
        if (o is null) return null;
        var lines = o.Products
            .OrderBy(p => p.ID)
            .Select(p => new OrderLineRow(p.ProductID, p.ProductName, null, p.Rate, p.Quantity, p.Amount))
            .ToList();
        return new RestaurantReceiptRow(
            o.ID, o.OrderNo, o.OrderDate, o.Currency?.CS_Currency,
            o.SubTotal, o.VATPer, o.VATAmount, o.STPer, o.STAmount,
            o.GrandTotal, o.TotalPayment, o.PaymentDue, lines, hotel);
    }

    public async Task<IReadOnlyList<InvoicePickRow>> GetRestaurantReceiptPicksAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.RestaurantOrders.AsNoTracking()
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new InvoicePickRow(o.ID.ToString(), o.OrderNo, null, o.OrderDate))
            .ToListAsync();
    }

    public async Task<SalarySlipRow?> GetSalarySlipAsync(string paymentId)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var hotel = ToHeader(await db.HotelInfos.AsNoTracking().FirstOrDefaultAsync());
        var p = await db.EmployeePayments.AsNoTracking()
            .Include(x => x.Employee)
            .FirstOrDefaultAsync(x => x.PaymentID == paymentId);
        if (p is null) return null;
        return new SalarySlipRow(
            p.PaymentID, p.DateFrom, p.DateTo, p.EmployeeID,
            p.Employee?.EmployeeName, p.Employee?.Designation, p.Employee?.Department,
            p.Salary, p.PresentDays, p.Advance, p.Deduction, p.Overtime, p.OverTimeAmount,
            p.PaymentDate, p.ModeOfPayment, p.NetPay, hotel);
    }

    public async Task<IReadOnlyList<InvoicePickRow>> GetSalarySlipPicksAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.EmployeePayments.AsNoTracking()
            .OrderByDescending(p => p.PaymentDate)
            .Select(p => new InvoicePickRow(
                p.PaymentID, p.PaymentID,
                p.Employee != null ? p.Employee.EmployeeName : null, p.PaymentDate))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<AttendanceRow>> GetAttendanceAsync(ReportRequest request)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var q = db.Attendances.AsNoTracking().Include(a => a.Employee).AsQueryable();
        if (request.From is not null) q = q.Where(a => a.WorkingDate >= request.From);
        if (request.To is not null) q = q.Where(a => a.WorkingDate <= request.To);
        if (!string.IsNullOrWhiteSpace(request.Key)) q = q.Where(a => a.EmployeeID == request.Key);
        return await q.OrderBy(a => a.WorkingDate).ThenBy(a => a.EmployeeID)
            .Select(a => new AttendanceRow(
                a.EmployeeID, a.Employee != null ? a.Employee.EmployeeName : null,
                a.WorkingDate, a.BasicWorkingTime, a.Status, a.InTime, a.OutTime, a.Overtime))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<AdvancePaymentRow>> GetAdvancePaymentsAsync(ReportRequest request)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var q = db.AdvanceEntries.AsNoTracking().Include(a => a.Employee).AsQueryable();
        if (request.From is not null) q = q.Where(a => a.WorkingDate >= request.From);
        if (request.To is not null) q = q.Where(a => a.WorkingDate <= request.To);
        if (!string.IsNullOrWhiteSpace(request.Key)) q = q.Where(a => a.EmployeeID == request.Key);
        return await q.OrderBy(a => a.WorkingDate).ThenBy(a => a.EmployeeID)
            .Select(a => new AdvancePaymentRow(
                a.EmployeeID, a.Employee != null ? a.Employee.EmployeeName : null,
                a.WorkingDate, a.Amount, a.Deduction))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<EmployeePaymentRow>> GetEmployeePaymentsAsync(ReportRequest request)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var q = db.EmployeePayments.AsNoTracking().Include(p => p.Employee).AsQueryable();
        if (request.From is not null) q = q.Where(p => p.PaymentDate >= request.From);
        if (request.To is not null) q = q.Where(p => p.PaymentDate <= request.To);
        if (!string.IsNullOrWhiteSpace(request.Key)) q = q.Where(p => p.EmployeeID == request.Key);
        return await q.OrderBy(p => p.PaymentDate).ThenBy(p => p.PaymentID)
            .Select(p => new EmployeePaymentRow(
                p.PaymentID, p.EmployeeID, p.Employee != null ? p.Employee.EmployeeName : null,
                p.DateFrom, p.DateTo, p.PresentDays, p.Salary, p.Advance, p.Deduction,
                p.Overtime, p.OverTimeAmount, p.PaymentDate, p.ModeOfPayment, p.NetPay))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<GuestRow>> GetGuestsAsync(ReportRequest request)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var q = db.Guests.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Key))
        {
            var key = request.Key;
            q = q.Where(g => g.GuestID.Contains(key) || (g.GuestName != null && g.GuestName.Contains(key)));
        }
        return await q.OrderBy(g => g.GuestID)
            .Select(g => new GuestRow(g.GuestID, g.GuestName, g.Address, g.City, g.ContactNo, g.IDType, g.IDNumber, g.Notes))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ReservationRow>> GetReservationsAsync(ReportRequest request)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var q = db.Reservations.AsNoTracking()
            .Include(r => r.Guest).Include(r => r.Room).AsQueryable();
        if (request.From is not null) q = q.Where(r => r.DateIN >= request.From);
        if (request.To is not null) q = q.Where(r => r.DateIN <= request.To);
        if (!string.IsNullOrWhiteSpace(request.Key))
        {
            var key = request.Key;
            q = q.Where(r => r.ReservationID.Contains(key) || (r.GuestID != null && r.GuestID.Contains(key)));
        }
        return await q.OrderBy(r => r.DateIN).ThenBy(r => r.ReservationID)
            .Select(r => new ReservationRow(
                r.ReservationID, r.GuestID, r.Guest != null ? r.Guest.GuestName : null,
                r.RoomNo, r.Room != null ? r.Room.RoomType : null,
                r.DateIN, r.DateOUT, r.Status, r.Notes))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<PurchasedInventoryRow>> GetPurchasedInventoriesAsync(ReportRequest request)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var q = db.PurchasedInventories.AsNoTracking().AsQueryable();
        if (request.From is not null) q = q.Where(p => p.PurchaseDate >= request.From);
        if (request.To is not null) q = q.Where(p => p.PurchaseDate <= request.To);
        if (!string.IsNullOrWhiteSpace(request.Key))
        {
            var key = request.Key;
            q = q.Where(p => (p.ProductName != null && p.ProductName.Contains(key))
                || (p.PartyName != null && p.PartyName.Contains(key)));
        }
        return await q.OrderBy(p => p.PurchaseDate).ThenBy(p => p.ID)
            .Select(p => new PurchasedInventoryRow(
                p.ID, p.ProductName, p.Category, p.TransactionType, p.PartyName,
                p.PurchaseDate, p.Quantity, p.Unit, p.Price, p.TotalPrice))
            .ToListAsync();
    }
}
