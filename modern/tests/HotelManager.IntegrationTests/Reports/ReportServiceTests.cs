using HotelManager.Application.Interfaces;
using HotelManager.Web.Services.Reports;

namespace HotelManager.IntegrationTests.Reports;

public class ReportServiceTests : IClassFixture<ReportsDbFixture>
{
    private readonly ReportsDbFixture _fixture;
    private readonly ReportService _svc;

    public ReportServiceTests(ReportsDbFixture fixture)
    {
        _fixture = fixture;
        _svc = new ReportService(new ReportDataService(fixture));
    }

    private static void AssertIsPdf(byte[] bytes)
    {
        Assert.True(bytes.Length > 500);
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(bytes, 0, 4));
    }

    [Fact]
    public async Task RenderRoomInvoice_ProducesPdf() =>
        AssertIsPdf(await _svc.RenderRoomInvoicePdfAsync(_fixture.CheckoutId));

    [Fact]
    public async Task RenderRoomInvoice_UnknownId_Throws() =>
        await Assert.ThrowsAsync<InvalidOperationException>(() => _svc.RenderRoomInvoicePdfAsync(99999));

    [Fact]
    public async Task RenderHallAndGardenInvoice_ProducesPdf() =>
        AssertIsPdf(await _svc.RenderHallAndGardenInvoicePdfAsync("HG-001"));

    [Fact]
    public async Task RenderHallAndGardenInvoice_UnknownId_Throws() =>
        await Assert.ThrowsAsync<InvalidOperationException>(() => _svc.RenderHallAndGardenInvoicePdfAsync("x"));

    [Fact]
    public async Task RenderHallOrGardenInvoice_ProducesPdf() =>
        AssertIsPdf(await _svc.RenderHallOrGardenInvoicePdfAsync("HOG-001"));

    [Fact]
    public async Task RenderHallOrGardenInvoice_UnknownId_Throws() =>
        await Assert.ThrowsAsync<InvalidOperationException>(() => _svc.RenderHallOrGardenInvoicePdfAsync("x"));

    [Fact]
    public async Task RenderOrderInvoice_ProducesPdf() =>
        AssertIsPdf(await _svc.RenderOrderInvoicePdfAsync(_fixture.OrderId));

    [Fact]
    public async Task RenderOrderInvoice_UnknownId_Throws() =>
        await Assert.ThrowsAsync<InvalidOperationException>(() => _svc.RenderOrderInvoicePdfAsync(99999));

    [Fact]
    public async Task RenderRestaurantOrderReceipt_ProducesPdf() =>
        AssertIsPdf(await _svc.RenderRestaurantOrderReceiptPdfAsync(_fixture.RestaurantOrderId));

    [Fact]
    public async Task RenderRestaurantOrderReceipt_UnknownId_Throws() =>
        await Assert.ThrowsAsync<InvalidOperationException>(() => _svc.RenderRestaurantOrderReceiptPdfAsync(99999));

    [Fact]
    public async Task RenderSalarySlip_ProducesPdf() =>
        AssertIsPdf(await _svc.RenderSalarySlipPdfAsync("PAY-001"));

    [Fact]
    public async Task RenderSalarySlip_UnknownId_Throws() =>
        await Assert.ThrowsAsync<InvalidOperationException>(() => _svc.RenderSalarySlipPdfAsync("x"));

    [Fact]
    public async Task RenderAttendanceReport_ProducesPdf() =>
        AssertIsPdf(await _svc.RenderAttendanceReportPdfAsync(new ReportRequest()));

    [Fact]
    public async Task RenderAdvancePaymentReport_ProducesPdf() =>
        AssertIsPdf(await _svc.RenderAdvancePaymentReportPdfAsync(new ReportRequest()));

    [Fact]
    public async Task RenderEmployeePaymentReport_ProducesPdf() =>
        AssertIsPdf(await _svc.RenderEmployeePaymentReportPdfAsync(new ReportRequest()));

    [Fact]
    public async Task RenderGuestReport_ProducesPdf() =>
        AssertIsPdf(await _svc.RenderGuestReportPdfAsync(new ReportRequest()));

    [Fact]
    public async Task RenderReservationReport_ProducesPdf() =>
        AssertIsPdf(await _svc.RenderReservationReportPdfAsync(new ReportRequest()));

    [Fact]
    public async Task RenderPurchasedInventoryReport_ProducesPdf() =>
        AssertIsPdf(await _svc.RenderPurchasedInventoryReportPdfAsync(new ReportRequest()));
}
