using Bunit;
using Bunit.TestDoubles;
using HotelManager.Application.Interfaces;
using HotelManager.Domain.Entities;
using HotelManager.Infrastructure;
using HotelManager.Web.Services.Reports;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;

namespace HotelManager.ComponentTests.Infrastructure;

/// <summary>
/// Base bUnit context wiring MudBlazor services, loose JSInterop, an authenticated
/// (Admin) user, an in-memory SQLite database for pages that use a real
/// <see cref="HotelDbContext"/>, and Moq-backed <c>HotelManager.Application</c>
/// service interfaces. Service interfaces are consumed only — never modified.
/// </summary>
public abstract class ComponentTestBase : Bunit.TestContext
{
    protected ComponentDbFixture Db { get; } = new();

    protected Mock<IAuthService> AuthSvc { get; } = new();
    protected Mock<IHotelInfoService> HotelInfoSvc { get; } = new();
    protected Mock<ICurrencyService> CurrencySvc { get; } = new();
    protected Mock<IRoomService> RoomSvc { get; } = new();
    protected Mock<IGuestService> GuestSvc { get; } = new();
    protected Mock<IReservationService> ReservationSvc { get; } = new();
    protected Mock<ICheckInService> CheckInSvc { get; } = new();
    protected Mock<ICheckOutService> CheckOutSvc { get; } = new();
    protected Mock<IRestaurantOrderService> OrderSvc { get; } = new();
    protected Mock<ITransactionService> TransactionSvc { get; } = new();
    protected Mock<IInventoryService> InventorySvc { get; } = new();
    protected Mock<IHrPayrollService> HrSvc { get; } = new();
    protected Mock<IReportService> ReportSvc { get; } = new();
    protected Mock<MudBlazor.ISnackbar> Snackbar { get; } = new();
    protected Mock<MudBlazor.IDialogService> Dialog { get; } = new();

    protected TestAuthorizationContext Auth { get; }

    protected ComponentTestBase()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();

        ConfigureDefaultMocks();

        Services.AddSingleton(AuthSvc.Object);
        Services.AddSingleton(HotelInfoSvc.Object);
        Services.AddSingleton(CurrencySvc.Object);
        Services.AddSingleton(RoomSvc.Object);
        Services.AddSingleton(GuestSvc.Object);
        Services.AddSingleton(ReservationSvc.Object);
        Services.AddSingleton(CheckInSvc.Object);
        Services.AddSingleton(CheckOutSvc.Object);
        Services.AddSingleton(OrderSvc.Object);
        Services.AddSingleton(TransactionSvc.Object);
        Services.AddSingleton(InventorySvc.Object);
        Services.AddSingleton(HrSvc.Object);
        Services.AddSingleton(ReportSvc.Object);

        // Override MudBlazor's Snackbar/DialogService with deterministic mocks so
        // tests don't depend on the MudSnackbarProvider/MudDialogProvider render tree.
        Services.AddSingleton(Snackbar.Object);
        Services.AddSingleton(Dialog.Object);

        // Real in-memory database + factory for pages that inject them directly.
        Services.AddSingleton<IDbContextFactory<HotelDbContext>>(new TestDbContextFactory(Db.Options));
        Services.AddTransient(_ => Db.CreateContext());
        Services.AddSingleton(sp => new ReportDataService(
            sp.GetRequiredService<IDbContextFactory<HotelDbContext>>()));

