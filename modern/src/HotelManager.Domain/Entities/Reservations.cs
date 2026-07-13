namespace HotelManager.Domain.Entities;

public class Reservation
{
    public string ReservationID { get; set; } = null!;
    public string? GuestID { get; set; }
    public string? RoomNo { get; set; }
    public DateTime? DateIN { get; set; }
    public DateTime? DateOUT { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
    public Guest? Guest { get; set; }
    public Room? Room { get; set; }
}

public class TempReservation
{
    public string ReservationID { get; set; } = null!;
    public string? GuestID { get; set; }
    public string? RoomNo { get; set; }
    public DateTime? DateIN { get; set; }
    public DateTime? DateOUT { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
    public Guest? Guest { get; set; }
    public Room? Room { get; set; }
}

public class ReservationHallAndGarden
{
    public string ID { get; set; } = null!;
    public string? GuestID { get; set; }
    public int? CurrencyID { get; set; }
    public int? HotelID { get; set; }
    public string? Hall { get; set; }
    public DateTime? DateFrom_Hall { get; set; }
    public DateTime? DateTo_Hall { get; set; }
    public double? Days_Hall { get; set; }
    public double? Rate_Hall { get; set; }
    public double? TotalCharges_Hall { get; set; }
    public string? Garden { get; set; }
    public DateTime? DateFrom_Garden { get; set; }
    public DateTime? DateTo_Garden { get; set; }
    public int? Days_Garden { get; set; }
    public double? Rate_Garden { get; set; }
    public double? TotalCharges_Garden { get; set; }
    public double? OtherCharges { get; set; }
    public double? SubTotal { get; set; }
    public double? ServiceTaxPer { get; set; }
    public double? ServiceTaxAmount { get; set; }
    public double? LuxuryTaxPer { get; set; }
    public double? LuxuryTaxAmount { get; set; }
    public double? DiscountPer { get; set; }
    public double? Discount { get; set; }
    public double? GrandTotal { get; set; }
    public double? TotalPaid { get; set; }
    public double? Balance { get; set; }
    public string? Notes { get; set; }
    public Guest? Guest { get; set; }
    public CurrencySet? Currency { get; set; }
    public HotelInfo? Hotel { get; set; }
}

public class ReservationHallOrGarden
{
    public string ID { get; set; } = null!;
    public string? GuestID { get; set; }
    public string? Type { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int? Days { get; set; }
    public double? Rate { get; set; }
    public double? TotalCharges { get; set; }
    public double? OtherCharges { get; set; }
    public double? SubTotal { get; set; }
    public double? ServiceTaxPer { get; set; }
    public double? ServiceTaxAmount { get; set; }
    public double? LuxuryTaxPer { get; set; }
    public double? LuxuryTaxAmount { get; set; }
    public double? DiscountPer { get; set; }
    public double? Discount { get; set; }
    public double? GrandTotal { get; set; }
    public double? TotalPaid { get; set; }
    public double? Balance { get; set; }
    public string? Notes { get; set; }
    public int? CurrencyID { get; set; }
    public int? HotelID { get; set; }
    public Guest? Guest { get; set; }
    public CurrencySet? Currency { get; set; }
    public HotelInfo? Hotel { get; set; }
}

public class CheckInRoom
{
    public int ID { get; set; }
    public string? GuestID { get; set; }
    public int? CurrencyID { get; set; }
    public string? RoomNo { get; set; }
    public int? RoomCharges { get; set; }
    public DateTime? DateIN { get; set; }
    public DateTime? DateOUT { get; set; }
    public int? NoOfAdults { get; set; }
    public int? NoOfKids { get; set; }
    public int? NoOfDays { get; set; }
    public string? ExtraBed { get; set; }
    public double? TotalRoomCharges { get; set; }
    public double? OtherCharges { get; set; }
    public double? SubTotal { get; set; }
    public double? ServiceTaxPer { get; set; }
    public double? ServiceTaxAmount { get; set; }
    public double? LuxuryTaxPer { get; set; }
    public double? LuxuryTaxAmount { get; set; }
    public double? DiscountPer { get; set; }
    public double? Discount { get; set; }
    public double? GrandTotal { get; set; }
    public double? TotalPaid { get; set; }
    public double? Balance { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
    public Guest? Guest { get; set; }
    public Room? Room { get; set; }
    public CurrencySet? Currency { get; set; }
    public ICollection<CheckoutRoom> Checkouts { get; set; } = new List<CheckoutRoom>();
    public ICollection<OrderInfo> Orders { get; set; } = new List<OrderInfo>();
}

public class CheckoutRoom
{
    public int ID { get; set; }
    public string? BillNo { get; set; }
    public int? CheckInID { get; set; }
    public int? CurrencyID { get; set; }
    public int? HotelID { get; set; }
    public DateTime? CheckOutDate { get; set; }
    public string? Notes { get; set; }
    public CheckInRoom? CheckIn { get; set; }
    public CurrencySet? Currency { get; set; }
    public HotelInfo? Hotel { get; set; }
    public ICollection<TaxRoom> Taxes { get; set; } = new List<TaxRoom>();
}
