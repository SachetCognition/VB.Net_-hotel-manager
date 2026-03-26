using HotelManagement.Core.DTOs;

namespace HotelManagement.Core.Interfaces;

public interface IAttendanceService
{
    Task<AttendanceResponse> CreateAsync(AttendanceRequest request);
    Task<IEnumerable<AttendanceResponse>> GetAllAsync(string? employeeId = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task<AttendanceResponse?> GetByIdAsync(int id);
    Task DeleteAsync(int id);
    Task<int> GetPresentDaysAsync(string employeeId, DateTime fromDate, DateTime toDate);
    Task<TimeSpan> GetTotalOvertimeAsync(string employeeId, DateTime fromDate, DateTime toDate);
}
