using HotelManager.Domain.Entities;
using HotelManager.Infrastructure;
using HotelManager.Web.Services.Inventory;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.IntegrationTests.Inventory;

/// <summary>
/// Per-test in-memory SQLite fixture owned by Child D so each test gets an
/// isolated database (the shared SqliteDbFixture would leak state between tests).
/// </summary>
public sealed class InventoryDbFixture : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<HotelDbContext> _options;

    public InventoryDbFixture()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseSqlite(_connection)
            .Options;
        using var db = new HotelDbContext(_options);
        db.Database.EnsureCreated();
    }

    public HotelDbContext CreateContext() => new(_options);

    public void Dispose() => _connection.Dispose();
}

public class InventoryServiceTests : IDisposable
{
    private readonly InventoryDbFixture _fixture = new();
    private readonly HotelDbContext _db;
    private readonly InventoryService _service;

    public InventoryServiceTests()
    {
        _db = _fixture.CreateContext();
        _service = new InventoryService(_db);
    }

    public void Dispose()
    {
        _db.Dispose();
        _fixture.Dispose();
    }

    // ---------- Dish ----------

    [Fact]
    public async Task SaveDish_CreatesAndReads()
    {
        var dish = await _service.SaveDishAsync(new Dish { DishName = "Paneer Tikka", Category = "Veg", Rate = 250 });
        Assert.True(dish.ID > 0);
        var all = await _service.GetDishesAsync();
        Assert.Single(all);
        Assert.Equal("Paneer Tikka", all[0].DishName);
    }

