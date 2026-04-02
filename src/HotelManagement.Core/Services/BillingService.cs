namespace HotelManagement.Core.Services;

/// <summary>
/// Preserves exact billing calculation logic from frmCheckIn.vb Compute() sub.
/// VB.NET Val() returns 0 for invalid strings, CInt truncates to integer.
/// Math.Round rounds to 2 decimal places for tax amounts.
/// </summary>
public class BillingService
{
    /// <summary>
    /// Calculate number of days between check-in and check-out.
    /// Same-day = 1 day minimum (matches VB.NET original).
    /// </summary>
    public int CalculateNoOfDays(DateTime dateIn, DateTime dateOut)
    {
        if (dateOut.Date == dateIn.Date)
            return 1;
        return (dateOut.Date - dateIn.Date).Days;
    }

    /// <summary>
    /// Calculate total room charges = RoomCharges × NoOfDays.
    /// Uses CInt (truncation to integer) as in original VB.NET code.
    /// </summary>
    public int CalculateTotalRoomCharges(decimal roomCharges, int noOfDays)
    {
        return (int)(roomCharges * noOfDays);
    }

    /// <summary>
    /// Calculate discount amount.
    /// Discount = CInt(((TotalRoomCharges + OtherCharges) × DiscountPer) / 100)
    /// </summary>
    public int CalculateDiscount(decimal totalRoomCharges, decimal otherCharges, decimal discountPer)
    {
        return (int)(((totalRoomCharges + otherCharges) * discountPer) / 100m);
    }

    /// <summary>
    /// Calculate subtotal.
    /// SubTotal = CInt(TotalRoomCharges + OtherCharges) - Discount
    /// Note: VB.NET original has a subtle bug where CInt is applied to (TotalRoomCharges + OtherCharges)
    /// before subtracting discount. We preserve this behavior exactly.
    /// </summary>
    public int CalculateSubTotal(decimal totalRoomCharges, decimal otherCharges, decimal discount)
    {
        return (int)(totalRoomCharges + otherCharges) - (int)discount;
    }

    /// <summary>
    /// Calculate service tax amount.
    /// ServiceTaxAmount = Math.Round((SubTotal × ServiceTaxPer) / 100, 2)
    /// </summary>
    public double CalculateServiceTaxAmount(decimal subTotal, decimal serviceTaxPer)
    {
        double result = (double)((subTotal * serviceTaxPer) / 100m);
        return Math.Round(result, 2);
    }

    /// <summary>
    /// Calculate luxury tax amount.
    /// LuxuryTaxAmount = Math.Round(((SubTotal + ServiceTaxAmount) × LuxuryTaxPer) / 100, 2)
    /// Note: Luxury tax base includes ServiceTaxAmount (cascading tax).
    /// </summary>
    public double CalculateLuxuryTaxAmount(decimal subTotal, double serviceTaxAmount, decimal luxuryTaxPer)
    {
        double result = (((double)subTotal + serviceTaxAmount) * (double)luxuryTaxPer) / 100.0;
        return Math.Round(result, 2);
    }

    /// <summary>
    /// Calculate grand total.
    /// GrandTotal = CInt(SubTotal + ServiceTaxAmount + LuxuryTaxAmount)
    /// </summary>
    public int CalculateGrandTotal(decimal subTotal, double serviceTaxAmount, double luxuryTaxAmount)
    {
        return (int)((double)subTotal + serviceTaxAmount + luxuryTaxAmount);
    }

    /// <summary>
    /// Calculate balance.
    /// Balance = CInt(GrandTotal - TotalPaid)
    /// </summary>
    public int CalculateBalance(decimal grandTotal, decimal totalPaid)
    {
        return (int)(grandTotal - totalPaid);
    }

    /// <summary>
    /// Full check-in billing computation matching frmCheckIn.vb Compute() exactly.
    /// </summary>
    public CheckInBillingResult ComputeCheckIn(
        DateTime dateIn, DateTime dateOut,
        decimal roomCharges, decimal otherCharges,
        decimal discountPer, decimal serviceTaxPer,
        decimal luxuryTaxPer, decimal totalPaid)
    {
        int noOfDays = CalculateNoOfDays(dateIn, dateOut);
        int totalRoomCharges = CalculateTotalRoomCharges(roomCharges, noOfDays);
        int discount = CalculateDiscount(totalRoomCharges, otherCharges, discountPer);
        int subTotal = CalculateSubTotal(totalRoomCharges, otherCharges, discount);
        double serviceTaxAmount = CalculateServiceTaxAmount(subTotal, serviceTaxPer);
        double luxuryTaxAmount = CalculateLuxuryTaxAmount(subTotal, serviceTaxAmount, luxuryTaxPer);
        int grandTotal = CalculateGrandTotal(subTotal, serviceTaxAmount, luxuryTaxAmount);
        int balance = CalculateBalance(grandTotal, totalPaid);

        return new CheckInBillingResult
        {
            NoOfDays = noOfDays,
            TotalRoomCharges = totalRoomCharges,
            Discount = discount,
            SubTotal = subTotal,
            ServiceTaxAmount = serviceTaxAmount,
            LuxuryTaxAmount = luxuryTaxAmount,
            GrandTotal = grandTotal,
            Balance = balance
        };
    }

