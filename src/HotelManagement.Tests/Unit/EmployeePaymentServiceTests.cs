using HotelManagement.Core.Services;
using Xunit;

namespace HotelManagement.Tests.Unit;

/// <summary>
/// Category 2: Unit Tests — Employee Calculations (~20 tests)
/// Tests exact employee payment logic from frmEmployeePayment.vb.
/// </summary>
public class EmployeePaymentServiceTests
{
    private readonly EmployeePaymentService _svc = new();

    // TC-EMP-001: Salary = (BasicSalary × PresentDays) / 30
    [Fact]
    public void CalculateSalary_Basic()
    {
        Assert.Equal(10000, _svc.CalculateSalary(30000m, 10));
    }

    // TC-EMP-002: OvertimeAmount = (TotalMinutes × Rate) / 60
    [Fact]
    public void CalculateOvertimeAmount_Basic()
    {
        Assert.Equal(500, _svc.CalculateOvertimeAmount(120, 250m));
    }

    // TC-EMP-003: NetPay = Salary + OvertimeAmount - Deduction
    [Fact]
    public void CalculateNetPay_Basic()
    {
        Assert.Equal(9500, _svc.CalculateNetPay(10000m, 500m, 1000m));
    }

    // TC-EMP-004: Overtime = OutTime - InTime - BasicWorkingTime
    [Fact]
    public void CalculateOvertime_Basic()
    {
        var result = _svc.CalculateOvertime(
            new TimeSpan(18, 0, 0), new TimeSpan(9, 0, 0), new TimeSpan(8, 0, 0));
        Assert.Equal(new TimeSpan(1, 0, 0), result);
    }

    // TC-EMP-005: Zero present days
    [Fact]
    public void CalculateSalary_ZeroPresentDays_ReturnsZero()
    {
        Assert.Equal(0, _svc.CalculateSalary(30000m, 0));
    }

    // TC-EMP-006: Full 30 days
    [Fact]
    public void CalculateSalary_Full30Days()
    {
        Assert.Equal(30000, _svc.CalculateSalary(30000m, 30));
    }

    // TC-EMP-007: 15 days (half month)
    [Fact]
    public void CalculateSalary_HalfMonth()
    {
        Assert.Equal(15000, _svc.CalculateSalary(30000m, 15));
    }

    // TC-EMP-008: Zero overtime
    [Fact]
    public void CalculateOvertimeAmount_ZeroMinutes()
    {
        Assert.Equal(0, _svc.CalculateOvertimeAmount(0, 250m));
    }

    // TC-EMP-009: Negative overtime returns zero
    [Fact]
    public void CalculateOvertime_NegativeReturnsZero()
    {
        var result = _svc.CalculateOvertime(
            new TimeSpan(16, 0, 0), new TimeSpan(9, 0, 0), new TimeSpan(8, 0, 0));
        Assert.True(result >= TimeSpan.Zero);
    }

    // TC-EMP-010: Zero deduction
    [Fact]
    public void CalculateNetPay_ZeroDeduction()
    {
        Assert.Equal(10500, _svc.CalculateNetPay(10000m, 500m, 0m));
    }

    // TC-EMP-011: Large salary calculation
    [Fact]
    public void CalculateSalary_LargeValues()
    {
        Assert.Equal(50000, _svc.CalculateSalary(100000m, 15));
    }

    // TC-EMP-012: Overtime with 30 minutes
    [Fact]
    public void CalculateOvertimeAmount_30Minutes()
    {
        Assert.Equal(125, _svc.CalculateOvertimeAmount(30, 250m));
    }

    // TC-EMP-013: Full ComputePayment
    [Fact]
    public void ComputePayment_TypicalValues()
    {
        var result = _svc.ComputePayment(30000m, 20, new TimeSpan(2, 0, 0), 200m, 1000m);
        Assert.Equal(20000, result.Salary);
        Assert.True(result.OvertimeAmount > 0);
        Assert.True(result.NetPay > 0);
    }

    // TC-EMP-014: ComputePayment zero overtime
    [Fact]
    public void ComputePayment_ZeroOvertime()
    {
        var result = _svc.ComputePayment(30000m, 25, TimeSpan.Zero, 200m, 0m);
        Assert.Equal(25000, result.Salary);
        Assert.Equal(0, result.OvertimeAmount);
    }

    // TC-EMP-015: Overtime exact working time = 0 overtime
    [Fact]
    public void CalculateOvertime_ExactWorkingTime_ReturnsZero()
    {
        var result = _svc.CalculateOvertime(
            new TimeSpan(17, 0, 0), new TimeSpan(9, 0, 0), new TimeSpan(8, 0, 0));
        Assert.Equal(TimeSpan.Zero, result);
    }

    // TC-EMP-016: One present day
    [Fact]
    public void CalculateSalary_OneDay()
    {
        Assert.Equal(1000, _svc.CalculateSalary(30000m, 1));
    }

    // TC-EMP-017: NetPay can be negative if deduction > earnings
    [Fact]
    public void CalculateNetPay_NegativeResult()
    {
        int result = _svc.CalculateNetPay(1000m, 0m, 2000m);
        Assert.Equal(-1000, result);
    }

    // TC-EMP-018: Overtime rate zero
    [Fact]
    public void CalculateOvertimeAmount_ZeroRate()
    {
        Assert.Equal(0, _svc.CalculateOvertimeAmount(120, 0m));
    }

    // TC-EMP-019: 31 present days
    [Fact]
    public void CalculateSalary_31Days()
    {
        int result = _svc.CalculateSalary(30000m, 31);
        Assert.Equal(31000, result);
    }

    // TC-EMP-020: ComputePayment with all zeros
    [Fact]
    public void ComputePayment_AllZeros()
    {
        var result = _svc.ComputePayment(0m, 0, TimeSpan.Zero, 0m, 0m);
        Assert.Equal(0, result.Salary);
        Assert.Equal(0, result.OvertimeAmount);
        Assert.Equal(0, result.NetPay);
    }
}
