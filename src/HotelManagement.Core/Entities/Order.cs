using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagement.Core.Entities;

[Table("Orders")]
public class Order
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    [MaxLength(20)]
    public string GuestID { get; set; } = string.Empty;

    [MaxLength(200)]
    public string GuestName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string RoomNo { get; set; } = string.Empty;

    [MaxLength(200)]
    public string ItemName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Rate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    public DateTime OrderDate { get; set; }

    [MaxLength(50)]
    public string OrderType { get; set; } = string.Empty; // "Room" or "Restaurant"

    [MaxLength(1000)]
    public string Notes { get; set; } = string.Empty;
}
