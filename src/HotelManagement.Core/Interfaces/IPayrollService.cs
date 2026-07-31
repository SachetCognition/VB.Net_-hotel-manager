using HotelManagement.Core.DTOs;

namespace HotelManagement.Core.Interfaces;

public interface IPayrollService
{
    Task<PaymentResponse> ProcessPaymentAsync(ProcessPaymentRequest request);
    Task<IEnumerable<PaymentResponse>> GetPaymentsAsync(string? employeeId = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task<PaymentResponse?> GetPaymentByIdAsync(int id);
    Task DeletePaymentAsync(int id);
    int CalculateSalary(decimal basicSalary, int presentDays);
    int CalculateOvertimeAmount(double totalOvertimeMinutes, decimal rate);
    decimal CalculateNetPay(decimal salary, decimal overtimeAmount, decimal deduction);
    Task<AdvanceEntryResponse> CreateAdvanceEntryAsync(AdvanceEntryRequest request);
    Task<IEnumerable<AdvanceEntryResponse>> GetAdvanceEntriesAsync(string? employeeId = null);
    Task<decimal> GetAdvanceBalanceAsync(string employeeId);
}
