using HotelManagement.Core.Services;
using Xunit;

namespace HotelManagement.Tests.Validation;

/// <summary>
/// Category 4: Validation Tests (~80 tests)
/// Category 5: Input Restriction Tests (~20 tests)
/// Tests input validation logic from VB.NET KeyPress events and validation checks.
/// </summary>
public class ValidationServiceTests
{
    private readonly ValidationService _svc = new();

    // ============= GUEST VALIDATION (12 tests) =============

    [Fact] public void ValidateGuest_AllValid_ReturnsTrue()
    {
        var (valid, errors) = _svc.ValidateGuest("John Doe", "123 Main St", "NYC", "1234567890", "Passport", "AB123");
        Assert.True(valid); Assert.Empty(errors);
    }

    [Fact] public void ValidateGuest_EmptyName_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateGuest("", "123 Main St", "NYC", "1234567890", "Passport", "AB123");
        Assert.False(valid); Assert.Contains("Guest Name is required", errors);
    }

    [Fact] public void ValidateGuest_EmptyAddress_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateGuest("John", "", "NYC", "1234567890", "Passport", "AB123");
        Assert.False(valid); Assert.Contains("Address is required", errors);
    }

    [Fact] public void ValidateGuest_EmptyCity_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateGuest("John", "123 Main", "", "1234567890", "Passport", "AB123");
        Assert.False(valid); Assert.Contains("City is required", errors);
    }

    [Fact] public void ValidateGuest_EmptyContact_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateGuest("John", "123 Main", "NYC", "", "Passport", "AB123");
        Assert.False(valid); Assert.Contains("Contact No is required", errors);
    }

    [Fact] public void ValidateGuest_NonNumericContact_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateGuest("John", "123 Main", "NYC", "abc", "Passport", "AB123");
        Assert.False(valid); Assert.Contains("Contact No must be numeric", errors);
    }

    [Fact] public void ValidateGuest_EmptyIdType_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateGuest("John", "123 Main", "NYC", "123", "","AB123");
        Assert.False(valid); Assert.Contains("ID Type is required", errors);
    }

    [Fact] public void ValidateGuest_EmptyIdNumber_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateGuest("John", "123 Main", "NYC", "123", "Passport", "");
        Assert.False(valid); Assert.Contains("ID Number is required", errors);
    }

    [Fact] public void ValidateGuest_AllEmpty_Returns6Errors()
    {
        var (valid, errors) = _svc.ValidateGuest("", "", "", "", "", "");
        Assert.False(valid); Assert.Equal(6, errors.Count);
    }

    [Fact] public void ValidateGuest_WhitespaceOnly_ReturnsFalse()
    {
        var (valid, _) = _svc.ValidateGuest("  ", "  ", "  ", "  ", "  ", "  ");
        Assert.False(valid);
    }

    [Fact] public void ValidateGuest_NumericContact_ReturnsTrue()
    {
        var (valid, _) = _svc.ValidateGuest("John", "Addr", "City", "9876543210", "DL", "X1");
        Assert.True(valid);
    }

    [Fact] public void ValidateGuest_ContactWithSpaces_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateGuest("John", "Addr", "City", "123 456", "DL", "X1");
        Assert.False(valid); Assert.Contains("Contact No must be numeric", errors);
    }

    // ============= ROOM VALIDATION (6 tests) =============

    [Fact] public void ValidateRoom_AllValid_ReturnsTrue()
    {
        var (valid, _) = _svc.ValidateRoom("101", "Deluxe", "5000");
        Assert.True(valid);
    }

    [Fact] public void ValidateRoom_EmptyRoomNo_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateRoom("", "Deluxe", "5000");
        Assert.False(valid); Assert.Contains("Room No is required", errors);
    }

    [Fact] public void ValidateRoom_EmptyRoomType_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateRoom("101", "", "5000");
        Assert.False(valid); Assert.Contains("Room Type is required", errors);
    }

    [Fact] public void ValidateRoom_EmptyCharges_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateRoom("101", "Deluxe", "");
        Assert.False(valid); Assert.Contains("Room Charges is required", errors);
    }

    [Fact] public void ValidateRoom_NonNumericCharges_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateRoom("101", "Deluxe", "abc");
        Assert.False(valid); Assert.Contains("Room Charges must be numeric", errors);
    }

    [Fact] public void ValidateRoom_AllEmpty_Returns3Errors()
    {
        var (valid, errors) = _svc.ValidateRoom("", "", "");
        Assert.False(valid); Assert.Equal(3, errors.Count);
    }

    // ============= HOTEL INFO VALIDATION (6 tests) =============

    [Fact] public void ValidateHotelInfo_AllValid_ReturnsTrue()
    {
        var (valid, _) = _svc.ValidateHotelInfo("Grand Hotel", "123 Main", "1234567890");
        Assert.True(valid);
    }

    [Fact] public void ValidateHotelInfo_EmptyName_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateHotelInfo("", "123 Main", "123");
        Assert.False(valid); Assert.Contains("Hotel Name is required", errors);
    }

    [Fact] public void ValidateHotelInfo_EmptyAddress_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateHotelInfo("Hotel", "", "123");
        Assert.False(valid); Assert.Contains("Address is required", errors);
    }

    [Fact] public void ValidateHotelInfo_EmptyContact_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateHotelInfo("Hotel", "Addr", "");
        Assert.False(valid); Assert.Contains("Contact No is required", errors);
    }

    [Fact] public void ValidateHotelInfo_AllEmpty_Returns3Errors()
    {
        var (valid, errors) = _svc.ValidateHotelInfo("", "", "");
        Assert.False(valid); Assert.Equal(3, errors.Count);
    }

    [Fact] public void ValidateHotelInfo_WhitespaceOnly_ReturnsFalse()
    {
        var (valid, _) = _svc.ValidateHotelInfo("  ", " ", " ");
        Assert.False(valid);
    }

    // ============= CHECK-IN VALIDATION (10 tests) =============

    [Fact] public void ValidateCheckIn_AllValid_ReturnsTrue()
    {
        var (valid, _) = _svc.ValidateCheckIn("101", "G-123456", new DateTime(2024, 1, 1), new DateTime(2024, 1, 5), 1000m, 5000m);
        Assert.True(valid);
    }

    [Fact] public void ValidateCheckIn_EmptyRoom_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateCheckIn("", "G-123456", DateTime.Now, DateTime.Now.AddDays(1), 0m, 0m);
        Assert.False(valid); Assert.Contains("Room must be selected", errors);
    }

    [Fact] public void ValidateCheckIn_EmptyGuest_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateCheckIn("101", "", DateTime.Now, DateTime.Now.AddDays(1), 0m, 0m);
        Assert.False(valid); Assert.Contains("Guest must be selected", errors);
    }

    [Fact] public void ValidateCheckIn_DateOutBeforeDateIn_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateCheckIn("101", "G-123456", new DateTime(2024, 1, 5), new DateTime(2024, 1, 1), 0m, 0m);
        Assert.False(valid); Assert.Contains("Check-out date must be on or after check-in date", errors);
    }

    [Fact] public void ValidateCheckIn_TotalPaidExceedsGrandTotal_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateCheckIn("101", "G-123456", DateTime.Now, DateTime.Now.AddDays(1), 6000m, 5000m);
        Assert.False(valid); Assert.Contains("Total Paid cannot exceed Grand Total", errors);
    }

    [Fact] public void ValidateCheckIn_SameDateValid_ReturnsTrue()
    {
        var d = DateTime.Now;
        var (valid, _) = _svc.ValidateCheckIn("101", "G-123456", d, d, 0m, 5000m);
        Assert.True(valid);
    }

    [Fact] public void ValidateCheckIn_ZeroPaid_ReturnsTrue()
    {
        var (valid, _) = _svc.ValidateCheckIn("101", "G-123456", DateTime.Now, DateTime.Now.AddDays(1), 0m, 5000m);
        Assert.True(valid);
    }

    [Fact] public void ValidateCheckIn_ExactPaid_ReturnsTrue()
    {
        var (valid, _) = _svc.ValidateCheckIn("101", "G-123456", DateTime.Now, DateTime.Now.AddDays(1), 5000m, 5000m);
        Assert.True(valid);
    }

    [Fact] public void ValidateCheckIn_AllEmpty_ReturnsMultipleErrors()
    {
        var (valid, errors) = _svc.ValidateCheckIn("", "", new DateTime(2024, 1, 5), new DateTime(2024, 1, 1), 6000m, 5000m);
        Assert.False(valid); Assert.True(errors.Count >= 3);
    }

    [Fact] public void ValidateCheckIn_WhitespaceRoom_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateCheckIn("  ", "G-123456", DateTime.Now, DateTime.Now.AddDays(1), 0m, 0m);
        Assert.False(valid);
    }

    // ============= CHECK-OUT VALIDATION (14 tests) =============

    [Fact] public void ValidateCheckOut_AllValid_ReturnsTrue()
    {
        var (valid, _) = _svc.ValidateCheckOut("USD", "G-123456", 3000m, 5000m);
        Assert.True(valid);
    }

    [Fact] public void ValidateCheckOut_EmptyCurrency_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateCheckOut("", "G-123456", 0m, 0m);
        Assert.False(valid); Assert.Contains("Currency must be selected", errors);
    }

    [Fact] public void ValidateCheckOut_EmptyGuest_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateCheckOut("USD", "", 0m, 0m);
        Assert.False(valid); Assert.Contains("Guest must be selected", errors);
    }

    [Fact] public void ValidateCheckOut_PaidExceedsTotal_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateCheckOut("USD", "G-123456", 6000m, 5000m);
        Assert.False(valid); Assert.Contains("Total Paid cannot exceed Grand Total", errors);
    }

    [Fact] public void ValidateCheckOut_ExactPayment_ReturnsTrue()
    {
        var (valid, _) = _svc.ValidateCheckOut("USD", "G-123456", 5000m, 5000m);
        Assert.True(valid);
    }

    [Fact] public void ValidateCheckOut_ZeroPayment_ReturnsTrue()
    {
        var (valid, _) = _svc.ValidateCheckOut("USD", "G-123456", 0m, 5000m);
        Assert.True(valid);
    }

    [Fact] public void ValidateCheckOut_AllEmpty_ReturnsErrors()
    {
        var (valid, errors) = _svc.ValidateCheckOut("", "", 6000m, 5000m);
        Assert.False(valid); Assert.True(errors.Count >= 2);
    }

    // ============= EMPLOYEE VALIDATION (16 tests) =============

    [Fact] public void ValidateEmployee_AllValid_ReturnsTrue()
    {
        var (valid, _) = _svc.ValidateEmployee("John Doe", "123 Main", "9876543210", "john@test.com", "IT", "Manager", "50000");
        Assert.True(valid);
    }

    [Fact] public void ValidateEmployee_EmptyName_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateEmployee("", "Addr", "123", "a@b.com", "IT", "Mgr", "50000");
        Assert.False(valid); Assert.Contains("Employee Name is required", errors);
    }

    [Fact] public void ValidateEmployee_NonAlphaName_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateEmployee("John123", "Addr", "123", "a@b.com", "IT", "Mgr", "50000");
        Assert.False(valid); Assert.Contains("Employee Name must contain only letters", errors);
    }

    [Fact] public void ValidateEmployee_EmptyAddress_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateEmployee("John", "", "123", "a@b.com", "IT", "Mgr", "50000");
        Assert.False(valid); Assert.Contains("Address is required", errors);
    }

    [Fact] public void ValidateEmployee_EmptyMobile_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateEmployee("John", "Addr", "", "a@b.com", "IT", "Mgr", "50000");
        Assert.False(valid); Assert.Contains("Mobile No is required", errors);
    }

    [Fact] public void ValidateEmployee_EmptyEmail_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateEmployee("John", "Addr", "123", "", "IT", "Mgr", "50000");
        Assert.False(valid); Assert.Contains("Email is required", errors);
    }

    [Fact] public void ValidateEmployee_InvalidEmail_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateEmployee("John", "Addr", "123", "notanemail", "IT", "Mgr", "50000");
        Assert.False(valid); Assert.Contains("Invalid email format", errors);
    }

    [Fact] public void ValidateEmployee_EmptyDepartment_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateEmployee("John", "Addr", "123", "a@b.com", "", "Mgr", "50000");
        Assert.False(valid); Assert.Contains("Department is required", errors);
    }

    [Fact] public void ValidateEmployee_NonAlphaDepartment_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateEmployee("John", "Addr", "123", "a@b.com", "IT123", "Mgr", "50000");
        Assert.False(valid); Assert.Contains("Department must contain only letters", errors);
    }

    [Fact] public void ValidateEmployee_EmptyDesignation_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateEmployee("John", "Addr", "123", "a@b.com", "IT", "", "50000");
        Assert.False(valid); Assert.Contains("Designation is required", errors);
    }

    [Fact] public void ValidateEmployee_NonAlphaDesignation_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateEmployee("John", "Addr", "123", "a@b.com", "IT", "Mgr123", "50000");
        Assert.False(valid); Assert.Contains("Designation must contain only letters", errors);
    }

    [Fact] public void ValidateEmployee_EmptySalary_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateEmployee("John", "Addr", "123", "a@b.com", "IT", "Mgr", "");
        Assert.False(valid); Assert.Contains("Salary is required", errors);
    }

    [Fact] public void ValidateEmployee_NonNumericSalary_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateEmployee("John", "Addr", "123", "a@b.com", "IT", "Mgr", "abc");
        Assert.False(valid); Assert.Contains("Salary must be a valid number", errors);
    }

    [Fact] public void ValidateEmployee_AllEmpty_ReturnsMultipleErrors()
    {
        var (valid, errors) = _svc.ValidateEmployee("", "", "", "", "", "", "");
        Assert.False(valid); Assert.True(errors.Count >= 7);
    }

    [Fact] public void ValidateEmployee_NameWithSpaces_ReturnsTrue()
    {
        var (valid, _) = _svc.ValidateEmployee("John Doe", "Addr", "123", "john@test.com", "IT", "Manager", "50000");
        Assert.True(valid);
    }

    [Fact] public void ValidateEmployee_DecimalSalary_ReturnsTrue()
    {
        var (valid, _) = _svc.ValidateEmployee("John", "Addr", "123", "john@test.com", "IT", "Mgr", "50000.50");
        Assert.True(valid);
    }

    // ============= ATTENDANCE VALIDATION (4 tests) =============

    [Fact] public void ValidateAttendance_AllValid_ReturnsTrue()
    {
        var (valid, _) = _svc.ValidateAttendance("E-123456", "P");
        Assert.True(valid);
    }

    [Fact] public void ValidateAttendance_EmptyEmployeeId_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateAttendance("", "P");
        Assert.False(valid); Assert.Contains("Employee ID is required", errors);
    }

    [Fact] public void ValidateAttendance_EmptyStatus_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateAttendance("E-123456", "");
        Assert.False(valid); Assert.Contains("Status is required", errors);
    }

    [Fact] public void ValidateAttendance_InvalidStatus_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateAttendance("E-123456", "X");
        Assert.False(valid); Assert.Contains("Status must be P or A", errors);
    }

    // ============= PAYMENT VALIDATION (10 tests) =============

    [Fact] public void ValidatePayment_AllValid_ReturnsTrue()
    {
        var (valid, _) = _svc.ValidatePayment("E-123456", "200", "Cash", 500m, 1000m, 25000m);
        Assert.True(valid);
    }

    [Fact] public void ValidatePayment_EmptyEmployeeId_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidatePayment("", "200", "Cash", 0m, 0m, 25000m);
        Assert.False(valid); Assert.Contains("Employee ID is required", errors);
    }

    [Fact] public void ValidatePayment_EmptyOvertimeRate_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidatePayment("E-123456", "", "Cash", 0m, 0m, 25000m);
        Assert.False(valid); Assert.Contains("Overtime Rate is required", errors);
    }

    [Fact] public void ValidatePayment_EmptyPaymentMode_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidatePayment("E-123456", "200", "", 0m, 0m, 25000m);
        Assert.False(valid); Assert.Contains("Mode of Payment is required", errors);
    }

    [Fact] public void ValidatePayment_DeductionExceedsAdvance_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidatePayment("E-123456", "200", "Cash", 2000m, 1000m, 25000m);
        Assert.False(valid); Assert.Contains("Deduction cannot exceed Advance", errors);
    }

    [Fact] public void ValidatePayment_NegativeNetPay_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidatePayment("E-123456", "200", "Cash", 0m, 0m, -100m);
        Assert.False(valid); Assert.Contains("Net Pay must be greater than or equal to 0", errors);
    }

    [Fact] public void ValidatePayment_ZeroNetPay_ReturnsTrue()
    {
        var (valid, _) = _svc.ValidatePayment("E-123456", "200", "Cash", 0m, 0m, 0m);
        Assert.True(valid);
    }

    [Fact] public void ValidatePayment_EqualDeductionAdvance_ReturnsTrue()
    {
        var (valid, _) = _svc.ValidatePayment("E-123456", "200", "Cash", 1000m, 1000m, 25000m);
        Assert.True(valid);
    }

    [Fact] public void ValidatePayment_AllEmpty_ReturnsMultipleErrors()
    {
        var (valid, errors) = _svc.ValidatePayment("", "", "", 2000m, 1000m, -100m);
        Assert.False(valid); Assert.True(errors.Count >= 3);
    }

    [Fact] public void ValidatePayment_LargeNetPay_ReturnsTrue()
    {
        var (valid, _) = _svc.ValidatePayment("E-123456", "500", "Bank Transfer", 0m, 0m, 500000m);
        Assert.True(valid);
    }

    // ============= PURCHASE INVENTORY VALIDATION (12 tests) =============

    [Fact] public void ValidatePurchaseInventory_AllValid_ReturnsTrue()
    {
        var (valid, _) = _svc.ValidatePurchaseInventory("Rice", "Food", "Purchase", "ABC Supplier", "100", "Kg");
        Assert.True(valid);
    }

    [Fact] public void ValidatePurchaseInventory_EmptyProduct_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidatePurchaseInventory("", "Food", "Purchase", "ABC", "100", "Kg");
        Assert.False(valid); Assert.Contains("Product Name is required", errors);
    }

    [Fact] public void ValidatePurchaseInventory_EmptyCategory_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidatePurchaseInventory("Rice", "", "Purchase", "ABC", "100", "Kg");
        Assert.False(valid); Assert.Contains("Category is required", errors);
    }

    [Fact] public void ValidatePurchaseInventory_EmptyTransactionType_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidatePurchaseInventory("Rice", "Food", "", "ABC", "100", "Kg");
        Assert.False(valid); Assert.Contains("Transaction Type is required", errors);
    }

    [Fact] public void ValidatePurchaseInventory_EmptyPartyName_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidatePurchaseInventory("Rice", "Food", "Purchase", "", "100", "Kg");
        Assert.False(valid); Assert.Contains("Party Name is required", errors);
    }

    [Fact] public void ValidatePurchaseInventory_EmptyQuantity_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidatePurchaseInventory("Rice", "Food", "Purchase", "ABC", "", "Kg");
        Assert.False(valid); Assert.Contains("Quantity is required", errors);
    }

    [Fact] public void ValidatePurchaseInventory_NonNumericQuantity_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidatePurchaseInventory("Rice", "Food", "Purchase", "ABC", "abc", "Kg");
        Assert.False(valid); Assert.Contains("Quantity must be numeric", errors);
    }

    [Fact] public void ValidatePurchaseInventory_EmptyUnit_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidatePurchaseInventory("Rice", "Food", "Purchase", "ABC", "100", "");
        Assert.False(valid); Assert.Contains("Unit is required", errors);
    }

    [Fact] public void ValidatePurchaseInventory_AllEmpty_ReturnsMultipleErrors()
    {
        var (valid, errors) = _svc.ValidatePurchaseInventory("", "", "", "", "", "");
        Assert.False(valid); Assert.True(errors.Count >= 6);
    }

    [Fact] public void ValidatePurchaseInventory_WhitespaceFields_ReturnsFalse()
    {
        var (valid, _) = _svc.ValidatePurchaseInventory("  ", "  ", "  ", "  ", "  ", "  ");
        Assert.False(valid);
    }

    [Fact] public void ValidatePurchaseInventory_ValidQuantity_ReturnsTrue()
    {
        var (valid, _) = _svc.ValidatePurchaseInventory("Rice", "Food", "Purchase", "ABC", "999", "Kg");
        Assert.True(valid);
    }

    [Fact] public void ValidatePurchaseInventory_QuantityWithDecimals_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidatePurchaseInventory("Rice", "Food", "Purchase", "ABC", "10.5", "Kg");
        Assert.False(valid); Assert.Contains("Quantity must be numeric", errors);
    }

    // ============= STOCK VALIDATION (10 tests) =============

    [Fact] public void ValidateStock_AllValid_ReturnsTrue()
    {
        var (valid, _) = _svc.ValidateStock("Whisky", "10", "750");
        Assert.True(valid);
    }

    [Fact] public void ValidateStock_EmptyName_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateStock("", "10", "750");
        Assert.False(valid); Assert.Contains("Liquor Name is required", errors);
    }

    [Fact] public void ValidateStock_EmptyBottles_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateStock("Whisky", "", "750");
        Assert.False(valid); Assert.Contains("No of Bottles is required", errors);
    }

    [Fact] public void ValidateStock_NonNumericBottles_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateStock("Whisky", "abc", "750");
        Assert.False(valid); Assert.Contains("No of Bottles must be numeric", errors);
    }

    [Fact] public void ValidateStock_EmptyVolume_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateStock("Whisky", "10", "");
        Assert.False(valid); Assert.Contains("Volume is required", errors);
    }

    [Fact] public void ValidateStock_AllEmpty_Returns3Errors()
    {
        var (valid, errors) = _svc.ValidateStock("", "", "");
        Assert.False(valid); Assert.Equal(3, errors.Count);
    }

    [Fact] public void ValidateStockBeer_AllValid_ReturnsTrue()
    {
        var (valid, _) = _svc.ValidateStockBeer("1", "10");
        Assert.True(valid);
    }

    [Fact] public void ValidateStockBeer_EmptyBeer_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateStockBeer("", "10");
        Assert.False(valid); Assert.Contains("Beer must be selected", errors);
    }

    [Fact] public void ValidateStockBeer_EmptyBottles_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateStockBeer("1", "");
        Assert.False(valid); Assert.Contains("No of Bottles is required", errors);
    }

    [Fact] public void ValidateStockBeer_NonNumericBottles_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateStockBeer("1", "abc");
        Assert.False(valid); Assert.Contains("No of Bottles must be numeric", errors);
    }

    // ============= CURRENCY VALIDATION (4 tests) =============

    [Fact] public void ValidateCurrency_Valid_ReturnsTrue()
    {
        var (valid, _) = _svc.ValidateCurrency("USD");
        Assert.True(valid);
    }

    [Fact] public void ValidateCurrency_Empty_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateCurrency("");
        Assert.False(valid); Assert.Contains("Currency Name is required", errors);
    }

    [Fact] public void ValidateCurrency_Whitespace_ReturnsFalse()
    {
        var (valid, _) = _svc.ValidateCurrency("  ");
        Assert.False(valid);
    }

    [Fact] public void ValidateCurrency_LongName_ReturnsTrue()
    {
        var (valid, _) = _svc.ValidateCurrency("United States Dollar");
        Assert.True(valid);
    }

    // ============= TAX INFO VALIDATION (4 tests) =============

    [Fact] public void ValidateTaxInfo_Valid_ReturnsTrue()
    {
        var (valid, _) = _svc.ValidateTaxInfo("0-50000", "10");
        Assert.True(valid);
    }

    [Fact] public void ValidateTaxInfo_EmptySalary_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateTaxInfo("", "10");
        Assert.False(valid); Assert.Contains("Salary range is required", errors);
    }

    [Fact] public void ValidateTaxInfo_EmptyTax_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateTaxInfo("0-50000", "");
        Assert.False(valid); Assert.Contains("Tax Percentage is required", errors);
    }

    [Fact] public void ValidateTaxInfo_NonNumericTax_ReturnsFalse()
    {
        var (valid, errors) = _svc.ValidateTaxInfo("0-50000", "abc");
        Assert.False(valid); Assert.Contains("Tax Percentage must be a valid number", errors);
    }

    // ============= INPUT RESTRICTION TESTS (20 tests) =============

    [Theory]
    [InlineData("123456", true)]
    [InlineData("0", true)]
    [InlineData("abc", false)]
    [InlineData("12.5", false)]
    [InlineData("", false)]
    [InlineData("123 456", false)]
    public void IsNumericOnly_Various(string input, bool expected)
    {
        Assert.Equal(expected, _svc.IsNumericOnly(input));
    }

    [Theory]
    [InlineData("John", true)]
    [InlineData("John Doe", true)]
    [InlineData("John123", false)]
    [InlineData("", false)]
    [InlineData("John!@#", false)]
    public void IsAlphaOnly_Various(string input, bool expected)
    {
        Assert.Equal(expected, _svc.IsAlphaOnly(input));
    }

    [Theory]
    [InlineData("123.45", true)]
    [InlineData("0", true)]
    [InlineData("abc", false)]
    [InlineData("", false)]
    public void IsDecimalInput_Various(string input, bool expected)
    {
        Assert.Equal(expected, _svc.IsDecimalInput(input));
    }

    [Theory]
    [InlineData("john@test.com", true)]
    [InlineData("john.doe@company.org", true)]
    [InlineData("notanemail", false)]
    [InlineData("@test.com", false)]
    [InlineData("", false)]
    public void IsValidEmail_Various(string input, bool expected)
    {
        Assert.Equal(expected, _svc.IsValidEmail(input));
    }
}
