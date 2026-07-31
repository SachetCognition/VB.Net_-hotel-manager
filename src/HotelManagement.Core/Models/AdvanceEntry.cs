using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagement.Core.Models;

public class AdvanceEntry
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    public DateTime WorkingDate { get; set; }

    [Required]
    [StringLength(9)]
    public string EmployeeID { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public decimal Deduction { get; set; }

    [ForeignKey(nameof(EmployeeID))]
    public Employee? Employee { get; set; }
}
