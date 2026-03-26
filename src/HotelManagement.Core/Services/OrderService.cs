using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Core.Services;

public class OrderService : IOrderService
{
    private readonly DbContext _context;

    public OrderService(DbContext context)
    {
        _context = context;
    }

    public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ItemName))
            throw new ArgumentException("Please select item");
        if (request.Quantity <= 0)
            throw new ArgumentException("Quantity must be positive");

        var order = new Order
        {
            GuestID = request.GuestID,
            GuestName = request.GuestName,
            RoomNo = request.RoomNo,
            ItemName = request.ItemName,
            Category = request.Category ?? string.Empty,
            Quantity = request.Quantity,
            Rate = request.Rate,
            TotalAmount = request.Quantity * request.Rate,
            OrderDate = DateTime.UtcNow,
            OrderType = "Room",
            Notes = request.Notes ?? string.Empty
        };

        _context.Set<Order>().Add(order);
        await _context.SaveChangesAsync();
        return MapOrderToResponse(order);
    }

    public async Task<IEnumerable<OrderResponse>> GetOrdersAsync(
        string? guestId = null, string? roomNo = null,
        DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Set<Order>().AsQueryable();
        if (!string.IsNullOrWhiteSpace(guestId))
            query = query.Where(o => o.GuestID == guestId);
        if (!string.IsNullOrWhiteSpace(roomNo))
            query = query.Where(o => o.RoomNo == roomNo);
        if (fromDate.HasValue)
            query = query.Where(o => o.OrderDate >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(o => o.OrderDate <= toDate.Value);

        var results = await query.OrderByDescending(o => o.ID).ToListAsync();
        return results.Select(MapOrderToResponse);
    }

    public async Task<OrderResponse?> GetOrderByIdAsync(int id)
    {
        var order = await _context.Set<Order>().FindAsync(id);
        return order == null ? null : MapOrderToResponse(order);
    }

    public async Task DeleteOrderAsync(int id)
    {
        var order = await _context.Set<Order>().FindAsync(id)
            ?? throw new KeyNotFoundException($"Order {id} not found");
        _context.Set<Order>().Remove(order);
        await _context.SaveChangesAsync();
    }

    public async Task<RestaurantOrderResponse> CreateRestaurantOrderAsync(CreateRestaurantOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ItemName))
            throw new ArgumentException("Please select item");
        if (request.Quantity <= 0)
            throw new ArgumentException("Quantity must be positive");

        var order = new RestaurantOrder
        {
            CustomerName = request.CustomerName,
            ItemName = request.ItemName,
            Category = request.Category ?? string.Empty,
            Quantity = request.Quantity,
            Rate = request.Rate,
            TotalAmount = request.Quantity * request.Rate,
            OrderDate = DateTime.UtcNow,
            Notes = request.Notes ?? string.Empty
        };

        _context.Set<RestaurantOrder>().Add(order);
        await _context.SaveChangesAsync();
        return MapRestaurantOrderToResponse(order);
    }

    public async Task<IEnumerable<RestaurantOrderResponse>> GetRestaurantOrdersAsync(
        DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Set<RestaurantOrder>().AsQueryable();
        if (fromDate.HasValue)
            query = query.Where(o => o.OrderDate >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(o => o.OrderDate <= toDate.Value);

        var results = await query.OrderByDescending(o => o.ID).ToListAsync();
        return results.Select(MapRestaurantOrderToResponse);
    }

    public async Task<RestaurantOrderResponse?> GetRestaurantOrderByIdAsync(int id)
    {
        var order = await _context.Set<RestaurantOrder>().FindAsync(id);
        return order == null ? null : MapRestaurantOrderToResponse(order);
    }

    public async Task DeleteRestaurantOrderAsync(int id)
    {
        var order = await _context.Set<RestaurantOrder>().FindAsync(id)
            ?? throw new KeyNotFoundException($"Restaurant order {id} not found");
        _context.Set<RestaurantOrder>().Remove(order);
        await _context.SaveChangesAsync();
    }

    public async Task<TransactionResponse> CreateTransactionAsync(CreateTransactionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TransactionType))
            throw new ArgumentException("Please select transaction type");

        var transaction = new Transaction
        {
            GuestID = request.GuestID,
            GuestName = request.GuestName,
            TransactionType = request.TransactionType,
            Amount = request.Amount,
            TransactionDate = DateTime.UtcNow,
            Description = request.Description ?? string.Empty,
            Currency = request.Currency ?? string.Empty,
            Notes = request.Notes ?? string.Empty
        };

        _context.Set<Transaction>().Add(transaction);
        await _context.SaveChangesAsync();
        return MapTransactionToResponse(transaction);
    }

    public async Task<IEnumerable<TransactionResponse>> GetTransactionsAsync(
        string? guestId = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Set<Transaction>().AsQueryable();
        if (!string.IsNullOrWhiteSpace(guestId))
            query = query.Where(t => t.GuestID == guestId);
        if (fromDate.HasValue)
            query = query.Where(t => t.TransactionDate >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(t => t.TransactionDate <= toDate.Value);

        var results = await query.OrderByDescending(t => t.ID).ToListAsync();
        return results.Select(MapTransactionToResponse);
    }

    public async Task<TransactionResponse?> GetTransactionByIdAsync(int id)
    {
        var transaction = await _context.Set<Transaction>().FindAsync(id);
        return transaction == null ? null : MapTransactionToResponse(transaction);
    }

    public async Task DeleteTransactionAsync(int id)
    {
        var transaction = await _context.Set<Transaction>().FindAsync(id)
            ?? throw new KeyNotFoundException($"Transaction {id} not found");
        _context.Set<Transaction>().Remove(transaction);
        await _context.SaveChangesAsync();
    }

    private static OrderResponse MapOrderToResponse(Order o) =>
        new(o.ID, o.GuestID, o.GuestName, o.RoomNo,
            o.ItemName, o.Category, o.Quantity, o.Rate,
            o.TotalAmount, o.OrderDate, o.OrderType, o.Notes);

    private static RestaurantOrderResponse MapRestaurantOrderToResponse(RestaurantOrder o) =>
        new(o.ID, o.CustomerName, o.ItemName, o.Category,
            o.Quantity, o.Rate, o.TotalAmount, o.OrderDate, o.Notes);

    private static TransactionResponse MapTransactionToResponse(Transaction t) =>
        new(t.ID, t.GuestID, t.GuestName, t.TransactionType,
            t.Amount, t.TransactionDate, t.Description, t.Currency, t.Notes);
}
