using HotelManager.Web.Components.Pages;
using MudBlazor;

namespace HotelManager.ComponentTests.Pages;

public class MasterDataPageTests : ComponentTestBase
{
    [Fact]
    public void Currencies_renders_and_saves()
    {
        var cut = RenderPage<Currencies>();
        Assert.Contains("USD", cut.Markup);
        ClickPrimary(cut);
        CurrencySvc.Verify(s => s.CreateAsync(It.IsAny<CurrencySet>()), Times.Once);
    }

    [Fact]
    public void Currencies_edit_then_save_updates()
    {
        var cut = RenderPage<Currencies>();
        ClickIcon(cut, Icons.Material.Filled.Edit);
        ClickPrimary(cut);
        CurrencySvc.Verify(s => s.UpdateAsync(It.IsAny<CurrencySet>()), Times.Once);
    }

    [Fact]
    public void Currencies_delete_confirmed()
    {
        var cut = RenderPage<Currencies>();
        ClickIcon(cut, Icons.Material.Filled.Delete);
        CurrencySvc.Verify(s => s.DeleteAsync(It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public void Rooms_renders_saves_and_deletes()
    {
        var cut = RenderPage<Rooms>();
        Assert.Contains("101", cut.Markup);
        ClickPrimary(cut);
        RoomSvc.Verify(s => s.CreateRoomAsync(It.IsAny<Room>()), Times.Once);
        ClickIcon(cut, Icons.Material.Filled.Delete);
        RoomSvc.Verify(s => s.DeleteRoomAsync("101"), Times.Once);
    }

    [Fact]
    public void Rooms_edit_then_update()
    {
        var cut = RenderPage<Rooms>();
        ClickIcon(cut, Icons.Material.Filled.Edit);
        ClickPrimary(cut);
        RoomSvc.Verify(s => s.UpdateRoomAsync(It.IsAny<Room>()), Times.Once);
    }

    [Fact]
    public void Halls_renders_saves_and_deletes()
    {
        var cut = RenderPage<Halls>();
        Assert.Contains("Grand", cut.Markup);
        ClickPrimary(cut);
        RoomSvc.Verify(s => s.SaveHallAsync(It.IsAny<Hall>()), Times.Once);
        ClickIcon(cut, Icons.Material.Filled.Delete);
        RoomSvc.Verify(s => s.DeleteHallAsync(It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public void Gardens_renders_saves_and_deletes()
    {
        var cut = RenderPage<Gardens>();
        ClickPrimary(cut);
        RoomSvc.Verify(s => s.SaveGardenAsync(It.IsAny<Garden>()), Times.Once);
        ClickIcon(cut, Icons.Material.Filled.Delete);
        RoomSvc.Verify(s => s.DeleteGardenAsync(It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public void ExtraBeds_renders_saves_and_deletes()
    {
        var cut = RenderPage<ExtraBeds>();
        ClickPrimary(cut);
        RoomSvc.Verify(s => s.SaveExtraBedAsync(It.IsAny<ExtraBed>()), Times.Once);
        ClickIcon(cut, Icons.Material.Filled.Delete);
        RoomSvc.Verify(s => s.DeleteExtraBedAsync(It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public void HotelInfo_renders_and_saves()
    {
        var cut = RenderPage<HotelInfoPage>();
        Assert.Contains("Grand Hotel", cut.Markup);
        ClickPrimary(cut);
        HotelInfoSvc.Verify(s => s.SaveAsync(It.IsAny<HotelInfo>()), Times.Once);
    }
}
