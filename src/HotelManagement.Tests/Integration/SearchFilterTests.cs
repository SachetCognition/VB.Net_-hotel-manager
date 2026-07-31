using HotelManagement.Core.Models;
using HotelManagement.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HotelManagement.Tests.Integration;

/// <summary>
/// Category 8: Search/Filter Tests (~20 tests)
/// Tests search and filter operations matching original VB.NET LIKE queries.
/// </summary>
public class SearchFilterTests : IDisposable
{
    private readonly HotelDbContext _context;

    public SearchFilterTests()
    {
        var options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new HotelDbContext(options);
        SeedData();
    }

    private void SeedData()
    {
        // Guests
        _context.Guests.AddRange(
            new Guest { GuestID = "G-000001", GuestName = "John Smith", Address = "123 Main", City = "NYC", ContactNo = "1111111111", IDType = "Passport", IDNumber = "P1" },
            new Guest { GuestID = "G-000002", GuestName = "John Doe", Address = "456 Oak", City = "LA", ContactNo = "2222222222", IDType = "DL", IDNumber = "D1" },
            new Guest { GuestID = "G-000003", GuestName = "Jane Wilson", Address = "789 Pine", City = "NYC", ContactNo = "3333333333", IDType = "ID", IDNumber = "I1" },
            new Guest { GuestID = "G-000004", GuestName = "Alice Brown", Address = "321 Elm", City = "Chicago", ContactNo = "4444444444", IDType = "Passport", IDNumber = "P2" }
        );

        // Rooms
        _context.Rooms.AddRange(
            new Room { RoomNo = "101", RoomType = "Deluxe", RoomCharges = 5000m },
            new Room { RoomNo = "102", RoomType = "Standard", RoomCharges = 3000m },
            new Room { RoomNo = "201", RoomType = "Suite", RoomCharges = 8000m },
            new Room { RoomNo = "202", RoomType = "Deluxe", RoomCharges = 5500m }
        );

        // Employees
        _context.Employees.AddRange(
            new Employee { EmployeeID = "E-000001", EmployeeName = "Bob Manager", Address = "A", MobileNo = "1", Email = "b@t.com", Department = "ADMIN", Designation = "MANAGER", DateOfJoining = DateTime.Now, Salary = 50000m, BasicWorkingTime = new TimeSpan(8, 0, 0) },
            new Employee { EmployeeID = "E-000002", EmployeeName = "Carol Chef", Address = "B", MobileNo = "2", Email = "c@t.com", Department = "KITCHEN", Designation = "CHEF", DateOfJoining = DateTime.Now, Salary = 35000m, BasicWorkingTime = new TimeSpan(8, 0, 0) },
            new Employee { EmployeeID = "E-000003", EmployeeName = "Bob Server", Address = "C", MobileNo = "3", Email = "d@t.com", Department = "SERVICE", Designation = "WAITER", DateOfJoining = DateTime.Now, Salary = 25000m, BasicWorkingTime = new TimeSpan(8, 0, 0) }
        );

        // Stocks
        _context.Stocks.AddRange(
            new Stock { StockID = "S-001", LiquorName = "Jack Daniels", NoOfBottles = 10, Volume = 750m, TotalVolume = 7500m, StockDate = DateTime.Now },
            new Stock { StockID = "S-002", LiquorName = "Johnnie Walker", NoOfBottles = 5, Volume = 1000m, TotalVolume = 5000m, StockDate = DateTime.Now },
            new Stock { StockID = "S-003", LiquorName = "Absolut Vodka", NoOfBottles = 8, Volume = 750m, TotalVolume = 6000m, StockDate = DateTime.Now }
        );

        // Purchase Inventory
        _context.PurchasedInventories.AddRange(
            new PurchasedInventory { ID = 1, ProductName = "Rice", Category = "Food", TransactionType = "Purchase", PartyName = "ABC Traders", Quantity = 100, Unit = "Kg", Price = 50m, TotalPrice = 5000m, PurchaseDate = DateTime.Now },
            new PurchasedInventory { ID = 2, ProductName = "Whisky", Category = "Liquor", TransactionType = "Purchase", PartyName = "XYZ Imports", Quantity = 24, Unit = "Bottles", Price = 1500m, TotalPrice = 36000m, PurchaseDate = DateTime.Now },
            new PurchasedInventory { ID = 3, ProductName = "Oil", Category = "Food", TransactionType = "Sale", PartyName = "ABC Traders", Quantity = 10, Unit = "L", Price = 120m, TotalPrice = 1200m, PurchaseDate = DateTime.Now }
        );

        // CheckIn records
        _context.CheckInRooms.AddRange(
            new CheckInRoom { ID = 1, GuestID = "G-000001", RoomNo = "101", RoomCharges = 5000m, DateIN = DateTime.Now.AddDays(-2), DateOUT = DateTime.Now, NoOfDays = 2, TotalRoomCharges = 10000m, SubTotal = 10000m, GrandTotal = 10000m, Status = "Checked In" },
            new CheckInRoom { ID = 2, GuestID = "G-000002", RoomNo = "102", RoomCharges = 3000m, DateIN = DateTime.Now.AddDays(-3), DateOUT = DateTime.Now.AddDays(-1), NoOfDays = 2, TotalRoomCharges = 6000m, SubTotal = 6000m, GrandTotal = 6000m, Status = "Checked Out" }
        );

        _context.SaveChanges();
    }

    public void Dispose() => _context.Dispose();

    // ============= GUEST SEARCH =============

    // TC-SRCH-001: Search guest by name prefix "John"
    [Fact]
    public async Task SearchGuest_ByNamePrefix_John()
    {
        var result = await _context.Guests.Where(g => g.GuestName.StartsWith("John")).ToListAsync();
        Assert.Equal(2, result.Count); // John Smith, John Doe
    }

