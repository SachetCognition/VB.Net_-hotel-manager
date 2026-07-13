using HotelManager.Application.Interfaces;
using HotelManager.Domain.Entities;
using HotelManager.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.Web.Services;

/// <summary>
/// Room-service (Order_Info) and restaurant (Restaurant_OrderInfo) orders.
/// Totals follow the legacy shape: SubTotal = Σ line amounts,
/// VAT on SubTotal, service tax on SubTotal + VAT, integer grand total
/// (VB CInt banker's rounding), PaymentDue = GrandTotal − TotalPayment.
/// </summary>
public class OrderService : IRestaurantOrderService
{
    private readonly IDbContextFactory<HotelDbContext> _factory;

    public OrderService(IDbContextFactory<HotelDbContext> factory) => _factory = factory;

    private static int CInt(double value) => (int)Math.Round(value, MidpointRounding.ToEven);

    internal static void ComputeRoomOrderTotals(OrderInfo order, IReadOnlyCollection<OrderedProduct> products)
    {
        int subTotal = products.Sum(p => p.Amount ?? 0);
        double vatAmount = Math.Round(subTotal * (order.VATPer ?? 0) / 100, 2);
        double stAmount = Math.Round((subTotal + vatAmount) * (order.STPer ?? 0) / 100, 2);
        order.SubTotal = subTotal;
        order.VATAmount = vatAmount;
        order.STAmount = stAmount;
        order.GrandTotal = CInt(subTotal + vatAmount + stAmount);
        order.PaymentDue = order.GrandTotal - (order.TotalPayment ?? 0);
    }

    internal static void ComputeRestaurantOrderTotals(RestaurantOrderInfo order, IReadOnlyCollection<RestaurantOrderedProduct> products)
    {
        int subTotal = products.Sum(p => p.Amount ?? 0);
        double vatAmount = Math.Round(subTotal * (order.VATPer ?? 0) / 100, 2);
        double stAmount = Math.Round((subTotal + vatAmount) * (order.STPer ?? 0) / 100, 2);
        order.SubTotal = subTotal;
        order.VATAmount = vatAmount;
        order.STAmount = stAmount;
        order.GrandTotal = CInt(subTotal + vatAmount + stAmount);
        order.PaymentDue = order.GrandTotal - (order.TotalPayment ?? 0);
    }

    // ---- Restaurant orders (Restaurant_OrderInfo) ----

