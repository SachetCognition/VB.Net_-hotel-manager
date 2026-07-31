using ClosedXML.Excel;
using HotelManagement.Core.Models;

namespace HotelManagement.Reports;

/// <summary>
/// Excel export service replacing Microsoft.Office.Interop.Excel with ClosedXML.
/// </summary>
public class ExcelExportService
{
    public byte[] ExportEmployeesToExcel(IEnumerable<Employee> employees)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Employees");

        worksheet.Cell(1, 1).Value = "Employee ID";
        worksheet.Cell(1, 2).Value = "Name";
        worksheet.Cell(1, 3).Value = "Address";
        worksheet.Cell(1, 4).Value = "Mobile No";
        worksheet.Cell(1, 5).Value = "Email";
        worksheet.Cell(1, 6).Value = "Department";
        worksheet.Cell(1, 7).Value = "Designation";
        worksheet.Cell(1, 8).Value = "Date of Joining";
        worksheet.Cell(1, 9).Value = "Salary";

        var headerRange = worksheet.Range(1, 1, 1, 9);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;

        int row = 2;
        foreach (var emp in employees)
        {
            worksheet.Cell(row, 1).Value = emp.EmployeeID;
            worksheet.Cell(row, 2).Value = emp.EmployeeName;
            worksheet.Cell(row, 3).Value = emp.Address;
            worksheet.Cell(row, 4).Value = emp.MobileNo;
            worksheet.Cell(row, 5).Value = emp.Email;
            worksheet.Cell(row, 6).Value = emp.Department;
            worksheet.Cell(row, 7).Value = emp.Designation;
            worksheet.Cell(row, 8).Value = emp.DateOfJoining.ToString("dd-MMM-yyyy");
            worksheet.Cell(row, 9).Value = emp.Salary;
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportGuestsToExcel(IEnumerable<Guest> guests)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Guests");

        worksheet.Cell(1, 1).Value = "Guest ID";
        worksheet.Cell(1, 2).Value = "Name";
        worksheet.Cell(1, 3).Value = "Address";
        worksheet.Cell(1, 4).Value = "City";
        worksheet.Cell(1, 5).Value = "Contact No";
        worksheet.Cell(1, 6).Value = "ID Type";
        worksheet.Cell(1, 7).Value = "ID Number";

        var headerRange = worksheet.Range(1, 1, 1, 7);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;

        int row = 2;
        foreach (var g in guests)
        {
            worksheet.Cell(row, 1).Value = g.GuestID;
            worksheet.Cell(row, 2).Value = g.GuestName;
            worksheet.Cell(row, 3).Value = g.Address;
            worksheet.Cell(row, 4).Value = g.City;
            worksheet.Cell(row, 5).Value = g.ContactNo;
            worksheet.Cell(row, 6).Value = g.IDType;
            worksheet.Cell(row, 7).Value = g.IDNumber;
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportPurchaseInventoryToExcel(IEnumerable<PurchasedInventory> items)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Purchase Inventory");

        worksheet.Cell(1, 1).Value = "ID";
        worksheet.Cell(1, 2).Value = "Product Name";
        worksheet.Cell(1, 3).Value = "Category";
        worksheet.Cell(1, 4).Value = "Transaction Type";
        worksheet.Cell(1, 5).Value = "Party Name";
        worksheet.Cell(1, 6).Value = "Quantity";
        worksheet.Cell(1, 7).Value = "Unit";
        worksheet.Cell(1, 8).Value = "Price";
        worksheet.Cell(1, 9).Value = "Total Price";
        worksheet.Cell(1, 10).Value = "Purchase Date";

        var headerRange = worksheet.Range(1, 1, 1, 10);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;

        int row = 2;
        foreach (var item in items)
        {
            worksheet.Cell(row, 1).Value = item.ID;
            worksheet.Cell(row, 2).Value = item.ProductName;
            worksheet.Cell(row, 3).Value = item.Category;
            worksheet.Cell(row, 4).Value = item.TransactionType;
            worksheet.Cell(row, 5).Value = item.PartyName;
            worksheet.Cell(row, 6).Value = item.Quantity;
            worksheet.Cell(row, 7).Value = item.Unit;
            worksheet.Cell(row, 8).Value = item.Price;
            worksheet.Cell(row, 9).Value = item.TotalPrice;
            worksheet.Cell(row, 10).Value = item.PurchaseDate.ToString("dd-MMM-yyyy");
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
