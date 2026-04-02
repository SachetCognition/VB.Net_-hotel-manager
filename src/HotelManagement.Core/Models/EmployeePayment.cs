using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagement.Core.Models;

public class EmployeePayment
{
    [Key]
    [StringLength(15)]
    public string PaymentID { get; set; } = string.Empty;

    public DateTime DateFrom { get; set; }

    public DateTime DateTo { get; set; }

    [Required]
    [StringLength(9)]
    public string EmployeeID { get; set; } = string.Empty;

    public int PresentDays { get; set; }

    public decimal Salary { get; set; }

    public decimal Advance { get; set; }

    public decimal Deduction { get; set; }

    public TimeSpan OverTime { get; set; }

    public decimal OverTimeRate { get; set; }

    public decimal OverTimeAmount { get; set; }

    public DateTime PaymentDate { get; set; }

    [StringLength(50)]
    public string ModeOfPayment { get; set; } = string.Empty;

    [StringLength(200)]
    public string? PaymentModeDetails { get; set; }

    public decimal NetPay { get; set; }

    [ForeignKey(nameof(EmployeeID))]
    public Employee? Employee { get; set; }
}
