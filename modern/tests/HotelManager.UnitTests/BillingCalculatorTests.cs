using HotelManager.Application.Billing;

namespace HotelManager.UnitTests;

public class BillingCalculatorStayTests
{
    [Fact]
    public void SameDayStay_CountsAsOneDay()
    {
        var d = new DateTime(2024, 5, 1);
        var bill = BillingCalculator.ComputeStay(d, d, 100, 0, 0, 0, 0, 0);
        Assert.Equal(1, bill.NoOfDays);
        Assert.Equal(100, bill.TotalCharges);
    }

    [Fact]
    public void MultiDayStay_ComputesLegacyMath()
    {
        // 3 nights @ 100, other 50, discount 10%, service tax 12.36%, luxury 5%, paid 200
        var bill = BillingCalculator.ComputeStay(
            new DateTime(2024, 5, 1), new DateTime(2024, 5, 4),
            100, 50, 10, 12.36, 5, 200);
        Assert.Equal(3, bill.NoOfDays);
        Assert.Equal(300, bill.TotalCharges);
        Assert.Equal(35, bill.Discount);            // CInt(350*10/100)
        Assert.Equal(315, bill.SubTotal);           // 350-35
        Assert.Equal(38.93, bill.ServiceTaxAmount); // round(315*12.36%,2)
        Assert.Equal(17.70, bill.LuxuryTaxAmount);  // round((315+38.93)*5%,2)
        Assert.Equal(372, bill.GrandTotal);         // CInt(315+38.93+17.70)=CInt(371.63)
        Assert.Equal(172, bill.Balance);
    }

    [Fact]
    public void Cess_MatchesLegacyDerivation()
    {
        var bill = BillingCalculator.ComputeStay(
            new DateTime(2024, 5, 1), new DateTime(2024, 5, 2),
            1000, 0, 0, 12.36, 0, 0);
        Assert.Equal(0.25, bill.EducessTax);         // round(12.36*2/100,2)
        Assert.Equal(0.12, bill.HEduCessTax);        // round(12.36*1/100,2)
        Assert.Equal(Math.Round(bill.ServiceTaxAmount * 0.25 / 100, 2), bill.EducessTaxAmount);
        Assert.Equal(Math.Round(bill.ServiceTaxAmount * 0.12 / 100, 2), bill.HEduCessTaxAmount);
    }

    [Fact]
    public void NegativeRange_ClampsToOneDay()
    {
        var bill = BillingCalculator.ComputeStay(
            new DateTime(2024, 5, 4), new DateTime(2024, 5, 1), 100, 0, 0, 0, 0, 0);
        Assert.Equal(1, bill.NoOfDays);
    }

    [Fact]
    public void HallOrGarden_ComputesLegacyMath()
    {
        var bill = BillingCalculator.ComputeHallOrGarden(
            new DateTime(2024, 6, 1), new DateTime(2024, 6, 3),
            500, 100, 5, 10, 8, 500);
        Assert.Equal(2, bill.NoOfDays);
        Assert.Equal(1000, bill.TotalCharges);
        Assert.Equal(55, bill.Discount);           // CInt(1100*5/100)
        Assert.Equal(1045, bill.SubTotal);
        Assert.Equal(104.5, bill.ServiceTaxAmount);
        Assert.Equal(91.96, bill.LuxuryTaxAmount); // round(1149.5*8%,2)
        Assert.Equal(1241, bill.GrandTotal);       // CInt(1241.46)
        Assert.Equal(741, bill.Balance);
    }

    [Fact]
    public void HallOrGarden_SameDay_CountsAsOneDay()
    {
        var d = new DateTime(2024, 6, 1);
        var bill = BillingCalculator.ComputeHallOrGarden(d, d, 500, 0, 0, 0, 0, 0);
        Assert.Equal(1, bill.NoOfDays);
        Assert.Equal(500, bill.TotalCharges);
    }

    [Fact]
    public void HallAndGarden_SameDay_CountsAsOneDayEach()
    {
        var d = new DateTime(2024, 6, 1);
        var b = BillingCalculator.ComputeHallAndGarden(d, d, 500, d, d, 400, 0, 0, 0, 0, 0);
        Assert.Equal(1, b.DaysHall);
        Assert.Equal(1, b.DaysGarden);
        Assert.Equal(500, b.TotalChargesHall);
        Assert.Equal(400, b.TotalChargesGarden);
    }

    [Fact]
    public void HallAndGarden_CombinesBothVenues()
    {
        var b = BillingCalculator.ComputeHallAndGarden(
            new DateTime(2024, 6, 1), new DateTime(2024, 6, 2), 500,
            new DateTime(2024, 6, 1), new DateTime(2024, 6, 3), 400,
            0, 0, 0, 0, 0);
        Assert.Equal(1, b.DaysHall);
        Assert.Equal(500, b.TotalChargesHall);
        Assert.Equal(2, b.DaysGarden);
        Assert.Equal(800, b.TotalChargesGarden);
        Assert.Equal(1300, b.Bill.SubTotal);
        Assert.Equal(1300, b.Bill.GrandTotal);
    }
}

public class BillingCalculatorPayrollTests
{
    [Fact]
    public void Salary_IsProRatedOver30Days()
    {
        Assert.Equal(15000, BillingCalculator.ComputeSalary(15000, 30));
        Assert.Equal(7500, BillingCalculator.ComputeSalary(15000, 15));
        Assert.Equal(500, BillingCalculator.ComputeSalary(15000, 1));
    }

    [Fact]
    public void Overtime_UsesTotalMinutesTimesRateOver60()
    {
        Assert.Equal(100, BillingCalculator.ComputeOvertimeAmount(TimeSpan.FromHours(2), 50));
        Assert.Equal(25, BillingCalculator.ComputeOvertimeAmount(TimeSpan.FromMinutes(30), 50));
    }

    [Fact]
    public void OutstandingAdvance_IsAmountMinusDeductions()
    {
        var entries = new[] { (1000, 0), (500, 0), (0, 300) };
        Assert.Equal(1200, BillingCalculator.ComputeOutstandingAdvance(entries));
    }

    [Fact]
    public void Payroll_HappyPath()
    {
        var r = BillingCalculator.ComputePayroll(15000, 30, TimeSpan.FromHours(10), 60, 2000, 1000);
        Assert.Equal(15000, r.Salary);
        Assert.Equal(600, r.OvertimeAmount);
        Assert.Equal(14600, r.NetPay);
    }

    [Fact]
    public void Payroll_RejectsDeductionGreaterThanAdvance()
    {
        Assert.Throws<InvalidOperationException>(() =>
            BillingCalculator.ComputePayroll(15000, 30, TimeSpan.Zero, 0, 500, 1000));
    }

    [Fact]
    public void Payroll_RejectsNegativeNetPay()
    {
        Assert.Throws<InvalidOperationException>(() =>
            BillingCalculator.ComputePayroll(300, 1, TimeSpan.Zero, 0, 5000, 5000));
    }
}
