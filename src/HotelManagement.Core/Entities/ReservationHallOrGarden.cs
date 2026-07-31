using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagement.Core.Entities;

[Table("Reservation_HallorGarden")]
public class ReservationHallOrGarden
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    [MaxLength(20)]
    public string GuestID { get; set; } = string.Empty;

    [MaxLength(200)]
    public string GuestName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string VenueName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string VenueType { get; set; } = string.Empty; // "Hall" or "Garden"

    public DateTime DateIN { get; set; }
    public DateTime DateOUT { get; set; }

    public int NoOfDays { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Rate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCharges { get; set; }

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
    public decimal EducessTax { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal EducessTaxAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal HEducessTax { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal HEducessTaxAmount { get; set; }

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
