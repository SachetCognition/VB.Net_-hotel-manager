using HotelManager.Web.Components.Pages;

namespace HotelManager.ComponentTests.Pages;

public class GuestsPageTests : ComponentTestBase
{
    public GuestsPageTests()
    {
        GuestSvc.Setup(s => s.GetAllAsync(It.IsAny<string?>()))
            .ReturnsAsync(new[] { TestData.Guest() });
    }

    [Fact]
    public void Renders_guest_rows_from_service()
    {
        var cut = RenderPage<Guests>();

        Assert.Contains("Guests", cut.Markup);
        Assert.Contains("G-00001", cut.Markup);
        GuestSvc.Verify(s => s.GetAllAsync(It.IsAny<string?>()), Times.AtLeastOnce);
    }

    [Fact]
    public void Save_new_guest_invokes_create()
    {
        var cut = RenderPage<Guests>();

        SetText(cut, "Guest Name", "Alice");
        SetText(cut, "Address", "1 St");
        SetText(cut, "City", "Town");
        SetText(cut, "Contact No", "555");
        SetSelect(cut, "ID Type", "Passport");
        SetText(cut, "ID Number", "P1");
        cut.FindAll("button").First(b => b.TextContent.Trim() == "Add").Click();

        GuestSvc.Verify(s => s.CreateAsync(It.IsAny<Guest>()), Times.Once);
    }

    [Fact]
    public void Delete_confirmed_invokes_delete()
    {
        var cut = RenderPage<Guests>();

        cut.FindComponents<MudBlazor.MudIconButton>()
            .First(b => b.Instance.Icon == MudBlazor.Icons.Material.Filled.Delete)
            .Find("button").Click();

        Dialog.Verify(d => d.ShowMessageBox(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<MudBlazor.DialogOptions?>()), Times.Once);
        GuestSvc.Verify(s => s.DeleteAsync("G-00001"), Times.Once);
    }
}
