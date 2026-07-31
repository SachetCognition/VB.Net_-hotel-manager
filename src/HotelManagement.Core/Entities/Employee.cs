using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagement.Core.Entities;

[Table("Employee")]
public class Employee
{
    [Key]
    [MaxLength(20)]
    public string EmployeeID { get; set; } = string.Empty; // Format: "E-" + 6 random digits

    [Required]
    [MaxLength(200)]
    public string EmployeeName { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string MobileNo { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(10)]
    public string BloodGroup { get; set; } = string.Empty;

    [MaxLength(10)]
    public string Gender { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Department { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Designation { get; set; } = string.Empty;

    public DateTime DateOfJoining { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Salary { get; set; }

    [MaxLength(20)]
    public string BasicWorkingTime { get; set; } = string.Empty;
}
