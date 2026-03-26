using ClosedXML.Excel;
using HotelManagement.Core.Interfaces;

namespace HotelManagement.Core.Services;

/// <summary>
/// Excel export service using ClosedXML (replaces legacy COM Interop with Microsoft.Office.Interop.Excel).
/// No Microsoft Office installation required.
/// </summary>
public class ExcelExportService : IExcelExportService
{
    public byte[] ExportToExcel<T>(IEnumerable<T> data, string sheetName = "Sheet1") where T : class
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(sheetName);

        var properties = typeof(T).GetProperties();

        // Header row
        for (int i = 0; i < properties.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = properties[i].Name;
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightBlue;
        }

        // Data rows
        var items = data.ToList();
        for (int row = 0; row < items.Count; row++)
        {
            for (int col = 0; col < properties.Length; col++)
            {
                var value = properties[col].GetValue(items[row]);
                var cell = worksheet.Cell(row + 2, col + 1);

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

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
