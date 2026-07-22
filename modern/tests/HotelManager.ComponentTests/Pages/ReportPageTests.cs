using HotelManager.Web.Components.Pages.Reports;

namespace HotelManager.ComponentTests.Pages;

public class ReportPageTests : ComponentTestBase
{
    [Fact]
    public void GuestsReport_export_invokes_render()
    {
        var cut = RenderPage<GuestsReport>();
        ClickButton(cut, "Export PDF");
        ReportSvc.Verify(r => r.RenderGuestReportPdfAsync(It.IsAny<ReportRequest>()), Times.Once);
    }

    [Fact]
    public void ReservationsReport_export_invokes_render()
    {
        var cut = RenderPage<ReservationsReport>();
        ClickButton(cut, "Export PDF");
        ReportSvc.Verify(r => r.RenderReservationReportPdfAsync(It.IsAny<ReportRequest>()), Times.Once);
    }

    [Fact]
    public void PurchasedInventoryReport_export_invokes_render()
    {
        var cut = RenderPage<PurchasedInventoryReport>();
        ClickButton(cut, "Export PDF");
        ReportSvc.Verify(r => r.RenderPurchasedInventoryReportPdfAsync(It.IsAny<ReportRequest>()), Times.Once);
    }

    [Fact]
    public void AttendanceReport_export_invokes_render()
    {
        var cut = RenderPage<AttendanceReport>();
        ClickButton(cut, "Export PDF");
        ReportSvc.Verify(r => r.RenderAttendanceReportPdfAsync(It.IsAny<ReportRequest>()), Times.Once);
    }

    [Fact]
    public void AdvancePaymentsReport_export_invokes_render()
    {
        var cut = RenderPage<AdvancePaymentsReport>();
        ClickButton(cut, "Export PDF");
        ReportSvc.Verify(r => r.RenderAdvancePaymentReportPdfAsync(It.IsAny<ReportRequest>()), Times.Once);
    }

    [Fact]
    public void EmployeePaymentsReport_export_invokes_render()
    {
        var cut = RenderPage<EmployeePaymentsReport>();
        ClickButton(cut, "Export PDF");
        ReportSvc.Verify(r => r.RenderEmployeePaymentReportPdfAsync(It.IsAny<ReportRequest>()), Times.Once);
    }

    [Fact]
    public void ReportsHub_lists_report_links()
    {
        var cut = RenderPage<ReportsHub>();
        Assert.Contains("Room Invoices", cut.Markup);
        Assert.Contains("Salary Slips", cut.Markup);
    }

    [Fact]
    public void RoomInvoices_renders_seeded_pick_and_exports()
    {
        Seed(db =>
        {
            db.CheckIns.Add(TestData.CheckIn());
            db.Checkouts.Add(new CheckoutRoom { ID = 1, BillNo = "B-1", CheckInID = 1, CheckOutDate = DateTime.Today });
        });
        var cut = RenderPage<RoomInvoices>();
        Assert.Contains("B-1", cut.Markup);
        ClickButton(cut, "Export PDF");
        ReportSvc.Verify(r => r.RenderRoomInvoicePdfAsync(1), Times.Once);
    }

    [Fact]
    public void SalarySlips_renders_seeded_pick_and_exports()
    {
        Seed(db => db.EmployeePayments.Add(new EmployeePayment { PaymentID = "P-000001", PaymentDate = DateTime.Today }));
        var cut = RenderPage<SalarySlips>();
        Assert.Contains("P-000001", cut.Markup);
        ClickButton(cut, "Export PDF");
        ReportSvc.Verify(r => r.RenderSalarySlipPdfAsync("P-000001"), Times.Once);
    }

    [Fact]
    public void OrderInvoices_renders_seeded_pick_and_exports()
    {
        Seed(db => db.Orders.Add(new OrderInfo { ID = 5, OrderNo = "O-5", OrderDate = DateTime.Today }));
        var cut = RenderPage<OrderInvoices>();
        ClickButton(cut, "Export PDF");
        ReportSvc.Verify(r => r.RenderOrderInvoicePdfAsync(5), Times.Once);
    }

    [Fact]
    public void RestaurantReceipts_renders_seeded_pick_and_exports()
    {
        Seed(db => db.RestaurantOrders.Add(new RestaurantOrderInfo { ID = 7, OrderNo = "RO-7", OrderDate = DateTime.Today }));
        var cut = RenderPage<RestaurantReceipts>();
        ClickButton(cut, "Export PDF");
        ReportSvc.Verify(r => r.RenderRestaurantOrderReceiptPdfAsync(7), Times.Once);
    }

    [Fact]
    public void HallAndGardenInvoices_renders_empty()
    {
        var cut = RenderPage<HallAndGardenInvoices>();
        Assert.NotNull(cut);
    }

    [Fact]
    public void HallOrGardenInvoices_renders_empty()
    {
        var cut = RenderPage<HallOrGardenInvoices>();
        Assert.NotNull(cut);
    }
}
