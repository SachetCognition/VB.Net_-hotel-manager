using HotelManagement.Core.Services;
using Xunit;

namespace HotelManagement.Tests.Unit;

/// <summary>
/// Category 3: Unit Tests — Inventory Calculations (~10 tests)
/// </summary>
public class InventoryServiceTests
{
    private readonly InventoryService _svc = new();

    // TC-INV-001: TotalPrice = Quantity × UnitPrice
    [Fact]
    public void CalculateTotalPrice_Basic()
    {
        Assert.Equal(500m, _svc.CalculateTotalPrice(10, 50m));
    }

    // TC-INV-002: TotalVolume = (NoOfBottles × Volume) + ExistingVolume
    [Fact]
    public void CalculateTotalVolume_Basic()
    {
        Assert.Equal(850m, _svc.CalculateTotalVolume(5, 100m, 350m));
    }

    // TC-INV-003: Zero quantity
    [Fact]
    public void CalculateTotalPrice_ZeroQuantity()
    {
        Assert.Equal(0m, _svc.CalculateTotalPrice(0, 50m));
    }

    // TC-INV-004: Zero unit price
    [Fact]
    public void CalculateTotalPrice_ZeroPrice()
    {
        Assert.Equal(0m, _svc.CalculateTotalPrice(10, 0m));
    }

    // TC-INV-005: Large quantity
    [Fact]
    public void CalculateTotalPrice_LargeQuantity()
    {
        Assert.Equal(100000m, _svc.CalculateTotalPrice(1000, 100m));
    }

    // TC-INV-006: Zero existing volume
    [Fact]
    public void CalculateTotalVolume_ZeroExisting()
    {
        Assert.Equal(500m, _svc.CalculateTotalVolume(5, 100m, 0m));
    }

    // TC-INV-007: Zero bottles
    [Fact]
    public void CalculateTotalVolume_ZeroBottles()
    {
        Assert.Equal(350m, _svc.CalculateTotalVolume(0, 100m, 350m));
    }

    // TC-INV-008: Decimal volume
    [Fact]
    public void CalculateTotalVolume_DecimalVolume()
    {
        Assert.Equal(625.5m, _svc.CalculateTotalVolume(5, 75.1m, 250m));
    }

    // TC-INV-009: Single item
    [Fact]
    public void CalculateTotalPrice_SingleItem()
    {
        Assert.Equal(99.99m, _svc.CalculateTotalPrice(1, 99.99m));
    }

    // TC-INV-010: Both zeros
    [Fact]
    public void CalculateTotalVolume_AllZeros()
    {
        Assert.Equal(0m, _svc.CalculateTotalVolume(0, 0m, 0m));
    }
}
