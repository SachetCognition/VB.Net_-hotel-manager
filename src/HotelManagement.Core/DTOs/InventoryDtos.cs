namespace HotelManagement.Core.DTOs;

public record CreateDishRequest(string DishName, string Category, decimal Rate);
public record UpdateDishRequest(string DishName, string Category, decimal Rate);
public record DishResponse(int ID, string DishName, string Category, decimal Rate);

public record CreateBeerRequest(string BeerName, string Category, decimal Rate, int Quantity);
public record BeerResponse(int ID, string BeerName, string Category, decimal Rate, int Quantity);

public record CreateLiquorRequest(string LiquorName, string Category, decimal Rate, int Quantity);
public record LiquorResponse(int ID, string LiquorName, string Category, decimal Rate, int Quantity);

public record CreateLiquorMasterRequest(string LiquorName, string Category, decimal Rate);
public record LiquorMasterResponse(int ID, string LiquorName, string Category, decimal Rate);

public record CreatePurchaseRequest(
    string ItemName, string Category, int Quantity, decimal Rate,
    DateTime PurchaseDate, string Supplier, string Notes);
public record PurchaseResponse(
    int ID, string ItemName, string Category, int Quantity, decimal Rate,
    decimal TotalAmount, DateTime PurchaseDate, string Supplier, string Notes);

public record StockResponse(int ID, string ItemName, string Category, int Quantity, decimal Rate, string Unit);
public record UpdateStockRequest(int Quantity);
