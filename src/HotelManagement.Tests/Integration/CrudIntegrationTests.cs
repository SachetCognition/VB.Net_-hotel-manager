using HotelManagement.Core.Models;
using HotelManagement.Data;
using HotelManagement.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HotelManagement.Tests.Integration;

/// <summary>
/// Category 6: CRUD Integration Tests (~60 tests)
/// Tests Create, Read, Update, Delete operations with in-memory database.
/// </summary>
public class CrudIntegrationTests : IDisposable
{
    private readonly HotelDbContext _context;
    private readonly Repository<Guest> _guestRepo;
    private readonly Repository<Room> _roomRepo;
    private readonly Repository<Employee> _employeeRepo;
    private readonly Repository<CurrencySet> _currencyRepo;
    private readonly Repository<HotelInfo> _hotelInfoRepo;
    private readonly Repository<Stock> _stockRepo;
    private readonly Repository<PurchasedInventory> _inventoryRepo;

    public CrudIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new HotelDbContext(options);
        _guestRepo = new Repository<Guest>(_context);
        _roomRepo = new Repository<Room>(_context);
        _employeeRepo = new Repository<Employee>(_context);
        _currencyRepo = new Repository<CurrencySet>(_context);
        _hotelInfoRepo = new Repository<HotelInfo>(_context);
        _stockRepo = new Repository<Stock>(_context);
        _inventoryRepo = new Repository<PurchasedInventory>(_context);
    }

    public void Dispose() => _context.Dispose();

    // ============= GUEST CRUD (5 tests) =============

    [Fact]
    public async Task Guest_Create_ShouldPersist()
    {
        var guest = new Guest { GuestID = "G-123456", GuestName = "John Doe", Address = "123 Main St", City = "NYC", ContactNo = "1234567890", IDType = "Passport", IDNumber = "AB123" };
        await _guestRepo.AddAsync(guest);
        var result = await _guestRepo.GetByIdAsync("G-123456");
        Assert.NotNull(result);
        Assert.Equal("John Doe", result!.GuestName);
    }

    [Fact]
    public async Task Guest_Read_ShouldReturnAll()
    {
        await _guestRepo.AddAsync(new Guest { GuestID = "G-100001", GuestName = "Guest 1", Address = "A", City = "C", ContactNo = "1", IDType = "DL", IDNumber = "1" });
        await _guestRepo.AddAsync(new Guest { GuestID = "G-100002", GuestName = "Guest 2", Address = "B", City = "D", ContactNo = "2", IDType = "DL", IDNumber = "2" });
        var all = await _guestRepo.GetAllAsync();
        Assert.Equal(2, all.Count());
    }

    [Fact]
    public async Task Guest_Update_ShouldModify()
    {
        var guest = new Guest { GuestID = "G-200001", GuestName = "Original", Address = "A", City = "C", ContactNo = "1", IDType = "DL", IDNumber = "1" };
        await _guestRepo.AddAsync(guest);
        guest.GuestName = "Updated";
        await _guestRepo.UpdateAsync(guest);
        var result = await _guestRepo.GetByIdAsync("G-200001");
        Assert.Equal("Updated", result!.GuestName);
    }

    [Fact]
    public async Task Guest_Delete_ShouldRemove()
    {
        var guest = new Guest { GuestID = "G-300001", GuestName = "ToDelete", Address = "A", City = "C", ContactNo = "1", IDType = "DL", IDNumber = "1" };
        await _guestRepo.AddAsync(guest);
        await _guestRepo.DeleteAsync(guest);
        var result = await _guestRepo.GetByIdAsync("G-300001");
        Assert.Null(result);
    }

    [Fact]
    public async Task Guest_Find_ShouldFilter()
    {
        await _guestRepo.AddAsync(new Guest { GuestID = "G-400001", GuestName = "John Smith", Address = "A", City = "NYC", ContactNo = "1", IDType = "DL", IDNumber = "1" });
        await _guestRepo.AddAsync(new Guest { GuestID = "G-400002", GuestName = "Jane Doe", Address = "B", City = "LA", ContactNo = "2", IDType = "DL", IDNumber = "2" });
        var result = await _guestRepo.FindAsync(g => g.City == "NYC");
        Assert.Single(result);
    }

    // ============= ROOM CRUD (5 tests) =============

    [Fact]
    public async Task Room_Create_ShouldPersist()
    {
        var room = new Room { RoomNo = "101", RoomType = "Deluxe", RoomCharges = 5000m };
        await _roomRepo.AddAsync(room);
        var result = await _roomRepo.GetByIdAsync("101");
        Assert.NotNull(result);
        Assert.Equal("Deluxe", result!.RoomType);
    }

    [Fact]
    public async Task Room_Read_ShouldReturnAll()
    {
        await _roomRepo.AddAsync(new Room { RoomNo = "201", RoomType = "Standard", RoomCharges = 3000m });
        await _roomRepo.AddAsync(new Room { RoomNo = "202", RoomType = "Suite", RoomCharges = 8000m });
        var all = await _roomRepo.GetAllAsync();
        Assert.Equal(2, all.Count());
    }

    [Fact]
    public async Task Room_Update_ShouldModify()
    {
        var room = new Room { RoomNo = "301", RoomType = "Standard", RoomCharges = 3000m };
        await _roomRepo.AddAsync(room);
        room.RoomCharges = 3500m;
        await _roomRepo.UpdateAsync(room);
        var result = await _roomRepo.GetByIdAsync("301");
        Assert.Equal(3500m, result!.RoomCharges);
    }

    [Fact]
    public async Task Room_Delete_ShouldRemove()
    {
        var room = new Room { RoomNo = "401", RoomType = "Standard", RoomCharges = 3000m };
        await _roomRepo.AddAsync(room);
        await _roomRepo.DeleteAsync(room);
        var result = await _roomRepo.GetByIdAsync("401");
        Assert.Null(result);
    }

    [Fact]
    public async Task Room_Find_ByType()
    {
        await _roomRepo.AddAsync(new Room { RoomNo = "501", RoomType = "Deluxe", RoomCharges = 5000m });
        await _roomRepo.AddAsync(new Room { RoomNo = "502", RoomType = "Standard", RoomCharges = 3000m });
        var result = await _roomRepo.FindAsync(r => r.RoomType == "Deluxe");
        Assert.Single(result);
    }

    // ============= HOTEL INFO CRUD (5 tests) =============

    [Fact]
    public async Task HotelInfo_Create_ShouldPersist()
    {
        var info = new HotelInfo { ID = 1, HotelName = "Grand Hotel", Address = "Main St", ContactNo = "123" };
        await _hotelInfoRepo.AddAsync(info);
        var result = await _hotelInfoRepo.GetByIdAsync(1);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task HotelInfo_Read_ShouldReturn()
    {
        await _hotelInfoRepo.AddAsync(new HotelInfo { ID = 2, HotelName = "Test Hotel", Address = "A", ContactNo = "1" });
        var result = await _hotelInfoRepo.GetByIdAsync(2);
        Assert.Equal("Test Hotel", result!.HotelName);
    }

    [Fact]
    public async Task HotelInfo_Update_ShouldModify()
    {
        var info = new HotelInfo { ID = 3, HotelName = "Old Name", Address = "A", ContactNo = "1" };
        await _hotelInfoRepo.AddAsync(info);
        info.HotelName = "New Name";
        await _hotelInfoRepo.UpdateAsync(info);
        var result = await _hotelInfoRepo.GetByIdAsync(3);
        Assert.Equal("New Name", result!.HotelName);
    }

    [Fact]
    public async Task HotelInfo_Delete_ShouldRemove()
    {
        var info = new HotelInfo { ID = 4, HotelName = "Delete Me", Address = "A", ContactNo = "1" };
        await _hotelInfoRepo.AddAsync(info);
        await _hotelInfoRepo.DeleteAsync(info);
        var result = await _hotelInfoRepo.GetByIdAsync(4);
        Assert.Null(result);
    }

    [Fact]
    public async Task HotelInfo_Count_ShouldBeAccurate()
    {
        await _hotelInfoRepo.AddAsync(new HotelInfo { ID = 5, HotelName = "H1", Address = "A", ContactNo = "1" });
        int count = await _hotelInfoRepo.CountAsync();
        Assert.Equal(1, count);
    }

    // ============= CURRENCY CRUD (5 tests) =============

    [Fact]
    public async Task Currency_Create_ShouldPersist()
    {
        var c = new CurrencySet { ID = 1, CS_Currency = "USD" };
        await _currencyRepo.AddAsync(c);
        var result = await _currencyRepo.GetByIdAsync(1);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Currency_Read_ShouldReturnAll()
    {
        await _currencyRepo.AddAsync(new CurrencySet { ID = 2, CS_Currency = "EUR" });
        await _currencyRepo.AddAsync(new CurrencySet { ID = 3, CS_Currency = "GBP" });
        var all = await _currencyRepo.GetAllAsync();
        Assert.Equal(2, all.Count());
    }

    [Fact]
    public async Task Currency_Update_ShouldModify()
    {
        var c = new CurrencySet { ID = 4, CS_Currency = "INR" };
        await _currencyRepo.AddAsync(c);
        c.CS_Currency = "Indian Rupee";
        await _currencyRepo.UpdateAsync(c);
        var result = await _currencyRepo.GetByIdAsync(4);
        Assert.Equal("Indian Rupee", result!.CS_Currency);
    }

    [Fact]
    public async Task Currency_Delete_ShouldRemove()
    {
        var c = new CurrencySet { ID = 5, CS_Currency = "JPY" };
        await _currencyRepo.AddAsync(c);
        await _currencyRepo.DeleteAsync(c);
        Assert.Null(await _currencyRepo.GetByIdAsync(5));
    }

    [Fact]
    public async Task Currency_Exists_ShouldReturnTrue()
    {
        await _currencyRepo.AddAsync(new CurrencySet { ID = 6, CS_Currency = "AUD" });
        Assert.True(await _currencyRepo.ExistsAsync(c => c.CS_Currency == "AUD"));
    }

    // ============= EMPLOYEE CRUD (5 tests) =============

    [Fact]
    public async Task Employee_Create_ShouldPersist()
    {
        var emp = new Employee { EmployeeID = "E-123456", EmployeeName = "John", Address = "A", MobileNo = "123", Email = "j@t.com", Department = "IT", Designation = "Dev", DateOfJoining = DateTime.Now, Salary = 50000m, BasicWorkingTime = new TimeSpan(8, 0, 0) };
        await _employeeRepo.AddAsync(emp);
        var result = await _employeeRepo.GetByIdAsync("E-123456");
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Employee_Read_ShouldReturnAll()
    {
        await _employeeRepo.AddAsync(new Employee { EmployeeID = "E-200001", EmployeeName = "A", Address = "A", MobileNo = "1", Email = "a@t.com", Department = "IT", Designation = "D", DateOfJoining = DateTime.Now, Salary = 30000m, BasicWorkingTime = new TimeSpan(8, 0, 0) });
        await _employeeRepo.AddAsync(new Employee { EmployeeID = "E-200002", EmployeeName = "B", Address = "B", MobileNo = "2", Email = "b@t.com", Department = "HR", Designation = "M", DateOfJoining = DateTime.Now, Salary = 40000m, BasicWorkingTime = new TimeSpan(8, 0, 0) });
        var all = await _employeeRepo.GetAllAsync();
        Assert.Equal(2, all.Count());
    }

    [Fact]
    public async Task Employee_Update_ShouldModify()
    {
        var emp = new Employee { EmployeeID = "E-300001", EmployeeName = "Old", Address = "A", MobileNo = "1", Email = "e@t.com", Department = "IT", Designation = "D", DateOfJoining = DateTime.Now, Salary = 30000m, BasicWorkingTime = new TimeSpan(8, 0, 0) };
        await _employeeRepo.AddAsync(emp);
        emp.Salary = 35000m;
        await _employeeRepo.UpdateAsync(emp);
        var result = await _employeeRepo.GetByIdAsync("E-300001");
        Assert.Equal(35000m, result!.Salary);
    }

    [Fact]
    public async Task Employee_Delete_ShouldRemove()
    {
        var emp = new Employee { EmployeeID = "E-400001", EmployeeName = "Del", Address = "A", MobileNo = "1", Email = "d@t.com", Department = "IT", Designation = "D", DateOfJoining = DateTime.Now, Salary = 30000m, BasicWorkingTime = new TimeSpan(8, 0, 0) };
        await _employeeRepo.AddAsync(emp);
        await _employeeRepo.DeleteAsync(emp);
        Assert.Null(await _employeeRepo.GetByIdAsync("E-400001"));
    }

    [Fact]
    public async Task Employee_Find_ByDepartment()
    {
        await _employeeRepo.AddAsync(new Employee { EmployeeID = "E-500001", EmployeeName = "A", Address = "A", MobileNo = "1", Email = "a@t.com", Department = "IT", Designation = "D", DateOfJoining = DateTime.Now, Salary = 30000m, BasicWorkingTime = new TimeSpan(8, 0, 0) });
        await _employeeRepo.AddAsync(new Employee { EmployeeID = "E-500002", EmployeeName = "B", Address = "B", MobileNo = "2", Email = "b@t.com", Department = "HR", Designation = "M", DateOfJoining = DateTime.Now, Salary = 40000m, BasicWorkingTime = new TimeSpan(8, 0, 0) });
        var result = await _employeeRepo.FindAsync(e => e.Department == "IT");
        Assert.Single(result);
    }

    // ============= STOCK CRUD (5 tests) =============

    [Fact]
    public async Task Stock_Create_ShouldPersist()
    {
        var stock = new Stock { StockID = "S-001", LiquorName = "Whisky", NoOfBottles = 10, Volume = 750m, TotalVolume = 7500m, StockDate = DateTime.Now };
        await _stockRepo.AddAsync(stock);
        Assert.NotNull(await _stockRepo.GetByIdAsync("S-001"));
    }

    [Fact]
    public async Task Stock_Read_ShouldReturnAll()
    {
        await _stockRepo.AddAsync(new Stock { StockID = "S-002", LiquorName = "Vodka", NoOfBottles = 5, Volume = 500m, TotalVolume = 2500m, StockDate = DateTime.Now });
        await _stockRepo.AddAsync(new Stock { StockID = "S-003", LiquorName = "Rum", NoOfBottles = 8, Volume = 750m, TotalVolume = 6000m, StockDate = DateTime.Now });
        var all = await _stockRepo.GetAllAsync();
        Assert.Equal(2, all.Count());
    }

    [Fact]
    public async Task Stock_Update_ShouldModify()
    {
        var stock = new Stock { StockID = "S-004", LiquorName = "Gin", NoOfBottles = 5, Volume = 750m, TotalVolume = 3750m, StockDate = DateTime.Now };
        await _stockRepo.AddAsync(stock);
        stock.NoOfBottles = 10;
        await _stockRepo.UpdateAsync(stock);
        var result = await _stockRepo.GetByIdAsync("S-004");
        Assert.Equal(10, result!.NoOfBottles);
    }

    [Fact]
    public async Task Stock_Delete_ShouldRemove()
    {
        var stock = new Stock { StockID = "S-005", LiquorName = "Wine", NoOfBottles = 3, Volume = 750m, TotalVolume = 2250m, StockDate = DateTime.Now };
        await _stockRepo.AddAsync(stock);
        await _stockRepo.DeleteAsync(stock);
        Assert.Null(await _stockRepo.GetByIdAsync("S-005"));
    }

    [Fact]
    public async Task Stock_Find_ByLiquorName()
    {
        await _stockRepo.AddAsync(new Stock { StockID = "S-006", LiquorName = "Whisky", NoOfBottles = 10, Volume = 750m, TotalVolume = 7500m, StockDate = DateTime.Now });
        var result = await _stockRepo.FindAsync(s => s.LiquorName == "Whisky");
        Assert.Single(result);
    }

    // ============= PURCHASE INVENTORY CRUD (5 tests) =============

    [Fact]
    public async Task PurchaseInventory_Create_ShouldPersist()
    {
        var item = new PurchasedInventory { ID = 1, ProductName = "Rice", Category = "Food", TransactionType = "Purchase", PartyName = "ABC", Quantity = 100, Unit = "Kg", Price = 50m, TotalPrice = 5000m, PurchaseDate = DateTime.Now };
        await _inventoryRepo.AddAsync(item);
        Assert.NotNull(await _inventoryRepo.GetByIdAsync(1));
    }

    [Fact]
    public async Task PurchaseInventory_Read_ShouldReturnAll()
    {
        await _inventoryRepo.AddAsync(new PurchasedInventory { ID = 2, ProductName = "Oil", Category = "Food", TransactionType = "Purchase", PartyName = "XYZ", Quantity = 50, Unit = "L", Price = 100m, TotalPrice = 5000m, PurchaseDate = DateTime.Now });
        await _inventoryRepo.AddAsync(new PurchasedInventory { ID = 3, ProductName = "Sugar", Category = "Food", TransactionType = "Purchase", PartyName = "ABC", Quantity = 200, Unit = "Kg", Price = 40m, TotalPrice = 8000m, PurchaseDate = DateTime.Now });
        var all = await _inventoryRepo.GetAllAsync();
        Assert.Equal(2, all.Count());
    }

    [Fact]
    public async Task PurchaseInventory_Update_ShouldModify()
    {
        var item = new PurchasedInventory { ID = 4, ProductName = "Salt", Category = "Food", TransactionType = "Purchase", PartyName = "DEF", Quantity = 30, Unit = "Kg", Price = 20m, TotalPrice = 600m, PurchaseDate = DateTime.Now };
        await _inventoryRepo.AddAsync(item);
        item.Quantity = 50;
        item.TotalPrice = 1000m;
        await _inventoryRepo.UpdateAsync(item);
        var result = await _inventoryRepo.GetByIdAsync(4);
        Assert.Equal(50, result!.Quantity);
    }

    [Fact]
    public async Task PurchaseInventory_Delete_ShouldRemove()
    {
        var item = new PurchasedInventory { ID = 5, ProductName = "Flour", Category = "Food", TransactionType = "Purchase", PartyName = "GHI", Quantity = 75, Unit = "Kg", Price = 35m, TotalPrice = 2625m, PurchaseDate = DateTime.Now };
        await _inventoryRepo.AddAsync(item);
        await _inventoryRepo.DeleteAsync(item);
        Assert.Null(await _inventoryRepo.GetByIdAsync(5));
    }

    [Fact]
    public async Task PurchaseInventory_Find_ByCategory()
    {
        await _inventoryRepo.AddAsync(new PurchasedInventory { ID = 6, ProductName = "Beer", Category = "Liquor", TransactionType = "Purchase", PartyName = "JKL", Quantity = 24, Unit = "Bottles", Price = 150m, TotalPrice = 3600m, PurchaseDate = DateTime.Now });
        await _inventoryRepo.AddAsync(new PurchasedInventory { ID = 7, ProductName = "Rice", Category = "Food", TransactionType = "Purchase", PartyName = "MNO", Quantity = 100, Unit = "Kg", Price = 50m, TotalPrice = 5000m, PurchaseDate = DateTime.Now });
        var result = await _inventoryRepo.FindAsync(i => i.Category == "Liquor");
        Assert.Single(result);
    }

    // ============= CHECK-IN CRUD (5 tests) =============

    [Fact]
    public async Task CheckInRoom_Create_ShouldPersist()
    {
        _context.Guests.Add(new Guest { GuestID = "G-CI0001", GuestName = "Test", Address = "A", City = "C", ContactNo = "1", IDType = "DL", IDNumber = "1" });
        _context.Rooms.Add(new Room { RoomNo = "CI-101", RoomType = "Deluxe", RoomCharges = 5000m });
        await _context.SaveChangesAsync();

        var checkIn = new CheckInRoom { ID = 1, GuestID = "G-CI0001", RoomNo = "CI-101", RoomCharges = 5000m, DateIN = DateTime.Now, DateOUT = DateTime.Now.AddDays(3), NoOfDays = 3, TotalRoomCharges = 15000m, SubTotal = 15000m, GrandTotal = 15000m, Status = "Checked In" };
        _context.CheckInRooms.Add(checkIn);
        await _context.SaveChangesAsync();
        Assert.NotNull(await _context.CheckInRooms.FindAsync(1));
    }

    [Fact]
    public async Task CheckInRoom_Read_ShouldReturn()
    {
        _context.Guests.Add(new Guest { GuestID = "G-CI0002", GuestName = "Test2", Address = "B", City = "D", ContactNo = "2", IDType = "PP", IDNumber = "2" });
        _context.Rooms.Add(new Room { RoomNo = "CI-102", RoomType = "Standard", RoomCharges = 3000m });
        await _context.SaveChangesAsync();

        _context.CheckInRooms.Add(new CheckInRoom { ID = 2, GuestID = "G-CI0002", RoomNo = "CI-102", RoomCharges = 3000m, DateIN = DateTime.Now, DateOUT = DateTime.Now.AddDays(2), NoOfDays = 2, TotalRoomCharges = 6000m, SubTotal = 6000m, GrandTotal = 6000m, Status = "Checked In" });
        await _context.SaveChangesAsync();
        var result = await _context.CheckInRooms.FindAsync(2);
        Assert.Equal("Checked In", result!.Status);
    }

    [Fact]
    public async Task CheckInRoom_Update_ShouldModify()
    {
        _context.Guests.Add(new Guest { GuestID = "G-CI0003", GuestName = "Test3", Address = "C", City = "E", ContactNo = "3", IDType = "ID", IDNumber = "3" });
        _context.Rooms.Add(new Room { RoomNo = "CI-103", RoomType = "Suite", RoomCharges = 8000m });
        await _context.SaveChangesAsync();

        var checkIn = new CheckInRoom { ID = 3, GuestID = "G-CI0003", RoomNo = "CI-103", RoomCharges = 8000m, DateIN = DateTime.Now, DateOUT = DateTime.Now.AddDays(1), NoOfDays = 1, TotalRoomCharges = 8000m, SubTotal = 8000m, GrandTotal = 8000m, Status = "Checked In" };
        _context.CheckInRooms.Add(checkIn);
        await _context.SaveChangesAsync();
        checkIn.Status = "Checked Out";
        await _context.SaveChangesAsync();
        var result = await _context.CheckInRooms.FindAsync(3);
        Assert.Equal("Checked Out", result!.Status);
    }

    [Fact]
    public async Task CheckInRoom_Delete_ShouldRemove()
    {
        _context.Guests.Add(new Guest { GuestID = "G-CI0004", GuestName = "Test4", Address = "D", City = "F", ContactNo = "4", IDType = "PP", IDNumber = "4" });
        _context.Rooms.Add(new Room { RoomNo = "CI-104", RoomType = "Standard", RoomCharges = 3000m });
        await _context.SaveChangesAsync();

        var checkIn = new CheckInRoom { ID = 4, GuestID = "G-CI0004", RoomNo = "CI-104", RoomCharges = 3000m, DateIN = DateTime.Now, DateOUT = DateTime.Now.AddDays(1), NoOfDays = 1, TotalRoomCharges = 3000m, SubTotal = 3000m, GrandTotal = 3000m, Status = "Checked In" };
        _context.CheckInRooms.Add(checkIn);
        await _context.SaveChangesAsync();
        _context.CheckInRooms.Remove(checkIn);
        await _context.SaveChangesAsync();
        Assert.Null(await _context.CheckInRooms.FindAsync(4));
    }

    [Fact]
    public async Task CheckInRoom_Find_ByStatus()
    {
        _context.Guests.Add(new Guest { GuestID = "G-CI0005", GuestName = "Test5", Address = "E", City = "G", ContactNo = "5", IDType = "DL", IDNumber = "5" });
        _context.Rooms.Add(new Room { RoomNo = "CI-105", RoomType = "Deluxe", RoomCharges = 5000m });
        await _context.SaveChangesAsync();

        _context.CheckInRooms.Add(new CheckInRoom { ID = 5, GuestID = "G-CI0005", RoomNo = "CI-105", RoomCharges = 5000m, DateIN = DateTime.Now, DateOUT = DateTime.Now.AddDays(2), NoOfDays = 2, TotalRoomCharges = 10000m, SubTotal = 10000m, GrandTotal = 10000m, Status = "Checked In" });
        await _context.SaveChangesAsync();
        var result = await _context.CheckInRooms.Where(c => c.Status == "Checked In").ToListAsync();
        Assert.Single(result);
    }

    // ============= TAX INFO CRUD (5 tests) =============

    [Fact]
    public async Task TaxInfo_Create_ShouldPersist()
    {
        var tax = new TaxInfo { ID = 1, Salray = "0-50000", TaxInP = 10m };
        _context.TaxInfos.Add(tax);
        await _context.SaveChangesAsync();
        Assert.NotNull(await _context.TaxInfos.FindAsync(1));
    }

    [Fact]
    public async Task TaxInfo_Read_ShouldReturn()
    {
                _context.TaxInfos.Add(new TaxInfo { ID = 2, Salray = "50000-100000", TaxInP = 20m });
                await _context.SaveChangesAsync();
                var result = await _context.TaxInfos.FindAsync(2);
                Assert.Equal(20m, result!.TaxInP);
    }

    [Fact]
    public async Task TaxInfo_Update_ShouldModify()
    {
                var tax = new TaxInfo { ID = 3, Salray = "100000+", TaxInP = 30m };
                _context.TaxInfos.Add(tax);
                await _context.SaveChangesAsync();
                tax.TaxInP = 25m;
                await _context.SaveChangesAsync();
                Assert.Equal(25m, (await _context.TaxInfos.FindAsync(3))!.TaxInP);
    }

    [Fact]
    public async Task TaxInfo_Delete_ShouldRemove()
    {
        var tax = new TaxInfo { ID = 4, Salray = "Test", TaxInP = 5m };
        _context.TaxInfos.Add(tax);
        await _context.SaveChangesAsync();
        _context.TaxInfos.Remove(tax);
        await _context.SaveChangesAsync();
        Assert.Null(await _context.TaxInfos.FindAsync(4));
    }

    [Fact]
    public async Task TaxInfo_Count_ShouldBeAccurate()
    {
                _context.TaxInfos.Add(new TaxInfo { ID = 5, Salray = "Range1", TaxInP = 10m });
                _context.TaxInfos.Add(new TaxInfo { ID = 6, Salray = "Range2", TaxInP = 20m });
        await _context.SaveChangesAsync();
        Assert.Equal(2, await _context.TaxInfos.CountAsync());
    }
}
