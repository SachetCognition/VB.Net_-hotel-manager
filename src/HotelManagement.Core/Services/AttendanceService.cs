using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Core.Services;

public class AttendanceService : IAttendanceService
{
    private readonly DbContext _context;

    public AttendanceService(DbContext context)
    {
        _context = context;
    }

    public async Task<AttendanceResponse> CreateAsync(AttendanceRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.EmployeeID))
            throw new ArgumentException("Please select employee");

        var attendance = new EmployeeAttendance
        {
            EmployeeID = request.EmployeeID,
            EmployeeName = request.EmployeeName,
            WorkingDate = request.WorkingDate,
            Status = request.Status,
            Overtime = request.Overtime ?? "00:00:00",
            Department = request.Department ?? string.Empty
        };

        _context.Set<EmployeeAttendance>().Add(attendance);
        await _context.SaveChangesAsync();
        return MapToResponse(attendance);
    }

    public async Task<IEnumerable<AttendanceResponse>> GetAllAsync(
        string? employeeId = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Set<EmployeeAttendance>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(employeeId))
            query = query.Where(a => a.EmployeeID == employeeId);
        if (fromDate.HasValue)
            query = query.Where(a => a.WorkingDate >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(a => a.WorkingDate <= toDate.Value);

        var results = await query.OrderByDescending(a => a.WorkingDate).ToListAsync();
        return results.Select(MapToResponse);
    }

    public async Task<AttendanceResponse?> GetByIdAsync(int id)
    {
        var attendance = await _context.Set<EmployeeAttendance>().FindAsync(id);
        return attendance == null ? null : MapToResponse(attendance);
    }

    public async Task DeleteAsync(int id)
    {
        var attendance = await _context.Set<EmployeeAttendance>().FindAsync(id)
            ?? throw new KeyNotFoundException($"Attendance record {id} not found");

        _context.Set<EmployeeAttendance>().Remove(attendance);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetPresentDaysAsync(string employeeId, DateTime fromDate, DateTime toDate)
    {
        return await _context.Set<EmployeeAttendance>()
            .CountAsync(a => a.EmployeeID == employeeId
                && a.WorkingDate >= fromDate
                && a.WorkingDate <= toDate
                && a.Status == "P");
    }

    public async Task<TimeSpan> GetTotalOvertimeAsync(string employeeId, DateTime fromDate, DateTime toDate)
    {
        var records = await _context.Set<EmployeeAttendance>()
            .Where(a => a.EmployeeID == employeeId
                && a.WorkingDate >= fromDate
                && a.WorkingDate <= toDate
                && a.Status == "P")
            .Select(a => a.Overtime)
            .ToListAsync();

        var total = TimeSpan.Zero;
        foreach (var ot in records)
        {
            if (TimeSpan.TryParse(ot, out var ts))
                total = total.Add(ts);
        }
        return total;
    }

    private static AttendanceResponse MapToResponse(EmployeeAttendance a) =>
        new(a.ID, a.EmployeeID, a.EmployeeName, a.WorkingDate, a.Status, a.Overtime, a.Department);
}
