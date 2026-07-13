using HotelManager.Application.Billing;
using HotelManager.Domain.Entities;
using HotelManager.Web.Services.Hr;

namespace HotelManager.IntegrationTests.Hr;

public class HrPayrollServiceTests : IClassFixture<HrSqliteDbFixture>
{
    private readonly HrSqliteDbFixture _fixture;

    public HrPayrollServiceTests(HrSqliteDbFixture fixture) => _fixture = fixture;

    private static int _seq;
    private static string NextId() => $"E-TEST{Interlocked.Increment(ref _seq):D4}";

    private async Task<EmployeeRegistration> SeedEmployeeAsync(
        HrPayrollService svc, int salary = 30000, string workingTime = "08:00:00")
    {
        var employee = new EmployeeRegistration
        {
            EmployeeID = NextId(),
            EmployeeName = "Test Employee",
            Address = "1 Test Street",
            MobileNo = "1234567890",
            Department = "Kitchen",
            Designation = "Chef",
            DateOfJoining = DateTime.Today,
            Salary = salary,
            BasicWorkingTime = workingTime
        };
        await svc.SaveEmployeeAsync(employee, isNew: true);
        return employee;
    }

    // ---- Employees ----

    [Fact]
    public async Task GenerateEmployeeId_HasLegacyFormat()
    {
        await using var db = _fixture.CreateContext();
        var svc = new HrPayrollService(db);
        var id = await svc.GenerateEmployeeIdAsync();
        Assert.Matches(@"^E-[1-9]{6}$", id);
    }

    [Fact]
    public async Task SaveEmployee_New_GeneratesIdAndPersists()
    {
        await using var db = _fixture.CreateContext();
        var svc = new HrPayrollService(db);
        var employee = new EmployeeRegistration
        {
            EmployeeID = "",
            EmployeeName = "Alice",
            Address = "2 Test Street",
            MobileNo = "1112223334",
            Department = "Reception",
            Designation = "Clerk",
            Salary = 15000,
            BasicWorkingTime = "08:00:00"
        };
        await svc.SaveEmployeeAsync(employee, isNew: true);
        Assert.Matches(@"^E-[1-9]{6}$", employee.EmployeeID);
        var fetched = await svc.GetEmployeeAsync(employee.EmployeeID);
        Assert.NotNull(fetched);
        Assert.Equal("Alice", fetched!.EmployeeName);
    }

