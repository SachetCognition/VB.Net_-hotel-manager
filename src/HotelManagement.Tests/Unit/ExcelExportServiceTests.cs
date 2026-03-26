using ClosedXML.Excel;
using FluentAssertions;
using HotelManagement.Core.Services;
using Xunit;

namespace HotelManagement.Tests.Unit;

public class ExcelExportServiceTests
{
    private readonly ExcelExportService _service = new();

    private record TestData(int Id, string Name, decimal Amount, DateTime Date);

    [Fact]
    public void ExportToExcel_WithData_ReturnsValidExcelBytes()
    {
        var data = new[]
        {
            new TestData(1, "Item 1", 100.50m, new DateTime(2026, 1, 1)),
            new TestData(2, "Item 2", 200.75m, new DateTime(2026, 1, 2))
        };

        var bytes = _service.ExportToExcel(data, "TestSheet");

        bytes.Should().NotBeNull();
        bytes.Length.Should().BeGreaterThan(0);

        // Verify it's a valid Excel file
        using var stream = new MemoryStream(bytes);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();

        worksheet.Name.Should().Be("TestSheet");
        worksheet.Cell(1, 1).Value.ToString().Should().Be("Id");
        worksheet.Cell(1, 2).Value.ToString().Should().Be("Name");
        worksheet.Cell(1, 3).Value.ToString().Should().Be("Amount");
        worksheet.Cell(1, 4).Value.ToString().Should().Be("Date");
        worksheet.Cell(2, 2).Value.ToString().Should().Be("Item 1");
        worksheet.Cell(3, 2).Value.ToString().Should().Be("Item 2");
    }

    [Fact]
    public void ExportToExcel_EmptyData_ReturnsHeaderOnly()
    {
        var data = Array.Empty<TestData>();

        var bytes = _service.ExportToExcel(data, "Empty");

        using var stream = new MemoryStream(bytes);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();

        worksheet.Cell(1, 1).Value.ToString().Should().Be("Id");
        worksheet.Cell(2, 1).Value.ToString().Should().BeEmpty();
    }

    [Fact]
    public void ExportToExcel_LargeDataset_HandlesCorrectly()
    {
        var data = Enumerable.Range(1, 1000).Select(i =>
            new TestData(i, $"Item {i}", i * 10.5m, DateTime.Today.AddDays(i))).ToArray();

        var bytes = _service.ExportToExcel(data, "Large");

        using var stream = new MemoryStream(bytes);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();

        // Header + 1000 data rows
        worksheet.RowsUsed().Count().Should().Be(1001);
    }

    [Fact]
    public void ExportToExcel_HeaderRow_HasBoldFormatting()
    {
        var data = new[] { new TestData(1, "Test", 100m, DateTime.Today) };

        var bytes = _service.ExportToExcel(data);

        using var stream = new MemoryStream(bytes);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();

        worksheet.Cell(1, 1).Style.Font.Bold.Should().BeTrue();
    }

    [Fact]
    public void ExportToExcel_DecimalValues_PreservedCorrectly()
    {
        var data = new[] { new TestData(1, "Test", 123.45m, DateTime.Today) };

        var bytes = _service.ExportToExcel(data);

        using var stream = new MemoryStream(bytes);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();

        worksheet.Cell(2, 3).GetValue<decimal>().Should().Be(123.45m);
    }
}
