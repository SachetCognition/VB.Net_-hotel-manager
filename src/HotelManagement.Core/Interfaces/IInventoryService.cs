using HotelManagement.Core.DTOs;

namespace HotelManagement.Core.Interfaces;

public interface IInventoryService
{
    // Dishes
    Task<DishResponse> CreateDishAsync(CreateDishRequest request);
    Task<DishResponse?> GetDishByIdAsync(int id);
    Task<IEnumerable<DishResponse>> GetDishesAsync(string? search = null);
    Task<DishResponse> UpdateDishAsync(int id, UpdateDishRequest request);
    Task DeleteDishAsync(int id);

    // Beer
    Task<BeerResponse> CreateBeerAsync(CreateBeerRequest request);
    Task<IEnumerable<BeerResponse>> GetBeersAsync(string? search = null);
    Task<BeerResponse> UpdateBeerAsync(int id, CreateBeerRequest request);
    Task DeleteBeerAsync(int id);

    // Liquor
    Task<LiquorResponse> CreateLiquorAsync(CreateLiquorRequest request);
    Task<IEnumerable<LiquorResponse>> GetLiquorsAsync(string? search = null);
    Task<LiquorResponse> UpdateLiquorAsync(int id, CreateLiquorRequest request);
    Task DeleteLiquorAsync(int id);

    // Liquor Master
    Task<LiquorMasterResponse> CreateLiquorMasterAsync(CreateLiquorMasterRequest request);
    Task<IEnumerable<LiquorMasterResponse>> GetLiquorMastersAsync();
    Task DeleteLiquorMasterAsync(int id);

    // Purchase Inventory
    Task<PurchaseResponse> CreatePurchaseAsync(CreatePurchaseRequest request);
    Task<IEnumerable<PurchaseResponse>> GetPurchasesAsync(DateTime? fromDate = null, DateTime? toDate = null);

    // Stock
    Task<IEnumerable<StockResponse>> GetStockAsync();
    Task UpdateStockAsync(int id, UpdateStockRequest request, string userRole);
}
