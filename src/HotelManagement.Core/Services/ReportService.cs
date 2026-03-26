using HotelManagement.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using HotelManagement.Core.Entities;
using ClosedXML.Excel;

namespace HotelManagement.Core.Services;

/// <summary>
/// Report Service generating PDF-equivalent reports using ClosedXML for Excel output.
/// Replaces legacy Crystal Reports (rpt* files) with Excel-based reports that preserve
/// the same data fields, groupings, and totals.
/// </summary>
public class ReportService : IReportService
{
    private readonly DbContext _context;

    public ReportService(DbContext context)
    {
        _context = context;
    }

    public async Task<byte[]> GenerateGuestReportAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Set<Guest>().AsQueryable();
        var guests = await query.OrderBy(g => g.GuestName).ToListAsync();
        return GenerateExcelReport("Guest Report", guests, new[] {
            "GuestID", "GuestName", "Address", "City", "ContactNo", "IDType", "IDNumber"
        });
    }

    public async Task<byte[]> GenerateEmployeeReportAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var employees = await _context.Set<Employee>().OrderBy(e => e.EmployeeName).ToListAsync();
        return GenerateExcelReport("Employee Report", employees, new[] {
            "EmployeeID", "EmployeeName", "Department", "Designation", "MobileNo", "Email", "Salary"
        });
    }

    public async Task<byte[]> GenerateCheckInReportAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Set<CheckInRoom>().AsQueryable();
        if (fromDate.HasValue) query = query.Where(c => c.DateIN >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(c => c.DateOUT <= toDate.Value);
        var data = await query.OrderByDescending(c => c.DateIN).ToListAsync();
        return GenerateExcelReport("Check-In Report", data, new[] {
            "GuestID", "GuestName", "RoomNo", "DateIN", "DateOUT", "NoOfDays",
            "RoomCharges", "TotalRoomCharges", "GrandTotal", "Status"
        });
    }

    public async Task<byte[]> GenerateCheckOutReportAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Set<CheckoutRoom>().AsQueryable();
        if (fromDate.HasValue) query = query.Where(c => c.CheckOutDate >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(c => c.CheckOutDate <= toDate.Value);
        var data = await query.OrderByDescending(c => c.CheckOutDate).ToListAsync();
        return GenerateExcelReport("Check-Out Report", data, new[] {
            "BillNo", "GuestID", "GuestName", "RoomNo", "DateIN", "DateOUT",
            "GrandTotal", "TotalPaid", "Balance", "CheckOutDate"
        });
    }

    public async Task<byte[]> GenerateReservationReportAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Set<Reservation>().AsQueryable();
        if (fromDate.HasValue) query = query.Where(r => r.DateIN >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(r => r.DateOUT <= toDate.Value);
        var data = await query.OrderByDescending(r => r.DateIN).ToListAsync();
        return GenerateExcelReport("Reservation Report", data, new[] {
            "GuestID", "GuestName", "RoomNo", "RoomType", "DateIN", "DateOUT", "Status"
        });
    }

    public async Task<byte[]> GenerateTransactionReportAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Set<Transaction>().AsQueryable();
        if (fromDate.HasValue) query = query.Where(t => t.TransactionDate >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(t => t.TransactionDate <= toDate.Value);
        var data = await query.OrderByDescending(t => t.TransactionDate).ToListAsync();
        return GenerateExcelReport("Transaction Report", data, new[] {
            "GuestID", "GuestName", "TransactionType", "Amount", "TransactionDate", "Currency"
        });
    }

    public async Task<byte[]> GenerateAttendanceReportAsync(string? employeeId = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Set<EmployeeAttendance>().AsQueryable();
        if (!string.IsNullOrWhiteSpace(employeeId)) query = query.Where(a => a.EmployeeID == employeeId);
        if (fromDate.HasValue) query = query.Where(a => a.WorkingDate >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(a => a.WorkingDate <= toDate.Value);
        var data = await query.OrderByDescending(a => a.WorkingDate).ToListAsync();
        return GenerateExcelReport("Attendance Report", data, new[] {
            "EmployeeID", "EmployeeName", "WorkingDate", "Status", "Overtime", "Department"
        });
    }

    public async Task<byte[]> GenerateOvertimeReportAsync(string? employeeId = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Set<EmployeeAttendance>().Where(a => a.Overtime != "00:00:00");
        if (!string.IsNullOrWhiteSpace(employeeId)) query = query.Where(a => a.EmployeeID == employeeId);
        if (fromDate.HasValue) query = query.Where(a => a.WorkingDate >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(a => a.WorkingDate <= toDate.Value);
        var data = await query.OrderByDescending(a => a.WorkingDate).ToListAsync();
        return GenerateExcelReport("Overtime Report", data, new[] {
            "EmployeeID", "EmployeeName", "WorkingDate", "Overtime", "Department"
        });
    }

    public async Task<byte[]> GenerateAdvancePaymentReportAsync(string? employeeId = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Set<AdvanceEntry>().AsQueryable();
        if (!string.IsNullOrWhiteSpace(employeeId)) query = query.Where(a => a.EmployeeID == employeeId);
        if (fromDate.HasValue) query = query.Where(a => a.WorkingDate >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(a => a.WorkingDate <= toDate.Value);
        var data = await query.OrderByDescending(a => a.WorkingDate).ToListAsync();
        return GenerateExcelReport("Advance Payment Report", data, new[] {
            "EmployeeID", "EmployeeName", "WorkingDate", "Amount", "Deduction"
        });
    }

    public async Task<byte[]> GenerateEmployeePaymentReportAsync(string? employeeId = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Set<EmployeePayment>().AsQueryable();
        if (!string.IsNullOrWhiteSpace(employeeId)) query = query.Where(p => p.EmployeeID == employeeId);
        if (fromDate.HasValue) query = query.Where(p => p.PaymentDate >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(p => p.PaymentDate <= toDate.Value);
        var data = await query.OrderByDescending(p => p.PaymentDate).ToListAsync();
        return GenerateExcelReport("Employee Payment Report", data, new[] {
            "PaymentID", "EmployeeID", "EmployeeName", "PaymentDate", "BasicSalary",
            "PresentDays", "Salary", "OvertimeAmount", "Deduction", "NetPay"
        });
    }

    public async Task<byte[]> GenerateSalarySlipAsync(int paymentId)
    {
        var payment = await _context.Set<EmployeePayment>().FindAsync(paymentId)
            ?? throw new KeyNotFoundException($"Payment {paymentId} not found");
        return GenerateExcelReport("Salary Slip", new[] { payment }, new[] {
            "PaymentID", "EmployeeID", "EmployeeName", "Department", "Designation",
            "PaymentDate", "BasicSalary", "PresentDays", "Salary",
            "TotalOvertime", "OvertimeRate", "OvertimeAmount", "Advance", "Deduction", "NetPay"
        });
    }

    public async Task<byte[]> GenerateSalarySlipsReportAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Set<EmployeePayment>().AsQueryable();
        if (fromDate.HasValue) query = query.Where(p => p.PaymentDate >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(p => p.PaymentDate <= toDate.Value);
        var data = await query.OrderByDescending(p => p.PaymentDate).ToListAsync();
        return GenerateExcelReport("Salary Slips Report", data, new[] {
            "PaymentID", "EmployeeID", "EmployeeName", "PaymentDate", "Salary", "NetPay"
        });
    }

    public async Task<byte[]> GeneratePurchasedInventoryReportAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Set<PurchaseInventory>().AsQueryable();
        if (fromDate.HasValue) query = query.Where(p => p.PurchaseDate >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(p => p.PurchaseDate <= toDate.Value);
        var data = await query.OrderByDescending(p => p.PurchaseDate).ToListAsync();
        return GenerateExcelReport("Purchased Inventory Report", data, new[] {
            "ItemName", "Category", "Quantity", "Rate", "TotalAmount", "PurchaseDate", "Supplier"
        });
    }

    public async Task<byte[]> GenerateRoomInvoiceAsync(int checkoutId)
    {
        var checkout = await _context.Set<CheckoutRoom>().FindAsync(checkoutId)
            ?? throw new KeyNotFoundException($"Checkout {checkoutId} not found");
        return GenerateExcelReport("Room Invoice", new[] { checkout }, new[] {
            "BillNo", "GuestID", "GuestName", "RoomNo", "DateIN", "DateOUT", "NoOfDays",
            "RoomCharges", "TotalRoomCharges", "OtherCharges", "Discount", "SubTotal",
            "ServiceTaxAmount", "LuxuryTaxAmount", "EducessTaxAmount", "HEducessTaxAmount",
            "GrandTotal", "TotalPaid", "Balance"
        });
    }

    public async Task<byte[]> GenerateHallAndGardenInvoiceAsync(int reservationId)
    {
        var reservation = await _context.Set<ReservationHallAndGarden>().FindAsync(reservationId)
            ?? throw new KeyNotFoundException($"Reservation {reservationId} not found");
        return GenerateExcelReport("Hall and Garden Invoice", new[] { reservation }, new[] {
            "GuestID", "GuestName", "HallName", "GardenName", "DateIN", "DateOUT",
            "TotalHall", "TotalGarden", "SubTotal", "GrandTotal", "TotalPaid", "Balance"
        });
    }

    public async Task<byte[]> GenerateHallOrGardenInvoiceAsync(int reservationId)
    {
        var reservation = await _context.Set<ReservationHallOrGarden>().FindAsync(reservationId)
            ?? throw new KeyNotFoundException($"Reservation {reservationId} not found");
        return GenerateExcelReport("Hall/Garden Invoice", new[] { reservation }, new[] {
            "GuestID", "GuestName", "VenueName", "VenueType", "DateIN", "DateOUT",
            "TotalCharges", "SubTotal", "GrandTotal", "TotalPaid", "Balance"
        });
    }

    public async Task<byte[]> GenerateOrderInvoiceAsync(int orderId)
    {
        var order = await _context.Set<Order>().FindAsync(orderId)
            ?? throw new KeyNotFoundException($"Order {orderId} not found");
        return GenerateExcelReport("Order Invoice", new[] { order }, new[] {
            "GuestID", "GuestName", "RoomNo", "ItemName", "Category",
            "Quantity", "Rate", "TotalAmount", "OrderDate"
        });
    }

    public async Task<byte[]> GenerateRestaurantOrderReceiptAsync(int orderId)
    {
        var order = await _context.Set<RestaurantOrder>().FindAsync(orderId)
            ?? throw new KeyNotFoundException($"Restaurant order {orderId} not found");
        return GenerateExcelReport("Restaurant Order Receipt", new[] { order }, new[] {
            "CustomerName", "ItemName", "Category", "Quantity", "Rate", "TotalAmount", "OrderDate"
        });
    }

    private static byte[] GenerateExcelReport<T>(string title, IEnumerable<T> data, string[] columns)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add(title.Length > 31 ? title[..31] : title);

        // Title row
        ws.Cell(1, 1).Value = title;
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;
        ws.Range(1, 1, 1, columns.Length).Merge();

        // Header row
        for (int i = 0; i < columns.Length; i++)
        {
            var cell = ws.Cell(3, i + 1);
            cell.Value = columns[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightBlue;
            cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        }

        // Data rows
        var items = data.ToList();
        var type = typeof(T);
        for (int row = 0; row < items.Count; row++)
        {
            for (int col = 0; col < columns.Length; col++)
            {
                var prop = type.GetProperty(columns[col]);
                if (prop == null) continue;
                var value = prop.GetValue(items[row]);
                var cell = ws.Cell(row + 4, col + 1);

                if (value is DateTime dt)
                    cell.Value = dt;
                else if (value is decimal dec)
                    cell.Value = dec;
                else if (value is int intVal)
                    cell.Value = intVal;
                else if (value is double dbl)
                    cell.Value = dbl;
                else
                    cell.Value = value?.ToString() ?? string.Empty;
            }
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
