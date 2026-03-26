using FluentAssertions;
using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Services;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HotelManagement.Tests.Integration;

public class InventoryIntegrationTests : IDisposable
{
    private readonly HotelDbContext _context;
    private readonly InventoryService _inventoryService;

    public InventoryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new HotelDbContext(options);
        _inventoryService = new InventoryService(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task FullDishLifecycle_CreateReadUpdateDelete()
    {
        // Create
        var created = await _inventoryService.CreateDishAsync(
            new CreateDishRequest("Biryani", "Main Course", 250m));
        created.DishName.Should().Be("Biryani");

        // Read
        var fetched = await _inventoryService.GetDishByIdAsync(created.ID);
        fetched.Should().NotBeNull();
        fetched!.DishName.Should().Be("Biryani");

        // Update
        var updated = await _inventoryService.UpdateDishAsync(
            created.ID, new UpdateDishRequest("Hyderabadi Biryani", "Main Course", 300m));
        updated.DishName.Should().Be("Hyderabadi Biryani");
        updated.Rate.Should().Be(300m);

        // Delete
        await _inventoryService.DeleteDishAsync(created.ID);
        var deleted = await _inventoryService.GetDishByIdAsync(created.ID);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task PurchaseInventory_CalculatesTotalAmount()
    {
        var purchase = await _inventoryService.CreatePurchaseAsync(
            new CreatePurchaseRequest("Chicken", "Meat", 10, 200m, DateTime.Today, "Farm Fresh", ""));

        purchase.TotalAmount.Should().Be(2000m); // 10 * 200
        purchase.ItemName.Should().Be("Chicken");

        // Verify in database
        var purchases = await _inventoryService.GetPurchasesAsync();
        purchases.Should().HaveCount(1);
    }

    [Fact]
    public async Task StockUpdate_AdminOnly()
    {
        _context.Set<Stock>().Add(new Stock
        {
            ItemName = "Rice", Category = "Grocery", Quantity = 100, Rate = 40m, Unit = "kg"
        });
        await _context.SaveChangesAsync();
        var stock = await _context.Set<Stock>().FirstAsync();

        // Admin can update
        await _inventoryService.UpdateStockAsync(stock.ID, new UpdateStockRequest(200), "Admin");
        var updated = await _context.Set<Stock>().FindAsync(stock.ID);
        updated!.Quantity.Should().Be(200);

        // User cannot update
        var act = async () => await _inventoryService.UpdateStockAsync(
            stock.ID, new UpdateStockRequest(300), "User");
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
