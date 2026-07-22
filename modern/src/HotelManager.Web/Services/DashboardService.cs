using HotelManager.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.Web.Services;

/// <summary>
/// Web-layer aggregation service for the dashboard. It composes read-only
/// summaries from existing data via <see cref="HotelDbContext"/> and does NOT
/// modify any frozen <c>HotelManager.Application</c> service interfaces.
/// </summary>
public interface IDashboardService
{
    Task<DashboardData> GetDashboardAsync(CancellationToken ct = default);
}

public sealed record MonthlyRevenuePoint(string Label, double Room, double Restaurant);

public sealed record DashboardData(
    int TotalRooms,
    int OccupiedRooms,
    double OccupancyPercent,
    int CurrentReservations,
    double TotalRevenue,
    double PendingPayments,
    double RoomRevenue,
    double RestaurantRevenue,
    double RoomStayRevenue,
    IReadOnlyList<MonthlyRevenuePoint> MonthlyRevenue);

public sealed class DashboardService(IDbContextFactory<HotelDbContext> dbFactory) : IDashboardService
{
    public async Task<DashboardData> GetDashboardAsync(CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var totalRooms = await db.Rooms.CountAsync(ct);
        var occupiedRooms = await db.CheckIns
            .Where(c => c.Status == "Checked In")
            .Select(c => c.RoomNo)
            .Distinct()
            .CountAsync(ct);

        var today = DateTime.Today;
        var currentReservations = await db.Reservations
            .CountAsync(r => r.Status != "Cancelled" && r.DateOUT >= today, ct);

        var roomStayRevenue = await db.CheckIns
            .Where(c => c.GrandTotal != null)
            .SumAsync(c => (double?)c.GrandTotal, ct) ?? 0;

        var roomOrders = await db.Orders.AsNoTracking()
            .Select(o => new { o.OrderDate, o.GrandTotal, o.PaymentDue })
            .ToListAsync(ct);
        var restaurantOrders = await db.RestaurantOrders.AsNoTracking()
            .Select(o => new { o.OrderDate, o.GrandTotal, o.PaymentDue })
            .ToListAsync(ct);

        double roomRevenue = roomOrders.Sum(o => (double?)o.GrandTotal ?? 0);
        double restaurantRevenue = restaurantOrders.Sum(o => (double?)o.GrandTotal ?? 0);

        double checkInPending = await db.CheckIns
            .Where(c => c.Status == "Checked In" && c.Balance != null)
            .SumAsync(c => (double?)c.Balance, ct) ?? 0;
        double orderPending = roomOrders.Sum(o => (double?)o.PaymentDue ?? 0)
            + restaurantOrders.Sum(o => (double?)o.PaymentDue ?? 0);

        var months = Enumerable.Range(0, 6)
            .Select(i => new DateTime(today.Year, today.Month, 1).AddMonths(-5 + i))
            .ToList();

        var monthly = months.Select(m =>
        {
            double room = roomOrders
                .Where(o => o.OrderDate is { } d && d.Year == m.Year && d.Month == m.Month)
                .Sum(o => (double?)o.GrandTotal ?? 0);
            double rest = restaurantOrders
                .Where(o => o.OrderDate is { } d && d.Year == m.Year && d.Month == m.Month)
                .Sum(o => (double?)o.GrandTotal ?? 0);
            return new MonthlyRevenuePoint(m.ToString("MMM"), room, rest);
        }).ToList();

        double occupancyPercent = totalRooms > 0
            ? Math.Round((double)occupiedRooms / totalRooms * 100, 1)
            : 0;

        return new DashboardData(
            TotalRooms: totalRooms,
            OccupiedRooms: occupiedRooms,
            OccupancyPercent: occupancyPercent,
            CurrentReservations: currentReservations,
            TotalRevenue: roomStayRevenue + roomRevenue + restaurantRevenue,
            PendingPayments: checkInPending + orderPending,
            RoomRevenue: roomRevenue,
            RestaurantRevenue: restaurantRevenue,
            RoomStayRevenue: roomStayRevenue,
            MonthlyRevenue: monthly);
    }
}
