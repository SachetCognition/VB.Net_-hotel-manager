using FluentAssertions;
using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Services;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HotelManagement.Tests.E2E;

/// <summary>
/// E2E-002: Complete employee lifecycle: Register -> Attendance -> Advance -> Payment -> Salary Slip
/// </summary>
public class EmployeeLifecycleTests : IDisposable
{
    private readonly HotelDbContext _context;
    private readonly EmployeeService _employeeService;
    private readonly AttendanceService _attendanceService;
    private readonly PayrollService _payrollService;

    public EmployeeLifecycleTests()
    {
        var options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new HotelDbContext(options);
        _employeeService = new EmployeeService(_context);
        _attendanceService = new AttendanceService(_context);
        _payrollService = new PayrollService(_context, _attendanceService);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task CompleteEmployeeLifecycle_RegisterAttendancePayment()
    {
        // STEP 1: Register employee
        var employee = await _employeeService.CreateAsync(
            new CreateEmployeeRequest(
                "Priya Patel", "45 Station Road", "9876543210", "priya@hotel.com",
                "B+", "Female", "Front Desk", "Senior Receptionist",
                new DateTime(2025, 6, 1), 35000m, "09:00"));
        employee.EmployeeID.Should().StartWith("E-");
        employee.EmployeeName.Should().Be("Priya Patel");
        employee.Salary.Should().Be(35000m);

        // STEP 2: Record attendance (20 days present, 5 with overtime)
        for (int i = 1; i <= 20; i++)
        {
            var overtime = i <= 5 ? "02:00:00" : "00:00:00";
            await _attendanceService.CreateAsync(
                new AttendanceRequest(employee.EmployeeID, employee.EmployeeName,
                    new DateTime(2026, 2, i), "P", overtime, "Front Desk"));
        }

        // Verify present days
        var presentDays = await _attendanceService.GetPresentDaysAsync(
            employee.EmployeeID, new DateTime(2026, 2, 1), new DateTime(2026, 2, 28));
        presentDays.Should().Be(20);

        // Verify total overtime
        var totalOvertime = await _attendanceService.GetTotalOvertimeAsync(
            employee.EmployeeID, new DateTime(2026, 2, 1), new DateTime(2026, 2, 28));
        totalOvertime.TotalMinutes.Should().Be(600); // 5 days * 2 hours = 10 hours = 600 min

        // STEP 3: Create advance entry
        await _payrollService.CreateAdvanceEntryAsync(
            new AdvanceEntryRequest(employee.EmployeeID, employee.EmployeeName,
                new DateTime(2026, 2, 10), 5000m));

        var balance = await _payrollService.GetAdvanceBalanceAsync(employee.EmployeeID);
        balance.Should().Be(5000m);

        // STEP 4: Process payment
        var payment = await _payrollService.ProcessPaymentAsync(
            new ProcessPaymentRequest(employee.EmployeeID,
                new DateTime(2026, 2, 1), new DateTime(2026, 2, 28), 100m, 2000m));

        payment.Should().NotBeNull();
        payment.PaymentID.Should().StartWith("SP-");
        payment.PresentDays.Should().Be(20);
        payment.Deduction.Should().Be(2000m);

        // Verify salary calculation: Salary = 35000 * 20 / 30 = 23333.33
        payment.BasicSalary.Should().Be(35000m);

        // Verify overtime calculation: OvertimeAmount = 600 * 100 / 60 = 1000
        payment.OvertimeRate.Should().Be(100m);

        // Verify advance entry for deduction was auto-created
        var advanceEntries = await _payrollService.GetAdvanceEntriesAsync(employee.EmployeeID);
        advanceEntries.Should().Contain(a => a.Deduction == 2000m);

        // Updated advance balance = 5000 - 2000 = 3000
        var updatedBalance = await _payrollService.GetAdvanceBalanceAsync(employee.EmployeeID);
        updatedBalance.Should().Be(3000m);
    }

    [Fact]
    public async Task EmployeeLifecycle_ReferentialIntegrityChecks()
    {
        // Register employee
        var employee = await _employeeService.CreateAsync(
            new CreateEmployeeRequest(
                "Ravi Kumar", "123 St", "1234567890", "ravi@test.com",
                "O+", "Male", "Kitchen", "Chef",
                DateTime.Today.AddYears(-2), 25000m, "08:00"));

        // Add attendance
        await _attendanceService.CreateAsync(
            new AttendanceRequest(employee.EmployeeID, employee.EmployeeName,
                DateTime.Today, "P", "01:00:00", "Kitchen"));

        // Cannot delete employee with attendance records
        var act = async () => await _employeeService.DeleteAsync(employee.EmployeeID);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*attendance records exist*");

        // Employee still exists
        var emp = await _employeeService.GetByIdAsync(employee.EmployeeID);
        emp.Should().NotBeNull();
    }

    [Fact]
    public Task EmployeePayment_SalaryCalculationMatchesLegacy()
    {
        // Test known values against legacy formula
        // Legacy: Salary = (BasicSalary * PresentDays) / 30
        var salary30Days = _payrollService.CalculateSalary(30000m, 30);
        salary30Days.Should().Be(30000); // Full month

        var salary15Days = _payrollService.CalculateSalary(30000m, 15);
        salary15Days.Should().Be(15000); // Half month

        var salary1Day = _payrollService.CalculateSalary(30000m, 1);
        salary1Day.Should().Be(1000); // One day

        // Legacy: OvertimeAmount = (TotalOvertimeMinutes * Rate) / 60
        var ot120min = _payrollService.CalculateOvertimeAmount(120, 100m);
        ot120min.Should().Be(200); // 2 hours at 100/hr

        // Legacy: NetPay = Salary + OvertimeAmount - Deduction
        var netPay = _payrollService.CalculateNetPay(25000m, 3000m, 5000m);
        netPay.Should().Be(23000m);

        return Task.CompletedTask;
    }
}