    [Fact]
    public async Task SaveDish_DuplicateName_Throws()
    {
        await _service.SaveDishAsync(new Dish { DishName = "Dal", Category = "Veg", Rate = 100 });
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.SaveDishAsync(new Dish { DishName = "Dal", Category = "Veg", Rate = 120 }));
        Assert.Contains("Already Exists", ex.Message);
    }

    [Fact]
    public async Task SaveDish_MissingFields_Throws()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.SaveDishAsync(new Dish { Category = "Veg", Rate = 100 }));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.SaveDishAsync(new Dish { DishName = "X", Rate = 100 }));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.SaveDishAsync(new Dish { DishName = "X", Category = "Veg" }));
    }

    [Fact]
    public async Task SaveDish_UpdatesExisting()
    {
        var dish = await _service.SaveDishAsync(new Dish { DishName = "Roti", Category = "Veg", Rate = 20 });
        await _service.SaveDishAsync(new Dish { ID = dish.ID, DishName = "Butter Roti", Category = "Veg", Rate = 30 });
        var all = await _service.GetDishesAsync();
        Assert.Single(all);
        Assert.Equal("Butter Roti", all[0].DishName);
        Assert.Equal(30, all[0].Rate);
    }

    [Fact]
    public async Task DeleteDish_RemovesRecord()
    {
        var dish = await _service.SaveDishAsync(new Dish { DishName = "Naan", Category = "Veg", Rate = 40 });
        await _service.DeleteDishAsync(dish.ID);
        Assert.Empty(await _service.GetDishesAsync());
    }

    [Fact]
    public async Task DeleteDish_Missing_Throws() =>
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteDishAsync(999));

    [Fact]
    public async Task GetDishes_SearchFiltersByPrefix()
    {
        await _service.SaveDishAsync(new Dish { DishName = "Chicken Curry", Category = "NonVeg", Rate = 300 });
        await _service.SaveDishAsync(new Dish { DishName = "Paneer Curry", Category = "Veg", Rate = 250 });
        var result = await _service.GetDishesAsync("Chicken");
        Assert.Single(result);
        Assert.Equal("Chicken Curry", result[0].DishName);
    }

    [Fact]
    public async Task GetDishes_SearchWithSqlInjectionText_IsSafe()
    {
        await _service.SaveDishAsync(new Dish { DishName = "Safe Dish", Category = "Veg", Rate = 10 });
        var result = await _service.GetDishesAsync("'; DROP TABLE Dish;--");
        Assert.Empty(result);
        Assert.Single(await _service.GetDishesAsync());
    }

    [Fact]
    public async Task GetDishes_SearchMatchesLiteralQuote()
    {
        await _service.SaveDishAsync(new Dish { DishName = "O'Brien Potatoes", Category = "Veg", Rate = 90 });
        var result = await _service.GetDishesAsync("O'Brien");
        Assert.Single(result);
    }

    // ---------- Beer ----------

    [Fact]
    public async Task SaveBeer_GeneratesPrefixedId()
    {
        var beer = await _service.SaveBeerAsync(new Beer { ID = "", BeerName = "Kingfisher", Rate = 180 });
        Assert.StartsWith("B-", beer.ID);
        Assert.Equal(5, beer.ID.Length);
    }

    [Fact]
    public async Task SaveBeer_DuplicateName_Throws()
    {
        await _service.SaveBeerAsync(new Beer { ID = "", BeerName = "Tuborg", Rate = 150 });
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.SaveBeerAsync(new Beer { ID = "", BeerName = "Tuborg", Rate = 160 }));
    }

    [Fact]
    public async Task SaveBeer_UpdatesExisting()
    {
        var beer = await _service.SaveBeerAsync(new Beer { ID = "", BeerName = "Carlsberg", Rate = 200 });
        await _service.SaveBeerAsync(new Beer { ID = beer.ID, BeerName = "Carlsberg Elephant", Rate = 220 });
        var all = await _service.GetBeersAsync();
        Assert.Single(all);
        Assert.Equal("Carlsberg Elephant", all[0].BeerName);
    }

    [Fact]
    public async Task DeleteBeer_RemovesRecord()
    {
        var beer = await _service.SaveBeerAsync(new Beer { ID = "", BeerName = "Corona", Rate = 300 });
        await _service.DeleteBeerAsync(beer.ID);
        Assert.Empty(await _service.GetBeersAsync());
    }

    [Fact]
    public async Task GetBeers_SearchFilters()
    {
        await _service.SaveBeerAsync(new Beer { ID = "", BeerName = "Heineken", Rate = 250 });
        await _service.SaveBeerAsync(new Beer { ID = "", BeerName = "Budweiser", Rate = 240 });
        var result = await _service.GetBeersAsync("Hei");
        Assert.Single(result);
        Assert.Equal("Heineken", result[0].BeerName);
    }

    [Fact]
    public async Task GetBeers_SearchWithWildcardText_TreatsAsPattern_NoError()
    {
        await _service.SaveBeerAsync(new Beer { ID = "", BeerName = "Amber Ale", Rate = 100 });
        var result = await _service.GetBeersAsync("'; DELETE FROM Beer;--");
        Assert.Empty(result);
        Assert.Single(await _service.GetBeersAsync());
    }

    // ---------- Liquor + LiquorMaster ----------

    [Fact]
    public async Task SaveLiquorMaster_CreatesAndDuplicateThrows()
    {
        await _service.SaveLiquorMasterAsync(new LiquorMaster { LiquorName = "Whisky" });
        Assert.Single(await _service.GetLiquorMastersAsync());
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.SaveLiquorMasterAsync(new LiquorMaster { LiquorName = "Whisky" }));
    }

    [Fact]
    public async Task DeleteLiquorMaster_RemovesRecord()
    {
        await _service.SaveLiquorMasterAsync(new LiquorMaster { LiquorName = "Rum" });
        await _service.DeleteLiquorMasterAsync("Rum");
        Assert.Empty(await _service.GetLiquorMastersAsync());
    }

    [Fact]
    public async Task SaveLiquor_CreatesWithMasterAndDuplicateThrows()
    {
        await _service.SaveLiquorMasterAsync(new LiquorMaster { LiquorName = "Vodka" });
        var liquor = await _service.SaveLiquorAsync(new Liquor { LiquorName = "Vodka", Volume = 750, Rate = 900 });
        Assert.True(liquor.ID > 0);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.SaveLiquorAsync(new Liquor { LiquorName = "Vodka", Volume = 750, Rate = 950 }));
    }

    [Fact]
    public async Task SaveLiquor_SameNameDifferentVolume_Allowed()
    {
        await _service.SaveLiquorMasterAsync(new LiquorMaster { LiquorName = "Gin" });
        await _service.SaveLiquorAsync(new Liquor { LiquorName = "Gin", Volume = 750, Rate = 800 });
        await _service.SaveLiquorAsync(new Liquor { LiquorName = "Gin", Volume = 375, Rate = 450 });
        Assert.Equal(2, (await _service.GetLiquorsAsync()).Count);
    }

    [Fact]
    public async Task SaveLiquor_UpdatesExisting()
    {
        await _service.SaveLiquorMasterAsync(new LiquorMaster { LiquorName = "Brandy" });
        var liquor = await _service.SaveLiquorAsync(new Liquor { LiquorName = "Brandy", Volume = 750, Rate = 700 });
        await _service.SaveLiquorAsync(new Liquor { ID = liquor.ID, LiquorName = "Brandy", Volume = 1000, Rate = 850 });
        var all = await _service.GetLiquorsAsync();
        Assert.Single(all);
        Assert.Equal(1000, all[0].Volume);
    }

    [Fact]
    public async Task DeleteLiquor_RemovesRecord()
    {
        await _service.SaveLiquorMasterAsync(new LiquorMaster { LiquorName = "Tequila" });
        var liquor = await _service.SaveLiquorAsync(new Liquor { LiquorName = "Tequila", Volume = 750, Rate = 1200 });
        await _service.DeleteLiquorAsync(liquor.ID);
        Assert.Empty(await _service.GetLiquorsAsync());
    }

    [Fact]
    public async Task GetLiquors_SearchFilters()
    {
        await _service.SaveLiquorMasterAsync(new LiquorMaster { LiquorName = "Scotch" });
        await _service.SaveLiquorMasterAsync(new LiquorMaster { LiquorName = "Bourbon" });
        await _service.SaveLiquorAsync(new Liquor { LiquorName = "Scotch", Volume = 750, Rate = 2000 });
        await _service.SaveLiquorAsync(new Liquor { LiquorName = "Bourbon", Volume = 750, Rate = 1800 });
        var result = await _service.GetLiquorsAsync("Sco");
        Assert.Single(result);
        Assert.Equal("Scotch", result[0].LiquorName);
    }

    // ---------- Liquor stock ----------

    [Fact]
    public async Task SaveStock_CreatesWithGeneratedIdAndTotalVolume()
    {
        await _service.SaveLiquorMasterAsync(new LiquorMaster { LiquorName = "Whisky" });
        var stock = await _service.SaveStockAsync(new Stock { StockID = "", LiquorName = "Whisky", NoOfBottles = 10, Volume = 750 });
        Assert.StartsWith("S-", stock.StockID);
        Assert.Equal(7500, stock.TotalVolume);
        Assert.NotNull(stock.StockDate);
    }

    [Fact]
    public async Task SaveStock_DuplicateLiquor_Throws()
    {
        await _service.SaveLiquorMasterAsync(new LiquorMaster { LiquorName = "Rum" });
        await _service.SaveStockAsync(new Stock { StockID = "", LiquorName = "Rum", NoOfBottles = 5, Volume = 750 });
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.SaveStockAsync(new Stock { StockID = "", LiquorName = "Rum", NoOfBottles = 3, Volume = 750 }));
    }

    [Fact]
    public async Task SaveStock_UpdateAddsToTotalVolume()
    {
        await _service.SaveLiquorMasterAsync(new LiquorMaster { LiquorName = "Vodka" });
        var stock = await _service.SaveStockAsync(new Stock { StockID = "", LiquorName = "Vodka", NoOfBottles = 4, Volume = 750 });
        var updated = await _service.SaveStockAsync(new Stock { StockID = stock.StockID, LiquorName = "Vodka", NoOfBottles = 2, Volume = 750 });
        Assert.Equal(4 * 750 + 2 * 750, updated.TotalVolume);
    }

    [Fact]
    public async Task DeleteStock_RemovesRecord()
    {
        await _service.SaveLiquorMasterAsync(new LiquorMaster { LiquorName = "Gin" });
        var stock = await _service.SaveStockAsync(new Stock { StockID = "", LiquorName = "Gin", NoOfBottles = 1, Volume = 750 });
        await _service.DeleteStockAsync(stock.StockID);
        Assert.Empty(await _service.GetStocksAsync());
    }

    [Fact]
    public async Task GetStocks_SearchFiltersByLiquorName()
    {
        await _service.SaveLiquorMasterAsync(new LiquorMaster { LiquorName = "Scotch" });
        await _service.SaveLiquorMasterAsync(new LiquorMaster { LiquorName = "Brandy" });
        await _service.SaveStockAsync(new Stock { StockID = "", LiquorName = "Scotch", NoOfBottles = 2, Volume = 750 });
        await _service.SaveStockAsync(new Stock { StockID = "", LiquorName = "Brandy", NoOfBottles = 3, Volume = 750 });
        var result = await _service.GetStocksAsync("Sco");
        Assert.Single(result);
        Assert.Equal("Scotch", result[0].LiquorName);
        Assert.Empty(await _service.GetStocksAsync("' OR 1=1 --"));
    }

    // ---------- Beer stock ----------

    private async Task<Beer> AddBeer(string name) =>
        await _service.SaveBeerAsync(new Beer { ID = "", BeerName = name, Rate = 150 });

    [Fact]
    public async Task SaveBeerStock_CreatesWithGeneratedId()
    {
        var beer = await AddBeer("Kingfisher");
        var stock = await _service.SaveBeerStockAsync(new StockBeer { StockID = "", BeerID = beer.ID, NoOfBottles = 24 });
        Assert.StartsWith("S-", stock.StockID);
        Assert.NotNull(stock.StockDate);
    }

    [Fact]
    public async Task SaveBeerStock_DuplicateBeer_Throws()
    {
        var beer = await AddBeer("Tuborg");
        await _service.SaveBeerStockAsync(new StockBeer { StockID = "", BeerID = beer.ID, NoOfBottles = 12 });
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.SaveBeerStockAsync(new StockBeer { StockID = "", BeerID = beer.ID, NoOfBottles = 6 }));
    }

    [Fact]
    public async Task SaveBeerStock_UpdatesExisting()
    {
        var beer = await AddBeer("Corona");
        var stock = await _service.SaveBeerStockAsync(new StockBeer { StockID = "", BeerID = beer.ID, NoOfBottles = 12 });
        var updated = await _service.SaveBeerStockAsync(new StockBeer { StockID = stock.StockID, BeerID = beer.ID, NoOfBottles = 30 });
        Assert.Equal(30, updated.NoOfBottles);
        Assert.Single(await _service.GetBeerStocksAsync());
    }

    [Fact]
    public async Task DeleteBeerStock_RemovesRecord()
    {
        var beer = await AddBeer("Heineken");
        var stock = await _service.SaveBeerStockAsync(new StockBeer { StockID = "", BeerID = beer.ID, NoOfBottles = 6 });
        await _service.DeleteBeerStockAsync(stock.StockID);
        Assert.Empty(await _service.GetBeerStocksAsync());
    }

    [Fact]
    public async Task GetBeerStocks_SearchFiltersByBeerName()
    {
        var kf = await AddBeer("Kingfisher");
        var bud = await AddBeer("Budweiser");
        await _service.SaveBeerStockAsync(new StockBeer { StockID = "", BeerID = kf.ID, NoOfBottles = 10 });
        await _service.SaveBeerStockAsync(new StockBeer { StockID = "", BeerID = bud.ID, NoOfBottles = 20 });
        var result = await _service.GetBeerStocksAsync("King");
        Assert.Single(result);
        Assert.Equal(kf.ID, result[0].BeerID);
    }

    // ---------- Purchased inventory ----------

    private static PurchasedInventory NewPurchase(string product = "Rice", string category = "Grocery",
        string type = "Cash", string party = "ABC Traders", double qty = 10, int price = 50) => new()
    {
        ProductName = product,
        Category = category,
        TransactionType = type,
        PartyName = party,
        PurchaseDate = new DateTime(2024, 1, 15),
        Quantity = qty,
        Unit = "Kg",
        Price = price
    };

    [Fact]
    public async Task SavePurchasedInventory_ComputesTotalPrice()
    {
        var record = await _service.SavePurchasedInventoryAsync(NewPurchase(qty: 10, price: 50));
        Assert.True(record.ID > 0);
        Assert.Equal(500, record.TotalPrice);
    }

    [Fact]
    public async Task SavePurchasedInventory_UpdatesExisting()
    {
        var record = await _service.SavePurchasedInventoryAsync(NewPurchase());
        var edited = NewPurchase(qty: 20, price: 60);
        edited.ID = record.ID;
        var updated = await _service.SavePurchasedInventoryAsync(edited);
        Assert.Equal(1200, updated.TotalPrice);
        Assert.Single(await _service.GetPurchasedInventoriesAsync());
    }

    [Fact]
    public async Task SavePurchasedInventory_MissingFields_Throws()
    {
        var record = NewPurchase();
        record.ProductName = null;
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.SavePurchasedInventoryAsync(record));
    }

    [Fact]
    public async Task DeletePurchasedInventory_RemovesRecord()
    {
        var record = await _service.SavePurchasedInventoryAsync(NewPurchase());
        await _service.DeletePurchasedInventoryAsync(record.ID);
        Assert.Empty(await _service.GetPurchasedInventoriesAsync());
    }

    [Fact]
    public async Task GetPurchasedInventories_SearchAcrossFields()
    {
        await _service.SavePurchasedInventoryAsync(NewPurchase(product: "Rice", party: "ABC Traders", type: "Cash"));
        await _service.SavePurchasedInventoryAsync(NewPurchase(product: "Oil", party: "XYZ Suppliers", type: "Credit"));

        Assert.Single(await _service.GetPurchasedInventoriesAsync("Rice"));
        Assert.Single(await _service.GetPurchasedInventoriesAsync("XYZ"));
        Assert.Single(await _service.GetPurchasedInventoriesAsync("Credit"));
        Assert.Empty(await _service.GetPurchasedInventoriesAsync("'; DROP TABLE PurchasedInventory;--"));
        Assert.Equal(2, (await _service.GetPurchasedInventoriesAsync()).Count);
    }

    // ---------- TaxInfo ----------

    [Fact]
    public async Task SaveTaxInfo_UpsertsByKey()
    {
        await _service.SaveTaxInfoAsync(new TaxInfo { Salray = "ServiceTax", TaxinP = 10 });
        var updated = await _service.SaveTaxInfoAsync(new TaxInfo { Salray = "ServiceTax", TaxinP = 12 });
        Assert.Equal(12, updated.TaxinP);
        var all = await _service.GetTaxInfosAsync();
        Assert.Single(all);
        Assert.Equal(12, all[0].TaxinP);
    }

    [Fact]
    public async Task DeleteTaxInfo_RemovesRecord()
    {
        await _service.SaveTaxInfoAsync(new TaxInfo { Salray = "GST", TaxinP = 18 });
        await _service.DeleteTaxInfoAsync("GST");
        Assert.Empty(await _service.GetTaxInfosAsync());
    }

    [Fact]
    public async Task DeleteTaxInfo_Missing_Throws() =>
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteTaxInfoAsync("Nope"));
}