    public async Task<IReadOnlyList<RestaurantOrderInfo>> GetRestaurantOrdersAsync(string? search = null)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var query = db.RestaurantOrders.AsNoTracking().Include(o => o.Products).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(o => o.OrderNo!.Contains(search));
        return await query.OrderByDescending(o => o.ID).ToListAsync();
    }

    public async Task<RestaurantOrderInfo?> GetRestaurantOrderAsync(int id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.RestaurantOrders.AsNoTracking()
            .Include(o => o.Products)
            .Include(o => o.Taxes)
            .FirstOrDefaultAsync(o => o.ID == id);
    }

    public async Task<string> GenerateRestaurantOrderNoAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        var maxId = await db.RestaurantOrders.MaxAsync(o => (int?)o.ID) ?? 0;
        return $"ROD-{maxId + 1:D4}";
    }

    public async Task<RestaurantOrderInfo> SaveRestaurantOrderAsync(
        RestaurantOrderInfo order, IEnumerable<RestaurantOrderedProduct> products, TaxRestaurantOrder? tax = null)
    {
        var productList = products.ToList();
        ComputeRestaurantOrderTotals(order, productList);
        order.Products = new List<RestaurantOrderedProduct>();
        order.Taxes = new List<TaxRestaurantOrder>();
        await using var db = await _factory.CreateDbContextAsync();
        if (order.ID == 0)
        {
            db.RestaurantOrders.Add(order);
        }
        else
        {
            db.RestaurantOrders.Update(order);
            var existingProducts = await db.RestaurantOrderedProducts
                .Where(p => p.OrderID == order.ID).ToListAsync();
            db.RestaurantOrderedProducts.RemoveRange(existingProducts);
            var existingTaxes = await db.TaxRestaurantOrders
                .Where(t => t.OrderID == order.ID).ToListAsync();
            db.TaxRestaurantOrders.RemoveRange(existingTaxes);
        }
        await db.SaveChangesAsync();

        foreach (var product in productList)
        {
            product.ID = 0;
            product.OrderID = order.ID;
        }
        db.RestaurantOrderedProducts.AddRange(productList);
        if (tax is not null)
        {
            tax.ID = 0;
            tax.OrderID = order.ID;
            db.TaxRestaurantOrders.Add(tax);
        }
        await db.SaveChangesAsync();
        return order;
    }

    public async Task DeleteRestaurantOrderAsync(int id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        db.RestaurantOrderedProducts.RemoveRange(db.RestaurantOrderedProducts.Where(p => p.OrderID == id));
        db.TaxRestaurantOrders.RemoveRange(db.TaxRestaurantOrders.Where(t => t.OrderID == id));
        var order = await db.RestaurantOrders.FindAsync(id);
        if (order is not null) db.RestaurantOrders.Remove(order);
        await db.SaveChangesAsync();
    }

    // ---- Room-service orders (Order_Info) ----

    public async Task<IReadOnlyList<OrderInfo>> GetRoomOrdersAsync(string? search = null)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var query = db.Orders.AsNoTracking()
            .Include(o => o.Products)
            .Include(o => o.CheckIn)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(o => o.OrderNo!.Contains(search));
        return await query.OrderByDescending(o => o.ID).ToListAsync();
    }

    public async Task<OrderInfo?> GetRoomOrderAsync(int id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Orders.AsNoTracking()
            .Include(o => o.Products)
            .Include(o => o.Taxes)
            .Include(o => o.CheckIn)
            .FirstOrDefaultAsync(o => o.ID == id);
    }

    public async Task<string> GenerateRoomOrderNoAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        var maxId = await db.Orders.MaxAsync(o => (int?)o.ID) ?? 0;
        return $"OD-{maxId + 1:D4}";
    }

    public async Task<OrderInfo> SaveRoomOrderAsync(
        OrderInfo order, IEnumerable<OrderedProduct> products, TaxOrder? tax = null)
    {
        var productList = products.ToList();
        ComputeRoomOrderTotals(order, productList);
        order.Products = new List<OrderedProduct>();
        order.Taxes = new List<TaxOrder>();
        await using var db = await _factory.CreateDbContextAsync();
        if (order.ID == 0)
        {
            db.Orders.Add(order);
        }
        else
        {
            db.Orders.Update(order);
            var existingProducts = await db.OrderedProducts
                .Where(p => p.OrderID == order.ID).ToListAsync();
            db.OrderedProducts.RemoveRange(existingProducts);
            var existingTaxes = await db.TaxOrders
                .Where(t => t.OrderID == order.ID).ToListAsync();
            db.TaxOrders.RemoveRange(existingTaxes);
        }
        await db.SaveChangesAsync();

        foreach (var product in productList)
        {
            product.ID = 0;
            product.OrderID = order.ID;
        }
        db.OrderedProducts.AddRange(productList);
        if (tax is not null)
        {
            tax.ID = 0;
            tax.OrderID = order.ID;
            db.TaxOrders.Add(tax);
        }
        await db.SaveChangesAsync();
        return order;
    }

    public async Task DeleteRoomOrderAsync(int id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        db.OrderedProducts.RemoveRange(db.OrderedProducts.Where(p => p.OrderID == id));
        db.TaxOrders.RemoveRange(db.TaxOrders.Where(t => t.OrderID == id));
        var order = await db.Orders.FindAsync(id);
        if (order is not null) db.Orders.Remove(order);
        await db.SaveChangesAsync();
    }
}
