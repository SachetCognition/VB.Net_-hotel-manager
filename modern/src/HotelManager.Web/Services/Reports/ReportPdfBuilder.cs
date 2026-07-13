using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HotelManager.Web.Services.Reports;

/// <summary>QuestPDF layouts for every report. Data comes from <see cref="ReportDataService"/>.</summary>
public static class ReportPdfBuilder
{
    static ReportPdfBuilder() => QuestPDF.Settings.License = LicenseType.Community;

    private static string D(DateTime? d) => d?.ToString("dd-MMM-yyyy") ?? "";
    private static string N(double? v) => v?.ToString("0.##") ?? "";
    private static string N(int? v) => v?.ToString() ?? "";

    private static byte[] Render(Action<IDocumentContainer> compose) =>
        Document.Create(compose).GeneratePdf();

    private static void Header(ColumnDescriptor col, HotelHeaderRow hotel, string title)
    {
        col.Item().AlignCenter().Text(hotel.HotelName ?? "Hotel").Bold().FontSize(18);
        if (!string.IsNullOrWhiteSpace(hotel.Address))
            col.Item().AlignCenter().Text(hotel.Address);
        var contact = string.Join("  ", new[] { hotel.ContactNo, hotel.ContactNo1, hotel.Email }
            .Where(s => !string.IsNullOrWhiteSpace(s)));
        if (contact.Length > 0) col.Item().AlignCenter().Text(contact);
        var tax = string.Join("  ", new[]
        {
            string.IsNullOrWhiteSpace(hotel.TIN) ? null : $"TIN: {hotel.TIN}",
            string.IsNullOrWhiteSpace(hotel.STNo) ? null : $"ST No: {hotel.STNo}"
        }.Where(s => s is not null));
        if (tax.Length > 0) col.Item().AlignCenter().Text(tax);
        col.Item().PaddingVertical(4).LineHorizontal(1);
        col.Item().AlignCenter().Text(title).Bold().FontSize(14);
        col.Item().PaddingBottom(6);
    }

    private static void KeyValue(ColumnDescriptor col, params (string Label, string Value)[] pairs)
    {
        foreach (var chunk in pairs.Chunk(2))
        {
            col.Item().Row(row =>
            {
                foreach (var (label, value) in chunk)
                {
                    row.RelativeItem().Text(t =>
                    {
                        t.Span($"{label}: ").SemiBold();
                        t.Span(value);
                    });
                }
                if (chunk.Length == 1) row.RelativeItem();
            });
        }
        col.Item().PaddingBottom(6);
    }

    private static void Totals(ColumnDescriptor col, params (string Label, string Value)[] rows)
    {
        col.Item().AlignRight().Column(c =>
        {
            foreach (var (label, value) in rows)
            {
                c.Item().Row(r =>
                {
                    r.ConstantItem(180).Text(label).SemiBold();
                    r.ConstantItem(100).AlignRight().Text(value);
                });
            }
        });
    }

