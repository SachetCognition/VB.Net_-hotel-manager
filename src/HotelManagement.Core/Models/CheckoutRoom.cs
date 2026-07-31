using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagement.Core.Models;

public class CheckoutRoom
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    [StringLength(50)]
    public string BillNo { get; set; } = string.Empty;

    public int CheckInID { get; set; }

    public DateTime CheckOutDate { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    [StringLength(20)]
    public string? HotelID { get; set; }

    [StringLength(20)]
    public string? CurrencyID { get; set; }

    [ForeignKey(nameof(CheckInID))]
    public CheckInRoom? CheckInRoom { get; set; }
}