        Auth = this.AddTestAuthorization();
        Auth.SetAuthorized("admin");
        Auth.SetRoles("Admin");
    }

    private void ConfigureDefaultMocks()
    {
        Dialog.Setup(d => d.ShowMessageBox(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<MudBlazor.DialogOptions?>()))
            .ReturnsAsync(true);

        // Auth / users
        AuthSvc.Setup(s => s.GetUsersAsync())
            .ReturnsAsync(new[] { new Registration { UserName = "admin", UserType = "Admin" } });
        AuthSvc.Setup(s => s.GetUserAsync(It.IsAny<string>())).ReturnsAsync((Registration?)null);

        // Master data
        HotelInfoSvc.Setup(s => s.GetAsync()).ReturnsAsync(TestData.HotelInfo());
        CurrencySvc.Setup(s => s.GetAllAsync()).ReturnsAsync(new[] { TestData.Currency() });
        RoomSvc.Setup(s => s.GetRoomsAsync(It.IsAny<string?>())).ReturnsAsync(new[] { TestData.Room() });
        RoomSvc.Setup(s => s.GetHallsAsync()).ReturnsAsync(new[] { TestData.Hall() });
        RoomSvc.Setup(s => s.GetGardensAsync()).ReturnsAsync(new[] { TestData.Garden() });
        RoomSvc.Setup(s => s.GetExtraBedsAsync()).ReturnsAsync(new[] { TestData.ExtraBed() });
        GuestSvc.Setup(s => s.GetAllAsync(It.IsAny<string?>())).ReturnsAsync(new[] { TestData.Guest() });

        // Reservations
        ReservationSvc.Setup(s => s.GetReservationsAsync(It.IsAny<string?>()))
            .ReturnsAsync(new[] { TestData.Reservation() });
        ReservationSvc.Setup(s => s.GetReservationAsync(It.IsAny<string>()))
            .ReturnsAsync(TestData.Reservation());
        ReservationSvc.Setup(s => s.GetTempReservationsAsync())
            .ReturnsAsync(new[] { TestData.TempReservation() });
        ReservationSvc.Setup(s => s.GetRoomAvailabilityAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new[] { new RoomAvailability("101", "Deluxe", 1000, true) });
        ReservationSvc.Setup(s => s.GetHallAndGardenReservationsAsync())
            .ReturnsAsync(new[] { TestData.HallAndGarden() });
        ReservationSvc.Setup(s => s.GetHallOrGardenReservationsAsync())
            .ReturnsAsync(new[] { TestData.HallOrGarden() });

        // Check-in / check-out
        CheckInSvc.Setup(s => s.GetCheckInsAsync(It.IsAny<string?>(), It.IsAny<string?>()))
            .ReturnsAsync(new[] { TestData.CheckIn() });
        CheckInSvc.Setup(s => s.GetCheckInAsync(It.IsAny<int>())).ReturnsAsync(TestData.CheckIn());
        CheckInSvc.Setup(s => s.CheckInAsync(It.IsAny<CheckInRoom>())).ReturnsAsync(TestData.CheckIn());
        CheckOutSvc.Setup(s => s.GetCheckoutsAsync(It.IsAny<string?>()))
            .ReturnsAsync(new[] { TestData.Checkout() });
        CheckOutSvc.Setup(s => s.CheckOutAsync(It.IsAny<int>(), It.IsAny<CheckoutRoom>(), It.IsAny<TaxRoom?>()))
            .ReturnsAsync(TestData.Checkout());

        // Orders
        OrderSvc.Setup(s => s.GetRoomOrdersAsync(It.IsAny<string?>()))
            .ReturnsAsync(new[] { TestData.RoomOrder() });
        OrderSvc.Setup(s => s.GetRoomOrderAsync(It.IsAny<int>())).ReturnsAsync(TestData.RoomOrder());
        OrderSvc.Setup(s => s.GenerateRoomOrderNoAsync()).ReturnsAsync("O-1");
        OrderSvc.Setup(s => s.SaveRoomOrderAsync(It.IsAny<OrderInfo>(), It.IsAny<IEnumerable<OrderedProduct>>(), It.IsAny<TaxOrder?>()))
            .ReturnsAsync(TestData.RoomOrder());
        OrderSvc.Setup(s => s.GetRestaurantOrdersAsync(It.IsAny<string?>()))
            .ReturnsAsync(new[] { TestData.RestaurantOrder() });
        OrderSvc.Setup(s => s.GetRestaurantOrderAsync(It.IsAny<int>())).ReturnsAsync(TestData.RestaurantOrder());
        OrderSvc.Setup(s => s.GenerateRestaurantOrderNoAsync()).ReturnsAsync("RO-1");
        OrderSvc.Setup(s => s.SaveRestaurantOrderAsync(It.IsAny<RestaurantOrderInfo>(), It.IsAny<IEnumerable<RestaurantOrderedProduct>>(), It.IsAny<TaxRestaurantOrder?>()))
            .ReturnsAsync(TestData.RestaurantOrder());

        // Transactions
        TransactionSvc.Setup(s => s.GetAllAsync(It.IsAny<string?>())).ReturnsAsync(new[] { TestData.Trans() });
        TransactionSvc.Setup(s => s.SaveAsync(It.IsAny<Trans>())).ReturnsAsync(TestData.Trans());

        // Inventory
        InventorySvc.Setup(s => s.GetDishesAsync(It.IsAny<string?>())).ReturnsAsync(new[] { TestData.Dish() });
        InventorySvc.Setup(s => s.GetBeersAsync(It.IsAny<string?>())).ReturnsAsync(new[] { TestData.Beer() });
        InventorySvc.Setup(s => s.GetLiquorsAsync(It.IsAny<string?>())).ReturnsAsync(new[] { TestData.Liquor() });
        InventorySvc.Setup(s => s.GetLiquorMastersAsync()).ReturnsAsync(new[] { TestData.LiquorMaster() });
        InventorySvc.Setup(s => s.GetStocksAsync(It.IsAny<string?>())).ReturnsAsync(new[] { TestData.Stock() });
        InventorySvc.Setup(s => s.GetBeerStocksAsync(It.IsAny<string?>())).ReturnsAsync(new[] { TestData.BeerStock() });
        InventorySvc.Setup(s => s.GetPurchasedInventoriesAsync(It.IsAny<string?>())).ReturnsAsync(new[] { TestData.Purchase() });
        InventorySvc.Setup(s => s.GetTaxInfosAsync()).ReturnsAsync(new[] { TestData.TaxInfo() });
        InventorySvc.Setup(s => s.SaveDishAsync(It.IsAny<Dish>())).ReturnsAsync(TestData.Dish());
        InventorySvc.Setup(s => s.SaveBeerAsync(It.IsAny<Beer>())).ReturnsAsync(TestData.Beer());
        InventorySvc.Setup(s => s.SaveLiquorAsync(It.IsAny<Liquor>())).ReturnsAsync(TestData.Liquor());
        InventorySvc.Setup(s => s.SaveLiquorMasterAsync(It.IsAny<LiquorMaster>())).ReturnsAsync(TestData.LiquorMaster());
        InventorySvc.Setup(s => s.SaveStockAsync(It.IsAny<Stock>())).ReturnsAsync(TestData.Stock());
        InventorySvc.Setup(s => s.SaveBeerStockAsync(It.IsAny<StockBeer>())).ReturnsAsync(TestData.BeerStock());
        InventorySvc.Setup(s => s.SavePurchasedInventoryAsync(It.IsAny<PurchasedInventory>())).ReturnsAsync(TestData.Purchase());
        InventorySvc.Setup(s => s.SaveTaxInfoAsync(It.IsAny<TaxInfo>())).ReturnsAsync(TestData.TaxInfo());

        // HR / payroll
        HrSvc.Setup(s => s.GetEmployeesAsync(It.IsAny<string?>())).ReturnsAsync(new[] { TestData.Employee() });
        HrSvc.Setup(s => s.GetEmployeeAsync(It.IsAny<string>())).ReturnsAsync(TestData.Employee());
        HrSvc.Setup(s => s.GenerateEmployeeIdAsync()).ReturnsAsync("E-000002");
        HrSvc.Setup(s => s.GetAttendanceAsync(It.IsAny<string?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(new[] { TestData.Attendance() });
        HrSvc.Setup(s => s.SaveAttendanceAsync(It.IsAny<EmployeeAttendance>())).ReturnsAsync(TestData.Attendance());
        HrSvc.Setup(s => s.GetAdvanceEntriesAsync(It.IsAny<string?>())).ReturnsAsync(new[] { TestData.Advance() });
        HrSvc.Setup(s => s.SaveAdvanceEntryAsync(It.IsAny<AdvanceEntry>())).ReturnsAsync(TestData.Advance());
        HrSvc.Setup(s => s.GetOutstandingAdvanceAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(0);
        HrSvc.Setup(s => s.GetTotalOvertimeAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(TimeSpan.Zero);
        HrSvc.Setup(s => s.GetPresentDaysAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(30);
        HrSvc.Setup(s => s.GetPaymentsAsync(It.IsAny<string?>())).ReturnsAsync(new[] { TestData.Payment() });
        HrSvc.Setup(s => s.GeneratePaymentIdAsync()).ReturnsAsync("P-000002");
        HrSvc.Setup(s => s.RunPaymentAsync(It.IsAny<EmployeePayment>())).ReturnsAsync(TestData.Payment());

        var pdf = new byte[] { 1, 2, 3 };
        ReportSvc.Setup(r => r.RenderRoomInvoicePdfAsync(It.IsAny<int>())).ReturnsAsync(pdf);
        ReportSvc.Setup(r => r.RenderHallAndGardenInvoicePdfAsync(It.IsAny<string>())).ReturnsAsync(pdf);
        ReportSvc.Setup(r => r.RenderHallOrGardenInvoicePdfAsync(It.IsAny<string>())).ReturnsAsync(pdf);
        ReportSvc.Setup(r => r.RenderOrderInvoicePdfAsync(It.IsAny<int>())).ReturnsAsync(pdf);
        ReportSvc.Setup(r => r.RenderRestaurantOrderReceiptPdfAsync(It.IsAny<int>())).ReturnsAsync(pdf);
        ReportSvc.Setup(r => r.RenderSalarySlipPdfAsync(It.IsAny<string>())).ReturnsAsync(pdf);
        ReportSvc.Setup(r => r.RenderAttendanceReportPdfAsync(It.IsAny<ReportRequest>())).ReturnsAsync(pdf);
        ReportSvc.Setup(r => r.RenderAdvancePaymentReportPdfAsync(It.IsAny<ReportRequest>())).ReturnsAsync(pdf);
        ReportSvc.Setup(r => r.RenderEmployeePaymentReportPdfAsync(It.IsAny<ReportRequest>())).ReturnsAsync(pdf);
        ReportSvc.Setup(r => r.RenderGuestReportPdfAsync(It.IsAny<ReportRequest>())).ReturnsAsync(pdf);
        ReportSvc.Setup(r => r.RenderReservationReportPdfAsync(It.IsAny<ReportRequest>())).ReturnsAsync(pdf);
        ReportSvc.Setup(r => r.RenderPurchasedInventoryReportPdfAsync(It.IsAny<ReportRequest>())).ReturnsAsync(pdf);
    }

    /// <summary>
    /// Renders the page under test alongside the MudBlazor popover provider so
    /// components that open popovers (MudSelect, MudMenu, MudDatePicker, ...) work.
    /// </summary>
    protected IRenderedComponent<TComponent> RenderPage<TComponent>() where TComponent : IComponent
    {
        var root = Render(builder =>
        {
            builder.OpenComponent<MudPopoverProvider>(0);
            builder.CloseComponent();
            builder.OpenComponent<TComponent>(1);
            builder.CloseComponent();
        });
        return root.FindComponent<TComponent>();
    }

    /// <summary>Seeds the shared in-memory database used by pages that query EF directly.</summary>
    protected void Seed(Action<HotelDbContext> seed)
    {
        using var db = Db.CreateContext();
        seed(db);
        db.SaveChanges();
    }

    /// <summary>Clicks the first rendered &lt;button&gt; whose visible text matches (trimmed).</summary>
    protected static void ClickButton(IRenderedFragment cut, string text) =>
        cut.FindAll("button").First(b => b.TextContent.Trim() == text).Click();

    /// <summary>Clicks the first filled-primary MudButton (the "Save"/"Add" action on most pages).</summary>
    protected static void ClickPrimary(IRenderedFragment cut) =>
        cut.FindAll("button.mud-button-filled-primary").First().Click();

    /// <summary>Clicks the MudIconButton rendering the given Material icon.</summary>
    protected static void ClickIcon(IRenderedFragment cut, string icon) =>
        cut.FindComponents<MudIconButton>()
            .First(b => b.Instance.Icon == icon)
            .Find("button").Click();

    protected override void Dispose(bool disposing)
    {
        if (disposing) Db.Dispose();
        base.Dispose(disposing);
    }
}
