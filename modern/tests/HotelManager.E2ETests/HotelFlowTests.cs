using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace HotelManager.E2ETests;

/// <summary>
/// End-to-end journey driven with Playwright against a running instance of the
/// modern Hotel Manager app:
///   login → dashboard → create reservation → check-in (verify billing)
///   → place room order → place restaurant order → check-out (verify final bill)
///   → HR employee + payroll run → generate a report PDF.
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
        _page.SetDefaultTimeout(20_000);
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
        Assert.True(checkInBill > 0, "Check-in billing (grand total) should be greater than zero.");
        await PlaceRoomOrderAsync();
        await PlaceRestaurantOrderAsync();
        var finalBill = await CheckOutAsync();
        Assert.True(finalBill >= checkInBill,
            $"Final bill ({finalBill}) should be at least the room charges at check-in ({checkInBill}).");
        await CreateEmployeeAsync();
        await RunPayrollAsync();
        await GenerateReportPdfAsync();
    }

    private async Task LoginAsync()
    {
        // The login form is a plain static POST to /account/login. On a freshly
        // started (cold) server, the client-side Blazor script that governs enhanced
        // navigation can still be initializing when the submit is clicked, causing the
        // click to be swallowed and the form to never post. Wait for the page to be
        // fully loaded/interactive before submitting, and retry a few times.
        const int maxAttempts = 3;
        for (var attempt = 1; ; attempt++)
        {
            await _page.GotoAsync(Url("/login"), new() { WaitUntil = WaitUntilState.Load });
            await _page.WaitForSelectorAsync("#username", new() { State = WaitForSelectorState.Visible });
            // give blazor.web.js time to attach so data-enhance="false" is honoured
            await _page.WaitForTimeoutAsync(1_500);
            await _page.FillAsync("#username", E2EEnvironment.UserName);
            await _page.FillAsync("#password", E2EEnvironment.Password);
            await _page.ClickAsync("button[type=submit]");
            try
            {
                // Poll the location instead of waiting on navigation events (Blazor Server
                // keeps a persistent websocket open, so "load"/"networkidle" waits are unreliable).
                await _page.WaitForFunctionAsync(
                    "() => !location.pathname.startsWith('/login')", null, new() { Timeout = 20_000 });
                return;
            }
            catch (TimeoutException) when (attempt < maxAttempts)
            {
                // fall through and retry
            }
        }
    }

    private async Task AssertDashboardAsync()
    {
        await GotoAsync("/");
        await Assertions.Expect(
            _page.GetByRole(AriaRole.Heading, new() { Name = "Dashboard" })).ToBeVisibleAsync();
    }

    private async Task CreateReservationAsync()
    {
        await GotoAsync("/reservations");
        await SelectFirstOptionAsync("Guest");
        await SelectFirstOptionAsync("Room No");
        await ClickButtonAsync("Save");
        await ExpectSnackbarAsync("Reservation saved");
    }

    private async Task<double> CheckInAsync()
    {
        await GotoAsync("/check-in");
        await SelectFirstOptionAsync("Guest");
        // Check into a different room than the one just reserved to avoid a booking conflict.
        await SelectOptionByIndexAsync("Room No", 1); // auto-fills Room Charges
        await ClickButtonAsync("Check In");
        await ExpectSnackbarAsync("Successfully checked in");
        // The "Checked In Guests" table carries a Grand Total column.
        return await FirstPositiveNumberInTableAsync("Status");
    }

    private async Task PlaceRoomOrderAsync()
    {
        await GotoAsync("/orders/room");
        await SelectFirstOptionAsync("Checked-In Guest");
        await SelectFirstOptionAsync("Product");
        await ClickButtonAsync("Add Item");
        await ClickButtonAsync("Save Order");
        await ExpectSnackbarAsync("Order created");
    }

    private async Task PlaceRestaurantOrderAsync()
    {
        await GotoAsync("/orders/restaurant");
        await SelectFirstOptionAsync("Dish");
        await ClickButtonAsync("Add Item");
        await ClickButtonAsync("Save Order");
        await ExpectSnackbarAsync("Order created");
    }

    private async Task<double> CheckOutAsync()
    {
        await GotoAsync("/check-out");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Check Out", Exact = true }).First.ClickAsync();
        await ExpectSnackbarAsync("Successfully checked out");
        // "Checkout Bills" table is the one carrying a Bill No column.
        return await FirstPositiveNumberInTableAsync("Bill No");
    }

    private async Task CreateEmployeeAsync()
    {
        await GotoAsync("/hr/employees");
        await FillByLabelAsync("Full Name", "E2E Tester");
        await FillByLabelAsync("Address", "123 Test Street");
        await FillByLabelAsync("Mobile No", "9990001111");
        await FillByLabelAsync("Department", "QA");
        await FillByLabelAsync("Designation", "Tester");
        await FillByLabelAsync("Basic Salary", "3000");
        await FillByLabelAsync("Basic Working Time (hh:mm:ss)", "08:00:00");
        await ClickButtonAsync("Save");
        await ExpectSnackbarAsync("Employee Profile Successfully saved");
    }

    private async Task RunPayrollAsync()
    {
        await GotoAsync("/hr/payment-run");
        await SelectFirstOptionAsync("Employee");
        await ClickButtonAsync("Save Payment");
        await ExpectSnackbarAsync("Successfully saved");
    }

    private async Task GenerateReportPdfAsync()
    {
        await GotoAsync("/reports/guests");
        var download = await _page.RunAndWaitForDownloadAsync(async () =>
        {
            await _page.GetByRole(AriaRole.Button, new() { Name = "Export PDF" }).First.ClickAsync();
        });
        var path = await download.PathAsync();
        Assert.False(string.IsNullOrEmpty(path));
        Assert.EndsWith(".pdf", download.SuggestedFilename);
    }

    // --- helpers -----------------------------------------------------------

    private static string Url(string path) => E2EEnvironment.BaseUrl + path;

    private async Task GotoAsync(string path)
    {
        // Blazor Server holds a persistent websocket, so do not wait for "networkidle".
        await _page.GotoAsync(Url(path), new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        // Allow the interactive Blazor circuit to connect and re-render; the initial
        // static-rendered DOM is replaced once the circuit is established, which would
        // otherwise detach elements mid-interaction.
        await _page.WaitForTimeoutAsync(1_200);
    }

    private async Task ClickButtonAsync(string name) =>
        await _page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true }).First.ClickAsync();

    private async Task ExpectSnackbarAsync(string text)
    {
        var snackbar = _page.Locator(".mud-snackbar").First;
        try
        {
            await snackbar.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 20_000 });
        }
        catch (TimeoutException)
        {
            throw new Xunit.Sdk.XunitException($"No snackbar appeared while expecting '{text}'.");
        }
        var actual = await snackbar.InnerTextAsync();
        Assert.Contains(text, actual);
    }

    /// <summary>Opens a MudSelect identified by its label and picks the first item.</summary>
    private Task SelectFirstOptionAsync(string label) => SelectOptionByIndexAsync(label, 0);

    /// <summary>Opens a MudSelect identified by its label and picks the item at the given index.</summary>
    private async Task SelectOptionByIndexAsync(string label, int index)
    {
        var control = _page.Locator($".mud-input-control:has(label:text-is(\"{label}\"))").First;
        await control.ScrollIntoViewIfNeededAsync();
        await control.ClickAsync();
        var items = _page.Locator(".mud-list-item");
        await items.First.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10_000 });
        await items.Nth(index).ClickAsync();
        // let the popover close before the next interaction
        await _page.WaitForTimeoutAsync(200);
    }

    private async Task FillByLabelAsync(string label, string value)
    {
        var input = _page.Locator($".mud-input-control:has(label:text-is(\"{label}\")) input").First;
        await input.ScrollIntoViewIfNeededAsync();
        await input.FillAsync(value);
    }

    /// <summary>Returns the first positive number found in the table that contains the given column header.</summary>
    private async Task<double> FirstPositiveNumberInTableAsync(string headerText)
    {
        var table = _page.Locator($".mud-table:has(th:has-text(\"{headerText}\"))").First;
        await table.Locator("tbody tr").First.WaitForAsync(new() { Timeout = 10_000 });
        var cells = await table.Locator("tbody tr").First.Locator("td").AllInnerTextsAsync();
        foreach (var cell in cells)
        {
            var cleaned = Regex.Replace(cell, "[^0-9.]", "");
            if (double.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var n) && n > 0)
                return n;
        }
        return 0;
    }
}
