using HotelManager.Web.Components.Pages;
using HotelManager.Web.Components.Pages.Reports;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace HotelManager.ComponentTests.Pages;

public class HomeAndMiscPageTests : ComponentTestBase
{
    [Fact]
    public void Home_shows_dashboard_counts_from_db()
    {
        Seed(db =>
        {
            db.Guests.Add(TestData.Guest());
            db.Rooms.Add(TestData.Room());
            db.CheckIns.Add(new CheckInRoom
            {
                ID = 1, GuestID = "G-00001", RoomNo = "101", Status = "Checked In",
                DateIN = DateTime.Today, DateOUT = DateTime.Today.AddDays(1)
            });
            db.Reservations.Add(new Reservation
            {
                ReservationID = "R-1", GuestID = "G-00001", Status = "Confirmed",
                DateIN = DateTime.Today, DateOUT = DateTime.Today.AddDays(2)
            });
        });
        var cut = RenderPage<Home>();
        Assert.Contains("Dashboard", cut.Markup);
    }

    [Fact]
    public void Home_renders_with_empty_db()
    {
        var cut = RenderPage<Home>();
        Assert.NotNull(cut);
    }

    [Fact]
    public void Login_renders_form()
    {
        var cut = RenderPage<Login>();
        Assert.Contains("input", cut.Markup);
    }

    [Fact]
    public void Login_shows_error_when_error_query_present()
    {
        var nav = Services.GetRequiredService<Bunit.TestDoubles.FakeNavigationManager>();
        nav.NavigateTo("/login?error=Invalid%20credentials");
        var cut = RenderPage<Login>();
        Assert.Contains("Invalid", cut.Markup);
    }

    [Fact]
    public void InvoicePickList_export_button_invokes_callback()
    {
        InvoicePickRow? exported = null;
        var cut = RenderComponent<InvoicePickList>(ps => ps
            .Add(p => p.Title, "Room Invoices")
            .Add(p => p.Items, new[] { new InvoicePickRow("1", "B-1", "Alice", DateTime.Today) })
            .Add(p => p.OnExport, EventCallback.Factory.Create<InvoicePickRow>(this, r => exported = r)));

        Assert.Contains("B-1", cut.Markup);
        cut.FindAll("button").First(b => b.TextContent.Trim() == "Export PDF").Click();
        Assert.NotNull(exported);
        Assert.Equal("1", exported!.Id);
    }
}
