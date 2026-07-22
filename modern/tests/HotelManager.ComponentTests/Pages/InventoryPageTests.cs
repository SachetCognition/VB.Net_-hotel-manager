using HotelManager.Web.Components.Pages.Inventory;
using MudBlazor;
using BeerPage = HotelManager.Web.Components.Pages.Inventory.Beer;

namespace HotelManager.ComponentTests.Pages;

public class InventoryPageTests : ComponentTestBase
{
    [Fact]
    public void Food_renders_saves_and_deletes()
    {
        var cut = RenderPage<Food>();
        Assert.Contains("Soup", cut.Markup);
        ClickPrimary(cut);
        InventorySvc.Verify(s => s.SaveDishAsync(It.IsAny<Dish>()), Times.Once);
        ClickIcon(cut, Icons.Material.Filled.Delete);
        InventorySvc.Verify(s => s.DeleteDishAsync(1), Times.Once);
    }

    [Fact]
    public void Beer_renders_saves_and_deletes()
    {
        var cut = RenderPage<BeerPage>();
        Assert.Contains("Lager", cut.Markup);
        ClickPrimary(cut);
        InventorySvc.Verify(s => s.SaveBeerAsync(It.IsAny<HotelManager.Domain.Entities.Beer>()), Times.Once);
        ClickIcon(cut, Icons.Material.Filled.Delete);
        InventorySvc.Verify(s => s.DeleteBeerAsync("B-1"), Times.Once);
    }

    [Fact]
    public void Liquor_renders_saves_and_deletes()
    {
        var cut = RenderPage<LiquorPage>();
        Assert.Contains("Whisky", cut.Markup);
        ClickPrimary(cut);
        InventorySvc.Verify(s => s.SaveLiquorAsync(It.IsAny<Liquor>()), Times.Once);
        ClickIcon(cut, Icons.Material.Filled.Delete);
        InventorySvc.Verify(s => s.DeleteLiquorAsync(1), Times.Once);
    }

    [Fact]
    public void LiquorMaster_renders_saves_and_deletes()
    {
        var cut = RenderPage<LiquorMasterPage>();
        ClickPrimary(cut);
        InventorySvc.Verify(s => s.SaveLiquorMasterAsync(It.IsAny<LiquorMaster>()), Times.Once);
        ClickIcon(cut, Icons.Material.Filled.Delete);
        InventorySvc.Verify(s => s.DeleteLiquorMasterAsync("Whisky"), Times.Once);
    }

    [Fact]
    public void Stock_renders_saves_and_deletes()
    {
        var cut = RenderPage<StockPage>();
        Assert.Contains("S-1", cut.Markup);
        ClickPrimary(cut);
        InventorySvc.Verify(s => s.SaveStockAsync(It.IsAny<Stock>()), Times.Once);
        ClickIcon(cut, Icons.Material.Filled.Delete);
        InventorySvc.Verify(s => s.DeleteStockAsync("S-1"), Times.Once);
    }

    [Fact]
    public void Stock_show_empty_filters_zero_volume()
    {
        InventorySvc.Setup(s => s.GetStocksAsync(null))
            .ReturnsAsync(new[] { new Stock { StockID = "S-EMPTY", LiquorName = "X", TotalVolume = 0 } });
        var cut = RenderPage<StockPage>();
        ClickButton(cut, "Empty Stock");
        Assert.Contains("S-EMPTY", cut.Markup);
    }

    [Fact]
    public void BeerStock_renders_saves_and_deletes()
    {
        var cut = RenderPage<BeerStockPage>();
        Assert.Contains("SB-1", cut.Markup);
        ClickPrimary(cut);
        InventorySvc.Verify(s => s.SaveBeerStockAsync(It.IsAny<StockBeer>()), Times.Once);
        ClickIcon(cut, Icons.Material.Filled.Delete);
        InventorySvc.Verify(s => s.DeleteBeerStockAsync("SB-1"), Times.Once);
    }

    [Fact]
    public void Purchase_renders_saves_and_deletes()
    {
        var cut = RenderPage<PurchaseInventoryPage>();
        Assert.Contains("Rice", cut.Markup);
        ClickPrimary(cut);
        InventorySvc.Verify(s => s.SavePurchasedInventoryAsync(It.IsAny<PurchasedInventory>()), Times.Once);
        ClickIcon(cut, Icons.Material.Filled.Delete);
        InventorySvc.Verify(s => s.DeletePurchasedInventoryAsync(1), Times.Once);
    }

    [Fact]
    public void TaxConfig_renders_saves_and_deletes()
    {
        var cut = RenderPage<TaxConfigPage>();
        Assert.Contains("VAT", cut.Markup);
        ClickPrimary(cut);
        InventorySvc.Verify(s => s.SaveTaxInfoAsync(It.IsAny<TaxInfo>()), Times.Once);
        ClickIcon(cut, Icons.Material.Filled.Delete);
        InventorySvc.Verify(s => s.DeleteTaxInfoAsync("VAT"), Times.Once);
    }
}
