using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagement.Core.Models;

public class EmployeeAttendance
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    public DateTime WorkingDate { get; set; }

    [Required]
    [StringLength(9)]
    public string EmployeeID { get; set; } = string.Empty;

    [Required]
    [StringLength(1)]
    public string Status { get; set; } = "P";

    public TimeSpan InTime { get; set; }

    public TimeSpan OutTime { get; set; }

    public TimeSpan Overtime { get; set; }

    public TimeSpan BasicWorkingTime { get; set; }

    [ForeignKey(nameof(EmployeeID))]
    public Employee? Employee { get; set; }
}
