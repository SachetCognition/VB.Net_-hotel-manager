using System.Security.Cryptography;
using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Core.Services;

/// <summary>
/// Payroll Service implementing exact legacy salary calculation formulas from frmEmployeePayment.vb
/// 
/// Salary = (BasicSalary * PresentDays) / 30
/// OvertimeAmount = (TotalOvertimeMinutes * OvertimeRate) / 60
/// NetPay = Salary + OvertimeAmount - Deduction
/// </summary>
public class PayrollService : IPayrollService
{
    private readonly DbContext _context;
    private readonly IAttendanceService _attendanceService;

    public PayrollService(DbContext context, IAttendanceService attendanceService)
    {
        _context = context;
        _attendanceService = attendanceService;
    }

    public async Task<PaymentResponse> ProcessPaymentAsync(ProcessPaymentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.EmployeeID))
            throw new ArgumentException("Please select employee");

        var employee = await _context.Set<Employee>().FindAsync(request.EmployeeID)
            ?? throw new KeyNotFoundException($"Employee {request.EmployeeID} not found");

        // Prevent duplicate payment on same date — from frmEmployeePayment.vb
        var existingPayment = await _context.Set<EmployeePayment>()
            .AnyAsync(p => p.EmployeeID == request.EmployeeID
                && p.PaymentDate.Date == DateTime.UtcNow.Date);
        if (existingPayment)
            throw new InvalidOperationException("Payment already processed for this employee today");

        // Calculate present days
        var presentDays = await _attendanceService.GetPresentDaysAsync(
            request.EmployeeID, request.FromDate, request.ToDate);

        // Calculate total overtime
        var totalOvertime = await _attendanceService.GetTotalOvertimeAsync(
            request.EmployeeID, request.FromDate, request.ToDate);

        // Calculate salary using exact legacy formula
        var salary = CalculateSalaryDecimal(employee.Salary, presentDays);

        // Calculate overtime amount using exact legacy formula
        var totalOvertimeMinutes = totalOvertime.TotalMinutes;
        var overtimeAmount = CalculateOvertimeAmountDecimal(totalOvertimeMinutes, request.OvertimeRate);

        // Get advance balance
        var advanceBalance = await GetAdvanceBalanceAsync(request.EmployeeID);

        // Validate deduction does not exceed advance — from frmEmployeePayment.vb
        if (request.Deduction > advanceBalance)
            throw new ArgumentException($"You can not deduct more than advance amount. Advance balance: {advanceBalance}");

        // NetPay = Salary + OvertimeAmount - Deduction
        var netPay = CalculateNetPay(salary, overtimeAmount, request.Deduction);

        // Validate NetPay must be positive — from frmEmployeePayment.vb
        if (netPay <= 0)
            throw new ArgumentException("Net pay should be greater than zero");

        var paymentId = GeneratePaymentId();

        var payment = new EmployeePayment
        {
            PaymentID = paymentId,
            EmployeeID = employee.EmployeeID,
            EmployeeName = employee.EmployeeName,
            Department = employee.Department,
            Designation = employee.Designation,
            PaymentDate = DateTime.UtcNow,
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            BasicSalary = employee.Salary,
            PresentDays = presentDays,
            Salary = salary,
            TotalOvertime = $"{(int)totalOvertime.TotalHours:D2}:{totalOvertime.Minutes:D2}:{totalOvertime.Seconds:D2}",
            OvertimeRate = request.OvertimeRate,
            OvertimeAmount = overtimeAmount,
            Advance = advanceBalance,
            Deduction = request.Deduction,
            NetPay = netPay
        };

        _context.Set<EmployeePayment>().Add(payment);

        // Auto-create advance entry for deduction (from frmEmployeePayment.vb)
        if (request.Deduction > 0)
        {
            var advanceEntry = new AdvanceEntry
            {
                EmployeeID = employee.EmployeeID,
                EmployeeName = employee.EmployeeName,
                WorkingDate = DateTime.UtcNow,
                Amount = 0,
                Deduction = request.Deduction
            };
            _context.Set<AdvanceEntry>().Add(advanceEntry);
        }

        await _context.SaveChangesAsync();
        return MapToResponse(payment);
    }

    public async Task<IEnumerable<PaymentResponse>> GetPaymentsAsync(
        string? employeeId = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Set<EmployeePayment>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(employeeId))
            query = query.Where(p => p.EmployeeID == employeeId);
        if (fromDate.HasValue)
            query = query.Where(p => p.PaymentDate >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(p => p.PaymentDate <= toDate.Value);

        var results = await query.OrderByDescending(p => p.PaymentDate).ToListAsync();
        return results.Select(MapToResponse);
    }

    public async Task<PaymentResponse?> GetPaymentByIdAsync(int id)
    {
        var payment = await _context.Set<EmployeePayment>().FindAsync(id);
        return payment == null ? null : MapToResponse(payment);
    }

    public async Task DeletePaymentAsync(int id)
    {
        var payment = await _context.Set<EmployeePayment>().FindAsync(id)
            ?? throw new KeyNotFoundException($"Payment record {id} not found");

        _context.Set<EmployeePayment>().Remove(payment);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Salary = (BasicSalary * PresentDays) / 30
    /// From frmEmployeePayment.vb: txtSalary.Text = Val(txtBasicSalary.Text) * Val(txtPresentDays.Text) / 30
    /// Returns integer for interface compliance (truncated)
    /// </summary>
    public int CalculateSalary(decimal basicSalary, int presentDays)
    {
        return (int)(basicSalary * presentDays / 30m);
    }

    private static decimal CalculateSalaryDecimal(decimal basicSalary, int presentDays)
    {
        return Math.Round(basicSalary * presentDays / 30m, 2);
    }

    /// <summary>
    /// OvertimeAmount = (TotalOvertimeMinutes * Rate) / 60
    /// From frmEmployeePayment.vb: OvertimeAmount = Val(TotalOvertimeMinutes) * Val(Rate) / 60
    /// Returns integer for interface compliance (truncated)
    /// </summary>
    public int CalculateOvertimeAmount(double totalOvertimeMinutes, decimal rate)
    {
        return (int)((decimal)totalOvertimeMinutes * rate / 60m);
    }

    private static decimal CalculateOvertimeAmountDecimal(double totalOvertimeMinutes, decimal rate)
    {
        return Math.Round((decimal)totalOvertimeMinutes * rate / 60m, 2);
    }

    /// <summary>
    /// NetPay = Salary + OvertimeAmount - Deduction
    /// From frmEmployeePayment.vb: txtNetPay.Text = Val(txtSalary.Text) + Val(txtOvertimeAmount.Text) - Val(txtDeduction.Text)
    /// </summary>
    public decimal CalculateNetPay(decimal salary, decimal overtimeAmount, decimal deduction)
    {
        return salary + overtimeAmount - deduction;
    }

    public async Task<AdvanceEntryResponse> CreateAdvanceEntryAsync(AdvanceEntryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.EmployeeID))
            throw new ArgumentException("Please select employee");

        var entry = new AdvanceEntry
        {
            EmployeeID = request.EmployeeID,
            EmployeeName = request.EmployeeName,
            WorkingDate = request.WorkingDate,
            Amount = request.Amount,
            Deduction = 0
        };

        _context.Set<AdvanceEntry>().Add(entry);
        await _context.SaveChangesAsync();
        return MapAdvanceToResponse(entry);
    }

    public async Task<IEnumerable<AdvanceEntryResponse>> GetAdvanceEntriesAsync(string? employeeId = null)
    {
        var query = _context.Set<AdvanceEntry>().AsQueryable();
        if (!string.IsNullOrWhiteSpace(employeeId))
            query = query.Where(a => a.EmployeeID == employeeId);

        var results = await query.OrderByDescending(a => a.WorkingDate).ToListAsync();
        return results.Select(MapAdvanceToResponse);
    }

    public async Task<decimal> GetAdvanceBalanceAsync(string employeeId)
    {
        var totalAdvance = await _context.Set<AdvanceEntry>()
            .Where(a => a.EmployeeID == employeeId)
            .SumAsync(a => a.Amount);
        var totalDeduction = await _context.Set<AdvanceEntry>()
            .Where(a => a.EmployeeID == employeeId)
            .SumAsync(a => a.Deduction);
        return totalAdvance - totalDeduction;
    }

    /// <summary>
    /// Generates a unique Payment ID in the format "SP-" + 9 random digits.
    /// Mirrors legacy GetUniqueKey usage.
    /// </summary>
    private static string GeneratePaymentId()
    {
        const string chars = "123456789";
        var data = new byte[9];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(data);
        var result = new char[9];
        for (int i = 0; i < 9; i++)
            result[i] = chars[data[i] % chars.Length];
        return "SP-" + new string(result);
    }

    private static PaymentResponse MapToResponse(EmployeePayment p) =>
        new(p.ID, p.PaymentID, p.EmployeeID, p.EmployeeName,
            p.Department, p.Designation, p.PaymentDate,
            p.FromDate, p.ToDate, p.BasicSalary, p.PresentDays,
            p.Salary, p.TotalOvertime, p.OvertimeRate,
            p.OvertimeAmount, p.Advance, p.Deduction, p.NetPay);

    private static AdvanceEntryResponse MapAdvanceToResponse(AdvanceEntry a) =>
        new(a.ID, a.EmployeeID, a.EmployeeName, a.WorkingDate, a.Amount, a.Deduction);
}
