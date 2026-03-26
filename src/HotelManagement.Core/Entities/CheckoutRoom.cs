using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagement.Core.Entities;

[Table("Checkout_Room")]
public class CheckoutRoom
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    [MaxLength(20)]
    public string BillNo { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string GuestID { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string RoomNo { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal RoomCharges { get; set; }

    public DateTime DateIN { get; set; }
    public DateTime DateOUT { get; set; }

    public int NoOfAdults { get; set; }
    public int NoOfKids { get; set; }

    [MaxLength(200)]
    public string GuestName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [MaxLength(20)]
    public string ContactNo { get; set; } = string.Empty;

    public int NoOfDays { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalRoomCharges { get; set; }

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

    [MaxLength(10)]
    public string ExtraBed { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Currency { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Status { get; set; } = "Checked Out";

    [MaxLength(1000)]
    public string Notes { get; set; } = string.Empty;

    public DateTime CheckOutDate { get; set; } = DateTime.UtcNow;
}
