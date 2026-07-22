using HotelManager.Web.Components.Pages.Hr;
using MudBlazor;

namespace HotelManager.ComponentTests.Pages;

public class HrPageTests : ComponentTestBase
{
    [Fact]
    public void Employees_renders_saves_and_deletes()
    {
        var cut = RenderPage<Employees>();
        Assert.Contains("Bob", cut.Markup);
        ClickButton(cut, "Save");
        HrSvc.Verify(s => s.SaveEmployeeAsync(It.IsAny<EmployeeRegistration>(), true), Times.Once);
        ClickIcon(cut, Icons.Material.Filled.Delete);
        HrSvc.Verify(s => s.DeleteEmployeeAsync("E-000001"), Times.Once);
    }

    [Fact]
    public void Employees_edit_then_update()
    {
        var cut = RenderPage<Employees>();
        ClickIcon(cut, Icons.Material.Filled.Edit);
        ClickButton(cut, "Update");
        HrSvc.Verify(s => s.SaveEmployeeAsync(It.IsAny<EmployeeRegistration>(), false), Times.Once);
    }

    [Fact]
    public void Attendance_saves()
    {
        var cut = RenderPage<Attendance>();
        ClickButton(cut, "Save");
        HrSvc.Verify(s => s.SaveAttendanceAsync(It.IsAny<EmployeeAttendance>()), Times.Once);
    }

    [Fact]
    public void AttendanceRecords_renders_and_deletes()
    {
        var cut = RenderPage<AttendanceRecords>();
        HrSvc.Verify(s => s.GetAttendanceAsync(It.IsAny<string?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()), Times.AtLeastOnce);
        ClickIcon(cut, Icons.Material.Filled.Delete);
        HrSvc.Verify(s => s.DeleteAttendanceAsync(1), Times.Once);
    }

    [Fact]
    public void Advances_renders_summary_and_saves()
    {
        var cut = RenderPage<Advances>();
        ClickButton(cut, "Save");
        HrSvc.Verify(s => s.SaveAdvanceEntryAsync(It.IsAny<AdvanceEntry>()), Times.Once);
    }

    [Fact]
    public void AdvanceRecords_renders_and_deletes()
    {
        var cut = RenderPage<AdvanceRecords>();
        ClickIcon(cut, Icons.Material.Filled.Delete);
        HrSvc.Verify(s => s.DeleteAdvanceEntryAsync(1), Times.Once);
    }

    [Fact]
    public void PaymentRun_renders_and_runs_payment()
    {
        var cut = RenderPage<PaymentRun>();
        ClickButton(cut, "Save Payment");
        HrSvc.Verify(s => s.RunPaymentAsync(It.IsAny<EmployeePayment>()), Times.Once);
    }

    [Fact]
    public void PaymentRecords_renders_and_deletes()
    {
        var cut = RenderPage<PaymentRecords>();
        ClickIcon(cut, Icons.Material.Filled.Delete);
        HrSvc.Verify(s => s.DeletePaymentAsync("P-000001"), Times.Once);
    }
}
