using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagement.Core.Entities;

[Table("EmployeePayment")]
public class EmployeePayment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    [Required]
    [MaxLength(20)]
    public string PaymentID { get; set; } = string.Empty; // Format: "SP-" + 9 random digits

    [Required]
    [MaxLength(20)]
    public string EmployeeID { get; set; } = string.Empty;

    [MaxLength(200)]
    public string EmployeeName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Department { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Designation { get; set; } = string.Empty;

    public DateTime PaymentDate { get; set; }

    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal BasicSalary { get; set; }

    public int PresentDays { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Salary { get; set; } // (BasicSalary * PresentDays) / 30

    [MaxLength(20)]
    public string TotalOvertime { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal OvertimeRate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal OvertimeAmount { get; set; } // (TotalOvertimeMinutes * Rate) / 60

    [Column(TypeName = "decimal(18,2)")]
    public decimal Advance { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Deduction { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal NetPay { get; set; } // Salary + OvertimeAmount - Deduction

    [ForeignKey("EmployeeID")]
    public Employee? Employee { get; set; }
}
