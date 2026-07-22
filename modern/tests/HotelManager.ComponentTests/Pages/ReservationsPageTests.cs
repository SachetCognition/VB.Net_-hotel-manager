using HotelManager.Application.Interfaces;
using HotelManager.Web.Components.Pages.Reservations;
using MudBlazor;

namespace HotelManager.ComponentTests.Pages;

public class ReservationsPageTests : ComponentTestBase
{
    [Fact]
    public void Reservations_renders_lists()
    {
        var cut = RenderPage<Reservations>();
        Assert.Contains("R-00001", cut.Markup);
        ReservationSvc.Verify(s => s.GetReservationsAsync(It.IsAny<string?>()), Times.AtLeastOnce);
        ReservationSvc.Verify(s => s.GetTempReservationsAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public void Reservations_save_creates_temp()
    {
        var cut = RenderPage<Reservations>();
        SetSelect(cut, "Guest", "G-00001");
        SetSelect(cut, "Room No", "101");
        SetDate(cut, "Date In", DateTime.Today);
        SetDate(cut, "Date Out", DateTime.Today.AddDays(2));
        ClickButton(cut, "Save");
        ReservationSvc.Verify(s => s.SaveTempReservationAsync(It.IsAny<TempReservation>()), Times.Once);
    }

    [Fact]
    public void Reservations_edit_then_save_updates()
    {
        var cut = RenderPage<Reservations>();
        ClickIcon(cut, Icons.Material.Filled.Edit);
        ClickButton(cut, "Save");
        ReservationSvc.Verify(s => s.UpdateReservationAsync(It.IsAny<Reservation>()), Times.Once);
    }

    [Fact]
    public void Reservations_confirm_temp()
    {
        var cut = RenderPage<Reservations>();
        ClickButton(cut, "Confirm");
        ReservationSvc.Verify(s => s.ConfirmTempReservationAsync("R-00001"), Times.Once);
    }

    [Fact]
    public void Reservations_delete_temp_and_cancel()
    {
        var cut = RenderPage<Reservations>();
        ClickIcon(cut, Icons.Material.Filled.Delete);
        ReservationSvc.Verify(s => s.DeleteTempReservationAsync("R-00001"), Times.Once);
        ClickIcon(cut, Icons.Material.Filled.Cancel);
        ReservationSvc.Verify(s => s.CancelReservationAsync("R-00001"), Times.Once);
    }

    [Fact]
    public void CheckInPage_renders_and_checks_in()
    {
        var cut = RenderPage<CheckInPage>();
        Assert.Contains("Room Check In", cut.Markup);
        SetSelect(cut, "Guest", "G-00001");
        SetSelect(cut, "Room No", "101");
        SetDate(cut, "Date In", DateTime.Today);
        SetDate(cut, "Date Out", DateTime.Today.AddDays(1));
        ClickButton(cut, "Check In");
        CheckInSvc.Verify(s => s.CheckInAsync(It.IsAny<CheckInRoom>()), Times.Once);
    }

    [Fact]
    public void CheckOutPage_renders_and_checks_out_with_billing()
    {
        var cut = RenderPage<CheckOutPage>();
        ClickButton(cut, "Check Out");
        CheckInSvc.Verify(s => s.GetCheckInAsync(1), Times.Once);
        CheckOutSvc.Verify(s => s.CheckOutAsync(1, It.IsAny<CheckoutRoom>(), It.IsAny<TaxRoom?>()), Times.Once);
    }

    [Fact]
    public void RoomAvailability_check_queries_service()
    {
        var cut = RenderPage<RoomAvailabilityPage>();
        ClickButton(cut, "Check");
        ReservationSvc.Verify(
            s => s.GetRoomAvailabilityAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()),
            Times.AtLeastOnce);
        Assert.Contains("101", cut.Markup);
    }
}
