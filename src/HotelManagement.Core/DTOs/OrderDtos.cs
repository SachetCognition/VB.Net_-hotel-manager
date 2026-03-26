namespace HotelManagement.Core.DTOs;

public record CreateOrderRequest(
    string GuestID, string GuestName, string RoomNo,
    string ItemName, string Category, int Quantity, decimal Rate, string Notes);

public record OrderResponse(
    int ID, string GuestID, string GuestName, string RoomNo,
    string ItemName, string Category, int Quantity, decimal Rate,
    decimal TotalAmount, DateTime OrderDate, string OrderType, string Notes);

public record CreateRestaurantOrderRequest(
    string CustomerName, string ItemName, string Category,
    int Quantity, decimal Rate, string Notes);

public record RestaurantOrderResponse(
    int ID, string CustomerName, string ItemName, string Category,
    int Quantity, decimal Rate, decimal TotalAmount, DateTime OrderDate, string Notes);

public record CreateTransactionRequest(
    string GuestID, string GuestName, string TransactionType,
    decimal Amount, string Description, string Currency, string Notes);

public record TransactionResponse(
    int ID, string GuestID, string GuestName, string TransactionType,
    decimal Amount, DateTime TransactionDate, string Description,
    string Currency, string Notes);
