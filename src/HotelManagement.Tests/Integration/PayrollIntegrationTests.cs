using FluentAssertions;
using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Services;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HotelManagement.Tests.Integration;

public class PayrollIntegrationTests : IDisposable
{
    private readonly HotelDbContext _context;
    private readonly AttendanceService _attendanceService;
    private readonly PayrollService _payrollService;

    public PayrollIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new HotelDbContext(options);
        _attendanceService = new AttendanceService(_context);
        _payrollService = new PayrollService(_context, _attendanceService);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task ProcessPayment_CreatesPaymentAndAdvanceEntry()
    {
        // Setup employee with attendance
        var employee = new Employee
        {
            EmployeeID = "E-123456", EmployeeName = "John Doe",
            Department = "Front Desk", Designation = "Receptionist",
            Salary = 30000m, DateOfJoining = DateTime.Today.AddYears(-1),
            Address = "123 St", MobileNo = "9876543210"
        };
        _context.Set<Employee>().Add(employee);

        // Add 25 present days with overtime
        for (int i = 1; i <= 25; i++)
        {
            _context.Set<EmployeeAttendance>().Add(new EmployeeAttendance
            {
                EmployeeID = "E-123456", EmployeeName = "John Doe",
                WorkingDate = new DateTime(2026, 1, i),
                Status = "P", Overtime = "01:30:00", Department = "Front Desk"
            });
        }
        await _context.SaveChangesAsync();

        // Add advance balance so deduction is valid
        _context.Set<AdvanceEntry>().Add(new AdvanceEntry
        {
            EmployeeID = "E-123456", EmployeeName = "John Doe",
            WorkingDate = new DateTime(2026, 1, 5), Amount = 5000m, Deduction = 0m
        });
        await _context.SaveChangesAsync();

        // Process payment with deduction
        var request = new ProcessPaymentRequest(
            "E-123456", new DateTime(2026, 1, 1), new DateTime(2026, 1, 31), 50m, 2000m);

        var result = await _payrollService.ProcessPaymentAsync(request);

        // Verify payment created
        result.Should().NotBeNull();
        result.PaymentID.Should().StartWith("SP-");
        result.EmployeeID.Should().Be("E-123456");
        result.PresentDays.Should().Be(25);
        result.Deduction.Should().Be(2000m);

        // Salary = 30000 * 25 / 30 = 25000
        result.BasicSalary.Should().Be(30000m);

        // Verify advance entry created for deduction
        var advanceEntries = await _context.Set<AdvanceEntry>()
            .Where(a => a.EmployeeID == "E-123456" && a.Deduction == 2000m)
            .ToListAsync();
        advanceEntries.Should().HaveCount(1);
    }

    [Fact]
    public async Task EmployeeDeletion_BlockedByAttendanceRecords()
    {
        var employeeService = new EmployeeService(_context);

        _context.Set<Employee>().Add(new Employee
        {
            EmployeeID = "E-123456", EmployeeName = "Test",
            Address = "Addr", MobileNo = "1234567890"
        });
        _context.Set<EmployeeAttendance>().Add(new EmployeeAttendance
        {
            EmployeeID = "E-123456", EmployeeName = "Test",
            WorkingDate = DateTime.Today, Status = "P"
        });
        await _context.SaveChangesAsync();

        var act = async () => await employeeService.DeleteAsync("E-123456");
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*attendance records exist*");

        // Employee should still exist
        var emp = await _context.Set<Employee>().FindAsync("E-123456");
        emp.Should().NotBeNull();
    }
}
