using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagement.Core.Models;

public class Stock
{
    [Key]
    [StringLength(20)]
    public string StockID { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LiquorName { get; set; } = string.Empty;

    public int NoOfBottles { get; set; }
    public decimal Volume { get; set; }
    public decimal TotalVolume { get; set; }
    public DateTime StockDate { get; set; }
}

public class StockBeer
{
    [Key]
    [StringLength(20)]
    public string StockID { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string BeerID { get; set; } = string.Empty;

    public int NoOfBottles { get; set; }
    public DateTime StockDate { get; set; }

    [ForeignKey(nameof(BeerID))]
    public Beer? Beer { get; set; }
}

public class Beer
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    [Required]
    [StringLength(100)]
    public string BeerName { get; set; } = string.Empty;
}

public class LiquorMaster
{
    [Key]
    [StringLength(100)]
    public string LiquorName { get; set; } = string.Empty;
}

public class Food
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    [Required]
    [StringLength(100)]
    public string FoodName { get; set; } = string.Empty;

    public decimal Price { get; set; }

    [StringLength(50)]
    public string? Category { get; set; }
}

public class PurchasedInventory
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    [Required]
    [StringLength(200)]
    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    [Required]
    [StringLength(50)]
    public string Unit { get; set; } = string.Empty;

    public decimal Price { get; set; }
    public DateTime PurchaseDate { get; set; }
    public decimal TotalPrice { get; set; }

    [StringLength(50)]
    public string? Category { get; set; }

    [StringLength(50)]
    public string? TransactionType { get; set; }

    [StringLength(200)]
    public string? PartyName { get; set; }
}

public class OrderInfo
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    public int CheckInId { get; set; }
    public decimal PaymentDue { get; set; }
    public DateTime OrderDate { get; set; }

    [StringLength(500)]
    public string? Items { get; set; }
}

public class Hall
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    public decimal Charges { get; set; }
}

public class Garden
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    public decimal Charges { get; set; }
}

public class ExtraBed
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    public decimal Charges { get; set; }
}

public class TaxInfo
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    [StringLength(200)]
    public string Salray { get; set; } = string.Empty;

    public decimal TaxInP { get; set; }
}

public class Activation
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    [StringLength(200)]
    public string HardwareID { get; set; } = string.Empty;

    [StringLength(200)]
    public string SerialNo { get; set; } = string.Empty;

    [StringLength(200)]
    public string ActivationID { get; set; } = string.Empty;
}

public class UserAccount
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    [Required]
    [StringLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string UserType { get; set; } = "User";
}
