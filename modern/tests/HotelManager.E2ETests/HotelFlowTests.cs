using System.Globalization;
using Microsoft.Playwright;

namespace HotelManager.E2ETests;

/// <summary>
/// End-to-end journey driven with Playwright against a running instance of the
/// modern Hotel Manager app:
///   login → dashboard → create reservation → check-in (verify billing)
///   → place room order → place restaurant order → check-out (verify final bill)
///   → HR payroll run → generate a report PDF.
///
/// The whole flow is skipped automatically when <c>E2E_BASE_URL</c>
/// (default <c>http://localhost:8080</c>) is unreachable, so ordinary
/// <c>dotnet test</c> runs and the unit+component CI job are unaffected.
/// </summary>
public sealed class HotelFlowTests : IAsyncLifetime
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IBrowserContext _context = null!;
    private IPage _page = null!;
    private bool _appAvailable;

    public async Task InitializeAsync()
    {
        _appAvailable = await E2EEnvironment.IsAppReachableAsync();
        if (!_appAvailable) return;

        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = Environment.GetEnvironmentVariable("E2E_HEADED") != "1"
        });
        _context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            AcceptDownloads = true,
            IgnoreHTTPSErrors = true
        });
        _page = await _context.NewPageAsync();
        _page.SetDefaultTimeout(15_000);
    }

    public async Task DisposeAsync()
    {
        if (_context is not null) await _context.CloseAsync();
        if (_browser is not null) await _browser.DisposeAsync();
        _playwright?.Dispose();
    }

    [SkippableFact]
    public async Task Full_hotel_journey()
    {
        Skip.IfNot(_appAvailable,
            $"App not reachable at {E2EEnvironment.BaseUrl}; skipping Playwright E2E flow.");

        await LoginAsync();
        await AssertDashboardAsync();
        await CreateReservationAsync();
        var checkInBill = await CheckInAsync();
        await PlaceRoomOrderAsync();
        await PlaceRestaurantOrderAsync();
        var finalBill = await CheckOutAsync();
        Assert.True(finalBill >= checkInBill,
            $"Final bill ({finalBill}) should be at least the room charges at check-in ({checkInBill}).");
        await RunPayrollAsync();
        await GenerateReportPdfAsync();
    }

    private async Task LoginAsync()
    {
        await _page.GotoAsync(E2EEnvironment.BaseUrl + "/login");
        await _page.FillAsync("#username", E2EEnvironment.UserName);
        await _page.FillAsync("#password", E2EEnvironment.Password);
        await _page.ClickAsync("button[type=submit]");
        await _page.WaitForURLAsync(u => !u.Contains("/login"), new() { Timeout = 15_000 });
    }

    private async Task AssertDashboardAsync()
    {
        await _page.GotoAsync(E2EEnvironment.BaseUrl + "/");
        await Assertions.Expect(_page.GetByText("Dashboard")).ToBeVisibleAsync();
    }

    private async Task CreateReservationAsync()
    {
        await _page.GotoAsync(E2EEnvironment.BaseUrl + "/reservations");
        await SelectFirstOptionAsync("Guest");
        await SelectFirstOptionAsync("Room");
        await ClickButtonAsync("Save");
        await Assertions.Expect(_page.Locator(".mud-snackbar")).ToBeVisibleAsync();
    }

    private async Task<double> CheckInAsync()
    {
        await _page.GotoAsync(E2EEnvironment.BaseUrl + "/check-in");
        await SelectFirstOptionAsync("Guest");
        await SelectFirstOptionAsync("Room");
        var charges = await ReadNumericFieldAsync("Room Charges");
        await ClickButtonAsync("Check In");
        await Assertions.Expect(_page.GetByText("Successfully checked in")).ToBeVisibleAsync();
        return charges;
    }

    private async Task PlaceRoomOrderAsync()
    {
        await _page.GotoAsync(E2EEnvironment.BaseUrl + "/room-orders");
        await SelectFirstOptionAsync("Guest");
        await SelectFirstOptionAsync("Product");
        await ClickButtonAsync("Add Item");
        await ClickButtonAsync("Save Order");
        await Assertions.Expect(_page.GetByText("Order created")).ToBeVisibleAsync();
    }

    private async Task PlaceRestaurantOrderAsync()
    {
        await _page.GotoAsync(E2EEnvironment.BaseUrl + "/restaurant-orders");
        await SelectFirstOptionAsync("Dish");
        await ClickButtonAsync("Add Item");
        await ClickButtonAsync("Save Order");
        await Assertions.Expect(_page.GetByText("Order created")).ToBeVisibleAsync();
    }

    private async Task<double> CheckOutAsync()
    {
        await _page.GotoAsync(E2EEnvironment.BaseUrl + "/check-out");
        await _page.Locator("button", new() { HasTextString = "Check Out" }).First.ClickAsync();
        await Assertions.Expect(_page.GetByText("Successfully checked out")).ToBeVisibleAsync();
        return await ReadFirstGrandTotalAsync();
    }

    private async Task RunPayrollAsync()
    {
        await _page.GotoAsync(E2EEnvironment.BaseUrl + "/hr/payment-run");
        await SelectFirstOptionAsync("Employee");
        await ClickButtonAsync("Save Payment");
        await Assertions.Expect(_page.GetByText("Successfully saved")).ToBeVisibleAsync();
    }

    private async Task GenerateReportPdfAsync()
    {
        await _page.GotoAsync(E2EEnvironment.BaseUrl + "/reports/guests");
        var download = await _page.RunAndWaitForDownloadAsync(async () =>
        {
            await _page.Locator("button", new() { HasTextString = "Export PDF" }).First.ClickAsync();
        });
        var path = await download.PathAsync();
        Assert.False(string.IsNullOrEmpty(path));
        Assert.EndsWith(".pdf", download.SuggestedFilename);
    }

    // --- MudBlazor helpers -------------------------------------------------

    private async Task ClickButtonAsync(string text) =>
        await _page.Locator("button", new() { HasTextString = text }).First.ClickAsync();

    /// <summary>Opens a MudSelect identified by its label and picks the first item.</summary>
    private async Task SelectFirstOptionAsync(string label)
    {
        var control = _page.Locator($".mud-input-control:has(label:has-text(\"{label}\"))").First;
        await control.ClickAsync();
        var item = _page.Locator(".mud-list-item").First;
        await item.WaitForAsync(new() { Timeout = 5_000 });
        await item.ClickAsync();
    }

    private async Task<double> ReadNumericFieldAsync(string label)
    {
        var input = _page.Locator($".mud-input-control:has(label:has-text(\"{label}\")) input").First;
        var value = await input.InputValueAsync();
        return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var n) ? n : 0;
    }

    private async Task<double> ReadFirstGrandTotalAsync()
    {
        var cells = await _page.Locator("td").AllInnerTextsAsync();
        foreach (var cell in cells)
        {
            var cleaned = cell.Replace(",", "").Trim();
            if (double.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var n) && n > 0)
                return n;
        }
        return 0;
    }
}
