using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagement.Core.Entities;

[Table("ExtraBed")]
public class ExtraBed
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    [MaxLength(100)]
    public string BedType { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Charges { get; set; }
}
