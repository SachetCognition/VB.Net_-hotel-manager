using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Core.Models;

public class Employee
{
    [Key]
    [StringLength(9)]
    public string EmployeeID { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string EmployeeName { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string MobileNo { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [StringLength(10)]
    public string? BloodGroup { get; set; }

    [Required]
    [StringLength(100)]
    public string Department { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Designation { get; set; } = string.Empty;

    public DateTime DateOfJoining { get; set; }

    public decimal Salary { get; set; }

    public TimeSpan BasicWorkingTime { get; set; }
}
