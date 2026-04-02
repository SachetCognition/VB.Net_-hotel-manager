using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Core.Models;

public class Guest
{
    [Key]
    [StringLength(9)]
    public string GuestID { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string GuestName { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string ContactNo { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string IDType { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string IDNumber { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Notes { get; set; }
}
