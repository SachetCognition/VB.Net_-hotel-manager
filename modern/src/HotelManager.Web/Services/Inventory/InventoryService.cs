using System.Security.Cryptography;
using System.Text;
using HotelManager.Application.Interfaces;
using HotelManager.Domain.Entities;
using HotelManager.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.Web.Services.Inventory;

/// <summary>
/// Inventory &amp; stock catalogs reconstructed from the legacy
/// frmFood/frmLiquor/frmLiquor_Master/frmBeer/frmStock/frmPurchaseInventory
/// forms. All searches use parameterized EF LINQ (EF.Functions.Like) instead
/// of the legacy concatenated SQL.
/// </summary>
public class InventoryService : IInventoryService
{
    private readonly HotelDbContext _db;

    public InventoryService(HotelDbContext db) => _db = db;

    private static string GenerateDigits(int length)
    {
        const string chars = "123456789";
        var bytes = RandomNumberGenerator.GetBytes(length);
        var sb = new StringBuilder(length);
        foreach (var b in bytes) sb.Append(chars[b % chars.Length]);
        return sb.ToString();
    }

    private static string Prefix(string? search) => (search ?? string.Empty) + "%";

    // ---------- Dish (frmFood) ----------

    public async Task<IReadOnlyList<Dish>> GetDishesAsync(string? search = null)
    {
        var query = _db.Dishes.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = Prefix(search);
            query = query.Where(d => EF.Functions.Like(d.DishName!, pattern));
        }
        return await query.OrderBy(d => d.DishName).ToListAsync();
    }

    public async Task<Dish> SaveDishAsync(Dish dish)
    {
        if (string.IsNullOrWhiteSpace(dish.DishName))
            throw new InvalidOperationException("Please enter food name");
        if (string.IsNullOrWhiteSpace(dish.Category))
            throw new InvalidOperationException("Please enter/select category");
        if (dish.Rate is null)
            throw new InvalidOperationException("Please enter rate");

        if (dish.ID == 0)
        {
            if (await _db.Dishes.AnyAsync(d => d.DishName == dish.DishName))
                throw new InvalidOperationException("Dish Name Already Exists");
            _db.Dishes.Add(dish);
        }
        else
        {
            var existing = await _db.Dishes.FindAsync(dish.ID)
                ?? throw new InvalidOperationException("No record found");
            existing.DishName = dish.DishName;
            existing.Category = dish.Category;
            existing.Rate = dish.Rate;
        }
        await _db.SaveChangesAsync();
        return dish.ID == 0 ? dish : (await _db.Dishes.FindAsync(dish.ID))!;
    }

    public async Task DeleteDishAsync(int id)
    {
        var dish = await _db.Dishes.FindAsync(id)
            ?? throw new InvalidOperationException("No record found");
        _db.Dishes.Remove(dish);
        await _db.SaveChangesAsync();
    }

    // ---------- Beer (frmBeer) ----------

    public async Task<IReadOnlyList<Beer>> GetBeersAsync(string? search = null)
    {
        var query = _db.Beers.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = Prefix(search);
            query = query.Where(b => EF.Functions.Like(b.BeerName!, pattern));
        }
        return await query.OrderBy(b => b.BeerName).ToListAsync();
    }

    public async Task<Beer> SaveBeerAsync(Beer beer)
    {
        if (string.IsNullOrWhiteSpace(beer.BeerName))
            throw new InvalidOperationException("Please enter beer name");
        if (beer.Rate is null)
            throw new InvalidOperationException("Please enter rate");

        if (string.IsNullOrWhiteSpace(beer.ID))
        {
            if (await _db.Beers.AnyAsync(b => b.BeerName == beer.BeerName))
                throw new InvalidOperationException("Beer name already exists");
            do beer.ID = "B-" + GenerateDigits(3);
            while (await _db.Beers.AnyAsync(b => b.ID == beer.ID));
            _db.Beers.Add(beer);
        }
        else
        {
            var existing = await _db.Beers.FindAsync(beer.ID)
                ?? throw new InvalidOperationException("No record found");
            existing.BeerName = beer.BeerName;
            existing.Rate = beer.Rate;
        }
        await _db.SaveChangesAsync();
        return beer;
    }

    public async Task DeleteBeerAsync(string id)
    {
        var beer = await _db.Beers.FindAsync(id)
            ?? throw new InvalidOperationException("No record found");
        _db.Beers.Remove(beer);
        await _db.SaveChangesAsync();
    }

    // ---------- Liquor (frmLiquor) ----------

    public async Task<IReadOnlyList<Liquor>> GetLiquorsAsync(string? search = null)
    {
        var query = _db.Liquors.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = Prefix(search);
            query = query.Where(l => EF.Functions.Like(l.LiquorName!, pattern));
        }
        return await query.OrderBy(l => l.LiquorName).ToListAsync();
    }

    public async Task<Liquor> SaveLiquorAsync(Liquor liquor)
    {
        if (string.IsNullOrWhiteSpace(liquor.LiquorName))
            throw new InvalidOperationException("Please select liquor name");
        if (liquor.Volume is null)
            throw new InvalidOperationException("Please enter volume");
        if (liquor.Rate is null)
            throw new InvalidOperationException("Please enter rate");

        if (liquor.ID == 0)
        {
            if (await _db.Liquors.AnyAsync(l => l.LiquorName == liquor.LiquorName && l.Volume == liquor.Volume))
                throw new InvalidOperationException("Record Already Exists");
            _db.Liquors.Add(liquor);
        }
        else
        {
            var existing = await _db.Liquors.FindAsync(liquor.ID)
                ?? throw new InvalidOperationException("No record found");
            existing.LiquorName = liquor.LiquorName;
            existing.Volume = liquor.Volume;
            existing.Rate = liquor.Rate;
        }
        await _db.SaveChangesAsync();
        return liquor;
    }

    public async Task DeleteLiquorAsync(int id)
    {
        var liquor = await _db.Liquors.FindAsync(id)
            ?? throw new InvalidOperationException("No record found");
        _db.Liquors.Remove(liquor);
        await _db.SaveChangesAsync();
    }

    // ---------- Liquor master (frmLiquor_Master) ----------

    public async Task<IReadOnlyList<LiquorMaster>> GetLiquorMastersAsync() =>
        await _db.LiquorMasters.AsNoTracking().OrderBy(m => m.LiquorName).ToListAsync();

    public async Task<LiquorMaster> SaveLiquorMasterAsync(LiquorMaster master)
    {
        if (string.IsNullOrWhiteSpace(master.LiquorName))
            throw new InvalidOperationException("Please enter liquor name");
        if (await _db.LiquorMasters.AnyAsync(m => m.LiquorName == master.LiquorName))
            throw new InvalidOperationException("Liquor Name Already Exists");
        _db.LiquorMasters.Add(master);
        await _db.SaveChangesAsync();
        return master;
    }

    public async Task DeleteLiquorMasterAsync(string liquorName)
    {
        var master = await _db.LiquorMasters.FindAsync(liquorName)
            ?? throw new InvalidOperationException("No record found");
        _db.LiquorMasters.Remove(master);
        await _db.SaveChangesAsync();
    }

    // ---------- Liquor stock (frmStock, tab 1) ----------

    public async Task<IReadOnlyList<Stock>> GetStocksAsync(string? search = null)
    {
        var query = _db.Stocks.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = Prefix(search);
            query = query.Where(s => EF.Functions.Like(s.LiquorName!, pattern));
        }
        return await query.OrderBy(s => s.LiquorName).ToListAsync();
    }

    public async Task<Stock> SaveStockAsync(Stock stock)
    {
        if (string.IsNullOrWhiteSpace(stock.LiquorName))
            throw new InvalidOperationException("Please select liquor name");
        if (stock.NoOfBottles is null)
            throw new InvalidOperationException("Please enter no. of bottles");
        if (stock.Volume is null)
            throw new InvalidOperationException("Please enter volume");

        if (string.IsNullOrWhiteSpace(stock.StockID))
        {
            if (await _db.Stocks.AnyAsync(s => s.LiquorName == stock.LiquorName))
                throw new InvalidOperationException("Liquor Name already exists, please update the stock of Liquor");
            do stock.StockID = "S-" + GenerateDigits(6);
            while (await _db.Stocks.AnyAsync(s => s.StockID == stock.StockID));
            stock.TotalVolume = stock.NoOfBottles * stock.Volume;
            stock.StockDate = DateTime.Now;
            _db.Stocks.Add(stock);
        }
        else
        {
            var existing = await _db.Stocks.FindAsync(stock.StockID)
                ?? throw new InvalidOperationException("No record found");
            existing.LiquorName = stock.LiquorName;
            existing.NoOfBottles = stock.NoOfBottles;
            existing.Volume = stock.Volume;
            existing.TotalVolume = (existing.TotalVolume ?? 0) + stock.NoOfBottles * stock.Volume;
            existing.StockDate = DateTime.Now;
            stock = existing;
        }
        await _db.SaveChangesAsync();
        return stock;
    }

    public async Task DeleteStockAsync(string stockId)
    {
        var stock = await _db.Stocks.FindAsync(stockId)
            ?? throw new InvalidOperationException("No record found");
        _db.Stocks.Remove(stock);
        await _db.SaveChangesAsync();
    }

    // ---------- Beer stock (frmStock, tab 2) ----------

    public async Task<IReadOnlyList<StockBeer>> GetBeerStocksAsync(string? search = null)
    {
        var query = _db.BeerStocks.AsNoTracking().Include(s => s.Beer);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = Prefix(search);
            return await query
                .Where(s => EF.Functions.Like(s.Beer!.BeerName!, pattern))
                .OrderBy(s => s.Beer!.BeerName)
                .ToListAsync();
        }
        return await query.OrderBy(s => s.Beer!.BeerName).ToListAsync();
    }

    public async Task<StockBeer> SaveBeerStockAsync(StockBeer stock)
    {
        if (string.IsNullOrWhiteSpace(stock.BeerID))
            throw new InvalidOperationException("Please select beer name");
        if (stock.NoOfBottles is null)
            throw new InvalidOperationException("Please enter no. of bottles");

        if (string.IsNullOrWhiteSpace(stock.StockID))
        {
            if (await _db.BeerStocks.AnyAsync(s => s.BeerID == stock.BeerID))
                throw new InvalidOperationException("Beer Name already exists, please update the stock of Beer");
            do stock.StockID = "S-" + GenerateDigits(5);
            while (await _db.BeerStocks.AnyAsync(s => s.StockID == stock.StockID));
            stock.StockDate = DateTime.Now;
            _db.BeerStocks.Add(stock);
        }
        else
        {
            var existing = await _db.BeerStocks.FindAsync(stock.StockID)
                ?? throw new InvalidOperationException("No record found");
            existing.BeerID = stock.BeerID;
            existing.NoOfBottles = stock.NoOfBottles;
            existing.StockDate = DateTime.Now;
            stock = existing;
        }
        await _db.SaveChangesAsync();
        return stock;
    }

    public async Task DeleteBeerStockAsync(string stockId)
    {
        var stock = await _db.BeerStocks.FindAsync(stockId)
            ?? throw new InvalidOperationException("No record found");
        _db.BeerStocks.Remove(stock);
        await _db.SaveChangesAsync();
    }

    // ---------- Purchased inventory (frmPurchaseInventory) ----------

    public async Task<IReadOnlyList<PurchasedInventory>> GetPurchasedInventoriesAsync(string? search = null)
    {
        var query = _db.PurchasedInventories.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = Prefix(search);
            query = query.Where(p =>
                EF.Functions.Like(p.ProductName!, pattern) ||
                EF.Functions.Like(p.Category!, pattern) ||
                EF.Functions.Like(p.PartyName!, pattern) ||
                EF.Functions.Like(p.TransactionType!, pattern));
        }
        return await query.OrderBy(p => p.ProductName).ThenBy(p => p.PurchaseDate).ToListAsync();
    }

    public async Task<PurchasedInventory> SavePurchasedInventoryAsync(PurchasedInventory inventory)
    {
        if (string.IsNullOrWhiteSpace(inventory.ProductName))
            throw new InvalidOperationException("Please select product name");
        if (string.IsNullOrWhiteSpace(inventory.Category))
            throw new InvalidOperationException("Please enter Category");
        if (string.IsNullOrWhiteSpace(inventory.TransactionType))
            throw new InvalidOperationException("Please select transaction type");
        if (inventory.Quantity is null)
            throw new InvalidOperationException("Please select quantity");
        if (string.IsNullOrWhiteSpace(inventory.Unit))
            throw new InvalidOperationException("Please select unit");
        if (inventory.Price is null)
            throw new InvalidOperationException("Please enter unit price");

        inventory.TotalPrice = (int)Math.Round(inventory.Quantity.Value * inventory.Price.Value);

        if (inventory.ID == 0)
        {
            _db.PurchasedInventories.Add(inventory);
        }
        else
        {
            var existing = await _db.PurchasedInventories.FindAsync(inventory.ID)
                ?? throw new InvalidOperationException("No record found");
            existing.ProductName = inventory.ProductName;
            existing.Category = inventory.Category;
            existing.TransactionType = inventory.TransactionType;
            existing.PartyName = inventory.PartyName;
            existing.PurchaseDate = inventory.PurchaseDate;
            existing.Quantity = inventory.Quantity;
            existing.Unit = inventory.Unit;
            existing.Price = inventory.Price;
            existing.TotalPrice = inventory.TotalPrice;
            inventory = existing;
        }
        await _db.SaveChangesAsync();
        return inventory;
    }

    public async Task DeletePurchasedInventoryAsync(int id)
    {
        var inventory = await _db.PurchasedInventories.FindAsync(id)
            ?? throw new InvalidOperationException("No record found");
        _db.PurchasedInventories.Remove(inventory);
        await _db.SaveChangesAsync();
    }

    // ---------- Additional tax config (Taxinfo) ----------

    public async Task<IReadOnlyList<TaxInfo>> GetTaxInfosAsync() =>
        await _db.TaxInfos.AsNoTracking().OrderBy(t => t.Salray).ToListAsync();

    public async Task<TaxInfo> SaveTaxInfoAsync(TaxInfo taxInfo)
    {
        if (string.IsNullOrWhiteSpace(taxInfo.Salray))
            throw new InvalidOperationException("Please enter tax key");

        var existing = await _db.TaxInfos.FindAsync(taxInfo.Salray);
        if (existing is null)
        {
            _db.TaxInfos.Add(taxInfo);
        }
        else
        {
            existing.TaxinP = taxInfo.TaxinP;
            taxInfo = existing;
        }
        await _db.SaveChangesAsync();
        return taxInfo;
    }

    public async Task DeleteTaxInfoAsync(string key)
    {
        var taxInfo = await _db.TaxInfos.FindAsync(key)
            ?? throw new InvalidOperationException("No record found");
        _db.TaxInfos.Remove(taxInfo);
        await _db.SaveChangesAsync();
    }
}
