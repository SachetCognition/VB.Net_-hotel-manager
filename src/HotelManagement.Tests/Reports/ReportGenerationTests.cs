using HotelManagement.Core.Models;
using HotelManagement.Reports;
using Xunit;

namespace HotelManagement.Tests.Reports;

/// <summary>
/// Category 10: Report Generation Tests (~16 tests)
/// Tests QuestPDF report and ClosedXML Excel export generation.
/// </summary>
public class ReportGenerationTests
{
    private readonly ReportService _reportService = new();
    private readonly ExcelExportService _excelService = new();

    // ============= PDF REPORT TESTS =============

    // TC-RPT-001: Generate room invoice PDF
    [Fact]
    public void GenerateRoomInvoice_ShouldReturnBytes()
    {
        var checkIn = CreateSampleCheckIn();
        var guest = CreateSampleGuest();
        var hotel = CreateSampleHotel();
        var result = _reportService.GenerateRoomInvoice(checkIn, guest, hotel, "B-001");
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    // TC-RPT-002: Room invoice should be valid PDF (starts with %PDF)
    [Fact]
    public void GenerateRoomInvoice_ShouldBeValidPdf()
    {
        var result = _reportService.GenerateRoomInvoice(CreateSampleCheckIn(), CreateSampleGuest(), CreateSampleHotel(), "B-001");
        Assert.Equal(0x25, result[0]); // '%'
        Assert.Equal(0x50, result[1]); // 'P'
        Assert.Equal(0x44, result[2]); // 'D'
        Assert.Equal(0x46, result[3]); // 'F'
    }

    // TC-RPT-003: Generate guest report PDF
    [Fact]
    public void GenerateGuestReport_ShouldReturnBytes()
    {
        var guests = new List<Guest> { CreateSampleGuest() };
        var result = _reportService.GenerateGuestReport(guests);
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    // TC-RPT-004: Guest report with empty list
    [Fact]
    public void GenerateGuestReport_EmptyList_ShouldStillGenerate()
    {
        var result = _reportService.GenerateGuestReport(new List<Guest>());
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    // TC-RPT-005: Generate employee report PDF
    [Fact]
    public void GenerateEmployeeReport_ShouldReturnBytes()
    {
        var employees = new List<Employee> { CreateSampleEmployee() };
        var result = _reportService.GenerateEmployeeReport(employees);
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    // TC-RPT-006: Employee report with empty list
    [Fact]
    public void GenerateEmployeeReport_EmptyList_ShouldStillGenerate()
    {
        var result = _reportService.GenerateEmployeeReport(new List<Employee>());
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    // TC-RPT-007: Generate salary slip PDF
    [Fact]
    public void GenerateSalarySlip_ShouldReturnBytes()
    {
        var payment = CreateSamplePayment();
        var employee = CreateSampleEmployee();
        var result = _reportService.GenerateSalarySlip(payment, employee);
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    // TC-RPT-008: Salary slip should be valid PDF
    [Fact]
    public void GenerateSalarySlip_ShouldBeValidPdf()
    {
        var result = _reportService.GenerateSalarySlip(CreateSamplePayment(), CreateSampleEmployee());
        Assert.Equal(0x25, result[0]); // '%'
        Assert.Equal(0x50, result[1]); // 'P'
    }

    // ============= EXCEL EXPORT TESTS =============

    // TC-RPT-009: Export employees to Excel
    [Fact]
    public void ExportEmployeesToExcel_ShouldReturnBytes()
    {
        var employees = new List<Employee> { CreateSampleEmployee() };
        var result = _excelService.ExportEmployeesToExcel(employees);
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    // TC-RPT-010: Export employees to Excel with empty list
    [Fact]
    public void ExportEmployeesToExcel_EmptyList()
    {
        var result = _excelService.ExportEmployeesToExcel(new List<Employee>());
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    // TC-RPT-011: Export guests to Excel
    [Fact]
    public void ExportGuestsToExcel_ShouldReturnBytes()
    {
        var guests = new List<Guest> { CreateSampleGuest() };
        var result = _excelService.ExportGuestsToExcel(guests);
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    // TC-RPT-012: Export guests to Excel with empty list
    [Fact]
    public void ExportGuestsToExcel_EmptyList()
    {
        var result = _excelService.ExportGuestsToExcel(new List<Guest>());
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    // TC-RPT-013: Export purchase inventory to Excel
    [Fact]
    public void ExportPurchaseInventoryToExcel_ShouldReturnBytes()
    {
        var items = new List<PurchasedInventory>
        {
            new PurchasedInventory { ID = 1, ProductName = "Rice", Category = "Food", TransactionType = "Purchase", PartyName = "ABC", Quantity = 100, Unit = "Kg", Price = 50m, TotalPrice = 5000m, PurchaseDate = DateTime.Now }
        };
        var result = _excelService.ExportPurchaseInventoryToExcel(items);
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    // TC-RPT-014: Export purchase inventory empty list
    [Fact]
    public void ExportPurchaseInventoryToExcel_EmptyList()
    {
        var result = _excelService.ExportPurchaseInventoryToExcel(new List<PurchasedInventory>());
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    // TC-RPT-015: Excel file should be valid xlsx (ZIP format starts with PK)
    [Fact]
    public void ExcelExport_ShouldBeValidXlsx()
    {
        var result = _excelService.ExportEmployeesToExcel(new List<Employee> { CreateSampleEmployee() });
        Assert.Equal(0x50, result[0]); // 'P'
        Assert.Equal(0x4B, result[1]); // 'K'
    }

    // TC-RPT-016: Multiple employees in report
    [Fact]
    public void GenerateEmployeeReport_MultipleEmployees()
    {
        var employees = new List<Employee>
        {
            CreateSampleEmployee(),
            new Employee { EmployeeID = "E-999998", EmployeeName = "Jane Doe", Address = "456 Oak", MobileNo = "9876543210", Email = "jane@test.com", Department = "HR", Designation = "Manager", DateOfJoining = DateTime.Now.AddYears(-2), Salary = 60000m, BasicWorkingTime = new TimeSpan(8, 0, 0) }
        };
        var result = _reportService.GenerateEmployeeReport(employees);
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    // ============= HELPERS =============

    private static Guest CreateSampleGuest() => new()
    {
        GuestID = "G-999999",
        GuestName = "Test Guest",
        Address = "123 Test St",
        City = "Test City",
        ContactNo = "1234567890",
        IDType = "Passport",
        IDNumber = "AB123456"
    };

    private static Employee CreateSampleEmployee() => new()
    {
        EmployeeID = "E-999999",
        EmployeeName = "Test Employee",
        Address = "456 Test Ave",
        MobileNo = "0987654321",
        Email = "test@test.com",
        Department = "IT",
        Designation = "Developer",
        DateOfJoining = DateTime.Now.AddYears(-1),
        Salary = 50000m,
        BasicWorkingTime = new TimeSpan(8, 0, 0)
    };

    private static HotelInfo CreateSampleHotel() => new()
    {
        ID = 1,
        HotelName = "Grand Test Hotel",
        Address = "789 Hotel Blvd",
        ContactNo = "5555555555",
        Email = "info@grandtest.com",
        TIN = "TIN123456",
        STNo = "ST123456"
    };

    private static CheckInRoom CreateSampleCheckIn() => new()
    {
        ID = 1,
        GuestID = "G-999999",
        RoomNo = "101",
        RoomCharges = 5000m,
        DateIN = DateTime.Now.AddDays(-3),
        DateOUT = DateTime.Now,
        NoOfDays = 3,
        TotalRoomCharges = 15000m,
        OtherCharges = 0m,
        DiscountPer = 0m,
        Discount = 0m,
        SubTotal = 15000m,
        ServiceTaxPer = 10m,
        ServiceTaxAmount = 1500m,
        LuxuryTaxPer = 5m,
        LuxuryTaxAmount = 825m,
        GrandTotal = 17325m,
        TotalPaid = 17325m,
        Balance = 0m,
        Status = "Checked In",
        ExtraBed = false,
        CurrencyID = "1"
    };

    private static EmployeePayment CreateSamplePayment() => new()
    {
        PaymentID = "P-000001",
        EmployeeID = "E-999999",
        DateFrom = DateTime.Now.AddDays(-30),
        DateTo = DateTime.Now,
        PresentDays = 25,
        Salary = 41666.67m,
        Advance = 0m,
        Deduction = 0m,
        OverTime = new TimeSpan(0, 10, 0),
        OverTimeRate = 100m,
        OverTimeAmount = 1000m,
        NetPay = 42666.67m,
        PaymentDate = DateTime.Now,
        ModeOfPayment = "Cash"
    };
}
