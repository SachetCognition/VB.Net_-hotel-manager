using HotelManagement.Core.Services;
using Xunit;

namespace HotelManagement.Tests.Unit;

/// <summary>
/// Category 1: Unit Tests — Billing Calculations (~30 tests)
/// Tests exact billing logic from frmCheckIn.vb Compute() and frmCheckOut.vb Calculate().
/// </summary>
public class BillingServiceTests
{
    private readonly BillingService _svc = new();

    // TC-CALC-001: Check-in same-day = 1 day
    [Fact]
    public void CalculateNoOfDays_SameDay_Returns1()
    {
        var d = new DateTime(2024, 1, 15);
        Assert.Equal(1, _svc.CalculateNoOfDays(d, d));
    }

    // TC-CALC-002: Check-in multi-day calculation
    [Fact]
    public void CalculateNoOfDays_MultiDay_ReturnsCorrect()
    {
        Assert.Equal(5, _svc.CalculateNoOfDays(new DateTime(2024, 1, 1), new DateTime(2024, 1, 6)));
    }

    // TC-CALC-003: TotalRoomCharges = RoomCharges × NoOfDays
    [Fact]
    public void CalculateTotalRoomCharges_Basic()
    {
        Assert.Equal(5000, _svc.CalculateTotalRoomCharges(1000m, 5));
    }

    // TC-CALC-004: Discount calculation with 0%
    [Fact]
    public void CalculateDiscount_ZeroPercent_ReturnsZero()
    {
        Assert.Equal(0, _svc.CalculateDiscount(5000m, 500m, 0m));
    }

    // TC-CALC-005: Discount calculation with 50%
    [Fact]
    public void CalculateDiscount_FiftyPercent()
    {
        Assert.Equal(2750, _svc.CalculateDiscount(5000m, 500m, 50m));
    }

    // TC-CALC-006: SubTotal = TotalRoomCharges + OtherCharges - Discount
    [Fact]
    public void CalculateSubTotal_Basic()
    {
        Assert.Equal(2750, _svc.CalculateSubTotal(5000m, 500m, 2750m));
    }

    // TC-CALC-007: ServiceTaxAmount rounding to 2 decimals
    [Fact]
    public void CalculateServiceTaxAmount_RoundsTo2Decimals()
    {
        double result = _svc.CalculateServiceTaxAmount(2750m, 12.36m);
        Assert.Equal(Math.Round(result, 2), result);
    }

    // TC-CALC-008: LuxuryTaxAmount includes ServiceTaxAmount in base
    [Fact]
    public void CalculateLuxuryTaxAmount_IncludesServiceTax()
    {
        double svcTax = _svc.CalculateServiceTaxAmount(2750m, 12.36m);
        double luxTax = _svc.CalculateLuxuryTaxAmount(2750m, svcTax, 5m);
        double expectedBase = (double)2750m + svcTax;
        double expected = Math.Round((expectedBase * 5.0) / 100.0, 2);
        Assert.Equal(expected, luxTax);
    }

    // TC-CALC-009: GrandTotal = SubTotal + ServiceTax + LuxuryTax
    [Fact]
    public void CalculateGrandTotal_Basic()
    {
        int result = _svc.CalculateGrandTotal(2750m, 339.90, 154.50);
        Assert.Equal((int)(2750.0 + 339.90 + 154.50), result);
    }

    // TC-CALC-010: Balance = GrandTotal - TotalPaid
    [Fact]
    public void CalculateBalance_Basic()
    {
        Assert.Equal(1000, _svc.CalculateBalance(3000m, 2000m));
    }

    // TC-CALC-011: Checkout EducessTax = ServiceTaxPer × 2 / 100
    [Fact]
    public void CalculateEducessTax_Basic()
    {
        double result = _svc.CalculateEducessTax(12.36m);
        Assert.Equal(Math.Round((12.36 * 2) / 100.0, 2), result);
    }

    // TC-CALC-012: Checkout HEduCessTax = ServiceTaxPer × 1 / 100
    [Fact]
    public void CalculateHEduCessTax_Basic()
    {
        double result = _svc.CalculateHEduCessTax(12.36m);
        Assert.Equal(Math.Round((12.36 * 1) / 100.0, 2), result);
    }

    // TC-CALC-013: EducessTaxAmount calculation
    [Fact]
    public void CalculateEducessTaxAmount_Basic()
    {
        double eduTax = _svc.CalculateEducessTax(12.36m);
        double svcAmount = 339.90;
        double result = _svc.CalculateEducessTaxAmount(svcAmount, eduTax);
        Assert.Equal(Math.Round((svcAmount * eduTax) / 100.0, 2), result);
    }

    // TC-CALC-014: HEducessTaxAmount calculation
    [Fact]
    public void CalculateHEducessTaxAmount_Basic()
    {
        double hEduTax = _svc.CalculateHEduCessTax(12.36m);
        double svcAmount = 339.90;
        double result = _svc.CalculateHEducessTaxAmount(svcAmount, hEduTax);
        Assert.Equal(Math.Round((svcAmount * hEduTax) / 100.0, 2), result);
    }

    // TC-CALC-015: Full ComputeCheckIn with typical values
    [Fact]
    public void ComputeCheckIn_TypicalValues()
    {
        var result = _svc.ComputeCheckIn(
            new DateTime(2024, 1, 1), new DateTime(2024, 1, 6),
            1000m, 500m, 10m, 12.36m, 5m, 2000m);
        Assert.Equal(5, result.NoOfDays);
        Assert.Equal(5000, result.TotalRoomCharges);
        Assert.Equal(550, result.Discount);
    }

