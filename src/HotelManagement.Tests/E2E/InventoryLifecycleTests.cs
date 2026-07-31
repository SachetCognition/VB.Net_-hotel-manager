using FluentAssertions;
using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Services;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HotelManagement.Tests.E2E;

/// <summary>
/// E2E-003: Complete inventory lifecycle: Add Item -> Purchase -> Stock Update -> Order -> Stock Deduction
/// </summary>
public class InventoryLifecycleTests : IDisposable
{
    private readonly HotelDbContext _context;
    private readonly InventoryService _inventoryService;
    private readonly ExcelExportService _excelExportService;

    public InventoryLifecycleTests()
    {
        var options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new HotelDbContext(options);
        _inventoryService = new InventoryService(_context);
        _excelExportService = new ExcelExportService();
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task CompleteInventoryLifecycle_AddPurchaseStockUpdate()
    {
        // STEP 1: Add food items
        var dish1 = await _inventoryService.CreateDishAsync(
            new CreateDishRequest("Chicken Biryani", "Main Course", 350m));
        dish1.DishName.Should().Be("Chicken Biryani");

        var dish2 = await _inventoryService.CreateDishAsync(
            new CreateDishRequest("Paneer Tikka", "Starter", 250m));
        dish2.DishName.Should().Be("Paneer Tikka");

        // Verify duplicate detection
        var dupAct = async () => await _inventoryService.CreateDishAsync(
            new CreateDishRequest("Chicken Biryani", "Other", 400m));
        await dupAct.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already exists*");

        // STEP 2: Add beer items
        var beer = await _inventoryService.CreateBeerAsync(
            new CreateBeerRequest("Kingfisher", "Lager", 180m, 200));
        beer.BeerName.Should().Be("Kingfisher");
        beer.Quantity.Should().Be(200);

        // STEP 3: Add liquor items
        var liquor = await _inventoryService.CreateLiquorAsync(
            new CreateLiquorRequest("Old Monk", "Rum", 400m, 50));
        liquor.LiquorName.Should().Be("Old Monk");
        liquor.Quantity.Should().Be(50);

        // STEP 4: Purchase inventory
        var purchase = await _inventoryService.CreatePurchaseAsync(
            new CreatePurchaseRequest("Basmati Rice", "Grocery", 25, 60m,
                DateTime.Today, "Rice Mill Ltd", "Premium grade"));
        purchase.TotalAmount.Should().Be(1500m); // 25 * 60
        purchase.Supplier.Should().Be("Rice Mill Ltd");

        // STEP 5: Stock management (admin only)
        _context.Set<Stock>().Add(new Stock
        {
            ItemName = "Basmati Rice", Category = "Grocery", Quantity = 100, Rate = 60m, Unit = "kg"
        });
        await _context.SaveChangesAsync();

        var stocks = await _inventoryService.GetStockAsync();
        stocks.Should().HaveCount(1);

        var stock = stocks.First();
        await _inventoryService.UpdateStockAsync(stock.ID, new UpdateStockRequest(125), "Admin");

        var updatedStocks = await _inventoryService.GetStockAsync();
        updatedStocks.First().Quantity.Should().Be(125);

        // STEP 6: Verify User cannot update stock
        var act = async () => await _inventoryService.UpdateStockAsync(
            stock.ID, new UpdateStockRequest(150), "User");
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task InventoryExcelExport_ProducesValidFile()
    {
        // Add items
        await _inventoryService.CreateDishAsync(new CreateDishRequest("Item 1", "Cat A", 100m));
        await _inventoryService.CreateDishAsync(new CreateDishRequest("Item 2", "Cat B", 200m));
        await _inventoryService.CreateDishAsync(new CreateDishRequest("Item 3", "Cat A", 150m));

        var dishes = await _inventoryService.GetDishesAsync();
        dishes.Should().HaveCount(3);

        // Export to Excel
        var bytes = _excelExportService.ExportToExcel(dishes.ToList(), "Dishes");
        bytes.Should().NotBeNull();
        bytes.Length.Should().BeGreaterThan(0);

        // Verify Excel content
        using var stream = new MemoryStream(bytes);
        using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();
        worksheet.Name.Should().Be("Dishes");
        worksheet.RowsUsed().Count().Should().Be(4); // Header + 3 items
    }

    [Fact]
    public async Task InventorySearch_FiltersByNamePrefix()
    {
        await _inventoryService.CreateDishAsync(new CreateDishRequest("Butter Chicken", "Main", 350m));
        await _inventoryService.CreateDishAsync(new CreateDishRequest("Butter Naan", "Bread", 50m));
        await _inventoryService.CreateDishAsync(new CreateDishRequest("Paneer Tikka", "Starter", 250m));

        var butterItems = await _inventoryService.GetDishesAsync("Butter");
        butterItems.Should().HaveCount(2);
        butterItems.Should().AllSatisfy(d => d.DishName.Should().StartWith("Butter"));

        var paneerItems = await _inventoryService.GetDishesAsync("Paneer");
        paneerItems.Should().HaveCount(1);
    }
}