    /// <summary>
    /// Calculate education cess tax for checkout.
    /// EducessTax = Math.Round((ServiceTaxPer × 2) / 100, 2)
    /// </summary>
    public double CalculateEducessTax(decimal serviceTaxPer)
    {
        double result = (double)(serviceTaxPer * 2m) / 100.0;
        return Math.Round(result, 2);
    }

    /// <summary>
    /// Calculate education cess tax amount.
    /// EducessTaxAmount = Math.Round((ServiceTaxAmount × EducessTax) / 100, 2)
    /// </summary>
    public double CalculateEducessTaxAmount(double serviceTaxAmount, double educessTax)
    {
        double result = (serviceTaxAmount * educessTax) / 100.0;
        return Math.Round(result, 2);
    }

    /// <summary>
    /// Calculate higher education cess tax.
    /// HEduCessTax = Math.Round((ServiceTaxPer × 1) / 100, 2)
    /// </summary>
    public double CalculateHEduCessTax(decimal serviceTaxPer)
    {
        double result = (double)(serviceTaxPer * 1m) / 100.0;
        return Math.Round(result, 2);
    }

    /// <summary>
    /// Calculate higher education cess tax amount.
    /// HEducessTaxAmount = Math.Round((ServiceTaxAmount × HEduCessTax) / 100, 2)
    /// </summary>
    public double CalculateHEducessTaxAmount(double serviceTaxAmount, double hEduCessTax)
    {
        double result = (serviceTaxAmount * hEduCessTax) / 100.0;
        return Math.Round(result, 2);
    }

    /// <summary>
    /// Full checkout billing computation matching frmCheckOut.vb Calculate() exactly.
    /// </summary>
    public CheckOutBillingResult ComputeCheckOut(
        DateTime dateIn, DateTime dateOut,
        decimal roomCharges, decimal otherCharges,
        decimal discountPer, decimal serviceTaxPer,
        decimal luxuryTaxPer, decimal totalPaid)
    {
        var checkIn = ComputeCheckIn(dateIn, dateOut, roomCharges, otherCharges,
            discountPer, serviceTaxPer, luxuryTaxPer, totalPaid);

        double educessTax = CalculateEducessTax(serviceTaxPer);
        double educessTaxAmount = CalculateEducessTaxAmount(checkIn.ServiceTaxAmount, educessTax);
        double hEduCessTax = CalculateHEduCessTax(serviceTaxPer);
        double hEducessTaxAmount = CalculateHEducessTaxAmount(checkIn.ServiceTaxAmount, hEduCessTax);

        return new CheckOutBillingResult
        {
            NoOfDays = checkIn.NoOfDays,
            TotalRoomCharges = checkIn.TotalRoomCharges,
            Discount = checkIn.Discount,
            SubTotal = checkIn.SubTotal,
            ServiceTaxAmount = checkIn.ServiceTaxAmount,
            LuxuryTaxAmount = checkIn.LuxuryTaxAmount,
            GrandTotal = checkIn.GrandTotal,
            Balance = checkIn.Balance,
            EducessTax = educessTax,
            EducessTaxAmount = educessTaxAmount,
            HEduCessTax = hEduCessTax,
            HEducessTaxAmount = hEducessTaxAmount
        };
    }
}

public class CheckInBillingResult
{
    public int NoOfDays { get; set; }
    public int TotalRoomCharges { get; set; }
    public int Discount { get; set; }
    public int SubTotal { get; set; }
    public double ServiceTaxAmount { get; set; }
    public double LuxuryTaxAmount { get; set; }
    public int GrandTotal { get; set; }
    public int Balance { get; set; }
}

public class CheckOutBillingResult : CheckInBillingResult
{
    public double EducessTax { get; set; }
    public double EducessTaxAmount { get; set; }
    public double HEduCessTax { get; set; }
    public double HEducessTaxAmount { get; set; }
}
