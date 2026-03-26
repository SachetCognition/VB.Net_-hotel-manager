using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagement.Core.Entities;

[Table("Guest")]
public class Guest
{
    [Key]
    [MaxLength(20)]
    public string GuestID { get; set; } = string.Empty; // Format: "G-" + 6 random digits

    [Required]
    [MaxLength(200)]
    public string GuestName { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string ContactNo { get; set; } = string.Empty;

    [MaxLength(50)]
    public string IDType { get; set; } = string.Empty;

    [MaxLength(100)]
    public string IDNumber { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Notes { get; set; } = string.Empty;
}
