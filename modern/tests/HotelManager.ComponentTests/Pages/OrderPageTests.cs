using HotelManager.Web.Components.Pages;
using MudBlazor;

namespace HotelManager.ComponentTests.Pages;

public class OrderPageTests : ComponentTestBase
{
    [Fact]
    public void RoomOrders_renders_and_generates_order_no()
    {
        var cut = RenderPage<RoomOrders>();
        Assert.Contains("O-1", cut.Markup);
        OrderSvc.Verify(s => s.GenerateRoomOrderNoAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public void RoomOrders_add_item_without_selection_warns()
    {
        var cut = RenderPage<RoomOrders>();
        ClickButton(cut, "Add Item");
        OrderSvc.Verify(s => s.SaveRoomOrderAsync(
            It.IsAny<OrderInfo>(), It.IsAny<IEnumerable<OrderedProduct>>(), It.IsAny<TaxOrder?>()), Times.Never);
    }

    [Fact]
    public void RoomOrders_save_without_checkin_does_not_persist()
    {
        var cut = RenderPage<RoomOrders>();
        ClickButton(cut, "Save Order");
        OrderSvc.Verify(s => s.SaveRoomOrderAsync(
            It.IsAny<OrderInfo>(), It.IsAny<IEnumerable<OrderedProduct>>(), It.IsAny<TaxOrder?>()), Times.Never);
    }

    [Fact]
    public void RoomOrders_edit_then_save_persists()
    {
        var cut = RenderPage<RoomOrders>();
        ClickIcon(cut, Icons.Material.Filled.Edit);
        ClickButton(cut, "Save Order");
        OrderSvc.Verify(s => s.SaveRoomOrderAsync(
            It.Is<OrderInfo>(o => o.ID == 1), It.IsAny<IEnumerable<OrderedProduct>>(), It.IsAny<TaxOrder?>()), Times.Once);
    }

    [Fact]
    public void RoomOrders_delete_confirmed_removes()
    {
        var cut = RenderPage<RoomOrders>();
        ClickIcon(cut, Icons.Material.Filled.Delete);
        OrderSvc.Verify(s => s.DeleteRoomOrderAsync(1), Times.Once);
    }

    [Fact]
    public void RestaurantOrders_renders_and_generates_order_no()
    {
        var cut = RenderPage<RestaurantOrders>();
        Assert.Contains("RO-1", cut.Markup);
        OrderSvc.Verify(s => s.GenerateRestaurantOrderNoAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public void RestaurantOrders_add_item_without_selection_warns()
    {
        var cut = RenderPage<RestaurantOrders>();
        ClickButton(cut, "Add Item");
        OrderSvc.Verify(s => s.SaveRestaurantOrderAsync(
            It.IsAny<RestaurantOrderInfo>(), It.IsAny<IEnumerable<RestaurantOrderedProduct>>(), It.IsAny<TaxRestaurantOrder?>()), Times.Never);
    }

    [Fact]
    public void RestaurantOrders_edit_then_save_persists()
    {
        var cut = RenderPage<RestaurantOrders>();
        ClickIcon(cut, Icons.Material.Filled.Edit);
        ClickButton(cut, "Save Order");
        OrderSvc.Verify(s => s.SaveRestaurantOrderAsync(
            It.Is<RestaurantOrderInfo>(o => o.ID == 1), It.IsAny<IEnumerable<RestaurantOrderedProduct>>(), It.IsAny<TaxRestaurantOrder?>()), Times.Once);
    }

    [Fact]
    public void RestaurantOrders_delete_confirmed_removes()
    {
        var cut = RenderPage<RestaurantOrders>();
        ClickIcon(cut, Icons.Material.Filled.Delete);
        OrderSvc.Verify(s => s.DeleteRestaurantOrderAsync(1), Times.Once);
    }
}
