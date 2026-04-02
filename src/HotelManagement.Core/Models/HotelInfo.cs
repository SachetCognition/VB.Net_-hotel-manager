using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagement.Core.Models;

public class HotelInfo
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    [Required]
    [StringLength(200)]
    public string HotelName { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string ContactNo { get; set; } = string.Empty;

    [StringLength(20)]
    public string? ContactNo1 { get; set; }

    [StringLength(200)]
    public string? Email { get; set; }

    [StringLength(50)]
    public string? TIN { get; set; }

    [StringLength(50)]
    public string? STNo { get; set; }

    public byte[]? Logo { get; set; }
}
