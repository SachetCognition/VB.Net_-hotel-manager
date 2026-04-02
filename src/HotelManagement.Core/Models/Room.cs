using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Core.Models;

public class Room
{
    [Key]
    [StringLength(20)]
    public string RoomNo { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string RoomType { get; set; } = string.Empty;

    [Required]
    public decimal RoomCharges { get; set; }
}
