namespace HotelManagement.Core.Interfaces;

public interface IExcelExportService
{
    byte[] ExportToExcel<T>(IEnumerable<T> data, string sheetName = "Sheet1") where T : class;
}
