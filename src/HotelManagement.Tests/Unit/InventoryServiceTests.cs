using FluentAssertions;
using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Services;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HotelManagement.Tests.Unit;

public class InventoryServiceTests : IDisposable
{
    private readonly HotelDbContext _context;
    private readonly InventoryService _service;

    public InventoryServiceTests()
    {
        var options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new HotelDbContext(options);
        _service = new InventoryService(_context);
    }

    public void Dispose() => _context.Dispose();

    // --- Dish Tests ---

    [Fact]
    public async Task CreateDishAsync_ValidDish_ReturnsResponse()
    {
        var request = new CreateDishRequest("Butter Chicken", "Main Course", 350m);
        var result = await _service.CreateDishAsync(request);

        result.DishName.Should().Be("Butter Chicken");
        result.Category.Should().Be("Main Course");
        result.Rate.Should().Be(350m);
    }

    [Fact]
    public async Task CreateDishAsync_DuplicateName_ThrowsInvalidOperation()
    {
        _context.Set<Dish>().Add(new Dish { DishName = "Butter Chicken", Category = "Main", Rate = 350m });
        await _context.SaveChangesAsync();

        var request = new CreateDishRequest("Butter Chicken", "Starter", 400m);
        var act = async () => await _service.CreateDishAsync(request);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already exists*");
    }

    [Fact]
    public async Task CreateDishAsync_EmptyName_ThrowsArgumentException()
    {
        var request = new CreateDishRequest("", "Main", 350m);
        var act = async () => await _service.CreateDishAsync(request);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*dish name*");
    }

    [Fact]
    public async Task UpdateDishAsync_DuplicateName_ThrowsInvalidOperation()
    {
        _context.Set<Dish>().Add(new Dish { DishName = "Dish A", Category = "Cat", Rate = 100m });
        _context.Set<Dish>().Add(new Dish { DishName = "Dish B", Category = "Cat", Rate = 200m });
        await _context.SaveChangesAsync();

        var dishB = await _context.Set<Dish>().FirstAsync(d => d.DishName == "Dish B");
        var request = new UpdateDishRequest("Dish A", "Cat", 200m);
        var act = async () => await _service.UpdateDishAsync(dishB.ID, request);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already exists*");
    }

    [Fact]
    public async Task DeleteDishAsync_ExistingDish_RemovesDish()
    {
        _context.Set<Dish>().Add(new Dish { DishName = "Test Dish", Category = "Cat", Rate = 100m });
        await _context.SaveChangesAsync();
        var dish = await _context.Set<Dish>().FirstAsync();

        await _service.DeleteDishAsync(dish.ID);

        var count = await _context.Set<Dish>().CountAsync();
        count.Should().Be(0);
    }

    // --- Beer Tests ---

    [Fact]
    public async Task CreateBeerAsync_ValidBeer_ReturnsResponse()
    {
        var request = new CreateBeerRequest("Kingfisher", "Lager", 150m, 100);
        var result = await _service.CreateBeerAsync(request);

        result.BeerName.Should().Be("Kingfisher");
        result.Quantity.Should().Be(100);
    }

    [Fact]
    public async Task CreateBeerAsync_EmptyName_ThrowsArgumentException()
    {
        var request = new CreateBeerRequest("", "Lager", 150m, 100);
        var act = async () => await _service.CreateBeerAsync(request);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*beer name*");
    }

    // --- Liquor Tests ---

    [Fact]
    public async Task CreateLiquorAsync_ValidLiquor_ReturnsResponse()
    {
        var request = new CreateLiquorRequest("Whiskey", "Spirit", 500m, 50);
        var result = await _service.CreateLiquorAsync(request);

        result.LiquorName.Should().Be("Whiskey");
        result.Quantity.Should().Be(50);
    }

    [Fact]
    public async Task CreateLiquorAsync_EmptyName_ThrowsArgumentException()
    {
        var request = new CreateLiquorRequest("", "Spirit", 500m, 50);
        var act = async () => await _service.CreateLiquorAsync(request);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*liquor name*");
    }

    // --- Purchase Inventory Tests ---

    [Fact]
    public async Task CreatePurchaseAsync_ValidPurchase_CalculatesTotalAmount()
    {
        var request = new CreatePurchaseRequest("Rice", "Grocery", 50, 40m, DateTime.Today, "Supplier A", "");
        var result = await _service.CreatePurchaseAsync(request);

        result.TotalAmount.Should().Be(2000m); // 50 * 40
        result.ItemName.Should().Be("Rice");
    }

    [Fact]
    public async Task CreatePurchaseAsync_EmptyItemName_ThrowsArgumentException()
    {
        var request = new CreatePurchaseRequest("", "Grocery", 50, 40m, DateTime.Today, "Supplier", "");
        var act = async () => await _service.CreatePurchaseAsync(request);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*item name*");
    }

    // --- Stock Tests ---

    [Fact]
    public async Task UpdateStockAsync_AdminRole_UpdatesStock()
    {
        _context.Set<Stock>().Add(new Stock { ItemName = "Rice", Category = "Grocery", Quantity = 100, Rate = 40m, Unit = "kg" });
        await _context.SaveChangesAsync();
        var stock = await _context.Set<Stock>().FirstAsync();

        await _service.UpdateStockAsync(stock.ID, new UpdateStockRequest(150), "Admin");

        var updated = await _context.Set<Stock>().FindAsync(stock.ID);
        updated!.Quantity.Should().Be(150);
    }

    [Fact]
    public async Task UpdateStockAsync_UserRole_ThrowsUnauthorized()
    {
        _context.Set<Stock>().Add(new Stock { ItemName = "Rice", Category = "Grocery", Quantity = 100, Rate = 40m, Unit = "kg" });
        await _context.SaveChangesAsync();
        var stock = await _context.Set<Stock>().FirstAsync();

        var act = async () => await _service.UpdateStockAsync(stock.ID, new UpdateStockRequest(150), "User");
        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("*admin*");
    }

    [Fact]
    public async Task GetDishesAsync_WithSearch_FiltersResults()
    {
        _context.Set<Dish>().Add(new Dish { DishName = "Butter Chicken", Category = "Main", Rate = 350m });
        _context.Set<Dish>().Add(new Dish { DishName = "Paneer Tikka", Category = "Starter", Rate = 250m });
        await _context.SaveChangesAsync();

        var results = await _service.GetDishesAsync("Butter");
        results.Should().HaveCount(1);
        results.First().DishName.Should().Be("Butter Chicken");
    }
}
