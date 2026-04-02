using HotelManagement.Core.Services;
using Xunit;

namespace HotelManagement.Tests.Boundary;

/// <summary>
/// Category 12: Boundary Tests (~30 tests)
/// Tests edge cases and boundary conditions.
/// </summary>
public class BoundaryTests
{
    private readonly BillingService _billing = new();
    private readonly EmployeePaymentService _empPayment = new();
    private readonly InventoryService _inventory = new();
    private readonly ValidationService _validation = new();

    // ============= DATE BOUNDARIES =============

    // TC-BOUND-001: Same-day check-in/out = 1 day minimum
    [Fact]
    public void SameDay_CheckInOut_OneDayMinimum()
    {
        var date = new DateTime(2026, 1, 1);
        int days = _billing.CalculateNoOfDays(date, date);
        Assert.Equal(1, days);
    }

    // TC-BOUND-002: One day difference
    [Fact]
    public void OneDay_Difference()
    {
        int days = _billing.CalculateNoOfDays(new DateTime(2026, 1, 1), new DateTime(2026, 1, 2));
        Assert.Equal(1, days);
    }

    // TC-BOUND-003: 30-day stay
    [Fact]
    public void ThirtyDay_Stay()
    {
        int days = _billing.CalculateNoOfDays(new DateTime(2026, 1, 1), new DateTime(2026, 1, 31));
        Assert.Equal(30, days);
    }

    // TC-BOUND-004: 365-day stay
    [Fact]
    public void YearLong_Stay()
    {
        int days = _billing.CalculateNoOfDays(new DateTime(2026, 1, 1), new DateTime(2027, 1, 1));
        Assert.Equal(365, days);
    }

    // TC-BOUND-005: Leap year February
    [Fact]
    public void LeapYear_February()
    {
        int days = _billing.CalculateNoOfDays(new DateTime(2028, 2, 1), new DateTime(2028, 3, 1));
        Assert.Equal(29, days); // 2028 is a leap year
    }

    // ============= CHARGE BOUNDARIES =============

    // TC-BOUND-006: Zero room charges
    [Fact]
    public void ZeroRoomCharges()
    {
        int total = _billing.CalculateTotalRoomCharges(0, 3);
        Assert.Equal(0, total);
    }

    // TC-BOUND-007: Zero other charges
    [Fact]
    public void ZeroOtherCharges()
    {
        var result = _billing.ComputeCheckIn(
            DateTime.Now.AddDays(-2), DateTime.Now, 5000, 0, 0, 10, 5, 0);
        // With zero other charges, discount base is just TotalRoomCharges
        Assert.Equal(0, result.Discount); // 0% of 10000 = 0
    }

    // TC-BOUND-008: Zero discount percentage
    [Fact]
    public void ZeroDiscountPercentage()
    {
        int discount = _billing.CalculateDiscount(10000, 0, 0);
        Assert.Equal(0, discount);
    }

    // TC-BOUND-009: 100% discount
    [Fact]
    public void FullDiscount_100Percent()
    {
        int discount = _billing.CalculateDiscount(10000, 0, 100);
        Assert.Equal(10000, discount);
    }

    // TC-BOUND-010: Zero tax percentages
    [Fact]
    public void ZeroTaxPercentages()
    {
        var result = _billing.ComputeCheckIn(
            DateTime.Now.AddDays(-2), DateTime.Now, 5000, 0, 0, 0, 0, 0);
        Assert.Equal(0.0, result.ServiceTaxAmount);
        Assert.Equal(0.0, result.LuxuryTaxAmount);
    }

    // TC-BOUND-011: Very large room charges
    [Fact]
    public void VeryLargeRoomCharges()
    {
        int total = _billing.CalculateTotalRoomCharges(999999, 30);
        Assert.Equal(29999970, total);
    }

    // TC-BOUND-012: Maximum tax rates (100%)
    [Fact]
    public void MaximumTaxRates()
    {
        var today = DateTime.Now;
        var result = _billing.ComputeCheckIn(
            today, today, 1000, 0, 0, 100, 100, 0);
        Assert.True(result.GrandTotal > result.SubTotal);
    }

    // ============= EMPLOYEE PAYMENT BOUNDARIES =============

