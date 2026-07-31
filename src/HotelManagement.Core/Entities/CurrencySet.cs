using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagement.Core.Entities;

[Table("CurrencySet")]
public class CurrencySet
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    [Required]
    [MaxLength(50)]
    public string CurrencyName { get; set; } = string.Empty;

    [MaxLength(10)]
    public string Symbol { get; set; } = string.Empty;
}
