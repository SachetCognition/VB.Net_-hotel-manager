using HotelManager.Application.Interfaces;
using HotelManager.Web.Services.Reports;

namespace HotelManager.IntegrationTests.Reports;

public class ReportDataServiceTests : IClassFixture<ReportsDbFixture>
{
    private readonly ReportsDbFixture _fixture;
    private readonly ReportDataService _svc;

    public ReportDataServiceTests(ReportsDbFixture fixture)
    {
        _fixture = fixture;
        _svc = new ReportDataService(fixture);
    }

    [Fact]
    public async Task GetHotelHeader_ReturnsHotelInfo()
    {
        var header = await _svc.GetHotelHeaderAsync();
        Assert.Equal("Test Grand Hotel", header.HotelName);
        Assert.Equal("42 Test Street", header.Address);
        Assert.Equal("111-222", header.ContactNo);
        Assert.Equal("333-444", header.ContactNo1);
        Assert.Equal("info@testgrand.example", header.Email);
        Assert.Equal("TIN-1", header.TIN);
        Assert.Equal("ST-1", header.STNo);
    }

    [Fact]
    public async Task GetRoomInvoice_JoinsCheckInCheckoutGuestAndHotel()
    {
        var row = await _svc.GetRoomInvoiceAsync(_fixture.CheckoutId);
        Assert.NotNull(row);
        Assert.Equal("BILL-1", row!.BillNo);
        Assert.Equal(new DateTime(2026, 1, 4), row.CheckOutDate);
        Assert.Equal("USD", row.Currency);
        Assert.Equal("G-001", row.GuestID);
        Assert.Equal("Alice Test", row.GuestName);
        Assert.Equal("9 Guest Lane", row.GuestAddress);
        Assert.Equal("Testville", row.City);
        Assert.Equal("Passport", row.IDType);
        Assert.Equal("P123", row.IDNumber);
        Assert.Equal("101", row.RoomNo);
        Assert.Equal(100, row.RoomCharges);
        Assert.Equal(3, row.NoOfDays);
        Assert.Equal("Yes", row.ExtraBed);
        Assert.Equal(300, row.TotalRoomCharges);
        Assert.Equal(50, row.OtherCharges);
        Assert.Equal(350, row.SubTotal);
        Assert.Equal(10, row.ServiceTaxPer);
        Assert.Equal(35, row.ServiceTaxAmount);
        Assert.Equal(5, row.LuxuryTaxPer);
        Assert.Equal(17.5, row.LuxuryTaxAmount);
        Assert.Equal(402.5, row.GrandTotal);
        Assert.Equal(400, row.TotalPaid);
        Assert.Equal(2.5, row.Balance);
        Assert.Equal("checkout note", row.Notes);
        Assert.Equal("Test Grand Hotel", row.Hotel.HotelName);
    }

    [Fact]
    public async Task GetRoomInvoice_UnknownId_ReturnsNull() =>
        Assert.Null(await _svc.GetRoomInvoiceAsync(99999));

    [Fact]
    public async Task GetRoomInvoicePicks_ReturnsCheckout()
    {
        var picks = await _svc.GetRoomInvoicePicksAsync();
        var pick = Assert.Single(picks);
        Assert.Equal(_fixture.CheckoutId.ToString(), pick.Id);
        Assert.Equal("BILL-1", pick.Number);
        Assert.Equal("Alice Test", pick.Name);
    }

    [Fact]
    public async Task GetHallAndGardenInvoice_JoinsGuestCurrencyHotel()
    {
        var row = await _svc.GetHallAndGardenInvoiceAsync("HG-001");
        Assert.NotNull(row);
        Assert.Equal("Alice Test", row!.GuestName);
        Assert.Equal("USD", row.Currency);
        Assert.Equal("Main Hall", row.Hall);
        Assert.Equal(2, row.DaysHall);
        Assert.Equal(500, row.RateHall);
        Assert.Equal(1000, row.TotalChargesHall);
        Assert.Equal("Rose Garden", row.Garden);
        Assert.Equal(1, row.DaysGarden);
        Assert.Equal(200, row.RateGarden);
        Assert.Equal(1300, row.GrandTotal);
        Assert.Equal("Test Grand Hotel", row.Hotel.HotelName);
    }

    [Fact]
    public async Task GetHallAndGardenInvoice_UnknownId_ReturnsNull() =>
        Assert.Null(await _svc.GetHallAndGardenInvoiceAsync("missing"));

    [Fact]
    public async Task GetHallAndGardenInvoicePicks_ReturnsReservation()
    {
        var picks = await _svc.GetHallAndGardenInvoicePicksAsync();
        var pick = Assert.Single(picks);
        Assert.Equal("HG-001", pick.Id);
        Assert.Equal("Alice Test", pick.Name);
    }

