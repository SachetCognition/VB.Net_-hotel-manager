using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagement.Core.Entities;

[Table("Temp_Reservation")]
public class Reservation
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    [Required]
    [MaxLength(20)]
    public string GuestID { get; set; } = string.Empty;

    [MaxLength(200)]
    public string GuestName { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string RoomNo { get; set; } = string.Empty;

    [MaxLength(50)]
    public string RoomType { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal RoomCharges { get; set; }

    public DateTime DateIN { get; set; }
    public DateTime DateOUT { get; set; }

    public int NoOfAdults { get; set; }
    public int NoOfKids { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Reserved";

    [MaxLength(50)]
    public string Currency { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Notes { get; set; } = string.Empty;

    [ForeignKey("GuestID")]
    public Guest? Guest { get; set; }

    [ForeignKey("RoomNo")]
    public Room? Room { get; set; }
}