    [Fact]
    public async Task SaveEmployee_MissingRequiredFields_Throws()
    {
        await using var db = _fixture.CreateContext();
        var svc = new HrPayrollService(db);
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.SaveEmployeeAsync(new EmployeeRegistration { EmployeeID = "" }, isNew: true));
        Assert.Equal("Please enter employee full name", ex.Message);
    }

    [Fact]
    public async Task SaveEmployee_Update_ChangesFields()
    {
        await using var db = _fixture.CreateContext();
        var svc = new HrPayrollService(db);
        var employee = await SeedEmployeeAsync(svc);
        employee.Designation = "Head Chef";
        employee.Salary = 45000;
        await svc.SaveEmployeeAsync(employee, isNew: false);

        await using var db2 = _fixture.CreateContext();
        var fetched = await new HrPayrollService(db2).GetEmployeeAsync(employee.EmployeeID);
        Assert.Equal("Head Chef", fetched!.Designation);
        Assert.Equal(45000, fetched.Salary);
    }

    [Fact]
    public async Task DeleteEmployee_InUse_Throws()
    {
        await using var db = _fixture.CreateContext();
        var svc = new HrPayrollService(db);
        var employee = await SeedEmployeeAsync(svc);
        await svc.SaveAttendanceAsync(new EmployeeAttendance
        {
            EmployeeID = employee.EmployeeID,
            WorkingDate = DateTime.Today,
            Status = "P",
            InTime = "09:00:00",
            OutTime = "17:00:00",
            Overtime = "00:00:00"
        });
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.DeleteEmployeeAsync(employee.EmployeeID));
        Assert.Equal("Unable to delete..Already in use", ex.Message);
    }

    [Fact]
    public async Task DeleteEmployee_Unused_Deletes()
    {
        await using var db = _fixture.CreateContext();
        var svc = new HrPayrollService(db);
        var employee = await SeedEmployeeAsync(svc);
        await svc.DeleteEmployeeAsync(employee.EmployeeID);
        Assert.Null(await svc.GetEmployeeAsync(employee.EmployeeID));
    }

    [Fact]
    public async Task GetEmployees_FiltersBySearch()
    {
        await using var db = _fixture.CreateContext();
        var svc = new HrPayrollService(db);
        var employee = await SeedEmployeeAsync(svc);
        var all = await svc.GetEmployeesAsync();
        Assert.Contains(all, e => e.EmployeeID == employee.EmployeeID);
        var byId = await svc.GetEmployeesAsync(employee.EmployeeID);
        Assert.Single(byId);
        var none = await svc.GetEmployeesAsync("zzz-no-match");
        Assert.Empty(none);
    }

    // ---- Attendance ----

    [Fact]
    public async Task SaveAttendance_DuplicateSameDay_Throws()
    {
        await using var db = _fixture.CreateContext();
        var svc = new HrPayrollService(db);
        var employee = await SeedEmployeeAsync(svc);
        var date = new DateTime(2026, 1, 5);
        await svc.SaveAttendanceAsync(new EmployeeAttendance
        {
            EmployeeID = employee.EmployeeID, WorkingDate = date, Status = "P",
            InTime = "09:00:00", OutTime = "18:00:00", Overtime = "01:00:00"
        });
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.SaveAttendanceAsync(new EmployeeAttendance
            {
                EmployeeID = employee.EmployeeID, WorkingDate = date, Status = "P"
            }));
        Assert.Equal("Employee today's attendance is already saved", ex.Message);
    }

    [Fact]
    public async Task SaveAttendance_Absent_ZeroesTimes()
    {
        await using var db = _fixture.CreateContext();
        var svc = new HrPayrollService(db);
        var employee = await SeedEmployeeAsync(svc);
        var saved = await svc.SaveAttendanceAsync(new EmployeeAttendance
        {
            EmployeeID = employee.EmployeeID, WorkingDate = new DateTime(2026, 1, 6), Status = "A"
        });
        Assert.Equal("00:00:00", saved.InTime);
        Assert.Equal("00:00:00", saved.OutTime);
        Assert.Equal("00:00:00", saved.Overtime);
        Assert.Equal(employee.BasicWorkingTime, saved.BasicWorkingTime);
    }

    [Fact]
    public async Task SaveAttendance_DefaultsBasicWorkingTimeFromEmployee()
    {
        await using var db = _fixture.CreateContext();
        var svc = new HrPayrollService(db);
        var employee = await SeedEmployeeAsync(svc, workingTime: "07:30:00");
        var saved = await svc.SaveAttendanceAsync(new EmployeeAttendance
        {
            EmployeeID = employee.EmployeeID, WorkingDate = new DateTime(2026, 1, 7), Status = "P",
            InTime = "09:00:00", OutTime = "17:00:00", Overtime = "00:30:00"
        });
        Assert.Equal("07:30:00", saved.BasicWorkingTime);
    }

    [Fact]
    public async Task Attendance_Aggregation_PresentDaysAndOvertime()
    {
        await using var db = _fixture.CreateContext();
        var svc = new HrPayrollService(db);
        var employee = await SeedEmployeeAsync(svc);
        var from = new DateTime(2026, 2, 1);
        await svc.SaveAttendanceAsync(new EmployeeAttendance
        {
            EmployeeID = employee.EmployeeID, WorkingDate = from, Status = "P",
            InTime = "09:00:00", OutTime = "18:30:00", Overtime = "01:30:00"
        });
        await svc.SaveAttendanceAsync(new EmployeeAttendance
        {
            EmployeeID = employee.EmployeeID, WorkingDate = from.AddDays(1), Status = "P",
            InTime = "09:00:00", OutTime = "19:15:00", Overtime = "02:15:00"
        });
        await svc.SaveAttendanceAsync(new EmployeeAttendance
        {
            EmployeeID = employee.EmployeeID, WorkingDate = from.AddDays(2), Status = "A"
        });
        // Outside the range — must be excluded.
        await svc.SaveAttendanceAsync(new EmployeeAttendance
        {
            EmployeeID = employee.EmployeeID, WorkingDate = from.AddDays(10), Status = "P",
            InTime = "09:00:00", OutTime = "18:00:00", Overtime = "01:00:00"
        });

        var to = from.AddDays(5);
        Assert.Equal(2, await svc.GetPresentDaysAsync(employee.EmployeeID, from, to));
        Assert.Equal(new TimeSpan(3, 45, 0), await svc.GetTotalOvertimeAsync(employee.EmployeeID, from, to));

        var records = await svc.GetAttendanceAsync(employee.EmployeeID, from, to);
        Assert.Equal(3, records.Count);
    }

    [Fact]
    public async Task DeleteAttendance_RemovesRow()
    {
        await using var db = _fixture.CreateContext();
        var svc = new HrPayrollService(db);
        var employee = await SeedEmployeeAsync(svc);
        var saved = await svc.SaveAttendanceAsync(new EmployeeAttendance
        {
            EmployeeID = employee.EmployeeID, WorkingDate = new DateTime(2026, 3, 1), Status = "P",
            InTime = "09:00:00", OutTime = "17:00:00", Overtime = "00:00:00"
        });
        await svc.DeleteAttendanceAsync(saved.AttendanceID);
        Assert.Empty(await svc.GetAttendanceAsync(employee.EmployeeID));
    }

    [Fact]
    public async Task SaveAttendance_Update_ChangesExistingRow()
    {
        await using var db = _fixture.CreateContext();
        var svc = new HrPayrollService(db);
        var employee = await SeedEmployeeAsync(svc);
        var saved = await svc.SaveAttendanceAsync(new EmployeeAttendance
        {
            EmployeeID = employee.EmployeeID, WorkingDate = new DateTime(2026, 3, 5), Status = "P",
            InTime = "09:00:00", OutTime = "17:00:00", Overtime = "00:00:00"
        });
        var updated = await svc.SaveAttendanceAsync(new EmployeeAttendance
        {
            AttendanceID = saved.AttendanceID, EmployeeID = employee.EmployeeID,
            WorkingDate = new DateTime(2026, 3, 5), Status = "P",
            InTime = "08:00:00", OutTime = "18:00:00", Overtime = "02:00:00",
            BasicWorkingTime = employee.BasicWorkingTime
        });
        Assert.Equal(saved.AttendanceID, updated.AttendanceID);
        Assert.Equal("08:00:00", updated.InTime);
        Assert.Equal("02:00:00", updated.Overtime);
        Assert.Single(await svc.GetAttendanceAsync(employee.EmployeeID));
    }

    [Fact]
    public async Task SaveAttendance_MissingEmployeeOrStatus_Throws()
    {
        await using var db = _fixture.CreateContext();
        var svc = new HrPayrollService(db);
        var employee = await SeedEmployeeAsync(svc);
        var ex1 = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.SaveAttendanceAsync(new EmployeeAttendance { Status = "P" }));
        Assert.Equal("Please select employee id", ex1.Message);
        var ex2 = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.SaveAttendanceAsync(new EmployeeAttendance { EmployeeID = employee.EmployeeID }));
        Assert.Equal("Please select Status", ex2.Message);
    }

    [Theory]
    [InlineData(null, "Addr", "123", "Dept", "Desig", 1, "08:00:00", "Please enter employee full name")]
    [InlineData("Name", null, "123", "Dept", "Desig", 1, "08:00:00", "Please enter address")]
    [InlineData("Name", "Addr", null, "Dept", "Desig", 1, "08:00:00", "Please enter mobile no.")]
    [InlineData("Name", "Addr", "123", null, "Desig", 1, "08:00:00", "Please enter department")]
    [InlineData("Name", "Addr", "123", "Dept", null, 1, "08:00:00", "Please enter designation")]
    [InlineData("Name", "Addr", "123", "Dept", "Desig", null, "08:00:00", "Please enter basic Salary")]
    [InlineData("Name", "Addr", "123", "Dept", "Desig", 1, null, "Please enter basic working time")]
    public async Task SaveEmployee_ValidationMessages_MatchLegacy(
        string? name, string? address, string? mobile, string? department,
        string? designation, int? salary, string? workingTime, string expected)
    {
        await using var db = _fixture.CreateContext();
        var svc = new HrPayrollService(db);
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.SaveEmployeeAsync(new EmployeeRegistration
            {
                EmployeeID = "", EmployeeName = name, Address = address, MobileNo = mobile,
                Department = department, Designation = designation, Salary = salary,
                BasicWorkingTime = workingTime
            }, isNew: true));
        Assert.Equal(expected, ex.Message);
    }

    // ---- Advances & deductions ----

    [Fact]
    public async Task SaveAdvanceEntry_MissingEmployeeOrAmount_Throws()
    {
        await using var db = _fixture.CreateContext();
        var svc = new HrPayrollService(db);
        var employee = await SeedEmployeeAsync(svc);
        var ex1 = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.SaveAdvanceEntryAsync(new AdvanceEntry { Amount = 100 }));
        Assert.Equal("Please select employee id", ex1.Message);
        var ex2 = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.SaveAdvanceEntryAsync(new AdvanceEntry { EmployeeID = employee.EmployeeID }));
        Assert.Equal("Please enter Amount", ex2.Message);
    }

    [Fact]
    public async Task SaveAdvance_DuplicateSameDay_Throws()
    {
        await using var db = _fixture.CreateContext();
        var svc = new HrPayrollService(db);
        var employee = await SeedEmployeeAsync(svc);
        var date = new DateTime(2026, 4, 1);
        await svc.SaveAdvanceEntryAsync(new AdvanceEntry
        {
            EmployeeID = employee.EmployeeID, WorkingDate = date, Amount = 500
        });
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.SaveAdvanceEntryAsync(new AdvanceEntry
            {
                EmployeeID = employee.EmployeeID, WorkingDate = date, Amount = 200
            }));
        Assert.Equal("advance is already paid to employee today", ex.Message);
    }

    [Fact]
    public async Task OutstandingAdvance_IsAmountMinusDeduction()
    {
        await using var db = _fixture.CreateContext();
        var svc = new HrPayrollService(db);
        var employee = await SeedEmployeeAsync(svc);
        await svc.SaveAdvanceEntryAsync(new AdvanceEntry
        {
            EmployeeID = employee.EmployeeID, WorkingDate = new DateTime(2026, 5, 1), Amount = 1000
        });
        await svc.SaveAdvanceEntryAsync(new AdvanceEntry
        {
            EmployeeID = employee.EmployeeID, WorkingDate = new DateTime(2026, 5, 2), Amount = 500
        });
        await svc.SaveAdvanceEntryAsync(new AdvanceEntry
        {
            EmployeeID = employee.EmployeeID, WorkingDate = new DateTime(2026, 5, 3), Deduction = 300
        });
        var outstanding = await svc.GetOutstandingAdvanceAsync(
            employee.EmployeeID, new DateTime(2026, 5, 1), new DateTime(2026, 5, 31));
        Assert.Equal(1200, outstanding);
    }

    [Fact]
    public async Task SaveAdvanceEntry_Update_ChangesExistingRow()
    {
        await using var db = _fixture.CreateContext();
        var svc = new HrPayrollService(db);
        var employee = await SeedEmployeeAsync(svc);
        var saved = await svc.SaveAdvanceEntryAsync(new AdvanceEntry
        {
            EmployeeID = employee.EmployeeID, WorkingDate = new DateTime(2026, 6, 10), Amount = 300
        });
        var updated = await svc.SaveAdvanceEntryAsync(new AdvanceEntry
        {
            ID = saved.ID, EmployeeID = employee.EmployeeID,
            WorkingDate = new DateTime(2026, 6, 10), Amount = 450
        });
        Assert.Equal(saved.ID, updated.ID);
        Assert.Equal(450, updated.Amount);
        Assert.Single(await svc.GetAdvanceEntriesAsync(employee.EmployeeID));
    }

    [Fact]
    public async Task DeleteAdvanceEntry_RemovesRow()
    {
        await using var db = _fixture.CreateContext();
        var svc = new HrPayrollService(db);
        var employee = await SeedEmployeeAsync(svc);
        var saved = await svc.SaveAdvanceEntryAsync(new AdvanceEntry
        {
            EmployeeID = employee.EmployeeID, WorkingDate = new DateTime(2026, 6, 1), Amount = 250
        });
        await svc.DeleteAdvanceEntryAsync(saved.ID);
        Assert.Empty(await svc.GetAdvanceEntriesAsync(employee.EmployeeID));
    }

    // ---- Payment run ----

    private static EmployeePayment NewPayment(string employeeId, DateTime from, DateTime to,
        int overtimeRate = 0, int deduction = 0, DateTime? paymentDate = null) => new()
    {
        PaymentID = "",
        EmployeeID = employeeId,
        DateFrom = from,
        DateTo = to,
        PaymentDate = paymentDate ?? to,
        OvertimeRate = overtimeRate,
        Deduction = deduction,
        ModeOfPayment = "Cash"
    };

    [Fact]
    public async Task RunPayment_UsesBillingCalculatorMath()
    {
        await using var db = _fixture.CreateContext();
        var svc = new HrPayrollService(db);
        var employee = await SeedEmployeeAsync(svc, salary: 30000);
        var from = new DateTime(2026, 7, 1);
        for (var i = 0; i < 3; i++)
        {
            await svc.SaveAttendanceAsync(new EmployeeAttendance
            {
                EmployeeID = employee.EmployeeID, WorkingDate = from.AddDays(i), Status = "P",
                InTime = "09:00:00", OutTime = "18:00:00", Overtime = "01:00:00"
            });
        }
        await svc.SaveAdvanceEntryAsync(new AdvanceEntry
        {
            EmployeeID = employee.EmployeeID, WorkingDate = from, Amount = 1000
        });

        var to = from.AddDays(6);
        var payment = await svc.RunPaymentAsync(NewPayment(
            employee.EmployeeID, from, to, overtimeRate: 60, deduction: 400));

        var expected = BillingCalculator.ComputePayroll(
            30000, 3, new TimeSpan(3, 0, 0), 60, 1000, 400);
        Assert.Matches(@"^SP-[1-9]{9}$", payment.PaymentID);
        Assert.Equal(3, payment.PresentDays);
        Assert.Equal(expected.Salary, payment.Salary);
        Assert.Equal(expected.OvertimeAmount, payment.OverTimeAmount);
        Assert.Equal(expected.Advance, payment.Advance);
        Assert.Equal(expected.Deduction, payment.Deduction);
        Assert.Equal(expected.NetPay, payment.NetPay);
        Assert.Equal(new TimeSpan(3, 0, 0).ToString(), payment.Overtime);

        // Legacy writes a matching deduction row into the advance ledger.
        var ledger = await svc.GetAdvanceEntriesAsync(employee.EmployeeID);
        Assert.Contains(ledger, e => e.Deduction == 400 && e.Amount == 0);
        var outstanding = await svc.GetOutstandingAdvanceAsync(employee.EmployeeID, from, to.AddDays(1));
        Assert.Equal(600, outstanding);
    }

    [Fact]
    public async Task RunPayment_DeductionExceedsAdvance_Throws()
    {
        await using var db = _fixture.CreateContext();
        var svc = new HrPayrollService(db);
        var employee = await SeedEmployeeAsync(svc);
        var from = new DateTime(2026, 8, 1);
        await svc.SaveAdvanceEntryAsync(new AdvanceEntry
        {
            EmployeeID = employee.EmployeeID, WorkingDate = from, Amount = 100
        });
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.RunPaymentAsync(NewPayment(employee.EmployeeID, from, from.AddDays(6), deduction: 500)));
        Assert.Equal("You can not deduct amount more than advance amount", ex.Message);
    }

    [Fact]
    public async Task RunPayment_NegativeNetPay_Throws()
    {
        await using var db = _fixture.CreateContext();
        var svc = new HrPayrollService(db);
        var employee = await SeedEmployeeAsync(svc, salary: 0);
        var from = new DateTime(2026, 9, 1);
        await svc.SaveAdvanceEntryAsync(new AdvanceEntry
        {
            EmployeeID = employee.EmployeeID, WorkingDate = from, Amount = 1000
        });
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.RunPaymentAsync(NewPayment(employee.EmployeeID, from, from.AddDays(6), deduction: 500)));
        Assert.Equal("Net pay should be more than 0", ex.Message);
    }

    [Fact]
    public async Task RunPayment_AlreadyPaidSameDate_Throws()
    {
        await using var db = _fixture.CreateContext();
        var svc = new HrPayrollService(db);
        var employee = await SeedEmployeeAsync(svc, salary: 30000);
        var from = new DateTime(2026, 10, 1);
        await svc.SaveAttendanceAsync(new EmployeeAttendance
        {
            EmployeeID = employee.EmployeeID, WorkingDate = from, Status = "P",
            InTime = "09:00:00", OutTime = "17:00:00", Overtime = "00:00:00"
        });
        var payDate = from.AddDays(7);
        await svc.RunPaymentAsync(NewPayment(employee.EmployeeID, from, from.AddDays(6), paymentDate: payDate));
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.RunPaymentAsync(NewPayment(employee.EmployeeID, from, from.AddDays(6), paymentDate: payDate)));
        Assert.Equal("Employee is already paid today", ex.Message);
    }

    [Fact]
    public async Task GeneratePaymentId_HasLegacyFormat()
    {
        await using var db = _fixture.CreateContext();
        var svc = new HrPayrollService(db);
        Assert.Matches(@"^SP-[1-9]{9}$", await svc.GeneratePaymentIdAsync());
    }

    [Fact]
    public async Task DeletePayment_RemovesRow()
    {
        await using var db = _fixture.CreateContext();
        var svc = new HrPayrollService(db);
        var employee = await SeedEmployeeAsync(svc, salary: 30000);
        var from = new DateTime(2026, 11, 1);
        await svc.SaveAttendanceAsync(new EmployeeAttendance
        {
            EmployeeID = employee.EmployeeID, WorkingDate = from, Status = "P",
            InTime = "09:00:00", OutTime = "17:00:00", Overtime = "00:00:00"
        });
        var payment = await svc.RunPaymentAsync(NewPayment(employee.EmployeeID, from, from.AddDays(6)));
        await svc.DeletePaymentAsync(payment.PaymentID);
        Assert.Empty(await svc.GetPaymentsAsync(employee.EmployeeID));
    }
}
