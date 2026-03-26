using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagement.Core.Entities;

[Table("EmployeeAttendance")]
public class EmployeeAttendance
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    [Required]
    [MaxLength(20)]
    public string EmployeeID { get; set; } = string.Empty;

    [MaxLength(200)]
    public string EmployeeName { get; set; } = string.Empty;

    public DateTime WorkingDate { get; set; }

    [MaxLength(10)]
    public string Status { get; set; } = string.Empty; // "P" (Present) or "A" (Absent)

    [MaxLength(20)]
    public string Overtime { get; set; } = string.Empty; // TimeSpan as string "HH:mm:ss"

    [MaxLength(100)]
    public string Department { get; set; } = string.Empty;

    [ForeignKey("EmployeeID")]
    public Employee? Employee { get; set; }
}
