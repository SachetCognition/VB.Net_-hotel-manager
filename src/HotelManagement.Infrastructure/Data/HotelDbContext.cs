using HotelManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Infrastructure.Data;

public class HotelDbContext : DbContext
{
    public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<LoginActivity> LoginActivities => Set<LoginActivity>();
    public DbSet<Guest> Guests => Set<Guest>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<CheckInRoom> CheckInRooms => Set<CheckInRoom>();
    public DbSet<CheckoutRoom> CheckoutRooms => Set<CheckoutRoom>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<ReservationHallAndGarden> ReservationHallAndGardens => Set<ReservationHallAndGarden>();
    public DbSet<ReservationHallOrGarden> ReservationHallOrGardens => Set<ReservationHallOrGarden>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EmployeeAttendance> EmployeeAttendances => Set<EmployeeAttendance>();
    public DbSet<EmployeePayment> EmployeePayments => Set<EmployeePayment>();
    public DbSet<AdvanceEntry> AdvanceEntries => Set<AdvanceEntry>();
    public DbSet<Dish> Dishes => Set<Dish>();
    public DbSet<Beer> Beers => Set<Beer>();
    public DbSet<Liquor> Liquors => Set<Liquor>();
    public DbSet<LiquorMaster> LiquorMasters => Set<LiquorMaster>();
    public DbSet<PurchaseInventory> PurchaseInventories => Set<PurchaseInventory>();
    public DbSet<Stock> Stocks => Set<Stock>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<RestaurantOrder> RestaurantOrders => Set<RestaurantOrder>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<CurrencySet> CurrencySets => Set<CurrencySet>();
    public DbSet<HotelInfo> HotelInfos => Set<HotelInfo>();
    public DbSet<Hall> Halls => Set<Hall>();
    public DbSet<Garden> Gardens => Set<Garden>();
    public DbSet<ExtraBed> ExtraBeds => Set<ExtraBed>();
    public DbSet<TaxInformation> TaxInformations => Set<TaxInformation>();
    public DbSet<AdditionalTax> AdditionalTaxes => Set<AdditionalTax>();
    public DbSet<TaxRoom> TaxRooms => Set<TaxRoom>();
    public DbSet<Schedule> Schedules => Set<Schedule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Guest - string PK
        modelBuilder.Entity<Guest>(entity =>
        {
            entity.HasKey(e => e.GuestID);
            entity.Property(e => e.GuestID).HasMaxLength(20);
        });

        // Room - string PK
        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.RoomNo);
            entity.Property(e => e.RoomNo).HasMaxLength(20);
        });

        // Employee - string PK
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeID);
            entity.Property(e => e.EmployeeID).HasMaxLength(20);
        });

        // CheckInRoom relationships
        modelBuilder.Entity<CheckInRoom>(entity =>
        {
            entity.HasOne(e => e.Guest)
                .WithMany()
                .HasForeignKey(e => e.GuestID)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Room)
                .WithMany()
                .HasForeignKey(e => e.RoomNo)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Reservation relationships
        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasOne(e => e.Guest)
                .WithMany()
                .HasForeignKey(e => e.GuestID)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Room)
                .WithMany()
                .HasForeignKey(e => e.RoomNo)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // EmployeeAttendance relationships
        modelBuilder.Entity<EmployeeAttendance>(entity =>
        {
            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // EmployeePayment relationships
        modelBuilder.Entity<EmployeePayment>(entity =>
        {
            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // AdvanceEntry relationships
        modelBuilder.Entity<AdvanceEntry>(entity =>
        {
            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Unique constraints
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<Dish>()
            .HasIndex(d => d.DishName)
            .IsUnique();

        // Decimal precision configurations
        modelBuilder.Entity<CheckInRoom>(entity =>
        {
            entity.Property(e => e.RoomCharges).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalRoomCharges).HasColumnType("decimal(18,2)");
            entity.Property(e => e.OtherCharges).HasColumnType("decimal(18,2)");
            entity.Property(e => e.DiscountPer).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Discount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.SubTotal).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ServiceTaxPer).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ServiceTaxAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.LuxuryTaxPer).HasColumnType("decimal(18,2)");
            entity.Property(e => e.LuxuryTaxAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.GrandTotal).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalPaid).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Balance).HasColumnType("decimal(18,2)");
        });
    }
}
