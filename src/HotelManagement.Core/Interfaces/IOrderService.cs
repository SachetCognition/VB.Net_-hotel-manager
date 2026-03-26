using HotelManagement.Core.DTOs;

namespace HotelManagement.Core.Interfaces;

public interface IOrderService
{
    Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request);
    Task<IEnumerable<OrderResponse>> GetOrdersAsync(string? guestId = null, string? roomNo = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task<OrderResponse?> GetOrderByIdAsync(int id);
    Task DeleteOrderAsync(int id);
    Task<RestaurantOrderResponse> CreateRestaurantOrderAsync(CreateRestaurantOrderRequest request);
    Task<IEnumerable<RestaurantOrderResponse>> GetRestaurantOrdersAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<RestaurantOrderResponse?> GetRestaurantOrderByIdAsync(int id);
    Task DeleteRestaurantOrderAsync(int id);
    Task<TransactionResponse> CreateTransactionAsync(CreateTransactionRequest request);
    Task<IEnumerable<TransactionResponse>> GetTransactionsAsync(string? guestId = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task<TransactionResponse?> GetTransactionByIdAsync(int id);
    Task DeleteTransactionAsync(int id);
}
