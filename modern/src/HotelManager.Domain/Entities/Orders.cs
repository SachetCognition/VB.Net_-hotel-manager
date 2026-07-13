namespace HotelManager.Domain.Entities;

public class OrderInfo
{
    public int ID { get; set; }
    public int? CurrencyID { get; set; }
    public int? HotelID { get; set; }
    public string? OrderNo { get; set; }
    public DateTime? OrderDate { get; set; }
    public int? CheckInID { get; set; }
    public int? SubTotal { get; set; }
    public double? VATPer { get; set; }
    public double? VATAmount { get; set; }
    public double? STPer { get; set; }
    public double? STAmount { get; set; }
    public int? GrandTotal { get; set; }
    public int? TotalPayment { get; set; }
    public int? PaymentDue { get; set; }
    public CheckInRoom? CheckIn { get; set; }
    public CurrencySet? Currency { get; set; }
    public HotelInfo? Hotel { get; set; }
    public ICollection<OrderedProduct> Products { get; set; } = new List<OrderedProduct>();
    public ICollection<TaxOrder> Taxes { get; set; } = new List<TaxOrder>();
}

public class OrderedProduct
{
    public int ID { get; set; }
    public int? OrderID { get; set; }
    public string? ProductID { get; set; }
    public string? ProductName { get; set; }
    public int? Volume { get; set; }
    public int? Rate { get; set; }
    public int? Quantity { get; set; }
    public int? Amount { get; set; }
    public OrderInfo? Order { get; set; }
}

public class RestaurantOrderInfo
{
    public int ID { get; set; }
    public string? OrderNo { get; set; }
    public int? CurrencyID { get; set; }
    public int? HotelID { get; set; }
    public DateTime? OrderDate { get; set; }
    public int? SubTotal { get; set; }
    public double? VATPer { get; set; }
    public double? STPer { get; set; }
    public double? VATAmount { get; set; }
    public double? STAmount { get; set; }
    public int? GrandTotal { get; set; }
    public int? TotalPayment { get; set; }
    public int? PaymentDue { get; set; }
    public CurrencySet? Currency { get; set; }
    public HotelInfo? Hotel { get; set; }
    public ICollection<RestaurantOrderedProduct> Products { get; set; } = new List<RestaurantOrderedProduct>();
    public ICollection<TaxRestaurantOrder> Taxes { get; set; } = new List<TaxRestaurantOrder>();
}

public class RestaurantOrderedProduct
{
    public int ID { get; set; }
    public int? OrderID { get; set; }
    public string? ProductID { get; set; }
    public string? ProductName { get; set; }
    public int? Rate { get; set; }
    public int? Quantity { get; set; }
    public int? Amount { get; set; }
    public RestaurantOrderInfo? Order { get; set; }
}

public class Trans
{
    public int ID { get; set; }
    public string? Employee_Party_Name { get; set; }
    public string? TransactionType { get; set; }
    public string? TransactionDetails { get; set; }
    public string? TransactionMonth { get; set; }
    public DateTime? TransactionDate { get; set; }
    public int? TransactionAmount { get; set; }
    public int? AmountReceived { get; set; }
    public int? DueAmount { get; set; }
}

public class TaxInfo
{
    public string Salray { get; set; } = null!;
    public int? TaxinP { get; set; }
}

public class TaxOrder
{
    public int ID { get; set; }
    public int? OrderID { get; set; }
    public double? HEduTax { get; set; }
    public double? HEduTaxAmount { get; set; }
    public double? EducationalTax { get; set; }
    public double? EducationalTaxAmount { get; set; }
    public OrderInfo? Order { get; set; }
}

public class TaxRoom
{
    public int ID { get; set; }
    public int? BillID { get; set; }
    public double? HEduTax { get; set; }
    public double? HEduTaxAmount { get; set; }
    public double? EducationalTax { get; set; }
    public double? EducationalTaxAmount { get; set; }
    public CheckoutRoom? Bill { get; set; }
}

public class TaxReservationHallAndGarden
{
    public int ID { get; set; }
    public string? ReservationID { get; set; }
    public double? HEduTax { get; set; }
    public double? HEduTaxAmount { get; set; }
    public double? EducationalTax { get; set; }
    public double? EducationalTaxAmount { get; set; }
    public ReservationHallAndGarden? Reservation { get; set; }
}

public class TaxReservationHallOrGarden
{
    public int ID { get; set; }
    public string? ReservationID { get; set; }
    public double? HEduTax { get; set; }
    public double? HEduTaxAmount { get; set; }
    public double? EducationalTax { get; set; }
    public double? EducationalTaxAmount { get; set; }
    public ReservationHallOrGarden? Reservation { get; set; }
}

public class TaxRestaurantOrder
{
    public int ID { get; set; }
    public int? OrderID { get; set; }
    public double? HEduTax { get; set; }
    public double? HEduTaxAmount { get; set; }
    public double? EducationalTax { get; set; }
    public double? EducationalTaxAmount { get; set; }
    public RestaurantOrderInfo? Order { get; set; }
}
