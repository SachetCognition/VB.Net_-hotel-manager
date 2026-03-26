using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagement.Core.Entities;

[Table("Reservation_HallandGarden")]
public class ReservationHallAndGarden
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    [MaxLength(20)]
    public string GuestID { get; set; } = string.Empty;

    [MaxLength(200)]
    public string GuestName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string HallName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string GardenName { get; set; } = string.Empty;

    public DateTime DateIN { get; set; }
    public DateTime DateOUT { get; set; }

    public int NoOfDaysHall { get; set; }
    public int NoOfDaysGarden { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal RateHall { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal RateGarden { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalHall { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalGarden { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal OtherCharges { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountPer { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Discount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal SubTotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ServiceTaxPer { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ServiceTaxAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal LuxuryTaxPer { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal LuxuryTaxAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal GrandTotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPaid { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Balance { get; set; }

    [MaxLength(50)]
    public string Currency { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Status { get; set; } = "Reserved";

    [MaxLength(1000)]
    public string Notes { get; set; } = string.Empty;
}
