using System.Security.Cryptography;
using System.Text.RegularExpressions;
using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Core.Services;

/// <summary>
/// Employee Service implementing referential integrity checks from frmEmployee_registration.vb
/// </summary>
public class EmployeeService : IEmployeeService
{
    private readonly DbContext _context;

    public EmployeeService(DbContext context)
    {
        _context = context;
    }

    public async Task<EmployeeResponse> CreateAsync(CreateEmployeeRequest request)
    {
        ValidateEmployee(request.EmployeeName, request.Address, request.MobileNo, request.Email);

        var employee = new Employee
        {
            EmployeeID = GenerateEmployeeId(),
            EmployeeName = request.EmployeeName,
            Address = request.Address,
            MobileNo = request.MobileNo,
            Email = request.Email,
            BloodGroup = request.BloodGroup ?? string.Empty,
            Gender = request.Gender ?? string.Empty,
            Department = request.Department ?? string.Empty,
            Designation = request.Designation ?? string.Empty,
            DateOfJoining = request.DateOfJoining,
            Salary = request.Salary,
            BasicWorkingTime = request.BasicWorkingTime ?? string.Empty
        };

        _context.Set<Employee>().Add(employee);
        await _context.SaveChangesAsync();
        return MapToResponse(employee);
    }

    public async Task<EmployeeResponse?> GetByIdAsync(string employeeId)
    {
        var employee = await _context.Set<Employee>().FindAsync(employeeId);
        return employee == null ? null : MapToResponse(employee);
    }

    public async Task<IEnumerable<EmployeeResponse>> GetAllAsync(string? search = null)
    {
        var query = _context.Set<Employee>().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(e => e.EmployeeName.StartsWith(search));

        var employees = await query.OrderBy(e => e.EmployeeName).ToListAsync();
        return employees.Select(MapToResponse);
    }

    public async Task<EmployeeResponse> UpdateAsync(string employeeId, UpdateEmployeeRequest request)
    {
        var employee = await _context.Set<Employee>().FindAsync(employeeId)
            ?? throw new KeyNotFoundException($"Employee {employeeId} not found");

        ValidateEmployee(request.EmployeeName, request.Address, request.MobileNo, request.Email);

        employee.EmployeeName = request.EmployeeName;
        employee.Address = request.Address;
        employee.MobileNo = request.MobileNo;
        employee.Email = request.Email;
        employee.BloodGroup = request.BloodGroup ?? string.Empty;
        employee.Gender = request.Gender ?? string.Empty;
        employee.Department = request.Department ?? string.Empty;
        employee.Designation = request.Designation ?? string.Empty;
        employee.DateOfJoining = request.DateOfJoining;
        employee.Salary = request.Salary;
        employee.BasicWorkingTime = request.BasicWorkingTime ?? string.Empty;

        await _context.SaveChangesAsync();
        return MapToResponse(employee);
    }

    /// <summary>
    /// Referential integrity check before delete — from frmEmployee_registration.vb:
    /// Checks attendance, payment, and advance tables before allowing deletion.
    /// </summary>
    public async Task DeleteAsync(string employeeId)
    {
        var employee = await _context.Set<Employee>().FindAsync(employeeId)
            ?? throw new KeyNotFoundException($"Employee {employeeId} not found");

        // Check referential integrity — attendance records
        var hasAttendance = await _context.Set<EmployeeAttendance>()
            .AnyAsync(a => a.EmployeeID == employeeId);
        if (hasAttendance)
            throw new InvalidOperationException("Cannot delete employee: attendance records exist. Delete attendance records first.");

        // Check referential integrity — payment records
        var hasPayments = await _context.Set<EmployeePayment>()
            .AnyAsync(p => p.EmployeeID == employeeId);
        if (hasPayments)
            throw new InvalidOperationException("Cannot delete employee: payment records exist. Delete payment records first.");

        // Check referential integrity — advance records
        var hasAdvance = await _context.Set<AdvanceEntry>()
            .AnyAsync(a => a.EmployeeID == employeeId);
        if (hasAdvance)
            throw new InvalidOperationException("Cannot delete employee: advance entry records exist. Delete advance records first.");

        _context.Set<Employee>().Remove(employee);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Generates a unique Employee ID in the format "E-" + 6 random digits.
    /// Mirrors legacy: txtID.Text = "E-" &amp; GetUniqueKey(6) using RNGCryptoServiceProvider
    /// </summary>
    public string GenerateEmployeeId()
    {
        return "E-" + GetUniqueKey(6);
    }

    public async Task<IEnumerable<string>> GetDepartmentsAsync()
    {
        return await _context.Set<Employee>()
            .Where(e => !string.IsNullOrEmpty(e.Department))
            .Select(e => e.Department)
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetDesignationsAsync()
    {
        return await _context.Set<Employee>()
            .Where(e => !string.IsNullOrEmpty(e.Designation))
            .Select(e => e.Designation)
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync();
    }

    private static string GetUniqueKey(int size)
    {
        const string chars = "123456789";
        var data = new byte[size];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(data);
        var result = new char[size];
        for (int i = 0; i < size; i++)
            result[i] = chars[data[i] % chars.Length];
        return new string(result);
    }

    private static void ValidateEmployee(string name, string address, string mobileNo, string email)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Please enter employee name");
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Please enter address");
        if (string.IsNullOrWhiteSpace(mobileNo))
            throw new ArgumentException("Please enter mobile number");
        if (!string.IsNullOrWhiteSpace(email) && !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new ArgumentException("Invalid email format");
    }

    private static EmployeeResponse MapToResponse(Employee e) =>
        new(e.EmployeeID, e.EmployeeName, e.Address, e.MobileNo,
            e.Email, e.BloodGroup, e.Gender, e.Department,
            e.Designation, e.DateOfJoining, e.Salary, e.BasicWorkingTime);
}
