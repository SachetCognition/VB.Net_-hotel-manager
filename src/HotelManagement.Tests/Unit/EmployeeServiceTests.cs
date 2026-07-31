using FluentAssertions;
using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Services;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HotelManagement.Tests.Unit;

public class EmployeeServiceTests : IDisposable
{
    private readonly HotelDbContext _context;
    private readonly EmployeeService _service;

    public EmployeeServiceTests()
    {
        var options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new HotelDbContext(options);
        _service = new EmployeeService(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public void GenerateEmployeeId_ReturnsCorrectFormat()
    {
        var id = _service.GenerateEmployeeId();
        id.Should().StartWith("E-");
        id.Length.Should().Be(8); // "E-" + 6 digits
        id[2..].Should().MatchRegex("^[1-9]{6}$");
    }

    [Fact]
    public async Task CreateAsync_ValidEmployee_ReturnsResponse()
    {
        var request = new CreateEmployeeRequest(
            "Jane Doe", "123 St", "9876543210", "jane@test.com",
            "O+", "Female", "HR", "Manager",
            DateTime.Today.AddYears(-1), 30000m, "08:00");

        var result = await _service.CreateAsync(request);
        result.EmployeeID.Should().StartWith("E-");
        result.EmployeeName.Should().Be("Jane Doe");
        result.Department.Should().Be("HR");
    }

    [Fact]
    public async Task CreateAsync_EmptyName_ThrowsArgumentException()
    {
        var request = new CreateEmployeeRequest(
            "", "123 St", "9876543210", "test@test.com",
            "O+", "Male", "HR", "Manager",
            DateTime.Today, 30000m, "08:00");

        var act = async () => await _service.CreateAsync(request);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*employee name*");
    }

    [Fact]
    public async Task CreateAsync_InvalidEmail_ThrowsArgumentException()
    {
        var request = new CreateEmployeeRequest(
            "John", "123 St", "9876543210", "invalid-email",
            "O+", "Male", "HR", "Manager",
            DateTime.Today, 30000m, "08:00");

        var act = async () => await _service.CreateAsync(request);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*email*");
    }

    [Fact]
    public async Task CreateAsync_EmptyEmail_DoesNotThrow()
    {
        var request = new CreateEmployeeRequest(
            "John", "123 St", "9876543210", "",
            "O+", "Male", "HR", "Manager",
            DateTime.Today, 30000m, "08:00");

        var result = await _service.CreateAsync(request);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithAttendanceRecords_ThrowsInvalidOperation()
    {
        _context.Set<Employee>().Add(new Employee
        {
            EmployeeID = "E-123456", EmployeeName = "Test",
            Address = "Addr", MobileNo = "1234567890"
        });
        _context.Set<EmployeeAttendance>().Add(new EmployeeAttendance
        {
            EmployeeID = "E-123456", EmployeeName = "Test",
            WorkingDate = DateTime.Today, Status = "P"
        });
        await _context.SaveChangesAsync();

        var act = async () => await _service.DeleteAsync("E-123456");
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*attendance records exist*");
    }

    [Fact]
    public async Task DeleteAsync_WithPaymentRecords_ThrowsInvalidOperation()
    {
        _context.Set<Employee>().Add(new Employee
        {
            EmployeeID = "E-123456", EmployeeName = "Test",
            Address = "Addr", MobileNo = "1234567890"
        });
        _context.Set<EmployeePayment>().Add(new EmployeePayment
        {
            EmployeeID = "E-123456", EmployeeName = "Test", PaymentID = "SP-123456789",
            PaymentDate = DateTime.Today
        });
        await _context.SaveChangesAsync();

        var act = async () => await _service.DeleteAsync("E-123456");
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*payment records exist*");
    }

    [Fact]
    public async Task DeleteAsync_WithAdvanceRecords_ThrowsInvalidOperation()
    {
        _context.Set<Employee>().Add(new Employee
        {
            EmployeeID = "E-123456", EmployeeName = "Test",
            Address = "Addr", MobileNo = "1234567890"
        });
        _context.Set<AdvanceEntry>().Add(new AdvanceEntry
        {
            EmployeeID = "E-123456", EmployeeName = "Test",
            WorkingDate = DateTime.Today, Amount = 1000m
        });
        await _context.SaveChangesAsync();

        var act = async () => await _service.DeleteAsync("E-123456");
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*advance entry records exist*");
    }

    [Fact]
    public async Task DeleteAsync_NoRelatedRecords_DeletesSuccessfully()
    {
        _context.Set<Employee>().Add(new Employee
        {
            EmployeeID = "E-123456", EmployeeName = "Test",
            Address = "Addr", MobileNo = "1234567890"
        });
        await _context.SaveChangesAsync();

        await _service.DeleteAsync("E-123456");
        var emp = await _context.Set<Employee>().FindAsync("E-123456");
        emp.Should().BeNull();
    }

    [Fact]
    public async Task GetDepartmentsAsync_ReturnsDistinctDepartments()
    {
        _context.Set<Employee>().Add(new Employee { EmployeeID = "E-111111", EmployeeName = "A", Department = "HR", Address = "A", MobileNo = "1" });
        _context.Set<Employee>().Add(new Employee { EmployeeID = "E-222222", EmployeeName = "B", Department = "HR", Address = "B", MobileNo = "2" });
        _context.Set<Employee>().Add(new Employee { EmployeeID = "E-333333", EmployeeName = "C", Department = "IT", Address = "C", MobileNo = "3" });
        await _context.SaveChangesAsync();

        var depts = await _service.GetDepartmentsAsync();
        depts.Should().HaveCount(2);
        depts.Should().Contain("HR");
        depts.Should().Contain("IT");
    }
}
