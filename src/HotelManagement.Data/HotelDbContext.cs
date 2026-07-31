using Microsoft.EntityFrameworkCore;
using HotelManagement.Core.Models;

namespace HotelManagement.Data;

public class HotelDbContext : DbContext
{
    public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options) { }

    public DbSet<Guest> Guests => Set<Guest>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<CheckInRoom> CheckInRooms => Set<CheckInRoom>();
    public DbSet<CheckoutRoom> CheckoutRooms => Set<CheckoutRoom>();
    public DbSet<TaxRoom> TaxRooms => Set<TaxRoom>();
    public DbSet<TempReservation> TempReservations => Set<TempReservation>();
    public DbSet<ReservationHallOrGarden> ReservationHallOrGardens => Set<ReservationHallOrGarden>();
    public DbSet<ReservationHallAndGarden> ReservationHallAndGardens => Set<ReservationHallAndGarden>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EmployeeAttendance> EmployeeAttendances => Set<EmployeeAttendance>();
    public DbSet<EmployeePayment> EmployeePayments => Set<EmployeePayment>();
    public DbSet<AdvanceEntry> AdvanceEntries => Set<AdvanceEntry>();
    public DbSet<HotelInfo> HotelInfos => Set<HotelInfo>();
    public DbSet<CurrencySet> CurrencySets => Set<CurrencySet>();
    public DbSet<Stock> Stocks => Set<Stock>();
    public DbSet<StockBeer> StockBeers => Set<StockBeer>();
    public DbSet<Beer> Beers => Set<Beer>();
    public DbSet<LiquorMaster> LiquorMasters => Set<LiquorMaster>();
    public DbSet<Food> Foods => Set<Food>();
    public DbSet<PurchasedInventory> PurchasedInventories => Set<PurchasedInventory>();
    public DbSet<OrderInfo> OrderInfos => Set<OrderInfo>();
    public DbSet<Hall> Halls => Set<Hall>();
    public DbSet<Garden> Gardens => Set<Garden>();
    public DbSet<ExtraBed> ExtraBeds => Set<ExtraBed>();
    public DbSet<TaxInfo> TaxInfos => Set<TaxInfo>();
    public DbSet<Activation> Activations => Set<Activation>();
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Guest>(entity =>
        {
            entity.HasKey(e => e.GuestID);
            entity.ToTable("Guest");
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.RoomNo);
            entity.ToTable("Room");
        });

        modelBuilder.Entity<CheckInRoom>(entity =>
        {
            entity.HasKey(e => e.ID);
            entity.ToTable("CheckIn_Room");
            entity.HasOne(e => e.Guest).WithMany().HasForeignKey(e => e.GuestID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Room).WithMany().HasForeignKey(e => e.RoomNo).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CheckoutRoom>(entity =>
        {
            entity.HasKey(e => e.ID);
            entity.ToTable("Checkout_Room");
            entity.HasOne(e => e.CheckInRoom).WithMany().HasForeignKey(e => e.CheckInID).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TaxRoom>(entity =>
        {
            entity.HasKey(e => e.BillID);
            entity.ToTable("Tax_Room");
            entity.HasOne(e => e.CheckoutRoom).WithMany().HasForeignKey(e => e.BillID).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TempReservation>(entity =>
        {
            entity.HasKey(e => e.ReservationID);
            entity.ToTable("Temp_Reservation");
            entity.HasOne(e => e.Guest).WithMany().HasForeignKey(e => e.GuestID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Room).WithMany().HasForeignKey(e => e.RoomNo).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ReservationHallOrGarden>(entity =>
        {
            entity.HasKey(e => e.ID);
            entity.ToTable("Reservation_HallorGarden");
            entity.HasOne(e => e.Guest).WithMany().HasForeignKey(e => e.GuestID).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ReservationHallAndGarden>(entity =>
        {
            entity.HasKey(e => e.ID);
            entity.ToTable("Reservation_HallandGarden");
            entity.HasOne(e => e.Guest).WithMany().HasForeignKey(e => e.GuestID).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeID);
            entity.ToTable("EmployeeRegistration");
        });

        modelBuilder.Entity<EmployeeAttendance>(entity =>
        {
            entity.HasKey(e => e.ID);
            entity.ToTable("EmployeeAttendance");
            entity.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeID).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EmployeePayment>(entity =>
        {
            entity.HasKey(e => e.PaymentID);
            entity.ToTable("EmployeePayment");
            entity.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeID).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AdvanceEntry>(entity =>
        {
            entity.HasKey(e => e.ID);
            entity.ToTable("AdvanceEntry");
            entity.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeID).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<HotelInfo>(entity =>
        {
            entity.HasKey(e => e.ID);
            entity.ToTable("HotelInfo");
        });

        modelBuilder.Entity<CurrencySet>(entity =>
        {
            entity.HasKey(e => e.ID);
            entity.ToTable("CurrencySet");
        });

        modelBuilder.Entity<Stock>(entity =>
        {
            entity.HasKey(e => e.StockID);
            entity.ToTable("Stock");
        });

        modelBuilder.Entity<StockBeer>(entity =>
        {
            entity.HasKey(e => e.StockID);
            entity.ToTable("Stock_Beer");
        });

        modelBuilder.Entity<Beer>(entity =>
        {
            entity.HasKey(e => e.ID);
            entity.ToTable("Beer");
        });

        modelBuilder.Entity<LiquorMaster>(entity =>
        {
            entity.HasKey(e => e.LiquorName);
            entity.ToTable("Liquor_master");
        });

        modelBuilder.Entity<Food>(entity =>
        {
            entity.HasKey(e => e.ID);
            entity.ToTable("Food");
        });

        modelBuilder.Entity<PurchasedInventory>(entity =>
        {
            entity.HasKey(e => e.ID);
            entity.ToTable("PurchasedInventory");
        });

        modelBuilder.Entity<OrderInfo>(entity =>
        {
            entity.HasKey(e => e.ID);
            entity.ToTable("Order_Info");
            entity.HasOne(e => e.CheckInRoom).WithMany().HasForeignKey(e => e.CheckInId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Hall>(entity =>
        {
            entity.HasKey(e => e.ID);
            entity.ToTable("Hall");
        });

        modelBuilder.Entity<Garden>(entity =>
        {
            entity.HasKey(e => e.ID);
            entity.ToTable("Garden");
        });

        modelBuilder.Entity<ExtraBed>(entity =>
        {
            entity.HasKey(e => e.ID);
            entity.ToTable("ExtraBed");
        });

        modelBuilder.Entity<TaxInfo>(entity =>
        {
            entity.HasKey(e => e.ID);
            entity.ToTable("Taxinfo");
        });

        modelBuilder.Entity<Activation>(entity =>
        {
            entity.HasKey(e => e.ID);
            entity.ToTable("Activation");
        });

        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.HasKey(e => e.ID);
            entity.ToTable("UserAccount");
        });
    }
}
