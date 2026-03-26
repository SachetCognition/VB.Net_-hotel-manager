using FluentAssertions;
using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Services;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HotelManagement.Tests.Unit;

public class PayrollServiceTests : IDisposable
{
    private readonly HotelDbContext _context;
    private readonly AttendanceService _attendanceService;
    private readonly PayrollService _service;

    public PayrollServiceTests()
    {
        var options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new HotelDbContext(options);
        _attendanceService = new AttendanceService(_context);
        _service = new PayrollService(_context, _attendanceService);
    }

    public void Dispose() => _context.Dispose();

    // --- CalculateSalary Tests ---

    [Fact]
    public void CalculateSalary_FullMonth_ReturnsBasicSalary()
    {
        // Salary = (BasicSalary * PresentDays) / 30
        _service.CalculateSalary(30000m, 30).Should().Be(30000);
    }

    [Fact]
    public void CalculateSalary_HalfMonth_ReturnsHalf()
    {
        _service.CalculateSalary(30000m, 15).Should().Be(15000);
    }

    [Fact]
    public void CalculateSalary_ZeroDays_ReturnsZero()
    {
        _service.CalculateSalary(30000m, 0).Should().Be(0);
    }

    [Fact]
    public void CalculateSalary_OneDayPresent_CalculatesCorrectly()
    {
        // 30000 * 1 / 30 = 1000
        _service.CalculateSalary(30000m, 1).Should().Be(1000);
    }

    [Fact]
    public void CalculateSalary_NonEvenDivision_Truncates()
    {
        // 25000 * 7 / 30 = 5833.33 -> truncated to 5833
        _service.CalculateSalary(25000m, 7).Should().Be(5833);
    }

    // --- CalculateOvertimeAmount Tests ---

    [Fact]
    public void CalculateOvertimeAmount_StandardOvertime_CalculatesCorrectly()
    {
        // OvertimeAmount = (TotalOvertimeMinutes * Rate) / 60
        // 120 minutes * 100 rate / 60 = 200
        _service.CalculateOvertimeAmount(120, 100m).Should().Be(200);
    }

    [Fact]
    public void CalculateOvertimeAmount_ZeroMinutes_ReturnsZero()
    {
        _service.CalculateOvertimeAmount(0, 100m).Should().Be(0);
    }

    [Fact]
    public void CalculateOvertimeAmount_PartialHour_Truncates()
    {
        // 45 min * 100 / 60 = 75
        _service.CalculateOvertimeAmount(45, 100m).Should().Be(75);
    }

    [Fact]
    public void CalculateOvertimeAmount_ZeroRate_ReturnsZero()
    {
        _service.CalculateOvertimeAmount(120, 0m).Should().Be(0);
    }

    // --- CalculateNetPay Tests ---

    [Fact]
    public void CalculateNetPay_StandardValues_CalculatesCorrectly()
    {
        // NetPay = Salary + OvertimeAmount - Deduction
        _service.CalculateNetPay(30000m, 5000m, 2000m).Should().Be(33000m);
    }

    [Fact]
    public void CalculateNetPay_NoOvertime_SubtractsDeduction()
    {
        _service.CalculateNetPay(30000m, 0m, 2000m).Should().Be(28000m);
    }

    [Fact]
    public void CalculateNetPay_NoDeduction_AddsSalaryAndOvertime()
    {
        _service.CalculateNetPay(30000m, 5000m, 0m).Should().Be(35000m);
    }

    [Fact]
    public void CalculateNetPay_DeductionExceedsSalary_NegativeResult()
    {
        _service.CalculateNetPay(1000m, 0m, 5000m).Should().Be(-4000m);
    }

    // --- ProcessPaymentAsync Tests ---

