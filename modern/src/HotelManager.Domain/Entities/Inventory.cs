namespace HotelManager.Domain.Entities;

public class Dish
{
    public int ID { get; set; }
    public string? DishName { get; set; }
    public string? Category { get; set; }
    public int? Rate { get; set; }
}

public class Beer
{
    public string ID { get; set; } = null!;
    public string? BeerName { get; set; }
    public int? Rate { get; set; }
}

public class Liquor
{
    public int ID { get; set; }
    public string? LiquorName { get; set; }
    public int? Volume { get; set; }
    public int? Rate { get; set; }
    public LiquorMaster? Master { get; set; }
}

public class LiquorMaster
{
    public string LiquorName { get; set; } = null!;
}

public class Stock
{
    public string StockID { get; set; } = null!;
    public string? LiquorName { get; set; }
    public int? NoOfBottles { get; set; }
    public int? Volume { get; set; }
    public int? TotalVolume { get; set; }
    public DateTime? StockDate { get; set; }
    public LiquorMaster? LiquorMaster { get; set; }
}

public class StockBeer
{
    public string StockID { get; set; } = null!;
    public string? BeerID { get; set; }
    public int? NoOfBottles { get; set; }
    public DateTime? StockDate { get; set; }
    public Beer? Beer { get; set; }
}

public class PurchasedInventory
{
    public int ID { get; set; }
    public string? ProductName { get; set; }
    public string? Category { get; set; }
    public string? TransactionType { get; set; }
    public string? PartyName { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public double? Quantity { get; set; }
    public string? Unit { get; set; }
    public int? Price { get; set; }
    public int? TotalPrice { get; set; }
}