    [Fact]
    public async Task GetHallOrGardenInvoice_JoinsGuestCurrencyHotel()
    {
        var row = await _svc.GetHallOrGardenInvoiceAsync("HOG-001");
        Assert.NotNull(row);
        Assert.Equal("Hall", row!.Type);
        Assert.Equal("Alice Test", row.GuestName);
        Assert.Equal("USD", row.Currency);
        Assert.Equal(1, row.Days);
        Assert.Equal(450, row.Rate);
        Assert.Equal(500, row.GrandTotal);
        Assert.Equal(250, row.Balance);
        Assert.Equal("Test Grand Hotel", row.Hotel.HotelName);
    }

    [Fact]
    public async Task GetHallOrGardenInvoice_UnknownId_ReturnsNull() =>
        Assert.Null(await _svc.GetHallOrGardenInvoiceAsync("missing"));

    [Fact]
    public async Task GetHallOrGardenInvoicePicks_ReturnsReservation()
    {
        var picks = await _svc.GetHallOrGardenInvoicePicksAsync();
        var pick = Assert.Single(picks);
        Assert.Equal("HOG-001", pick.Id);
    }

    [Fact]
    public async Task GetOrderInvoice_JoinsProductsCheckInGuest()
    {
        var row = await _svc.GetOrderInvoiceAsync(_fixture.OrderId);
        Assert.NotNull(row);
        Assert.Equal("ORD-1", row!.OrderNo);
        Assert.Equal("101", row.RoomNo);
        Assert.Equal("Alice Test", row.GuestName);
        Assert.Equal("USD", row.Currency);
        Assert.Equal(60, row.SubTotal);
        Assert.Equal(10, row.VATPer);
        Assert.Equal(6, row.VATAmount);
        Assert.Equal(69, row.GrandTotal);
        var line = Assert.Single(row.Lines);
        Assert.Equal("Pasta", line.ProductName);
        Assert.Equal(30, line.Rate);
        Assert.Equal(2, line.Quantity);
        Assert.Equal(60, line.Amount);
        Assert.Equal("Test Grand Hotel", row.Hotel.HotelName);
    }

    [Fact]
    public async Task GetOrderInvoice_UnknownId_ReturnsNull() =>
        Assert.Null(await _svc.GetOrderInvoiceAsync(99999));

    [Fact]
    public async Task GetOrderInvoicePicks_ReturnsOrder()
    {
        var picks = await _svc.GetOrderInvoicePicksAsync();
        var pick = Assert.Single(picks);
        Assert.Equal("ORD-1", pick.Number);
        Assert.Equal("101", pick.Name);
    }

    [Fact]
    public async Task GetRestaurantReceipt_JoinsProductsAndHotel()
    {
        var row = await _svc.GetRestaurantReceiptAsync(_fixture.RestaurantOrderId);
        Assert.NotNull(row);
        Assert.Equal("REST-1", row!.OrderNo);
        Assert.Equal("USD", row.Currency);
        Assert.Equal(40, row.SubTotal);
        Assert.Equal(44, row.GrandTotal);
        var line = Assert.Single(row.Lines);
        Assert.Equal("Soup", line.ProductName);
        Assert.Equal(20, line.Rate);
        Assert.Equal(2, line.Quantity);
        Assert.Equal(40, line.Amount);
    }

    [Fact]
    public async Task GetRestaurantReceipt_UnknownId_ReturnsNull() =>
        Assert.Null(await _svc.GetRestaurantReceiptAsync(99999));

    [Fact]
    public async Task GetRestaurantReceiptPicks_ReturnsOrder()
    {
        var picks = await _svc.GetRestaurantReceiptPicksAsync();
        var pick = Assert.Single(picks);
        Assert.Equal("REST-1", pick.Number);
    }

    [Fact]
    public async Task GetSalarySlip_JoinsEmployeeRegistration()
    {
        var row = await _svc.GetSalarySlipAsync("PAY-001");
        Assert.NotNull(row);
        Assert.Equal("E-001", row!.EmployeeID);
        Assert.Equal("Bob Worker", row.EmployeeName);
        Assert.Equal("Chef", row.Designation);
        Assert.Equal("Kitchen", row.Department);
        Assert.Equal(3000, row.Salary);
        Assert.Equal(26, row.PresentDays);
        Assert.Equal(150, row.Advance);
        Assert.Equal(50, row.Deduction);
        Assert.Equal("05:00", row.Overtime);
        Assert.Equal(100, row.OverTimeAmount);
        Assert.Equal("Cash", row.ModeOfPayment);
        Assert.Equal(2900, row.NetPay);
        Assert.Equal("Test Grand Hotel", row.Hotel.HotelName);
    }

    [Fact]
    public async Task GetSalarySlip_UnknownId_ReturnsNull() =>
        Assert.Null(await _svc.GetSalarySlipAsync("missing"));

    [Fact]
    public async Task GetSalarySlipPicks_ReturnsPayment()
    {
        var picks = await _svc.GetSalarySlipPicksAsync();
        var pick = Assert.Single(picks);
        Assert.Equal("PAY-001", pick.Id);
        Assert.Equal("Bob Worker", pick.Name);
    }

