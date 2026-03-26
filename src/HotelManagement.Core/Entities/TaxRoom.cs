using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagement.Core.Entities;

[Table("Tax_Room")]
public class TaxRoom
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    [MaxLength(20)]
    public string BillNo { get; set; } = string.Empty;

    [MaxLength(20)]
    public string RoomNo { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal ServiceTaxPer { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ServiceTaxAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal LuxuryTaxPer { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal LuxuryTaxAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal EducessTax { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal EducessTaxAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal HEducessTax { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal HEducessTaxAmount { get; set; }
}