    private static void Table(ColumnDescriptor col, string[] headers, IEnumerable<string[]> rows)
    {
        col.Item().Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                foreach (var _ in headers) c.RelativeColumn();
            });
            table.Header(h =>
            {
                foreach (var header in headers)
                    h.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text(header).SemiBold().FontSize(9);
            });
            foreach (var row in rows)
                foreach (var cell in row)
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text(cell).FontSize(9);
        });
    }

    private static byte[] Page(HotelHeaderRow hotel, string title, Action<ColumnDescriptor> body) =>
        Render(doc => doc.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(30);
            page.DefaultTextStyle(t => t.FontSize(10));
            page.Content().Column(col =>
            {
                Header(col, hotel, title);
                body(col);
            });
            page.Footer().AlignCenter().Text(t =>
            {
                t.Span("Page ");
                t.CurrentPageNumber();
                t.Span(" of ");
                t.TotalPages();
            });
        }));

    public static byte[] BuildRoomInvoice(RoomInvoiceRow r) =>
        Page(r.Hotel, "Room Invoice", col =>
        {
            KeyValue(col,
                ("Bill No", r.BillNo ?? ""), ("Checkout Date", D(r.CheckOutDate)),
                ("Guest ID", r.GuestID ?? ""), ("Guest Name", r.GuestName ?? ""),
                ("Address", r.GuestAddress ?? ""), ("City", r.City ?? ""),
                ("Contact No", r.GuestContactNo ?? ""), ("ID", $"{r.IDType} {r.IDNumber}".Trim()),
                ("Room No", r.RoomNo ?? ""), ("Room Charges", N(r.RoomCharges)),
                ("Date In", D(r.DateIN)), ("Date Out", D(r.DateOUT)),
                ("Adults", N(r.NoOfAdults)), ("Kids", N(r.NoOfKids)),
                ("No. of Days", N(r.NoOfDays)), ("Extra Bed", r.ExtraBed ?? ""),
                ("Currency", r.Currency ?? ""), ("Status", r.Status ?? ""));
            Totals(col,
                ("Total Room Charges", N(r.TotalRoomCharges)),
                ("Other Charges", N(r.OtherCharges)),
                ("Sub Total", N(r.SubTotal)),
                ($"Service Tax ({N(r.ServiceTaxPer)}%)", N(r.ServiceTaxAmount)),
                ($"Luxury Tax ({N(r.LuxuryTaxPer)}%)", N(r.LuxuryTaxAmount)),
                ($"Discount ({N(r.DiscountPer)}%)", N(r.Discount)),
                ("Grand Total", N(r.GrandTotal)),
                ("Total Paid", N(r.TotalPaid)),
                ("Balance", N(r.Balance)));
            if (!string.IsNullOrWhiteSpace(r.Notes))
                col.Item().PaddingTop(6).Text($"Notes: {r.Notes}");
        });

    public static byte[] BuildHallAndGardenInvoice(HallAndGardenInvoiceRow r) =>
        Page(r.Hotel, "Hall and Garden Invoice", col =>
        {
            KeyValue(col,
                ("Reservation ID", r.ReservationId), ("Currency", r.Currency ?? ""),
                ("Guest ID", r.GuestID ?? ""), ("Guest Name", r.GuestName ?? ""),
                ("Address", r.GuestAddress ?? ""), ("City", r.City ?? ""),
                ("Contact No", r.GuestContactNo ?? ""), ("", ""));
            Table(col, new[] { "Facility", "From", "To", "Days", "Rate", "Total" }, new[]
            {
                new[] { $"Hall: {r.Hall}", D(r.DateFromHall), D(r.DateToHall), N(r.DaysHall), N(r.RateHall), N(r.TotalChargesHall) },
                new[] { $"Garden: {r.Garden}", D(r.DateFromGarden), D(r.DateToGarden), N(r.DaysGarden), N(r.RateGarden), N(r.TotalChargesGarden) }
            });
            col.Item().PaddingTop(6);
            Totals(col,
                ("Other Charges", N(r.OtherCharges)),
                ("Sub Total", N(r.SubTotal)),
                ($"Service Tax ({N(r.ServiceTaxPer)}%)", N(r.ServiceTaxAmount)),
                ($"Luxury Tax ({N(r.LuxuryTaxPer)}%)", N(r.LuxuryTaxAmount)),
                ($"Discount ({N(r.DiscountPer)}%)", N(r.Discount)),
                ("Grand Total", N(r.GrandTotal)),
                ("Total Paid", N(r.TotalPaid)),
                ("Balance", N(r.Balance)));
            if (!string.IsNullOrWhiteSpace(r.Notes))
                col.Item().PaddingTop(6).Text($"Notes: {r.Notes}");
        });

    public static byte[] BuildHallOrGardenInvoice(HallOrGardenInvoiceRow r) =>
        Page(r.Hotel, $"{r.Type ?? "Hall/Garden"} Invoice", col =>
        {
            KeyValue(col,
                ("Reservation ID", r.ReservationId), ("Type", r.Type ?? ""),
                ("Guest ID", r.GuestID ?? ""), ("Guest Name", r.GuestName ?? ""),
                ("Address", r.GuestAddress ?? ""), ("City", r.City ?? ""),
                ("Contact No", r.GuestContactNo ?? ""), ("Currency", r.Currency ?? ""),
                ("Date From", D(r.DateFrom)), ("Date To", D(r.DateTo)),
                ("Days", N(r.Days)), ("Rate", N(r.Rate)));
            Totals(col,
                ("Total Charges", N(r.TotalCharges)),
                ("Other Charges", N(r.OtherCharges)),
                ("Sub Total", N(r.SubTotal)),
                ($"Service Tax ({N(r.ServiceTaxPer)}%)", N(r.ServiceTaxAmount)),
                ($"Luxury Tax ({N(r.LuxuryTaxPer)}%)", N(r.LuxuryTaxAmount)),
                ($"Discount ({N(r.DiscountPer)}%)", N(r.Discount)),
                ("Grand Total", N(r.GrandTotal)),
                ("Total Paid", N(r.TotalPaid)),
                ("Balance", N(r.Balance)));
            if (!string.IsNullOrWhiteSpace(r.Notes))
                col.Item().PaddingTop(6).Text($"Notes: {r.Notes}");
        });

    public static byte[] BuildOrderInvoice(OrderInvoiceRow r) =>
        Page(r.Hotel, "Room Order Invoice", col =>
        {
            KeyValue(col,
                ("Order No", r.OrderNo ?? ""), ("Order Date", D(r.OrderDate)),
                ("Room No", r.RoomNo ?? ""), ("Currency", r.Currency ?? ""),
                ("Guest ID", r.GuestID ?? ""), ("Guest Name", r.GuestName ?? ""),
                ("Address", r.GuestAddress ?? ""), ("Contact No", r.GuestContactNo ?? ""));
            Table(col, new[] { "Product", "Volume", "Rate", "Qty", "Amount" },
                r.Lines.Select(l => new[] { l.ProductName ?? "", N(l.Volume), N(l.Rate), N(l.Quantity), N(l.Amount) }));
            col.Item().PaddingTop(6);
            Totals(col,
                ("Sub Total", N(r.SubTotal)),
                ($"VAT ({N(r.VATPer)}%)", N(r.VATAmount)),
                ($"Service Tax ({N(r.STPer)}%)", N(r.STAmount)),
                ("Grand Total", N(r.GrandTotal)),
                ("Total Payment", N(r.TotalPayment)),
                ("Payment Due", N(r.PaymentDue)));
        });

    public static byte[] BuildRestaurantReceipt(RestaurantReceiptRow r) =>
        Page(r.Hotel, "Restaurant Order Receipt", col =>
        {
            KeyValue(col,
                ("Order No", r.OrderNo ?? ""), ("Order Date", D(r.OrderDate)),
                ("Currency", r.Currency ?? ""), ("", ""));
            Table(col, new[] { "Product", "Rate", "Qty", "Amount" },
                r.Lines.Select(l => new[] { l.ProductName ?? "", N(l.Rate), N(l.Quantity), N(l.Amount) }));
            col.Item().PaddingTop(6);
            Totals(col,
                ("Sub Total", N(r.SubTotal)),
                ($"VAT ({N(r.VATPer)}%)", N(r.VATAmount)),
                ($"Service Tax ({N(r.STPer)}%)", N(r.STAmount)),
                ("Grand Total", N(r.GrandTotal)),
                ("Total Payment", N(r.TotalPayment)),
                ("Payment Due", N(r.PaymentDue)));
        });

    public static byte[] BuildSalarySlip(SalarySlipRow r) =>
        Page(r.Hotel, "Salary Slip", col =>
        {
            KeyValue(col,
                ("Payment ID", r.PaymentID), ("Payment Date", D(r.PaymentDate)),
                ("Employee ID", r.EmployeeID ?? ""), ("Employee Name", r.EmployeeName ?? ""),
                ("Designation", r.Designation ?? ""), ("Department", r.Department ?? ""),
                ("Period From", D(r.DateFrom)), ("Period To", D(r.DateTo)),
                ("Present Days", N(r.PresentDays)), ("Mode of Payment", r.ModeOfPayment ?? ""));
            Totals(col,
                ("Salary", N(r.Salary)),
                ("Advance", N(r.Advance)),
                ("Deduction", N(r.Deduction)),
                ($"Overtime ({r.Overtime})", N(r.OverTimeAmount)),
                ("Net Pay", N(r.NetPay)));
        });

    public static byte[] BuildAttendanceReport(HotelHeaderRow hotel, IReadOnlyList<AttendanceRow> rows) =>
        Page(hotel, "Attendance Report", col =>
            Table(col,
                new[] { "Employee ID", "Name", "Date", "Basic Time", "Status", "In", "Out", "Overtime" },
                rows.Select(r => new[]
                {
                    r.EmployeeID ?? "", r.EmployeeName ?? "", D(r.WorkingDate), r.BasicWorkingTime ?? "",
                    r.Status ?? "", r.InTime ?? "", r.OutTime ?? "", r.Overtime ?? ""
                })));

    public static byte[] BuildAdvancePaymentReport(HotelHeaderRow hotel, IReadOnlyList<AdvancePaymentRow> rows) =>
        Page(hotel, "Advance Payment Report", col =>
            Table(col,
                new[] { "Employee ID", "Name", "Date", "Advance", "Deduction" },
                rows.Select(r => new[]
                {
                    r.EmployeeID ?? "", r.EmployeeName ?? "", D(r.WorkingDate), N(r.Amount), N(r.Deduction)
                })));

    public static byte[] BuildEmployeePaymentReport(HotelHeaderRow hotel, IReadOnlyList<EmployeePaymentRow> rows) =>
        Page(hotel, "Employee Payment Report", col =>
            Table(col,
                new[] { "Payment ID", "Employee", "From", "To", "Days", "Salary", "Advance", "Deduction", "OT Amt", "Paid On", "Net Pay" },
                rows.Select(r => new[]
                {
                    r.PaymentID, $"{r.EmployeeID} {r.EmployeeName}".Trim(), D(r.DateFrom), D(r.DateTo),
                    N(r.PresentDays), N(r.Salary), N(r.Advance), N(r.Deduction),
                    N(r.OverTimeAmount), D(r.PaymentDate), N(r.NetPay)
                })));

    public static byte[] BuildGuestReport(HotelHeaderRow hotel, IReadOnlyList<GuestRow> rows) =>
        Page(hotel, "Guest Report", col =>
            Table(col,
                new[] { "Guest ID", "Name", "Address", "City", "Contact No", "ID Type", "ID Number" },
                rows.Select(r => new[]
                {
                    r.GuestID, r.GuestName ?? "", r.Address ?? "", r.City ?? "",
                    r.ContactNo ?? "", r.IDType ?? "", r.IDNumber ?? ""
                })));

    public static byte[] BuildReservationReport(HotelHeaderRow hotel, IReadOnlyList<ReservationRow> rows) =>
        Page(hotel, "Reservation Report", col =>
            Table(col,
                new[] { "Reservation ID", "Guest", "Room", "Type", "Date In", "Date Out", "Status" },
                rows.Select(r => new[]
                {
                    r.ReservationID, $"{r.GuestID} {r.GuestName}".Trim(), r.RoomNo ?? "",
                    r.RoomType ?? "", D(r.DateIN), D(r.DateOUT), r.Status ?? ""
                })));

    public static byte[] BuildPurchasedInventoryReport(HotelHeaderRow hotel, IReadOnlyList<PurchasedInventoryRow> rows) =>
        Page(hotel, "Purchased Inventory Report", col =>
            Table(col,
                new[] { "ID", "Product", "Category", "Type", "Party", "Date", "Qty", "Unit", "Price", "Total" },
                rows.Select(r => new[]
                {
                    r.ID.ToString(), r.ProductName ?? "", r.Category ?? "", r.TransactionType ?? "",
                    r.PartyName ?? "", D(r.PurchaseDate), N(r.Quantity), r.Unit ?? "", N(r.Price), N(r.TotalPrice)
                })));
}
