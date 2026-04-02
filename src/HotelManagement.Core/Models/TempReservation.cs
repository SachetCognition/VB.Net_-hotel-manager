using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagement.Core.Models;

public class TempReservation
{
    [Key]
    [StringLength(20)]
    public string ReservationID { get; set; } = string.Empty;

    [Required]
    [StringLength(9)]
    public string GuestID { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string RoomNo { get; set; } = string.Empty;

    public DateTime DateIn { get; set; }
    public DateTime DateOut { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Reserved";

    [StringLength(1000)]
    public string? Notes { get; set; }

    [ForeignKey(nameof(GuestID))]
    public Guest? Guest { get; set; }

    [ForeignKey(nameof(RoomNo))]
    public Room? Room { get; set; }
}

public class ReservationHallOrGarden
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    [Required]
    [StringLength(9)]
    public string GuestID { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Type { get; set; } = string.Empty;

    public DateTime DateIn { get; set; }
    public DateTime DateOut { get; set; }
    public decimal Charges { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Reserved";

    [StringLength(1000)]
    public string? Notes { get; set; }

    [ForeignKey(nameof(GuestID))]
    public Guest? Guest { get; set; }
}

public class ReservationHallAndGarden
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    [Required]
    [StringLength(9)]
    public string GuestID { get; set; } = string.Empty;

    public DateTime DateIn { get; set; }
    public DateTime DateOut { get; set; }
    public decimal HallCharges { get; set; }
    public decimal GardenCharges { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Reserved";

    [StringLength(1000)]
    public string? Notes { get; set; }

    [ForeignKey(nameof(GuestID))]
    public Guest? Guest { get; set; }
}
