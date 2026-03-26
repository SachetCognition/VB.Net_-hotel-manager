namespace HotelManagement.Core.Interfaces;

public interface IReportService
{
    Task<byte[]> GenerateGuestReportAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<byte[]> GenerateEmployeeReportAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<byte[]> GenerateCheckInReportAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<byte[]> GenerateCheckOutReportAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<byte[]> GenerateReservationReportAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<byte[]> GenerateTransactionReportAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<byte[]> GenerateAttendanceReportAsync(string? employeeId = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task<byte[]> GenerateOvertimeReportAsync(string? employeeId = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task<byte[]> GenerateAdvancePaymentReportAsync(string? employeeId = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task<byte[]> GenerateEmployeePaymentReportAsync(string? employeeId = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task<byte[]> GenerateSalarySlipAsync(int paymentId);
    Task<byte[]> GenerateSalarySlipsReportAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<byte[]> GeneratePurchasedInventoryReportAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<byte[]> GenerateRoomInvoiceAsync(int checkoutId);
    Task<byte[]> GenerateHallAndGardenInvoiceAsync(int reservationId);
    Task<byte[]> GenerateHallOrGardenInvoiceAsync(int reservationId);
    Task<byte[]> GenerateOrderInvoiceAsync(int orderId);
    Task<byte[]> GenerateRestaurantOrderReceiptAsync(int orderId);
}
