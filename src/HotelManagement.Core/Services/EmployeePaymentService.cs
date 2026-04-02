namespace HotelManagement.Core.Services;

/// <summary>
/// Preserves exact employee payment calculation logic from frmEmployeePayment.vb.
/// </summary>
public class EmployeePaymentService
{
    /// <summary>
    /// Calculate salary based on basic salary and present days.
    /// Salary = CInt((BasicSalary × PresentDays) / 30)
    /// </summary>
    public int CalculateSalary(decimal basicSalary, int presentDays)
    {
        return (int)((basicSalary * presentDays) / 30m);
    }

    /// <summary>
    /// Calculate overtime amount.
    /// OvertimeAmount = CInt((TotalMinutes × Rate) / 60)
    /// </summary>
    public int CalculateOvertimeAmount(double totalMinutes, decimal rate)
    {
        return (int)((totalMinutes * (double)rate) / 60.0);
    }

    /// <summary>
    /// Calculate net pay.
    /// NetPay = Salary + OvertimeAmount - Deduction
    /// </summary>
    public int CalculateNetPay(decimal salary, decimal overtimeAmount, decimal deduction)
    {
        return (int)(salary + overtimeAmount - deduction);
    }

    /// <summary>
    /// Calculate overtime duration.
    /// Overtime = OutTime - InTime - BasicWorkingTime
    /// </summary>
    public TimeSpan CalculateOvertime(TimeSpan outTime, TimeSpan inTime, TimeSpan basicWorkingTime)
    {
        var worked = outTime - inTime;
        var overtime = worked - basicWorkingTime;
        return overtime < TimeSpan.Zero ? TimeSpan.Zero : overtime;
    }

    /// <summary>
    /// Full employee payment computation matching frmEmployeePayment.vb.
    /// </summary>
    public EmployeePaymentResult ComputePayment(
        decimal basicSalary, int presentDays,
        TimeSpan totalOvertime, decimal overtimeRate,
        decimal deduction)
    {
        int salary = CalculateSalary(basicSalary, presentDays);
        int overtimeAmount = CalculateOvertimeAmount(totalOvertime.TotalMinutes, overtimeRate);
        int netPay = CalculateNetPay(salary, overtimeAmount, deduction);

        return new EmployeePaymentResult
        {
            Salary = salary,
            OvertimeAmount = overtimeAmount,
            NetPay = netPay
        };
    }
}

public class EmployeePaymentResult
{
    public int Salary { get; set; }
    public int OvertimeAmount { get; set; }
    public int NetPay { get; set; }
}