    [Fact]
    public async Task ProcessPaymentAsync_ValidRequest_CreatesPayment()
    {
        var employee = new Employee
        {
            EmployeeID = "E-123456", EmployeeName = "Jane Doe",
            Department = "HR", Designation = "Manager",
            Salary = 30000m, DateOfJoining = DateTime.Today.AddYears(-1)
        };
        _context.Set<Employee>().Add(employee);

        // Add attendance records
        for (int i = 1; i <= 20; i++)
        {
            _context.Set<EmployeeAttendance>().Add(new EmployeeAttendance
            {
                EmployeeID = "E-123456", EmployeeName = "Jane Doe",
                WorkingDate = new DateTime(2026, 1, i),
                Status = "P", Overtime = "01:00:00", Department = "HR"
            });
        }
        await _context.SaveChangesAsync();

        var request = new ProcessPaymentRequest(
            "E-123456", new DateTime(2026, 1, 1), new DateTime(2026, 1, 31), 100m, 0m);

        var result = await _service.ProcessPaymentAsync(request);

        result.Should().NotBeNull();
        result.EmployeeID.Should().Be("E-123456");
        result.PaymentID.Should().StartWith("SP-");
        result.PresentDays.Should().Be(20);
        result.NetPay.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ProcessPaymentAsync_EmptyEmployeeID_ThrowsArgumentException()
    {
        var request = new ProcessPaymentRequest("", DateTime.Today, DateTime.Today, 100m, 0m);
        var act = async () => await _service.ProcessPaymentAsync(request);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*employee*");
    }

    [Fact]
    public async Task ProcessPaymentAsync_NonExistentEmployee_ThrowsKeyNotFound()
    {
        var request = new ProcessPaymentRequest("E-999999", DateTime.Today, DateTime.Today, 100m, 0m);
        var act = async () => await _service.ProcessPaymentAsync(request);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ProcessPaymentAsync_WithDeduction_CreatesAdvanceEntry()
    {
        var employee = new Employee
        {
            EmployeeID = "E-123456", EmployeeName = "Jane Doe",
            Salary = 30000m, DateOfJoining = DateTime.Today.AddYears(-1)
        };
        _context.Set<Employee>().Add(employee);

        // Must have advance balance >= deduction
        _context.Set<AdvanceEntry>().Add(new AdvanceEntry
        {
            EmployeeID = "E-123456", EmployeeName = "Jane Doe",
            WorkingDate = DateTime.Today.AddDays(-5), Amount = 10000m, Deduction = 0m
        });

        // Add attendance so salary > 0 and NetPay > 0
        for (int i = 1; i <= 20; i++)
        {
            _context.Set<EmployeeAttendance>().Add(new EmployeeAttendance
            {
                EmployeeID = "E-123456", EmployeeName = "Jane Doe",
                WorkingDate = DateTime.Today.AddDays(-30 + i),
                Status = "P", Overtime = "00:00:00", Department = "HR"
            });
        }
        await _context.SaveChangesAsync();

        var request = new ProcessPaymentRequest(
            "E-123456", DateTime.Today.AddDays(-30), DateTime.Today, 100m, 5000m);

        await _service.ProcessPaymentAsync(request);

        var advances = await _context.Set<AdvanceEntry>()
            .Where(a => a.EmployeeID == "E-123456" && a.Deduction == 5000m)
            .ToListAsync();
        advances.Should().HaveCount(1);
    }

    [Fact]
    public async Task ProcessPaymentAsync_DeductionExceedsAdvance_ThrowsArgumentException()
    {
        var employee = new Employee
        {
            EmployeeID = "E-123456", EmployeeName = "Jane Doe",
            Salary = 30000m, DateOfJoining = DateTime.Today.AddYears(-1)
        };
        _context.Set<Employee>().Add(employee);
        await _context.SaveChangesAsync();

        // No advance balance, but trying to deduct 5000
        var request = new ProcessPaymentRequest(
            "E-123456", DateTime.Today.AddDays(-30), DateTime.Today, 100m, 5000m);

        var act = async () => await _service.ProcessPaymentAsync(request);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*advance amount*");
    }

    [Fact]
    public async Task ProcessPaymentAsync_ZeroDeduction_NoAdvanceEntry()
    {
        var employee = new Employee
        {
            EmployeeID = "E-123456", EmployeeName = "Jane Doe",
            Salary = 30000m, DateOfJoining = DateTime.Today.AddYears(-1)
        };
        _context.Set<Employee>().Add(employee);

        // Add attendance so salary > 0 and NetPay > 0
        for (int i = 1; i <= 10; i++)
        {
            _context.Set<EmployeeAttendance>().Add(new EmployeeAttendance
            {
                EmployeeID = "E-123456", EmployeeName = "Jane Doe",
                WorkingDate = DateTime.Today.AddDays(-30 + i),
                Status = "P", Overtime = "00:00:00", Department = "HR"
            });
        }
        await _context.SaveChangesAsync();

        var request = new ProcessPaymentRequest(
            "E-123456", DateTime.Today.AddDays(-30), DateTime.Today, 100m, 0m);

        await _service.ProcessPaymentAsync(request);

        var advances = await _context.Set<AdvanceEntry>()
            .Where(a => a.EmployeeID == "E-123456" && a.Deduction > 0)
            .ToListAsync();
        advances.Should().BeEmpty();
    }

    // --- GetAdvanceBalanceAsync Tests ---

    [Fact]
    public async Task GetAdvanceBalanceAsync_NoEntries_ReturnsZero()
    {
        var balance = await _service.GetAdvanceBalanceAsync("E-123456");
        balance.Should().Be(0m);
    }

    [Fact]
    public async Task GetAdvanceBalanceAsync_WithAdvancesAndDeductions_CalculatesCorrectly()
    {
        _context.Set<AdvanceEntry>().Add(new AdvanceEntry
        {
            EmployeeID = "E-123456", EmployeeName = "Test",
            WorkingDate = DateTime.Today, Amount = 5000m, Deduction = 0m
        });
        _context.Set<AdvanceEntry>().Add(new AdvanceEntry
        {
            EmployeeID = "E-123456", EmployeeName = "Test",
            WorkingDate = DateTime.Today, Amount = 0m, Deduction = 2000m
        });
        await _context.SaveChangesAsync();

        var balance = await _service.GetAdvanceBalanceAsync("E-123456");
        balance.Should().Be(3000m); // 5000 - 2000
    }
}
