using HotelManagement.Core.DTOs;

namespace HotelManagement.Core.Interfaces;

public interface IEmployeeService
{
    Task<EmployeeResponse> CreateAsync(CreateEmployeeRequest request);
    Task<EmployeeResponse?> GetByIdAsync(string employeeId);
    Task<IEnumerable<EmployeeResponse>> GetAllAsync(string? search = null);
    Task<EmployeeResponse> UpdateAsync(string employeeId, UpdateEmployeeRequest request);
    Task DeleteAsync(string employeeId);
    string GenerateEmployeeId();
    Task<IEnumerable<string>> GetDepartmentsAsync();
    Task<IEnumerable<string>> GetDesignationsAsync();
}