    // TC-BOUND-013: Zero present days
    [Fact]
    public void Employee_ZeroPresentDays()
    {
        decimal salary = _empPayment.CalculateSalary(50000, 0);
        Assert.Equal(0m, salary);
    }

    // TC-BOUND-014: 30 present days (full month)
    [Fact]
    public void Employee_FullMonth_30Days()
    {
        decimal salary = _empPayment.CalculateSalary(30000, 30);
        Assert.Equal(30000m, salary);
    }

    // TC-BOUND-015: 31 present days (edge case)
    [Fact]
    public void Employee_31Days()
    {
        decimal salary = _empPayment.CalculateSalary(30000, 31);
        Assert.Equal(31000m, salary); // 30000 × 31 / 30 = 31000
    }

    // TC-BOUND-016: Zero overtime
    [Fact]
    public void Employee_ZeroOvertime()
    {
        decimal amount = _empPayment.CalculateOvertimeAmount(0, 100);
        Assert.Equal(0m, amount);
    }

    // TC-BOUND-017: Zero overtime rate
    [Fact]
    public void Employee_ZeroOvertimeRate()
    {
        decimal amount = _empPayment.CalculateOvertimeAmount(120, 0);
        Assert.Equal(0m, amount);
    }

    // TC-BOUND-018: Very large overtime hours
    [Fact]
    public void Employee_LargeOvertime()
    {
        decimal amount = _empPayment.CalculateOvertimeAmount(600, 200); // 10 hours * 200
        Assert.Equal(2000m, amount); // (600 * 200) / 60 = 2000
    }

    // TC-BOUND-019: Deduction equals salary + overtime (NetPay = 0)
    [Fact]
    public void Employee_NetPayZero()
    {
        decimal netPay = _empPayment.CalculateNetPay(10000, 500, 10500);
        Assert.Equal(0m, netPay);
    }

    // TC-BOUND-020: Deduction exceeds salary (negative NetPay)
    [Fact]
    public void Employee_NegativeNetPay()
    {
        decimal netPay = _empPayment.CalculateNetPay(10000, 0, 15000);
        Assert.Equal(-5000m, netPay);
    }

    // ============= INVENTORY BOUNDARIES =============

    // TC-BOUND-021: Zero quantity
    [Fact]
    public void Inventory_ZeroQuantity()
    {
        decimal total = _inventory.CalculateTotalPrice(0, 100);
        Assert.Equal(0m, total);
    }

    // TC-BOUND-022: Zero unit price
    [Fact]
    public void Inventory_ZeroUnitPrice()
    {
        decimal total = _inventory.CalculateTotalPrice(100, 0);
        Assert.Equal(0m, total);
    }

    // TC-BOUND-023: Single item
    [Fact]
    public void Inventory_SingleItem()
    {
        decimal total = _inventory.CalculateTotalPrice(1, 99.99m);
        Assert.Equal(99.99m, total);
    }

    // TC-BOUND-024: Very large quantity
    [Fact]
    public void Inventory_LargeQuantity()
    {
        decimal total = _inventory.CalculateTotalPrice(999999, 1);
        Assert.Equal(999999m, total);
    }

    // ============= VALIDATION BOUNDARIES =============

    // TC-BOUND-025: Empty string validation
    [Fact]
    public void Validation_EmptyString()
    {
        Assert.False(_validation.IsNumericOnly(""));
    }

    // TC-BOUND-026: Single character numeric
    [Fact]
    public void Validation_SingleDigit()
    {
        Assert.True(_validation.IsNumericOnly("0"));
    }

    // TC-BOUND-027: Single character alpha
    [Fact]
    public void Validation_SingleAlpha()
    {
        Assert.True(_validation.IsAlphaOnly("A"));
    }

    // TC-BOUND-028: Very long numeric string
    [Fact]
    public void Validation_LongNumericString()
    {
        string longNum = new string('1', 1000);
        Assert.True(_validation.IsNumericOnly(longNum));
    }

    // TC-BOUND-029: Decimal with leading zero
    [Fact]
    public void Validation_DecimalLeadingZero()
    {
        Assert.True(_validation.IsDecimalInput("0.5"));
    }

    // TC-BOUND-030: Decimal with trailing zero
    [Fact]
    public void Validation_DecimalTrailingZero()
    {
        Assert.True(_validation.IsDecimalInput("5.0"));
    }
}
