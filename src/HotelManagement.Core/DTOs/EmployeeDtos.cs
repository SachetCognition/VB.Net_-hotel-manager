namespace HotelManagement.Core.DTOs;

public record CreateEmployeeRequest(
    string EmployeeName, string Address, string MobileNo, string Email,
    string BloodGroup, string Gender, string Department, string Designation,
    DateTime DateOfJoining, decimal Salary, string BasicWorkingTime);

public record UpdateEmployeeRequest(
    string EmployeeName, string Address, string MobileNo, string Email,
    string BloodGroup, string Gender, string Department, string Designation,
    DateTime DateOfJoining, decimal Salary, string BasicWorkingTime);

public record EmployeeResponse(
    string EmployeeID, string EmployeeName, string Address, string MobileNo,
    string Email, string BloodGroup, string Gender, string Department,
    string Designation, DateTime DateOfJoining, decimal Salary, string BasicWorkingTime);

public record AttendanceRequest(
    string EmployeeID, string EmployeeName, DateTime WorkingDate,
    string Status, string Overtime, string Department);

public record AttendanceResponse(
    int ID, string EmployeeID, string EmployeeName, DateTime WorkingDate,
    string Status, string Overtime, string Department);

public record ProcessPaymentRequest(
    string EmployeeID, DateTime FromDate, DateTime ToDate,
    decimal OvertimeRate, decimal Deduction);

public record PaymentResponse(
    int ID, string PaymentID, string EmployeeID, string EmployeeName,
    string Department, string Designation, DateTime PaymentDate,
    DateTime FromDate, DateTime ToDate, decimal BasicSalary, int PresentDays,
    decimal Salary, string TotalOvertime, decimal OvertimeRate,
    decimal OvertimeAmount, decimal Advance, decimal Deduction, decimal NetPay);

public record AdvanceEntryRequest(
    string EmployeeID, string EmployeeName, DateTime WorkingDate, decimal Amount);

public record AdvanceEntryResponse(
    int ID, string EmployeeID, string EmployeeName, DateTime WorkingDate,
    decimal Amount, decimal Deduction);
