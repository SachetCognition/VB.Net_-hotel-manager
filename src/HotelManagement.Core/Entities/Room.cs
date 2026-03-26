using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagement.Core.Entities;

[Table("Room")]
public class Room
{
    [Key]
    [MaxLength(20)]
    public string RoomNo { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string RoomType { get; set; } = string.Empty; // AC, Non-AC, Deluxe, Suite

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal RoomCharges { get; set; }
}