    // TC-CALC-016: Full ComputeCheckOut with typical values
    [Fact]
    public void ComputeCheckOut_TypicalValues()
    {
        var result = _svc.ComputeCheckOut(
            new DateTime(2024, 1, 1), new DateTime(2024, 1, 6),
            1000m, 500m, 10m, 12.36m, 5m, 2000m);
        Assert.True(result.EducessTax > 0);
        Assert.True(result.HEduCessTax > 0);
    }

    // TC-CALC-017: Zero room charges
    [Fact]
    public void CalculateTotalRoomCharges_ZeroCharges()
    {
        Assert.Equal(0, _svc.CalculateTotalRoomCharges(0m, 5));
    }

    // TC-CALC-018: Zero days (same day minimum = 1)
    [Fact]
    public void CalculateNoOfDays_SameDate_Returns1()
    {
        var date = new DateTime(2024, 6, 15);
        Assert.Equal(1, _svc.CalculateNoOfDays(date, date));
    }

    // TC-CALC-019: Large number of days
    [Fact]
    public void CalculateNoOfDays_365Days()
    {
        // 2024 is a leap year: Jan 1 2024 → Jan 1 2025 = 366 days
        Assert.Equal(366, _svc.CalculateNoOfDays(new DateTime(2024, 1, 1), new DateTime(2025, 1, 1)));
    }

    // TC-CALC-020: Discount 100% edge case
    [Fact]
    public void CalculateDiscount_HundredPercent()
    {
        Assert.Equal(5500, _svc.CalculateDiscount(5000m, 500m, 100m));
    }

    // TC-CALC-021: Zero balance when fully paid
    [Fact]
    public void CalculateBalance_FullyPaid_ReturnsZero()
    {
        Assert.Equal(0, _svc.CalculateBalance(3000m, 3000m));
    }

    // TC-CALC-022: Full compute with zero taxes
    [Fact]
    public void ComputeCheckIn_ZeroTaxes()
    {
        var result = _svc.ComputeCheckIn(
            new DateTime(2024, 1, 1), new DateTime(2024, 1, 2),
            1000m, 0m, 0m, 0m, 0m, 0m);
        Assert.Equal(1, result.NoOfDays);
        Assert.Equal(1000, result.TotalRoomCharges);
        Assert.Equal(0, result.Discount);
        Assert.Equal(1000, result.SubTotal);
        Assert.Equal(0.0, result.ServiceTaxAmount);
        Assert.Equal(0.0, result.LuxuryTaxAmount);
        Assert.Equal(1000, result.GrandTotal);
        Assert.Equal(1000, result.Balance);
    }

    // TC-CALC-023: Negative balance not possible (TotalPaid > GrandTotal validation elsewhere)
    [Fact]
    public void CalculateBalance_Negative()
    {
        int result = _svc.CalculateBalance(1000m, 1500m);
        Assert.Equal(-500, result);
    }

    // TC-CALC-024: Decimal precision in service tax
    [Fact]
    public void CalculateServiceTaxAmount_DecimalPrecision()
    {
        double result = _svc.CalculateServiceTaxAmount(3333m, 12.36m);
        Assert.Equal(2, result.ToString("F10").Split('.')[1].TrimEnd('0').Length <= 2 ? 2 : result.ToString("F10").Split('.')[1].TrimEnd('0').Length);
    }

    // TC-CALC-025: One day checkout
    [Fact]
    public void CalculateNoOfDays_OneDayDifference()
    {
        Assert.Equal(1, _svc.CalculateNoOfDays(new DateTime(2024, 3, 1), new DateTime(2024, 3, 2)));
    }

    // TC-CALC-026: Large room charges
    [Fact]
    public void CalculateTotalRoomCharges_LargeValues()
    {
        Assert.Equal(50000, _svc.CalculateTotalRoomCharges(10000m, 5));
    }

    // TC-CALC-027: Discount with other charges only
    [Fact]
    public void CalculateDiscount_OtherChargesOnly()
    {
        Assert.Equal(50, _svc.CalculateDiscount(0m, 500m, 10m));
    }

    // TC-CALC-028: SubTotal with no discount
    [Fact]
    public void CalculateSubTotal_NoDiscount()
    {
        Assert.Equal(5500, _svc.CalculateSubTotal(5000m, 500m, 0m));
    }

    // TC-CALC-029: CheckOut has all additional tax fields
    [Fact]
    public void ComputeCheckOut_HasAllTaxFields()
    {
        var result = _svc.ComputeCheckOut(
            new DateTime(2024, 1, 1), new DateTime(2024, 1, 3),
            2000m, 0m, 0m, 10m, 5m, 0m);
        Assert.True(result.EducessTax >= 0);
        Assert.True(result.EducessTaxAmount >= 0);
        Assert.True(result.HEduCessTax >= 0);
        Assert.True(result.HEducessTaxAmount >= 0);
    }

    // TC-CALC-030: Full end-to-end billing computation
    [Fact]
    public void ComputeCheckIn_EndToEnd_VerifyAllFields()
    {
        var result = _svc.ComputeCheckIn(
            new DateTime(2024, 1, 1), new DateTime(2024, 1, 4),
            2000m, 1000m, 10m, 12m, 5m, 3000m);

        Assert.Equal(3, result.NoOfDays);
        Assert.Equal(6000, result.TotalRoomCharges);
        Assert.Equal(700, result.Discount);
        Assert.Equal(6300, result.SubTotal);
        Assert.True(result.ServiceTaxAmount > 0);
        Assert.True(result.LuxuryTaxAmount > 0);
        Assert.True(result.GrandTotal > result.SubTotal);
    }
}
