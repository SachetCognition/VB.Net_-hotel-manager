using HotelManager.Application.Interfaces;

namespace HotelManager.Web.Services.Reports;

/// <summary>Implements <see cref="IReportService"/> by combining data projections with QuestPDF layouts.</summary>
public class ReportService : IReportService
{
    private readonly ReportDataService _data;

    public ReportService(ReportDataService data) => _data = data;

    public async Task<byte[]> RenderRoomInvoicePdfAsync(int checkoutId)
    {
        var row = await _data.GetRoomInvoiceAsync(checkoutId)
            ?? throw new InvalidOperationException($"Checkout {checkoutId} not found.");
        return ReportPdfBuilder.BuildRoomInvoice(row);
    }

    public async Task<byte[]> RenderHallAndGardenInvoicePdfAsync(string reservationId)
    {
        var row = await _data.GetHallAndGardenInvoiceAsync(reservationId)
            ?? throw new InvalidOperationException($"Reservation {reservationId} not found.");
        return ReportPdfBuilder.BuildHallAndGardenInvoice(row);
    }

    public async Task<byte[]> RenderHallOrGardenInvoicePdfAsync(string reservationId)
    {
        var row = await _data.GetHallOrGardenInvoiceAsync(reservationId)
            ?? throw new InvalidOperationException($"Reservation {reservationId} not found.");
        return ReportPdfBuilder.BuildHallOrGardenInvoice(row);
    }

    public async Task<byte[]> RenderOrderInvoicePdfAsync(int orderId)
    {
        var row = await _data.GetOrderInvoiceAsync(orderId)
            ?? throw new InvalidOperationException($"Order {orderId} not found.");
        return ReportPdfBuilder.BuildOrderInvoice(row);
    }

    public async Task<byte[]> RenderRestaurantOrderReceiptPdfAsync(int orderId)
    {
        var row = await _data.GetRestaurantReceiptAsync(orderId)
            ?? throw new InvalidOperationException($"Restaurant order {orderId} not found.");
        return ReportPdfBuilder.BuildRestaurantReceipt(row);
    }

    public async Task<byte[]> RenderSalarySlipPdfAsync(string paymentId)
    {
        var row = await _data.GetSalarySlipAsync(paymentId)
            ?? throw new InvalidOperationException($"Payment {paymentId} not found.");
        return ReportPdfBuilder.BuildSalarySlip(row);
    }

    public async Task<byte[]> RenderAttendanceReportPdfAsync(ReportRequest request) =>
        ReportPdfBuilder.BuildAttendanceReport(
            await _data.GetHotelHeaderAsync(), await _data.GetAttendanceAsync(request));

    public async Task<byte[]> RenderAdvancePaymentReportPdfAsync(ReportRequest request) =>
        ReportPdfBuilder.BuildAdvancePaymentReport(
            await _data.GetHotelHeaderAsync(), await _data.GetAdvancePaymentsAsync(request));

    public async Task<byte[]> RenderEmployeePaymentReportPdfAsync(ReportRequest request) =>
        ReportPdfBuilder.BuildEmployeePaymentReport(
            await _data.GetHotelHeaderAsync(), await _data.GetEmployeePaymentsAsync(request));

    public async Task<byte[]> RenderGuestReportPdfAsync(ReportRequest request) =>
        ReportPdfBuilder.BuildGuestReport(
            await _data.GetHotelHeaderAsync(), await _data.GetGuestsAsync(request));

    public async Task<byte[]> RenderReservationReportPdfAsync(ReportRequest request) =>
        ReportPdfBuilder.BuildReservationReport(
            await _data.GetHotelHeaderAsync(), await _data.GetReservationsAsync(request));

    public async Task<byte[]> RenderPurchasedInventoryReportPdfAsync(ReportRequest request) =>
        ReportPdfBuilder.BuildPurchasedInventoryReport(
            await _data.GetHotelHeaderAsync(), await _data.GetPurchasedInventoriesAsync(request));
}
