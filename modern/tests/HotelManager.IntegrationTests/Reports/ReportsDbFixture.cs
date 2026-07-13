using HotelManager.Domain.Entities;
using HotelManager.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.IntegrationTests.Reports;

/// <summary>
/// In-memory SQLite fixture for report projection tests, pre-seeded with a
/// fully-joined data set (guest, room, check-in/out, orders, HR, inventory).
/// Own copy of the shared SqliteDbFixture pattern (Child F owns this file).
/// </summary>
public class ReportsDbFixture : IDisposable, IDbContextFactory<HotelDbContext>
{
    private readonly SqliteConnection _connection;
    public DbContextOptions<HotelDbContext> Options { get; }

    public int CheckoutId { get; private set; }
    public int OrderId { get; private set; }
    public int RestaurantOrderId { get; private set; }

    public ReportsDbFixture()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        Options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseSqlite(_connection)
            .Options;
        using var db = new HotelDbContext(Options);
        db.Database.EnsureCreated();
        Seed(db);
    }

    public HotelDbContext CreateContext() => new(Options);
    public HotelDbContext CreateDbContext() => new(Options);

    private void Seed(HotelDbContext db)
    {
        var hotel = new HotelInfo
        {
            HotelName = "Test Grand Hotel",
            Address = "42 Test Street",
            ContactNo = "111-222",
            ContactNo1 = "333-444",
            Email = "info@testgrand.example",
            TIN = "TIN-1",
            STNo = "ST-1"
        };
        var currency = new CurrencySet { CS_Currency = "USD" };
        var room = new Room { RoomNo = "101", RoomType = "Deluxe", RoomCharges = 100 };
        var guest = new Guest
        {
            GuestID = "G-001",
            GuestName = "Alice Test",
            Address = "9 Guest Lane",
            City = "Testville",
            ContactNo = "555-000",
            IDType = "Passport",
            IDNumber = "P123"
        };
        db.AddRange(hotel, currency, room, guest);
        db.SaveChanges();

        var checkIn = new CheckInRoom
        {
            GuestID = guest.GuestID,
            CurrencyID = currency.ID,
            RoomNo = room.RoomNo,
            RoomCharges = 100,
            DateIN = new DateTime(2026, 1, 1),
            DateOUT = new DateTime(2026, 1, 4),
            NoOfAdults = 2,
            NoOfKids = 1,
            NoOfDays = 3,
            ExtraBed = "Yes",
            TotalRoomCharges = 300,
            OtherCharges = 50,
            SubTotal = 350,
            ServiceTaxPer = 10,
            ServiceTaxAmount = 35,
            LuxuryTaxPer = 5,
            LuxuryTaxAmount = 17.5,
            DiscountPer = 0,
            Discount = 0,
            GrandTotal = 402.5,
            TotalPaid = 400,
            Balance = 2.5,
            Status = "CheckedOut"
        };
        db.Add(checkIn);
        db.SaveChanges();

        var checkout = new CheckoutRoom
        {
            BillNo = "BILL-1",
            CheckInID = checkIn.ID,
            CurrencyID = currency.ID,
            HotelID = hotel.ID,
            CheckOutDate = new DateTime(2026, 1, 4),
            Notes = "checkout note"
        };
        db.Add(checkout);
        db.SaveChanges();
        CheckoutId = checkout.ID;

        db.Add(new ReservationHallAndGarden
        {
            ID = "HG-001",
            GuestID = guest.GuestID,
            CurrencyID = currency.ID,
            HotelID = hotel.ID,
            Hall = "Main Hall",
            DateFrom_Hall = new DateTime(2026, 2, 1),
            DateTo_Hall = new DateTime(2026, 2, 3),
            Days_Hall = 2,
            Rate_Hall = 500,
            TotalCharges_Hall = 1000,
            Garden = "Rose Garden",
            DateFrom_Garden = new DateTime(2026, 2, 1),
            DateTo_Garden = new DateTime(2026, 2, 2),
            Days_Garden = 1,
            Rate_Garden = 200,
            TotalCharges_Garden = 200,
            SubTotal = 1200,
            GrandTotal = 1300,
            TotalPaid = 1300,
            Balance = 0
        });

        db.Add(new ReservationHallOrGarden
        {
            ID = "HOG-001",
            GuestID = guest.GuestID,
            CurrencyID = currency.ID,
            HotelID = hotel.ID,
            Type = "Hall",
            DateFrom = new DateTime(2026, 3, 1),
            DateTo = new DateTime(2026, 3, 2),
            Days = 1,
            Rate = 450,
            TotalCharges = 450,
            SubTotal = 450,
            GrandTotal = 500,
            TotalPaid = 250,
            Balance = 250
        });
        db.SaveChanges();

        var order = new OrderInfo
        {
            CurrencyID = currency.ID,
            HotelID = hotel.ID,
            OrderNo = "ORD-1",
            OrderDate = new DateTime(2026, 1, 2),
            CheckInID = checkIn.ID,
            SubTotal = 60,
            VATPer = 10,
            VATAmount = 6,
            STPer = 5,
            STAmount = 3,
            GrandTotal = 69,
            TotalPayment = 69,
            PaymentDue = 0
        };
        db.Add(order);
        db.SaveChanges();
        OrderId = order.ID;
        db.Add(new OrderedProduct
        {
            OrderID = order.ID,
            ProductID = "D-1",
            ProductName = "Pasta",
            Volume = null,
            Rate = 30,
            Quantity = 2,
            Amount = 60
        });

        var restOrder = new RestaurantOrderInfo
        {
            OrderNo = "REST-1",
            CurrencyID = currency.ID,
            HotelID = hotel.ID,
            OrderDate = new DateTime(2026, 1, 3),
            SubTotal = 40,
            VATPer = 10,
            VATAmount = 4,
            STPer = 0,
            STAmount = 0,
            GrandTotal = 44,
            TotalPayment = 44,
            PaymentDue = 0
        };
        db.Add(restOrder);
        db.SaveChanges();
        RestaurantOrderId = restOrder.ID;
        db.Add(new RestaurantOrderedProduct
        {
            OrderID = restOrder.ID,
            ProductID = "D-2",
            ProductName = "Soup",
            Rate = 20,
            Quantity = 2,
            Amount = 40
        });

        var employee = new EmployeeRegistration
        {
            EmployeeID = "E-001",
            EmployeeName = "Bob Worker",
            Department = "Kitchen",
            Designation = "Chef",
            Salary = 3000,
            BasicWorkingTime = "08:00"
        };
        db.Add(employee);
        db.SaveChanges();

        db.Add(new EmployeeAttendance
        {
            EmployeeID = employee.EmployeeID,
            WorkingDate = new DateTime(2026, 4, 1),
            BasicWorkingTime = "08:00",
            Status = "Present",
            InTime = "09:00",
            OutTime = "18:00",
            Overtime = "01:00"
        });
        db.Add(new EmployeeAttendance
        {
            EmployeeID = employee.EmployeeID,
            WorkingDate = new DateTime(2026, 5, 1),
            Status = "Absent"
        });

        db.Add(new AdvanceEntry
        {
            EmployeeID = employee.EmployeeID,
            Amount = 150,
            Deduction = 0,
            WorkingDate = new DateTime(2026, 4, 10)
        });

        db.Add(new EmployeePayment
        {
            PaymentID = "PAY-001",
            EmployeeID = employee.EmployeeID,
            DateFrom = new DateTime(2026, 4, 1),
            DateTo = new DateTime(2026, 4, 30),
            PresentDays = 26,
            Salary = 3000,
            Advance = 150,
            Deduction = 50,
            Overtime = "05:00",
            OvertimeRate = 20,
            OverTimeAmount = 100,
            PaymentDate = new DateTime(2026, 5, 1),
            ModeOfPayment = "Cash",
            NetPay = 2900
        });

        db.Add(new Reservation
        {
            ReservationID = "R-001",
            GuestID = guest.GuestID,
            RoomNo = room.RoomNo,
            DateIN = new DateTime(2026, 6, 1),
            DateOUT = new DateTime(2026, 6, 5),
            Status = "Reserved",
            Notes = "window seat"
        });

        db.Add(new PurchasedInventory
        {
            ProductName = "Rice",
            Category = "Grocery",
            TransactionType = "Purchase",
            PartyName = "Acme Supplies",
            PurchaseDate = new DateTime(2026, 4, 15),
            Quantity = 25,
            Unit = "kg",
            Price = 2,
            TotalPrice = 50
        });

        db.SaveChanges();
    }

    public void Dispose() => _connection.Dispose();
}
