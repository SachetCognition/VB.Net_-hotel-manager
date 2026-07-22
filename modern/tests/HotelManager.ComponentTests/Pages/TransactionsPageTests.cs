using HotelManager.Web.Components.Pages;
using MudBlazor;

namespace HotelManager.ComponentTests.Pages;

public class TransactionsPageTests : ComponentTestBase
{
    [Fact]
    public void Renders_transactions()
    {
        var cut = RenderPage<Transactions>();
        Assert.Contains("Bob", cut.Markup);
    }

    [Fact]
    public void Save_without_party_name_warns()
    {
        var cut = RenderPage<Transactions>();
        ClickButton(cut, "Save");
        TransactionSvc.Verify(s => s.SaveAsync(It.IsAny<Trans>()), Times.Never);
    }

    [Fact]
    public void Save_with_party_name_persists()
    {
        var cut = RenderPage<Transactions>();
        cut.FindAll("input")[0].Change("Acme");
        ClickButton(cut, "Save");
        TransactionSvc.Verify(s => s.SaveAsync(It.Is<Trans>(t => t.Employee_Party_Name == "Acme")), Times.Once);
    }

    [Fact]
    public void Edit_then_save_updates_existing()
    {
        var cut = RenderPage<Transactions>();
        ClickIcon(cut, Icons.Material.Filled.Edit);
        ClickButton(cut, "Save");
        TransactionSvc.Verify(s => s.SaveAsync(It.Is<Trans>(t => t.ID == 1)), Times.Once);
    }

    [Fact]
    public void Delete_confirmed_removes()
    {
        var cut = RenderPage<Transactions>();
        ClickIcon(cut, Icons.Material.Filled.Delete);
        TransactionSvc.Verify(s => s.DeleteAsync(1), Times.Once);
    }
}
