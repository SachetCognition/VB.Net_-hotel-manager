using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagement.Core.Models;

public class CheckInRoom
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    [Required]
    [StringLength(9)]
    public string GuestID { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string RoomNo { get; set; } = string.Empty;

    public decimal RoomCharges { get; set; }

    public DateTime DateIN { get; set; }

    public DateTime DateOUT { get; set; }

    public int NoOfAdults { get; set; }

    public int NoOfKids { get; set; }

    public int NoOfDays { get; set; }

    public decimal TotalRoomCharges { get; set; }

    public decimal OtherCharges { get; set; }

    public decimal SubTotal { get; set; }

    public decimal ServiceTaxPer { get; set; }

    public decimal ServiceTaxAmount { get; set; }

    public decimal LuxuryTaxPer { get; set; }

    public decimal LuxuryTaxAmount { get; set; }

    public decimal DiscountPer { get; set; }

    public decimal Discount { get; set; }

    public decimal GrandTotal { get; set; }

    public decimal TotalPaid { get; set; }

    public decimal Balance { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Checked In";

    [StringLength(1000)]
    public string? Notes { get; set; }

    public bool ExtraBed { get; set; }

    [StringLength(20)]
    public string? CurrencyID { get; set; }

    [ForeignKey(nameof(GuestID))]
    public Guest? Guest { get; set; }

    [ForeignKey(nameof(RoomNo))]
    public Room? Room { get; set; }
}
