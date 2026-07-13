using HotelManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.Infrastructure;

public class HotelDbContext : DbContext
{
    public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options) { }

    public DbSet<HotelInfo> HotelInfos => Set<HotelInfo>();
    public DbSet<CurrencySet> Currencies => Set<CurrencySet>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Hall> Halls => Set<Hall>();
    public DbSet<Garden> Gardens => Set<Garden>();
    public DbSet<ExtraBed> ExtraBeds => Set<ExtraBed>();
    public DbSet<Guest> Guests => Set<Guest>();
    public DbSet<Registration> Registrations => Set<Registration>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<TempReservation> TempReservations => Set<TempReservation>();
    public DbSet<ReservationHallAndGarden> ReservationsHallAndGarden => Set<ReservationHallAndGarden>();
    public DbSet<ReservationHallOrGarden> ReservationsHallOrGarden => Set<ReservationHallOrGarden>();
    public DbSet<CheckInRoom> CheckIns => Set<CheckInRoom>();
    public DbSet<CheckoutRoom> Checkouts => Set<CheckoutRoom>();
    public DbSet<OrderInfo> Orders => Set<OrderInfo>();
    public DbSet<OrderedProduct> OrderedProducts => Set<OrderedProduct>();
    public DbSet<RestaurantOrderInfo> RestaurantOrders => Set<RestaurantOrderInfo>();
    public DbSet<RestaurantOrderedProduct> RestaurantOrderedProducts => Set<RestaurantOrderedProduct>();
    public DbSet<Trans> Transactions => Set<Trans>();
    public DbSet<TaxInfo> TaxInfos => Set<TaxInfo>();
    public DbSet<TaxOrder> TaxOrders => Set<TaxOrder>();
    public DbSet<TaxRoom> TaxRooms => Set<TaxRoom>();
    public DbSet<TaxReservationHallAndGarden> TaxReservationsHallAndGarden => Set<TaxReservationHallAndGarden>();
    public DbSet<TaxReservationHallOrGarden> TaxReservationsHallOrGarden => Set<TaxReservationHallOrGarden>();
    public DbSet<TaxRestaurantOrder> TaxRestaurantOrders => Set<TaxRestaurantOrder>();
    public DbSet<Dish> Dishes => Set<Dish>();
    public DbSet<Beer> Beers => Set<Beer>();
    public DbSet<Liquor> Liquors => Set<Liquor>();
    public DbSet<LiquorMaster> LiquorMasters => Set<LiquorMaster>();
    public DbSet<Stock> Stocks => Set<Stock>();
    public DbSet<StockBeer> BeerStocks => Set<StockBeer>();
    public DbSet<PurchasedInventory> PurchasedInventories => Set<PurchasedInventory>();
    public DbSet<EmployeeRegistration> Employees => Set<EmployeeRegistration>();
    public DbSet<EmployeeAttendance> Attendances => Set<EmployeeAttendance>();
    public DbSet<EmployeePayment> EmployeePayments => Set<EmployeePayment>();
    public DbSet<AdvanceEntry> AdvanceEntries => Set<AdvanceEntry>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<HotelInfo>(e => { e.ToTable("HotelInfo"); e.HasKey(x => x.ID); });
        b.Entity<CurrencySet>(e => { e.ToTable("CurrencySet"); e.HasKey(x => x.ID); });
        b.Entity<Room>(e => { e.ToTable("Room"); e.HasKey(x => x.RoomNo); });
        b.Entity<Hall>(e =>
        {
            e.ToTable("Hall");
            e.HasKey(x => x.ID);
            e.Property(x => x.HallName).HasColumnName("Hall Name");
        });
        b.Entity<Garden>(e => { e.ToTable("Garden"); e.HasKey(x => x.ID); });
        b.Entity<ExtraBed>(e => { e.ToTable("ExtraBed"); e.HasKey(x => x.ID); });
        b.Entity<Guest>(e => { e.ToTable("Guest"); e.HasKey(x => x.GuestID); });
        b.Entity<Registration>(e => { e.ToTable("Registration"); e.HasKey(x => x.UserName); });
        b.Entity<User>(e =>
        {
            e.ToTable("Users");
            e.HasKey(x => x.Username);
            e.HasOne(x => x.Registration).WithMany().HasForeignKey(x => x.Username);
        });

        b.Entity<Reservation>(e =>
        {
            e.ToTable("Reservation");
            e.HasKey(x => x.ReservationID);
            e.HasOne(x => x.Guest).WithMany().HasForeignKey(x => x.GuestID);
            e.HasOne(x => x.Room).WithMany().HasForeignKey(x => x.RoomNo);
        });
        b.Entity<TempReservation>(e =>
        {
            e.ToTable("Temp_Reservation");
            e.HasKey(x => x.ReservationID);
            e.HasOne(x => x.Guest).WithMany().HasForeignKey(x => x.GuestID);
            e.HasOne(x => x.Room).WithMany().HasForeignKey(x => x.RoomNo);
        });
        b.Entity<ReservationHallAndGarden>(e =>
        {
            e.ToTable("Reservation_HallandGarden");
            e.HasKey(x => x.ID);
            e.HasOne(x => x.Guest).WithMany().HasForeignKey(x => x.GuestID);
            e.HasOne(x => x.Currency).WithMany().HasForeignKey(x => x.CurrencyID);
            e.HasOne(x => x.Hotel).WithMany().HasForeignKey(x => x.HotelID);
        });
        b.Entity<ReservationHallOrGarden>(e =>
        {
            e.ToTable("Reservation_HallorGarden");
            e.HasKey(x => x.ID);
            e.HasOne(x => x.Guest).WithMany().HasForeignKey(x => x.GuestID);
            e.HasOne(x => x.Currency).WithMany().HasForeignKey(x => x.CurrencyID);
            e.HasOne(x => x.Hotel).WithMany().HasForeignKey(x => x.HotelID);
        });
        b.Entity<CheckInRoom>(e =>
        {
            e.ToTable("CheckIN_Room");
            e.HasKey(x => x.ID);
            e.HasOne(x => x.Guest).WithMany().HasForeignKey(x => x.GuestID);
            e.HasOne(x => x.Room).WithMany().HasForeignKey(x => x.RoomNo);
            e.HasOne(x => x.Currency).WithMany().HasForeignKey(x => x.CurrencyID);
        });
        b.Entity<CheckoutRoom>(e =>
        {
            e.ToTable("Checkout_Room");
            e.HasKey(x => x.ID);
            e.HasOne(x => x.CheckIn).WithMany(c => c.Checkouts).HasForeignKey(x => x.CheckInID);
            e.HasOne(x => x.Currency).WithMany().HasForeignKey(x => x.CurrencyID);
            e.HasOne(x => x.Hotel).WithMany().HasForeignKey(x => x.HotelID);
        });

        b.Entity<OrderInfo>(e =>
        {
            e.ToTable("Order_Info");
            e.HasKey(x => x.ID);
            e.HasOne(x => x.CheckIn).WithMany(c => c.Orders).HasForeignKey(x => x.CheckInID);
            e.HasOne(x => x.Currency).WithMany().HasForeignKey(x => x.CurrencyID);
            e.HasOne(x => x.Hotel).WithMany().HasForeignKey(x => x.HotelID);
        });
        b.Entity<OrderedProduct>(e =>
        {
            e.ToTable("OrderedProduct");
            e.HasKey(x => x.ID);
            e.HasOne(x => x.Order).WithMany(o => o.Products).HasForeignKey(x => x.OrderID);
        });
        b.Entity<RestaurantOrderInfo>(e =>
        {
            e.ToTable("Restaurant_OrderInfo");
            e.HasKey(x => x.ID);
            e.HasOne(x => x.Currency).WithMany().HasForeignKey(x => x.CurrencyID);
            e.HasOne(x => x.Hotel).WithMany().HasForeignKey(x => x.HotelID);
        });
        b.Entity<RestaurantOrderedProduct>(e =>
        {
            e.ToTable("RestaurantOrderedProduct");
            e.HasKey(x => x.ID);
            e.HasOne(x => x.Order).WithMany(o => o.Products).HasForeignKey(x => x.OrderID);
        });
        b.Entity<Trans>(e => { e.ToTable("Trans"); e.HasKey(x => x.ID); });
        b.Entity<TaxInfo>(e =>
        {
            e.ToTable("Taxinfo");
            e.HasKey(x => x.Salray);
            e.Property(x => x.TaxinP).HasColumnName("taxinP");
        });
        b.Entity<TaxOrder>(e =>
        {
            e.ToTable("Tax_order");
            e.HasKey(x => x.ID);
            e.HasOne(x => x.Order).WithMany(o => o.Taxes).HasForeignKey(x => x.OrderID);
        });
        b.Entity<TaxRoom>(e =>
        {
            e.ToTable("Tax_Room");
            e.HasKey(x => x.ID);
            e.HasOne(x => x.Bill).WithMany(c => c.Taxes).HasForeignKey(x => x.BillID);
        });
        b.Entity<TaxReservationHallAndGarden>(e =>
        {
            e.ToTable("Tax_ReservationHallandGarden");
            e.HasKey(x => x.ID);
            e.HasOne(x => x.Reservation).WithMany().HasForeignKey(x => x.ReservationID);
        });
        b.Entity<TaxReservationHallOrGarden>(e =>
        {
            e.ToTable("Tax_ReservationHallorGarden");
            e.HasKey(x => x.ID);
            e.HasOne(x => x.Reservation).WithMany().HasForeignKey(x => x.ReservationID);
        });
        b.Entity<TaxRestaurantOrder>(e =>
        {
            e.ToTable("Tax_Restaurantorder");
            e.HasKey(x => x.ID);
            e.HasOne(x => x.Order).WithMany(o => o.Taxes).HasForeignKey(x => x.OrderID);
        });

        b.Entity<Dish>(e => { e.ToTable("Dish"); e.HasKey(x => x.ID); });
        b.Entity<Beer>(e => { e.ToTable("Beer"); e.HasKey(x => x.ID); });
        b.Entity<Liquor>(e =>
        {
            e.ToTable("Liquor");
            e.HasKey(x => x.ID);
            e.HasOne(x => x.Master).WithMany().HasForeignKey(x => x.LiquorName);
        });
        b.Entity<LiquorMaster>(e => { e.ToTable("Liquor_master"); e.HasKey(x => x.LiquorName); });
        b.Entity<Stock>(e =>
        {
            e.ToTable("Stock");
            e.HasKey(x => x.StockID);
            e.HasOne(x => x.LiquorMaster).WithMany().HasForeignKey(x => x.LiquorName);
        });
        b.Entity<StockBeer>(e =>
        {
            e.ToTable("Stock_Beer");
            e.HasKey(x => x.StockID);
            e.HasOne(x => x.Beer).WithMany().HasForeignKey(x => x.BeerID);
        });
        b.Entity<PurchasedInventory>(e => { e.ToTable("PurchasedInventory"); e.HasKey(x => x.ID); });

        b.Entity<EmployeeRegistration>(e => { e.ToTable("EmployeeRegistration"); e.HasKey(x => x.EmployeeID); });
        b.Entity<EmployeeAttendance>(e =>
        {
            e.ToTable("EmployeeAttendance");
            e.HasKey(x => x.AttendanceID);
            e.HasOne(x => x.Employee).WithMany(r => r.Attendances).HasForeignKey(x => x.EmployeeID);
        });
        b.Entity<EmployeePayment>(e =>
        {
            e.ToTable("EmployeePayment");
            e.HasKey(x => x.PaymentID);
            e.HasOne(x => x.Employee).WithMany(r => r.Payments).HasForeignKey(x => x.EmployeeID);
        });
        b.Entity<AdvanceEntry>(e =>
        {
            e.ToTable("AdvanceEntry");
            e.HasKey(x => x.ID);
            e.HasOne(x => x.Employee).WithMany(r => r.Advances).HasForeignKey(x => x.EmployeeID);
        });
    }
}
