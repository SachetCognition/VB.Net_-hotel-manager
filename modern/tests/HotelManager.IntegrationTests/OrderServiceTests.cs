using HotelManager.Domain.Entities;
using HotelManager.Infrastructure;
using HotelManager.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.IntegrationTests;

public class OrderServiceTests : IClassFixture<SqliteDbFixture>
{
    private readonly SqliteDbFixture _fixture;
    private readonly OrderService _service;

    public OrderServiceTests(SqliteDbFixture fixture)
    {
        _fixture = fixture;
        _service = new OrderService(new FixtureDbContextFactory(fixture));
    }

    [Fact]
    public async Task GenerateRoomOrderNo_StartsAtOne()
    {
        var orderNo = await _service.GenerateRoomOrderNoAsync();
        Assert.StartsWith("OD-", orderNo);
    }

    [Fact]
    public async Task SaveRoomOrder_ComputesTotals_AndPersistsProductsAndTax()
    {
        var order = new OrderInfo
        {
            OrderNo = "OD-T001",
            OrderDate = DateTime.Today,
            VATPer = 10,
            STPer = 5,
            TotalPayment = 100
        };
        var products = new[]
        {
            new OrderedProduct { ProductName = "Pizza", Rate = 100, Quantity = 2, Amount = 200 },
            new OrderedProduct { ProductName = "Beer", Rate = 50, Quantity = 1, Amount = 50 }
        };
        var tax = new TaxOrder { EducationalTax = 0.1, HEduTax = 0.05 };

        var saved = await _service.SaveRoomOrderAsync(order, products, tax);

        // SubTotal 250, VAT 25.00, ST on 275 = 13.75, GrandTotal CInt(288.75) = 289
        Assert.Equal(250, saved.SubTotal);
        Assert.Equal(25.00, saved.VATAmount);
        Assert.Equal(13.75, saved.STAmount);
        Assert.Equal(289, saved.GrandTotal);
        Assert.Equal(189, saved.PaymentDue);

        var loaded = await _service.GetRoomOrderAsync(saved.ID);
        Assert.NotNull(loaded);
        Assert.Equal(2, loaded!.Products.Count);
        Assert.Single(loaded.Taxes);
    }

    [Fact]
    public async Task SaveRoomOrder_Update_ReplacesProducts()
    {
        var saved = await _service.SaveRoomOrderAsync(
            new OrderInfo { OrderNo = "OD-T002" },
            new[] { new OrderedProduct { ProductName = "Tea", Rate = 10, Quantity = 1, Amount = 10 } });

        saved = await _service.SaveRoomOrderAsync(
            new OrderInfo { ID = saved.ID, OrderNo = "OD-T002" },
            new[]
            {
                new OrderedProduct { ProductName = "Coffee", Rate = 20, Quantity = 1, Amount = 20 },
                new OrderedProduct { ProductName = "Cake", Rate = 30, Quantity = 1, Amount = 30 }
            });

        var loaded = await _service.GetRoomOrderAsync(saved.ID);
        Assert.Equal(2, loaded!.Products.Count);
        Assert.Equal(50, loaded.SubTotal);
        Assert.DoesNotContain(loaded.Products, p => p.ProductName == "Tea");
    }

    [Fact]
    public async Task DeleteRoomOrder_RemovesOrderProductsAndTaxes()
    {
        var saved = await _service.SaveRoomOrderAsync(
            new OrderInfo { OrderNo = "OD-T003" },
            new[] { new OrderedProduct { ProductName = "Juice", Rate = 15, Quantity = 1, Amount = 15 } },
            new TaxOrder());

        await _service.DeleteRoomOrderAsync(saved.ID);

        Assert.Null(await _service.GetRoomOrderAsync(saved.ID));
        await using var db = _fixture.CreateContext();
        Assert.False(await db.OrderedProducts.AnyAsync(p => p.OrderID == saved.ID));
        Assert.False(await db.TaxOrders.AnyAsync(t => t.OrderID == saved.ID));
    }

    [Fact]
    public async Task SaveRestaurantOrder_ComputesTotals_AndPersists()
    {
        var order = new RestaurantOrderInfo
        {
            OrderNo = "ROD-T001",
            OrderDate = DateTime.Today,
            VATPer = 0,
            STPer = 10,
            TotalPayment = 0
        };
        var products = new[]
        {
            new RestaurantOrderedProduct { ProductName = "Biryani", Rate = 120, Quantity = 3, Amount = 360 }
        };

        var saved = await _service.SaveRestaurantOrderAsync(order, products, new TaxRestaurantOrder());

        // SubTotal 360, VAT 0, ST 36.00, GrandTotal 396
        Assert.Equal(360, saved.SubTotal);
        Assert.Equal(0, saved.VATAmount);
        Assert.Equal(36.00, saved.STAmount);
        Assert.Equal(396, saved.GrandTotal);
        Assert.Equal(396, saved.PaymentDue);

        var loaded = await _service.GetRestaurantOrderAsync(saved.ID);
        Assert.Single(loaded!.Products);
        Assert.Single(loaded.Taxes);
    }

    [Fact]
    public async Task GetRestaurantOrders_SearchFiltersByOrderNo()
    {
        await _service.SaveRestaurantOrderAsync(
            new RestaurantOrderInfo { OrderNo = "ROD-SRCH-1" },
            new[] { new RestaurantOrderedProduct { ProductName = "Soup", Rate = 40, Quantity = 1, Amount = 40 } });

        var results = await _service.GetRestaurantOrdersAsync("ROD-SRCH");
        Assert.All(results, o => Assert.Contains("ROD-SRCH", o.OrderNo));
        Assert.NotEmpty(results);
    }

    [Fact]
    public async Task DeleteRestaurantOrder_RemovesEverything()
    {
        var saved = await _service.SaveRestaurantOrderAsync(
            new RestaurantOrderInfo { OrderNo = "ROD-T002" },
            new[] { new RestaurantOrderedProduct { ProductName = "Salad", Rate = 60, Quantity = 1, Amount = 60 } },
            new TaxRestaurantOrder());

        await _service.DeleteRestaurantOrderAsync(saved.ID);

        Assert.Null(await _service.GetRestaurantOrderAsync(saved.ID));
        await using var db = _fixture.CreateContext();
        Assert.False(await db.RestaurantOrderedProducts.AnyAsync(p => p.OrderID == saved.ID));
        Assert.False(await db.TaxRestaurantOrders.AnyAsync(t => t.OrderID == saved.ID));
    }
}