    // TC-SRCH-002: Search guest by exact name
    [Fact]
    public async Task SearchGuest_ByExactName()
    {
        var result = await _context.Guests.Where(g => g.GuestName == "Jane Wilson").ToListAsync();
        Assert.Single(result);
    }

    // TC-SRCH-003: Search guest with no results
    [Fact]
    public async Task SearchGuest_NoResults()
    {
        var result = await _context.Guests.Where(g => g.GuestName.StartsWith("Zzz")).ToListAsync();
        Assert.Empty(result);
    }

    // TC-SRCH-004: Search guest case sensitivity
    [Fact]
    public async Task SearchGuest_ContainsSubstring()
    {
        var result = await _context.Guests.Where(g => g.GuestName.Contains("Smith")).ToListAsync();
        Assert.Single(result);
    }

    // ============= ROOM SEARCH =============

    // TC-SRCH-005: Search room by number
    [Fact]
    public async Task SearchRoom_ByNumber()
    {
        var result = await _context.Rooms.FindAsync("101");
        Assert.NotNull(result);
        Assert.Equal("Deluxe", result!.RoomType);
    }

    // TC-SRCH-006: Search room by type
    [Fact]
    public async Task SearchRoom_ByType()
    {
        var result = await _context.Rooms.Where(r => r.RoomType == "Deluxe").ToListAsync();
        Assert.Equal(2, result.Count);
    }

    // TC-SRCH-007: Search room not found
    [Fact]
    public async Task SearchRoom_NotFound()
    {
        var result = await _context.Rooms.FindAsync("999");
        Assert.Null(result);
    }

    // ============= EMPLOYEE SEARCH =============

    // TC-SRCH-008: Search employee by name prefix
    [Fact]
    public async Task SearchEmployee_ByNamePrefix()
    {
        var result = await _context.Employees.Where(e => e.EmployeeName.StartsWith("Bob")).ToListAsync();
        Assert.Equal(2, result.Count);
    }

    // TC-SRCH-009: Search employee by department
    [Fact]
    public async Task SearchEmployee_ByDepartment()
    {
        var result = await _context.Employees.Where(e => e.Department == "KITCHEN").ToListAsync();
        Assert.Single(result);
    }

    // TC-SRCH-010: Search employee by designation
    [Fact]
    public async Task SearchEmployee_ByDesignation()
    {
        var result = await _context.Employees.Where(e => e.Designation == "CHEF").ToListAsync();
        Assert.Single(result);
    }

    // ============= STOCK SEARCH =============

    // TC-SRCH-011: Search stock by liquor name prefix
    [Fact]
    public async Task SearchStock_ByNamePrefix()
    {
        var result = await _context.Stocks.Where(s => s.LiquorName.StartsWith("J")).ToListAsync();
        Assert.Equal(2, result.Count); // Jack Daniels, Johnnie Walker
    }

    // TC-SRCH-012: Search stock by exact name
    [Fact]
    public async Task SearchStock_ByExactName()
    {
        var result = await _context.Stocks.Where(s => s.LiquorName == "Absolut Vodka").ToListAsync();
        Assert.Single(result);
    }

    // ============= PURCHASE INVENTORY FILTER =============

    // TC-SRCH-013: Filter inventory by category "Food"
    [Fact]
    public async Task FilterInventory_ByCategory_Food()
    {
        var result = await _context.PurchasedInventories.Where(p => p.Category == "Food").ToListAsync();
        Assert.Equal(2, result.Count);
    }

    // TC-SRCH-014: Filter inventory by category "Liquor"
    [Fact]
    public async Task FilterInventory_ByCategory_Liquor()
    {
        var result = await _context.PurchasedInventories.Where(p => p.Category == "Liquor").ToListAsync();
        Assert.Single(result);
    }

    // TC-SRCH-015: Filter inventory by party name
    [Fact]
    public async Task FilterInventory_ByPartyName()
    {
        var result = await _context.PurchasedInventories.Where(p => p.PartyName == "ABC Traders").ToListAsync();
        Assert.Equal(2, result.Count);
    }

    // TC-SRCH-016: Filter inventory by transaction type "Purchase"
    [Fact]
    public async Task FilterInventory_ByTransactionType_Purchase()
    {
        var result = await _context.PurchasedInventories.Where(p => p.TransactionType == "Purchase").ToListAsync();
        Assert.Equal(2, result.Count);
    }

    // TC-SRCH-017: Filter inventory by transaction type "Sale"
    [Fact]
    public async Task FilterInventory_ByTransactionType_Sale()
    {
        var result = await _context.PurchasedInventories.Where(p => p.TransactionType == "Sale").ToListAsync();
        Assert.Single(result);
    }

    // ============= CHECK-IN SEARCH =============

    // TC-SRCH-018: Search checked-in records
    [Fact]
    public async Task SearchCheckIn_ByStatus_CheckedIn()
    {
        var result = await _context.CheckInRooms.Where(c => c.Status == "Checked In").ToListAsync();
        Assert.Single(result);
    }

    // TC-SRCH-019: Search checked-out records
    [Fact]
    public async Task SearchCheckIn_ByStatus_CheckedOut()
    {
        var result = await _context.CheckInRooms.Where(c => c.Status == "Checked Out").ToListAsync();
        Assert.Single(result);
    }

    // TC-SRCH-020: Filter inventory by product name
    [Fact]
    public async Task FilterInventory_ByProductName()
    {
        var result = await _context.PurchasedInventories.Where(p => p.ProductName == "Rice").ToListAsync();
        Assert.Single(result);
    }
}
