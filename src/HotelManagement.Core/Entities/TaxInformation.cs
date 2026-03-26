using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagement.Core.Entities;

[Table("TaxInformation")]
public class TaxInformation
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    [MaxLength(100)]
    public string TaxName { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,4)")]
    public decimal TaxPercentage { get; set; }

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
}
