using HotelManager.Domain.Entities;

namespace HotelManager.ComponentTests.Infrastructure;

/// <summary>Sample entities so that page table row templates render (and are covered).</summary>
public static class TestData
{
    public static Guest Guest() => new()
    {
        GuestID = "G-00001", GuestName = "Alice", Address = "1 St", City = "Town",
        ContactNo = "555", IDType = "Passport", IDNumber = "P1", Notes = "n"
    };

    public static Room Room() => new() { RoomNo = "101", RoomType = "Deluxe", RoomCharges = 1000 };
    public static Hall Hall() => new() { ID = 1, HallName = "Grand", Charges = 5000 };
    public static Garden Garden() => new() { ID = 1, Charges = 3000 };
    public static ExtraBed ExtraBed() => new() { ID = 1, Charges = 200 };
    public static CurrencySet Currency() => new() { ID = 1, CS_Currency = "USD" };
    public static HotelInfo HotelInfo() => new()
    {
        ID = 1, HotelName = "Grand Hotel", Address = "Main", ContactNo = "1", Email = "h@x.com"
    };

    public static Reservation Reservation() => new()
    {
        ReservationID = "R-00001", GuestID = "G-00001", RoomNo = "101",
        DateIN = DateTime.Today, DateOUT = DateTime.Today.AddDays(2), Status = "Confirmed",
        Guest = Guest(), Room = Room()
    };

    public static TempReservation TempReservation() => new()
    {
        ReservationID = "R-00001", GuestID = "G-00001", RoomNo = "101",
        DateIN = DateTime.Today, DateOUT = DateTime.Today.AddDays(2), Status = "Temp", Guest = Guest()
    };

    public static ReservationHallAndGarden HallAndGarden() => new()
    {
        ID = "HG-1", GuestID = "G-00001", Hall = "Grand", Garden = "G", GrandTotal = 100, Guest = Guest()
    };

    public static ReservationHallOrGarden HallOrGarden() => new()
    {
        ID = "HOG-1", GuestID = "G-00001", Type = "Hall", GrandTotal = 100, Guest = Guest()
    };

    public static CheckInRoom CheckIn() => new()
    {
        ID = 1, GuestID = "G-00001", RoomNo = "101", RoomCharges = 1000,
        DateIN = DateTime.Today, DateOUT = DateTime.Today.AddDays(1), NoOfDays = 1,
        Status = "Checked In", GrandTotal = 1000, Guest = Guest(), Room = Room(), Currency = Currency()
    };

    public static CheckoutRoom Checkout() => new()
    {
        ID = 1, BillNo = "B-1", CheckInID = 1, CheckOutDate = DateTime.Today, CheckIn = CheckIn()
    };

    public static OrderInfo RoomOrder() => new()
    {
        ID = 1, OrderNo = "O-1", OrderDate = DateTime.Today, CheckInID = 1, GrandTotal = 500,
        CheckIn = CheckIn(),
        Products = new List<OrderedProduct>
        {
            new() { ID = 1, ProductID = "D-1", ProductName = "Soup", Rate = 50, Quantity = 1, Amount = 50 }
        }
    };

    public static RestaurantOrderInfo RestaurantOrder() => new()
    {
        ID = 1, OrderNo = "RO-1", OrderDate = DateTime.Today, GrandTotal = 500,
        Products = new List<RestaurantOrderedProduct>
        {
            new() { ID = 1, ProductID = "1", ProductName = "Soup", Rate = 50, Quantity = 1, Amount = 50 }
        }
    };

    public static Trans Trans() => new()
    {
        ID = 1, Employee_Party_Name = "Bob", TransactionType = "Credit",
        TransactionDate = DateTime.Today, TransactionAmount = 100
    };

    public static Dish Dish() => new() { ID = 1, DishName = "Soup", Category = "Starter", Rate = 50 };
    public static Beer Beer() => new() { ID = "B-1", BeerName = "Lager", Rate = 80 };
    public static Liquor Liquor() => new() { ID = 1, LiquorName = "Whisky", Volume = 750, Rate = 500 };
    public static LiquorMaster LiquorMaster() => new() { LiquorName = "Whisky" };
    public static Stock Stock() => new()
    {
        StockID = "S-1", LiquorName = "Whisky", NoOfBottles = 10, Volume = 750,
        TotalVolume = 7500, StockDate = DateTime.Today
    };
    public static StockBeer BeerStock() => new()
    {
        StockID = "SB-1", BeerID = "B-1", NoOfBottles = 24, StockDate = DateTime.Today, Beer = Beer()
    };
    public static PurchasedInventory Purchase() => new()
    {
        ID = 1, ProductName = "Rice", Category = "Grocery", TransactionType = "Purchase",
        PurchaseDate = DateTime.Today, Quantity = 10, Price = 50, TotalPrice = 500
    };
    public static TaxInfo TaxInfo() => new() { Salray = "VAT", TaxinP = 5 };

    public static EmployeeRegistration Employee() => new()
    {
        EmployeeID = "E-000001", EmployeeName = "Bob", Department = "Front", Designation = "Clerk",
        Salary = 30000, DateOfJoining = DateTime.Today.AddYears(-1), BasicWorkingTime = "08:00"
    };
    public static EmployeeAttendance Attendance() => new()
    {
        AttendanceID = 1, EmployeeID = "E-000001", WorkingDate = DateTime.Today, Status = "Present",
        InTime = "09:00", OutTime = "18:00", Overtime = "01:00", Employee = Employee()
    };
    public static AdvanceEntry Advance() => new()
    {
        ID = 1, EmployeeID = "E-000001", Amount = 1000, WorkingDate = DateTime.Today, Employee = Employee()
    };
    public static EmployeePayment Payment() => new()
    {
        PaymentID = "P-000001", EmployeeID = "E-000001", DateFrom = DateTime.Today.AddDays(-30),
        DateTo = DateTime.Today, PresentDays = 30, Salary = 30000, NetPay = 30000,
        PaymentDate = DateTime.Today, Employee = Employee()
    };
}