    [Fact]
    public async Task GetAttendance_JoinsEmployeeName()
    {
        var rows = await _svc.GetAttendanceAsync(new ReportRequest());
        Assert.Equal(2, rows.Count);
        Assert.All(rows, r => Assert.Equal("Bob Worker", r.EmployeeName));
        Assert.Equal("Present", rows[0].Status);
        Assert.Equal("09:00", rows[0].InTime);
        Assert.Equal("18:00", rows[0].OutTime);
        Assert.Equal("01:00", rows[0].Overtime);
    }

    [Fact]
    public async Task GetAttendance_FiltersByDateRange()
    {
        var rows = await _svc.GetAttendanceAsync(
            new ReportRequest(new DateTime(2026, 4, 1), new DateTime(2026, 4, 30)));
        var row = Assert.Single(rows);
        Assert.Equal(new DateTime(2026, 4, 1), row.WorkingDate);
    }

    [Fact]
    public async Task GetAttendance_FiltersByEmployeeKey()
    {
        Assert.Equal(2, (await _svc.GetAttendanceAsync(new ReportRequest(Key: "E-001"))).Count);
        Assert.Empty(await _svc.GetAttendanceAsync(new ReportRequest(Key: "E-999")));
    }

    [Fact]
    public async Task GetAdvancePayments_JoinsEmployeeAndFilters()
    {
        var rows = await _svc.GetAdvancePaymentsAsync(new ReportRequest());
        var row = Assert.Single(rows);
        Assert.Equal("E-001", row.EmployeeID);
        Assert.Equal("Bob Worker", row.EmployeeName);
        Assert.Equal(150, row.Amount);
        Assert.Equal(0, row.Deduction);

        Assert.Empty(await _svc.GetAdvancePaymentsAsync(
            new ReportRequest(new DateTime(2026, 6, 1), null)));
    }

    [Fact]
    public async Task GetEmployeePayments_JoinsEmployeeAndFilters()
    {
        var rows = await _svc.GetEmployeePaymentsAsync(new ReportRequest());
        var row = Assert.Single(rows);
        Assert.Equal("PAY-001", row.PaymentID);
        Assert.Equal("Bob Worker", row.EmployeeName);
        Assert.Equal(2900, row.NetPay);

        Assert.Empty(await _svc.GetEmployeePaymentsAsync(
            new ReportRequest(new DateTime(2026, 6, 1), null)));
        Assert.Single(await _svc.GetEmployeePaymentsAsync(new ReportRequest(Key: "E-001")));
    }

    [Fact]
    public async Task GetGuests_ReturnsAllColumns_AndFiltersByKey()
    {
        var rows = await _svc.GetGuestsAsync(new ReportRequest());
        var row = Assert.Single(rows);
        Assert.Equal("G-001", row.GuestID);
        Assert.Equal("Alice Test", row.GuestName);
        Assert.Equal("Testville", row.City);
        Assert.Equal("Passport", row.IDType);

        Assert.Single(await _svc.GetGuestsAsync(new ReportRequest(Key: "Alice")));
        Assert.Empty(await _svc.GetGuestsAsync(new ReportRequest(Key: "Zed")));
    }

    [Fact]
    public async Task GetReservations_JoinsGuestAndRoom()
    {
        var rows = await _svc.GetReservationsAsync(new ReportRequest());
        var row = Assert.Single(rows);
        Assert.Equal("R-001", row.ReservationID);
        Assert.Equal("Alice Test", row.GuestName);
        Assert.Equal("101", row.RoomNo);
        Assert.Equal("Deluxe", row.RoomType);
        Assert.Equal("Reserved", row.Status);

        Assert.Single(await _svc.GetReservationsAsync(
            new ReportRequest(new DateTime(2026, 6, 1), new DateTime(2026, 6, 2))));
        Assert.Empty(await _svc.GetReservationsAsync(
            new ReportRequest(new DateTime(2026, 7, 1), null)));
        Assert.Single(await _svc.GetReservationsAsync(new ReportRequest(Key: "R-0")));
    }

    [Fact]
    public async Task GetPurchasedInventories_ReturnsAllColumns_AndFilters()
    {
        var rows = await _svc.GetPurchasedInventoriesAsync(new ReportRequest());
        var row = Assert.Single(rows);
        Assert.Equal("Rice", row.ProductName);
        Assert.Equal("Grocery", row.Category);
        Assert.Equal("Purchase", row.TransactionType);
        Assert.Equal("Acme Supplies", row.PartyName);
        Assert.Equal(25, row.Quantity);
        Assert.Equal("kg", row.Unit);
        Assert.Equal(2, row.Price);
        Assert.Equal(50, row.TotalPrice);

        Assert.Single(await _svc.GetPurchasedInventoriesAsync(new ReportRequest(Key: "Acme")));
        Assert.Empty(await _svc.GetPurchasedInventoriesAsync(new ReportRequest(Key: "Nope")));
        Assert.Empty(await _svc.GetPurchasedInventoriesAsync(
            new ReportRequest(new DateTime(2026, 5, 1), null)));
    }
}
