using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Core.Services;

public class InventoryService : IInventoryService
{
    private readonly DbContext _context;

    public InventoryService(DbContext context)
    {
        _context = context;
    }

    // --- Dishes ---
    public async Task<DishResponse> CreateDishAsync(CreateDishRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.DishName))
            throw new ArgumentException("Please enter dish name");

        var exists = await _context.Set<Dish>().AnyAsync(d => d.DishName == request.DishName);
        if (exists)
            throw new InvalidOperationException($"Dish '{request.DishName}' already exists");

        var dish = new Dish
        {
            DishName = request.DishName,
            Category = request.Category ?? string.Empty,
            Rate = request.Rate
        };

        _context.Set<Dish>().Add(dish);
        await _context.SaveChangesAsync();
        return MapDish(dish);
    }

    public async Task<DishResponse?> GetDishByIdAsync(int id)
    {
        var dish = await _context.Set<Dish>().FindAsync(id);
        return dish == null ? null : MapDish(dish);
    }

    public async Task<IEnumerable<DishResponse>> GetDishesAsync(string? search = null)
    {
        var query = _context.Set<Dish>().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(d => d.DishName.StartsWith(search));
        var results = await query.OrderBy(d => d.DishName).ToListAsync();
        return results.Select(MapDish);
    }

    public async Task<DishResponse> UpdateDishAsync(int id, UpdateDishRequest request)
    {
        var dish = await _context.Set<Dish>().FindAsync(id)
            ?? throw new KeyNotFoundException($"Dish {id} not found");

        var duplicate = await _context.Set<Dish>()
            .AnyAsync(d => d.DishName == request.DishName && d.ID != id);
        if (duplicate)
            throw new InvalidOperationException($"Dish '{request.DishName}' already exists");

        dish.DishName = request.DishName;
        dish.Category = request.Category ?? string.Empty;
        dish.Rate = request.Rate;
        await _context.SaveChangesAsync();
        return MapDish(dish);
    }

    public async Task DeleteDishAsync(int id)
    {
        var dish = await _context.Set<Dish>().FindAsync(id)
            ?? throw new KeyNotFoundException($"Dish {id} not found");
        _context.Set<Dish>().Remove(dish);
        await _context.SaveChangesAsync();
    }

    // --- Beer ---
    public async Task<BeerResponse> CreateBeerAsync(CreateBeerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.BeerName))
            throw new ArgumentException("Please enter beer name");

        var beer = new Beer
        {
            BeerName = request.BeerName,
            Category = request.Category ?? string.Empty,
            Rate = request.Rate,
            Quantity = request.Quantity
        };

        _context.Set<Beer>().Add(beer);
        await _context.SaveChangesAsync();
        return MapBeer(beer);
    }

    public async Task<IEnumerable<BeerResponse>> GetBeersAsync(string? search = null)
    {
        var query = _context.Set<Beer>().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(b => b.BeerName.StartsWith(search));
        var results = await query.OrderBy(b => b.BeerName).ToListAsync();
        return results.Select(MapBeer);
    }

    public async Task<BeerResponse> UpdateBeerAsync(int id, CreateBeerRequest request)
    {
        var beer = await _context.Set<Beer>().FindAsync(id)
            ?? throw new KeyNotFoundException($"Beer {id} not found");
        beer.BeerName = request.BeerName;
        beer.Category = request.Category ?? string.Empty;
        beer.Rate = request.Rate;
        beer.Quantity = request.Quantity;
        await _context.SaveChangesAsync();
        return MapBeer(beer);
    }

    public async Task DeleteBeerAsync(int id)
    {
        var beer = await _context.Set<Beer>().FindAsync(id)
            ?? throw new KeyNotFoundException($"Beer {id} not found");
        _context.Set<Beer>().Remove(beer);
        await _context.SaveChangesAsync();
    }

    // --- Liquor ---
    public async Task<LiquorResponse> CreateLiquorAsync(CreateLiquorRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.LiquorName))
            throw new ArgumentException("Please enter liquor name");

        var liquor = new Liquor
        {
            LiquorName = request.LiquorName,
            Category = request.Category ?? string.Empty,
            Rate = request.Rate,
            Quantity = request.Quantity
        };

        _context.Set<Liquor>().Add(liquor);
        await _context.SaveChangesAsync();
        return MapLiquor(liquor);
    }

    public async Task<IEnumerable<LiquorResponse>> GetLiquorsAsync(string? search = null)
    {
        var query = _context.Set<Liquor>().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(l => l.LiquorName.StartsWith(search));
        var results = await query.OrderBy(l => l.LiquorName).ToListAsync();
        return results.Select(MapLiquor);
    }

    public async Task<LiquorResponse> UpdateLiquorAsync(int id, CreateLiquorRequest request)
    {
        var liquor = await _context.Set<Liquor>().FindAsync(id)
            ?? throw new KeyNotFoundException($"Liquor {id} not found");
        liquor.LiquorName = request.LiquorName;
        liquor.Category = request.Category ?? string.Empty;
        liquor.Rate = request.Rate;
        liquor.Quantity = request.Quantity;
        await _context.SaveChangesAsync();
        return MapLiquor(liquor);
    }

    public async Task DeleteLiquorAsync(int id)
    {
        var liquor = await _context.Set<Liquor>().FindAsync(id)
            ?? throw new KeyNotFoundException($"Liquor {id} not found");
        _context.Set<Liquor>().Remove(liquor);
        await _context.SaveChangesAsync();
    }

    // --- Liquor Master ---
    public async Task<LiquorMasterResponse> CreateLiquorMasterAsync(CreateLiquorMasterRequest request)
    {
        var master = new LiquorMaster
        {
            LiquorName = request.LiquorName,
            Category = request.Category ?? string.Empty,
            Rate = request.Rate
        };
        _context.Set<LiquorMaster>().Add(master);
        await _context.SaveChangesAsync();
        return MapLiquorMaster(master);
    }

    public async Task<IEnumerable<LiquorMasterResponse>> GetLiquorMastersAsync()
    {
        var results = await _context.Set<LiquorMaster>().OrderBy(l => l.LiquorName).ToListAsync();
        return results.Select(MapLiquorMaster);
    }

    public async Task DeleteLiquorMasterAsync(int id)
    {
        var master = await _context.Set<LiquorMaster>().FindAsync(id)
            ?? throw new KeyNotFoundException($"Liquor master {id} not found");
        _context.Set<LiquorMaster>().Remove(master);
        await _context.SaveChangesAsync();
    }

    // --- Purchase Inventory ---
    public async Task<PurchaseResponse> CreatePurchaseAsync(CreatePurchaseRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ItemName))
            throw new ArgumentException("Please enter item name");

        var purchase = new PurchaseInventory
        {
            ItemName = request.ItemName,
            Category = request.Category ?? string.Empty,
            Quantity = request.Quantity,
            Rate = request.Rate,
            TotalAmount = request.Quantity * request.Rate,
            PurchaseDate = request.PurchaseDate,
            Supplier = request.Supplier ?? string.Empty,
            Notes = request.Notes ?? string.Empty
        };

        _context.Set<PurchaseInventory>().Add(purchase);
        await _context.SaveChangesAsync();
        return MapPurchase(purchase);
    }

    public async Task<IEnumerable<PurchaseResponse>> GetPurchasesAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Set<PurchaseInventory>().AsQueryable();
        if (fromDate.HasValue)
            query = query.Where(p => p.PurchaseDate >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(p => p.PurchaseDate <= toDate.Value);
        var results = await query.OrderByDescending(p => p.PurchaseDate).ToListAsync();
        return results.Select(MapPurchase);
    }

    // --- Stock ---
    public async Task<IEnumerable<StockResponse>> GetStockAsync()
    {
        var results = await _context.Set<Stock>().OrderBy(s => s.ItemName).ToListAsync();
        return results.Select(s => new StockResponse(s.ID, s.ItemName, s.Category, s.Quantity, s.Rate, s.Unit));
    }

    public async Task UpdateStockAsync(int id, UpdateStockRequest request, string userRole)
    {
        if (userRole != "Admin")
            throw new UnauthorizedAccessException("Only admin users can update stock");

        var stock = await _context.Set<Stock>().FindAsync(id)
            ?? throw new KeyNotFoundException($"Stock item {id} not found");

        stock.Quantity = request.Quantity;
        await _context.SaveChangesAsync();
    }

    private static DishResponse MapDish(Dish d) => new(d.ID, d.DishName, d.Category, d.Rate);
    private static BeerResponse MapBeer(Beer b) => new(b.ID, b.BeerName, b.Category, b.Rate, b.Quantity);
    private static LiquorResponse MapLiquor(Liquor l) => new(l.ID, l.LiquorName, l.Category, l.Rate, l.Quantity);
    private static LiquorMasterResponse MapLiquorMaster(LiquorMaster l) => new(l.ID, l.LiquorName, l.Category, l.Rate);
    private static PurchaseResponse MapPurchase(PurchaseInventory p) =>
        new(p.ID, p.ItemName, p.Category, p.Quantity, p.Rate, p.TotalAmount, p.PurchaseDate, p.Supplier, p.Notes);
}
