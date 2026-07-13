namespace HotelManager.Application.Billing;

/// <summary>
/// Reproduces the legacy billing math from frmCheckIn.Compute/Compute1/Compute2,
/// frmCheckOut.Calculate and frmEmployeePayment. Integer conversions use
/// banker's rounding to match VB.NET CInt semantics.
/// </summary>
public static class BillingCalculator
{
    private static int CInt(double value) => (int)Math.Round(value, MidpointRounding.ToEven);

    public static StayBill ComputeStay(
        DateTime dateIn, DateTime dateOut, double roomCharges, double otherCharges,
        double discountPer, double serviceTaxPer, double luxuryTaxPer, double totalPaid)
    {
        int noOfDays = dateOut.Date == dateIn.Date
            ? 1
            : Math.Max(1, (dateOut.Date - dateIn.Date).Days);
        double totalRoomCharges = CInt(roomCharges * noOfDays);
        double discount = CInt((totalRoomCharges + otherCharges) * discountPer / 100);
        double subTotal = CInt(totalRoomCharges + otherCharges) - discount;
        return FinishBill(noOfDays, totalRoomCharges, otherCharges, discount, subTotal,
            serviceTaxPer, luxuryTaxPer, discountPer, totalPaid);
    }

    /// <summary>Hall+Garden combined reservation (frmCheckIn.Compute1 shape).</summary>
    public static HallAndGardenBill ComputeHallAndGarden(
        DateTime hallFrom, DateTime hallTo, double hallRate,
        DateTime gardenFrom, DateTime gardenTo, double gardenRate,
        double otherCharges, double discountPer, double serviceTaxPer,
        double luxuryTaxPer, double totalPaid)
    {
        int daysHall = (hallTo.Date - hallFrom.Date).Days;
        int daysGarden = (gardenTo.Date - gardenFrom.Date).Days;
        double totalHall = CInt(daysHall * hallRate);
        double totalGarden = CInt(daysGarden * gardenRate);
        double discount = CInt((totalHall + totalGarden + otherCharges) * discountPer / 100);
        double subTotal = CInt(totalHall + totalGarden + otherCharges - discount);
        var b = FinishBill(daysHall, totalHall + totalGarden, otherCharges, discount, subTotal,
            serviceTaxPer, luxuryTaxPer, discountPer, totalPaid);
        return new HallAndGardenBill(daysHall, totalHall, daysGarden, totalGarden, b);
    }

    /// <summary>Hall-or-Garden reservation (frmCheckIn.Compute2 shape).</summary>
    public static StayBill ComputeHallOrGarden(
        DateTime dateFrom, DateTime dateTo, double rate, double otherCharges,
        double discountPer, double serviceTaxPer, double luxuryTaxPer, double totalPaid)
    {
        int days = (dateTo.Date - dateFrom.Date).Days;
        double totalCharges = CInt(days * rate);
        double discount = CInt((totalCharges + otherCharges) * discountPer / 100);
        double subTotal = CInt(totalCharges + otherCharges - discount);
        return FinishBill(days, totalCharges, otherCharges, discount, subTotal,
            serviceTaxPer, luxuryTaxPer, discountPer, totalPaid);
    }

    private static StayBill FinishBill(
        int noOfDays, double totalCharges, double otherCharges, double discount,
        double subTotal, double serviceTaxPer, double luxuryTaxPer, double discountPer,
        double totalPaid)
    {
        double serviceTaxAmount = Math.Round(subTotal * serviceTaxPer / 100, 2);
        double luxuryTaxAmount = Math.Round((subTotal + serviceTaxAmount) * luxuryTaxPer / 100, 2);
        double grandTotal = CInt(subTotal + serviceTaxAmount + luxuryTaxAmount);
        double balance = CInt(grandTotal - totalPaid);
        double educessTax = Math.Round(serviceTaxPer * 2 / 100, 2);
        double educessTaxAmount = Math.Round(serviceTaxAmount * educessTax / 100, 2);
        double hEduCessTax = Math.Round(serviceTaxPer * 1 / 100, 2);
        double hEduCessTaxAmount = Math.Round(serviceTaxAmount * hEduCessTax / 100, 2);
        return new StayBill(noOfDays, totalCharges, otherCharges, discountPer, discount,
            subTotal, serviceTaxPer, serviceTaxAmount, luxuryTaxPer, luxuryTaxAmount,
            grandTotal, totalPaid, balance,
            educessTax, educessTaxAmount, hEduCessTax, hEduCessTaxAmount);
    }

    // ---- Payroll (frmEmployeePayment) ----

    public static int ComputeSalary(int basicSalary, int presentDays)
        => CInt((double)basicSalary * presentDays / 30);

    public static int ComputeOvertimeAmount(TimeSpan overtime, int overtimeRate)
        => CInt(overtime.TotalMinutes * overtimeRate / 60);

    /// <summary>Outstanding advance = sum(Amount) - sum(Deduction) over AdvanceEntry rows.</summary>
    public static int ComputeOutstandingAdvance(IEnumerable<(int Amount, int Deduction)> entries)
        => entries.Sum(e => e.Amount) - entries.Sum(e => e.Deduction);

    public static int ComputeNetPay(int salary, int overtimeAmount, int deduction)
        => salary + overtimeAmount - deduction;

    public static PayrollResult ComputePayroll(
        int basicSalary, int presentDays, TimeSpan overtime, int overtimeRate,
        int outstandingAdvance, int deduction)
    {
        if (deduction > outstandingAdvance)
            throw new InvalidOperationException("You can not deduct amount more than advance amount");
        int salary = ComputeSalary(basicSalary, presentDays);
        int overtimeAmount = ComputeOvertimeAmount(overtime, overtimeRate);
        int netPay = ComputeNetPay(salary, overtimeAmount, deduction);
        if (netPay < 0)
            throw new InvalidOperationException("Net pay should be more than 0");
        return new PayrollResult(salary, overtimeAmount, outstandingAdvance, deduction, netPay);
    }
}

public record StayBill(
    int NoOfDays, double TotalCharges, double OtherCharges, double DiscountPer, double Discount,
    double SubTotal, double ServiceTaxPer, double ServiceTaxAmount, double LuxuryTaxPer,
    double LuxuryTaxAmount, double GrandTotal, double TotalPaid, double Balance,
    double EducessTax, double EducessTaxAmount, double HEduCessTax, double HEduCessTaxAmount);

public record HallAndGardenBill(
    int DaysHall, double TotalChargesHall, int DaysGarden, double TotalChargesGarden, StayBill Bill);

public record PayrollResult(int Salary, int OvertimeAmount, int Advance, int Deduction, int NetPay);
