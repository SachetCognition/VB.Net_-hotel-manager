using System.Security.Cryptography;
using HotelManager.Application.Billing;
using HotelManager.Application.Interfaces;
using HotelManager.Domain.Entities;
using HotelManager.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.Web.Services.Hr;

/// <summary>
/// HR &amp; payroll service reconstructed from the legacy Human Resource forms
/// (frmEmployee_registration, frmAttendance, frmAdvance, frmEmployeePayment,
/// frmDeductionEntryRecord).
/// </summary>
public class HrPayrollService : IHrPayrollService
{
    private readonly HotelDbContext _db;

    public HrPayrollService(HotelDbContext db) => _db = db;

    // ---- Employees ----

    public async Task<IReadOnlyList<EmployeeRegistration>> GetEmployeesAsync(string? search = null)
    {
        var query = _db.Employees.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(e => e.EmployeeName!.StartsWith(search) || e.EmployeeID.StartsWith(search));
        return await query.OrderBy(e => e.EmployeeName).ThenBy(e => e.DateOfJoining).ToListAsync();
    }

    public Task<EmployeeRegistration?> GetEmployeeAsync(string employeeId) =>
        _db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.EmployeeID == employeeId);

    public async Task<string> GenerateEmployeeIdAsync()
    {
        string id;
        do
        {
            id = "E-" + GetUniqueKey(6);
        } while (await _db.Employees.AnyAsync(e => e.EmployeeID == id));
        return id;
    }

    public async Task SaveEmployeeAsync(EmployeeRegistration employee, bool isNew)
    {
        ValidateEmployee(employee);
        if (isNew)
        {
            if (string.IsNullOrWhiteSpace(employee.EmployeeID))
                employee.EmployeeID = await GenerateEmployeeIdAsync();
            _db.Employees.Add(employee);
        }
        else
        {
            var existing = await _db.Employees.FirstOrDefaultAsync(e => e.EmployeeID == employee.EmployeeID)
                ?? throw new InvalidOperationException("No record found");
            existing.EmployeeName = employee.EmployeeName;
            existing.Address = employee.Address;
            existing.MobileNo = employee.MobileNo;
            existing.Email = employee.Email;
            existing.Bloodgroup = employee.Bloodgroup;
            existing.Department = employee.Department;
            existing.Designation = employee.Designation;
            existing.DateOfJoining = employee.DateOfJoining;
            existing.Salary = employee.Salary;
            existing.BasicWorkingTime = employee.BasicWorkingTime;
        }
        await _db.SaveChangesAsync();
    }

    public async Task DeleteEmployeeAsync(string employeeId)
    {
        var inUse = await _db.Attendances.AnyAsync(a => a.EmployeeID == employeeId)
            || await _db.EmployeePayments.AnyAsync(p => p.EmployeeID == employeeId)
            || await _db.AdvanceEntries.AnyAsync(a => a.EmployeeID == employeeId);
        if (inUse)
            throw new InvalidOperationException("Unable to delete..Already in use");
        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.EmployeeID == employeeId);
        if (employee is null) return;
        _db.Employees.Remove(employee);
        await _db.SaveChangesAsync();
    }

    // ---- Attendance ----

    public async Task<IReadOnlyList<EmployeeAttendance>> GetAttendanceAsync(
        string? employeeId = null, DateTime? from = null, DateTime? to = null)
    {
        var query = _db.Attendances.AsNoTracking().Include(a => a.Employee).AsQueryable();
        if (!string.IsNullOrWhiteSpace(employeeId))
            query = query.Where(a => a.EmployeeID == employeeId);
        if (from is not null)
            query = query.Where(a => a.WorkingDate >= from.Value.Date);
        if (to is not null)
            query = query.Where(a => a.WorkingDate <= to.Value.Date);
        return await query.OrderByDescending(a => a.WorkingDate).ToListAsync();
    }

    public async Task<EmployeeAttendance> SaveAttendanceAsync(EmployeeAttendance attendance)
    {
        if (string.IsNullOrWhiteSpace(attendance.EmployeeID))
            throw new InvalidOperationException("Please select employee id");
        if (string.IsNullOrWhiteSpace(attendance.Status))
            throw new InvalidOperationException("Please select Status");

        var employee = await _db.Employees.AsNoTracking()
            .FirstOrDefaultAsync(e => e.EmployeeID == attendance.EmployeeID)
            ?? throw new InvalidOperationException("No record found");
        attendance.BasicWorkingTime ??= employee.BasicWorkingTime;
        attendance.WorkingDate = (attendance.WorkingDate ?? DateTime.Today).Date;

        if (attendance.AttendanceID == 0)
        {
            var duplicate = await _db.Attendances.AnyAsync(a =>
                a.EmployeeID == attendance.EmployeeID && a.WorkingDate == attendance.WorkingDate);
            if (duplicate)
                throw new InvalidOperationException("Employee today's attendance is already saved");
            if (attendance.Status == "A")
            {
                attendance.InTime = "00:00:00";
                attendance.OutTime = "00:00:00";
                attendance.Overtime = "00:00:00";
            }
            _db.Attendances.Add(attendance);
        }
        else
        {
            var existing = await _db.Attendances
                .FirstOrDefaultAsync(a => a.AttendanceID == attendance.AttendanceID)
                ?? throw new InvalidOperationException("No record found");
            existing.Status = attendance.Status;
            existing.InTime = attendance.InTime;
            existing.OutTime = attendance.OutTime;
            existing.Overtime = attendance.Overtime;
            existing.BasicWorkingTime = attendance.BasicWorkingTime;
            attendance = existing;
        }
        await _db.SaveChangesAsync();
        return attendance;
    }

    public async Task DeleteAttendanceAsync(int attendanceId)
    {
        var attendance = await _db.Attendances.FirstOrDefaultAsync(a => a.AttendanceID == attendanceId);
        if (attendance is null) return;
        _db.Attendances.Remove(attendance);
        await _db.SaveChangesAsync();
    }

    // ---- Advances & deductions ----

    public async Task<IReadOnlyList<AdvanceEntry>> GetAdvanceEntriesAsync(string? employeeId = null)
    {
        var query = _db.AdvanceEntries.AsNoTracking().Include(a => a.Employee).AsQueryable();
        if (!string.IsNullOrWhiteSpace(employeeId))
            query = query.Where(a => a.EmployeeID == employeeId);
        return await query.OrderByDescending(a => a.WorkingDate).ToListAsync();
    }

    public async Task<AdvanceEntry> SaveAdvanceEntryAsync(AdvanceEntry entry)
    {
        if (string.IsNullOrWhiteSpace(entry.EmployeeID))
            throw new InvalidOperationException("Please select employee id");
        if ((entry.Amount ?? 0) <= 0 && (entry.Deduction ?? 0) <= 0)
            throw new InvalidOperationException("Please enter Amount");
        entry.WorkingDate = (entry.WorkingDate ?? DateTime.Today).Date;
        entry.Amount ??= 0;
        entry.Deduction ??= 0;

        if (entry.ID == 0 && entry.Amount > 0)
        {
            var duplicate = await _db.AdvanceEntries.AnyAsync(a =>
                a.EmployeeID == entry.EmployeeID && a.WorkingDate == entry.WorkingDate && a.Amount > 0);
            if (duplicate)
                throw new InvalidOperationException("advance is already paid to employee today");
        }
        _db.AdvanceEntries.Add(entry);
        await _db.SaveChangesAsync();
        return entry;
    }

    public async Task DeleteAdvanceEntryAsync(int id)
    {
        var entry = await _db.AdvanceEntries.FirstOrDefaultAsync(a => a.ID == id);
        if (entry is null) return;
        _db.AdvanceEntries.Remove(entry);
        await _db.SaveChangesAsync();
    }

    public async Task<int> GetOutstandingAdvanceAsync(string employeeId, DateTime from, DateTime to)
    {
        var entries = await _db.AdvanceEntries.AsNoTracking()
            .Where(a => a.EmployeeID == employeeId
                && a.WorkingDate >= from.Date && a.WorkingDate <= to.Date)
            .Select(a => new { Amount = a.Amount ?? 0, Deduction = a.Deduction ?? 0 })
            .ToListAsync();
        return BillingCalculator.ComputeOutstandingAdvance(entries.Select(e => (e.Amount, e.Deduction)));
    }

    public async Task<TimeSpan> GetTotalOvertimeAsync(string employeeId, DateTime from, DateTime to)
    {
        var overtimes = await _db.Attendances.AsNoTracking()
            .Where(a => a.EmployeeID == employeeId
                && a.WorkingDate >= from.Date && a.WorkingDate <= to.Date)
            .Select(a => a.Overtime)
            .ToListAsync();

        // Legacy frmEmployeePayment sums the hour/minute/second components
        // separately and rebuilds a TimeSpan from them.
        int hours = 0, minutes = 0, seconds = 0;
        foreach (var value in overtimes)
        {
            if (TimeSpan.TryParse(value, out var ts))
            {
                hours += ts.Hours;
                minutes += ts.Minutes;
                seconds += ts.Seconds;
            }
        }
        return new TimeSpan(hours, minutes, seconds);
    }

    public Task<int> GetPresentDaysAsync(string employeeId, DateTime from, DateTime to) =>
        _db.Attendances.AsNoTracking()
            .CountAsync(a => a.EmployeeID == employeeId && a.Status == "P"
                && a.WorkingDate >= from.Date && a.WorkingDate <= to.Date);

    // ---- Payments ----

    public async Task<IReadOnlyList<EmployeePayment>> GetPaymentsAsync(string? employeeId = null)
    {
        var query = _db.EmployeePayments.AsNoTracking().Include(p => p.Employee).AsQueryable();
        if (!string.IsNullOrWhiteSpace(employeeId))
            query = query.Where(p => p.EmployeeID == employeeId);
        return await query.OrderByDescending(p => p.PaymentDate).ToListAsync();
    }

    public async Task<string> GeneratePaymentIdAsync()
    {
        string id;
        do
        {
            id = "SP-" + GetUniqueKey(9);
        } while (await _db.EmployeePayments.AnyAsync(p => p.PaymentID == id));
        return id;
    }

    public async Task<EmployeePayment> RunPaymentAsync(EmployeePayment payment)
    {
        if (string.IsNullOrWhiteSpace(payment.EmployeeID))
            throw new InvalidOperationException("Please select employee id");
        var employee = await _db.Employees.AsNoTracking()
            .FirstOrDefaultAsync(e => e.EmployeeID == payment.EmployeeID)
            ?? throw new InvalidOperationException("No record found");

        var from = (payment.DateFrom ?? DateTime.Today).Date;
        var to = (payment.DateTo ?? DateTime.Today).Date;
        var paymentDate = (payment.PaymentDate ?? DateTime.Today).Date;

        var alreadyPaid = await _db.EmployeePayments.AnyAsync(p =>
            p.EmployeeID == payment.EmployeeID && p.PaymentDate == paymentDate);
        if (alreadyPaid)
            throw new InvalidOperationException("Employee is already paid today");

        // Legacy computes the outstanding advance over the whole ledger.
        var ledger = await _db.AdvanceEntries.AsNoTracking()
            .Where(a => a.EmployeeID == payment.EmployeeID)
            .Select(a => new { Amount = a.Amount ?? 0, Deduction = a.Deduction ?? 0 })
            .ToListAsync();
        var outstanding = BillingCalculator.ComputeOutstandingAdvance(ledger.Select(e => (e.Amount, e.Deduction)));

        var presentDays = await GetPresentDaysAsync(payment.EmployeeID, from, to);
        var overtime = await GetTotalOvertimeAsync(payment.EmployeeID, from, to);
        var result = BillingCalculator.ComputePayroll(
            employee.Salary ?? 0, presentDays, overtime,
            payment.OvertimeRate ?? 0, outstanding, payment.Deduction ?? 0);

        payment.PaymentID = string.IsNullOrWhiteSpace(payment.PaymentID)
            ? await GeneratePaymentIdAsync()
            : payment.PaymentID;
        payment.DateFrom = from;
        payment.DateTo = to;
        payment.PaymentDate = paymentDate;
        payment.PresentDays = presentDays;
        payment.Salary = result.Salary;
        payment.Advance = result.Advance;
        payment.Deduction = result.Deduction;
        payment.Overtime = overtime.ToString();
        payment.OverTimeAmount = result.OvertimeAmount;
        payment.NetPay = result.NetPay;

        _db.EmployeePayments.Add(payment);
        // Legacy inserts a deduction ledger row alongside each payment.
        _db.AdvanceEntries.Add(new AdvanceEntry
        {
            EmployeeID = payment.EmployeeID,
            WorkingDate = paymentDate,
            Amount = 0,
            Deduction = result.Deduction
        });
        await _db.SaveChangesAsync();
        return payment;
    }

    public async Task DeletePaymentAsync(string paymentId)
    {
        var payment = await _db.EmployeePayments.FirstOrDefaultAsync(p => p.PaymentID == paymentId);
        if (payment is null) return;
        _db.EmployeePayments.Remove(payment);
        await _db.SaveChangesAsync();
    }

    // ---- Helpers ----

    private static void ValidateEmployee(EmployeeRegistration employee)
    {
        if (string.IsNullOrWhiteSpace(employee.EmployeeName))
            throw new InvalidOperationException("Please enter employee full name");
        if (string.IsNullOrWhiteSpace(employee.Address))
            throw new InvalidOperationException("Please enter address");
        if (string.IsNullOrWhiteSpace(employee.MobileNo))
            throw new InvalidOperationException("Please enter mobile no.");
        if (string.IsNullOrWhiteSpace(employee.Department))
            throw new InvalidOperationException("Please enter department");
        if (string.IsNullOrWhiteSpace(employee.Designation))
            throw new InvalidOperationException("Please enter designation");
        if (employee.Salary is null)
            throw new InvalidOperationException("Please enter basic Salary");
        if (string.IsNullOrWhiteSpace(employee.BasicWorkingTime))
            throw new InvalidOperationException("Please enter basic working time");
    }

    /// <summary>Legacy GetUniqueKey: random digits 1-9.</summary>
    private static string GetUniqueKey(int maxSize)
    {
        const string chars = "123456789";
        var data = new byte[maxSize];
        RandomNumberGenerator.Fill(data);
        return new string(data.Select(b => chars[b % chars.Length]).ToArray());
    }
}
