using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using HotelManagement.Core.Models;

namespace HotelManagement.Reports;

/// <summary>
/// Report generation service replacing Crystal Reports with QuestPDF.
/// Generates PDF invoices for Room, Hall/Garden, and Hall+Garden bookings.
/// </summary>
public class ReportService
{
    static ReportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateRoomInvoice(CheckInRoom checkIn, Guest guest, HotelInfo? hotelInfo, string billNo)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().AlignCenter().Text(hotelInfo?.HotelName ?? "Hotel Management System").FontSize(18).Bold();
                    col.Item().AlignCenter().Text(hotelInfo?.Address ?? "").FontSize(9);
                    col.Item().AlignCenter().Text($"Contact: {hotelInfo?.ContactNo ?? ""}").FontSize(9);
                    col.Item().LineHorizontal(1);
                    col.Item().AlignCenter().Text("ROOM INVOICE").FontSize(14).Bold();
                });

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"Bill No: {billNo}").Bold();
                            c.Item().Text($"Check-In ID: {checkIn.ID}");
                            c.Item().Text($"Guest: {guest.GuestName}");
                            c.Item().Text($"Address: {guest.Address}");
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().AlignRight().Text($"Room No: {checkIn.RoomNo}");
                            c.Item().AlignRight().Text($"Check-In: {checkIn.DateIN:dd-MMM-yyyy}");
                            c.Item().AlignRight().Text($"Check-Out: {checkIn.DateOUT:dd-MMM-yyyy}");
                        });
                    });

                    col.Item().PaddingVertical(10).LineHorizontal(0.5f);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                        });

                        AddTableRow(table, "Room Charges (per day)", $"{checkIn.RoomCharges:N2}");
                        AddTableRow(table, "Number of Days", $"{checkIn.NoOfDays}");
                        AddTableRow(table, "Total Room Charges", $"{checkIn.TotalRoomCharges:N2}");
                        AddTableRow(table, "Other Charges", $"{checkIn.OtherCharges:N2}");
                        AddTableRow(table, $"Discount ({checkIn.DiscountPer}%)", $"-{checkIn.Discount:N2}");
                        AddTableRow(table, "Sub Total", $"{checkIn.SubTotal:N2}");
                        AddTableRow(table, $"Service Tax ({checkIn.ServiceTaxPer}%)", $"{checkIn.ServiceTaxAmount:N2}");
                        AddTableRow(table, $"Luxury Tax ({checkIn.LuxuryTaxPer}%)", $"{checkIn.LuxuryTaxAmount:N2}");
                    });

                    col.Item().PaddingVertical(5).LineHorizontal(1);

                    col.Item().Row(row =>
                    {
                        row.RelativeItem(3).Text("Grand Total").Bold().FontSize(12);
                        row.RelativeItem(1).AlignRight().Text($"{checkIn.GrandTotal:N2}").Bold().FontSize(12);
                    });
                    col.Item().Row(row =>
                    {
                        row.RelativeItem(3).Text("Total Paid");
                        row.RelativeItem(1).AlignRight().Text($"{checkIn.TotalPaid:N2}");
                    });
                    col.Item().Row(row =>
                    {
                        row.RelativeItem(3).Text("Balance").Bold();
                        row.RelativeItem(1).AlignRight().Text($"{checkIn.Balance:N2}").Bold();
                    });

                    col.Item().PaddingTop(20).Text(checkIn.Notes ?? "").FontSize(8).Italic();
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Generated on ").FontSize(8);
                    text.Span(DateTime.Now.ToString("dd-MMM-yyyy HH:mm")).FontSize(8);
                });
            });
        });

        return document.GeneratePdf();
    }

    public byte[] GenerateGuestReport(IEnumerable<Guest> guests)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().AlignCenter().Text("Guest Report").FontSize(16).Bold();

                page.Content().PaddingVertical(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Guest ID").Bold();
                        header.Cell().Text("Name").Bold();
                        header.Cell().Text("Address").Bold();
                        header.Cell().Text("City").Bold();
                        header.Cell().Text("Contact").Bold();
                        header.Cell().Text("ID Type").Bold();
                        header.Cell().Text("ID Number").Bold();
                    });

                    foreach (var g in guests)
                    {
                        table.Cell().Text(g.GuestID);
                        table.Cell().Text(g.GuestName);
                        table.Cell().Text(g.Address);
                        table.Cell().Text(g.City);
                        table.Cell().Text(g.ContactNo);
                        table.Cell().Text(g.IDType);
                        table.Cell().Text(g.IDNumber);
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Page ").FontSize(8);
                    text.CurrentPageNumber().FontSize(8);
                });
            });
        });

        return document.GeneratePdf();
    }

    public byte[] GenerateEmployeeReport(IEnumerable<Employee> employees)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().AlignCenter().Text("Employee Report").FontSize(16).Bold();

                page.Content().PaddingVertical(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Emp ID").Bold();
                        header.Cell().Text("Name").Bold();
                        header.Cell().Text("Mobile").Bold();
                        header.Cell().Text("Department").Bold();
                        header.Cell().Text("Designation").Bold();
                        header.Cell().Text("Joining").Bold();
                        header.Cell().Text("Salary").Bold();
                    });

                    foreach (var e in employees)
                    {
                        table.Cell().Text(e.EmployeeID);
                        table.Cell().Text(e.EmployeeName);
                        table.Cell().Text(e.MobileNo);
                        table.Cell().Text(e.Department);
                        table.Cell().Text(e.Designation);
                        table.Cell().Text(e.DateOfJoining.ToString("dd-MMM-yyyy"));
                        table.Cell().Text(e.Salary.ToString("N2"));
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Page ").FontSize(8);
                    text.CurrentPageNumber().FontSize(8);
                });
            });
        });

        return document.GeneratePdf();
    }

    public byte[] GenerateSalarySlip(EmployeePayment payment, Employee employee)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().AlignCenter().Text("SALARY SLIP").FontSize(16).Bold();
                    col.Item().LineHorizontal(1);
                });

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"Employee ID: {employee.EmployeeID}").Bold();
                            c.Item().Text($"Name: {employee.EmployeeName}");
                            c.Item().Text($"Department: {employee.Department}");
                            c.Item().Text($"Designation: {employee.Designation}");
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().AlignRight().Text($"Payment ID: {payment.PaymentID}");
                            c.Item().AlignRight().Text($"Period: {payment.DateFrom:dd-MMM-yyyy} to {payment.DateTo:dd-MMM-yyyy}");
                            c.Item().AlignRight().Text($"Payment Date: {payment.PaymentDate:dd-MMM-yyyy}");
                        });
                    });

                    col.Item().PaddingVertical(10).LineHorizontal(0.5f);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                        });

                        AddTableRow(table, "Basic Salary", $"{employee.Salary:N2}");
                        AddTableRow(table, "Present Days", $"{payment.PresentDays}");
                        AddTableRow(table, "Calculated Salary", $"{payment.Salary:N2}");
                        AddTableRow(table, "Overtime", $"{payment.OverTime}");
                        AddTableRow(table, $"Overtime Amount (Rate: {payment.OverTimeRate:N2})", $"{payment.OverTimeAmount:N2}");
                        AddTableRow(table, "Advance", $"{payment.Advance:N2}");
                        AddTableRow(table, "Deduction", $"-{payment.Deduction:N2}");
                    });

                    col.Item().PaddingVertical(5).LineHorizontal(1);

                    col.Item().Row(row =>
                    {
                        row.RelativeItem(3).Text("Net Pay").Bold().FontSize(14);
                        row.RelativeItem(1).AlignRight().Text($"{payment.NetPay:N2}").Bold().FontSize(14);
                    });

                    col.Item().PaddingTop(5).Text($"Mode of Payment: {payment.ModeOfPayment}").FontSize(9);
                    if (!string.IsNullOrEmpty(payment.PaymentModeDetails))
                        col.Item().Text($"Details: {payment.PaymentModeDetails}").FontSize(9);
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Generated on ").FontSize(8);
                    text.Span(DateTime.Now.ToString("dd-MMM-yyyy HH:mm")).FontSize(8);
                });
            });
        });

        return document.GeneratePdf();
    }

    private static void AddTableRow(TableDescriptor table, string label, string value)
    {
        table.Cell().PaddingVertical(2).Text(label);
        table.Cell().PaddingVertical(2).AlignRight().Text(value);
    }
}
