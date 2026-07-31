using HotelManagement.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("guests")]
    public async Task<IActionResult> GuestReport([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var bytes = await _reportService.GenerateGuestReportAsync(fromDate, toDate);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "GuestReport.xlsx");
    }

    [HttpGet("employees")]
    public async Task<IActionResult> EmployeeReport([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var bytes = await _reportService.GenerateEmployeeReportAsync(fromDate, toDate);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "EmployeeReport.xlsx");
    }

    [HttpGet("checkin")]
    public async Task<IActionResult> CheckInReport([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var bytes = await _reportService.GenerateCheckInReportAsync(fromDate, toDate);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CheckInReport.xlsx");
    }

    [HttpGet("checkout")]
    public async Task<IActionResult> CheckOutReport([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var bytes = await _reportService.GenerateCheckOutReportAsync(fromDate, toDate);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CheckOutReport.xlsx");
    }

    [HttpGet("reservations")]
    public async Task<IActionResult> ReservationReport([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var bytes = await _reportService.GenerateReservationReportAsync(fromDate, toDate);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ReservationReport.xlsx");
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> TransactionReport([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var bytes = await _reportService.GenerateTransactionReportAsync(fromDate, toDate);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TransactionReport.xlsx");
    }

    [HttpGet("attendance")]
    public async Task<IActionResult> AttendanceReport([FromQuery] string? employeeId = null, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var bytes = await _reportService.GenerateAttendanceReportAsync(employeeId, fromDate, toDate);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "AttendanceReport.xlsx");
    }

    [HttpGet("overtime")]
    public async Task<IActionResult> OvertimeReport([FromQuery] string? employeeId = null, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var bytes = await _reportService.GenerateOvertimeReportAsync(employeeId, fromDate, toDate);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "OvertimeReport.xlsx");
    }

    [HttpGet("advance-payment")]
    public async Task<IActionResult> AdvancePaymentReport([FromQuery] string? employeeId = null, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var bytes = await _reportService.GenerateAdvancePaymentReportAsync(employeeId, fromDate, toDate);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "AdvancePaymentReport.xlsx");
    }

    [HttpGet("employee-payment")]
    public async Task<IActionResult> EmployeePaymentReport([FromQuery] string? employeeId = null, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var bytes = await _reportService.GenerateEmployeePaymentReportAsync(employeeId, fromDate, toDate);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "EmployeePaymentReport.xlsx");
    }

    [HttpGet("salary-slip/{paymentId}")]
    public async Task<IActionResult> SalarySlip(int paymentId)
    {
        var bytes = await _reportService.GenerateSalarySlipAsync(paymentId);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SalarySlip.xlsx");
    }

    [HttpGet("salary-slips")]
    public async Task<IActionResult> SalarySlipsReport([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var bytes = await _reportService.GenerateSalarySlipsReportAsync(fromDate, toDate);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SalarySlipsReport.xlsx");
    }

    [HttpGet("purchased-inventory")]
    public async Task<IActionResult> PurchasedInventoryReport([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var bytes = await _reportService.GeneratePurchasedInventoryReportAsync(fromDate, toDate);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PurchasedInventoryReport.xlsx");
    }

    [HttpGet("invoice/room/{checkoutId}")]
    public async Task<IActionResult> RoomInvoice(int checkoutId)
    {
        var bytes = await _reportService.GenerateRoomInvoiceAsync(checkoutId);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "RoomInvoice.xlsx");
    }

    [HttpGet("invoice/hall-garden/{reservationId}")]
    public async Task<IActionResult> HallAndGardenInvoice(int reservationId)
    {
        var bytes = await _reportService.GenerateHallAndGardenInvoiceAsync(reservationId);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "HallGardenInvoice.xlsx");
    }

    [HttpGet("invoice/hall-or-garden/{reservationId}")]
    public async Task<IActionResult> HallOrGardenInvoice(int reservationId)
    {
        var bytes = await _reportService.GenerateHallOrGardenInvoiceAsync(reservationId);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "HallOrGardenInvoice.xlsx");
    }

    [HttpGet("invoice/order/{orderId}")]
    public async Task<IActionResult> OrderInvoice(int orderId)
    {
        var bytes = await _reportService.GenerateOrderInvoiceAsync(orderId);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "OrderInvoice.xlsx");
    }

    [HttpGet("invoice/restaurant/{orderId}")]
    public async Task<IActionResult> RestaurantOrderReceipt(int orderId)
    {
        var bytes = await _reportService.GenerateRestaurantOrderReceiptAsync(orderId);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "RestaurantOrderReceipt.xlsx");
    }
}
