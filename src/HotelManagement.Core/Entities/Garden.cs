using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagement.Core.Entities;

[Table("Garden")]
public class Garden
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    [Required]
    [MaxLength(100)]
    public string GardenName { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Charges { get; set; }

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
}
