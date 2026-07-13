using HotelManager.Domain.Entities;
using HotelManager.Infrastructure.Services;

namespace HotelManager.IntegrationTests;

public class HotelInfoServiceTests : IClassFixture<SqliteDbFixture>
{
    private readonly SqliteDbFixture _fixture;

    public HotelInfoServiceTests(SqliteDbFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Save_CreatesSingleRecord_ThenUpdatesIt()
    {
        await using var db = _fixture.CreateContext();
        var svc = new HotelInfoService(db);

        await svc.SaveAsync(new HotelInfo { HotelName = "Grand", Address = "1 Main St", ContactNo = "12345" });
        var first = await svc.GetAsync();
        Assert.NotNull(first);
        Assert.Equal("Grand", first!.HotelName);

        await svc.SaveAsync(new HotelInfo { HotelName = "Grand Plaza", Address = "2 Main St", ContactNo = "99999", Email = "x@y.z" });
        var updated = await svc.GetAsync();
        Assert.NotNull(updated);
        Assert.Equal(first.ID, updated!.ID);
        Assert.Equal("Grand Plaza", updated.HotelName);
        Assert.Equal("x@y.z", updated.Email);
        Assert.Single(db.HotelInfos);
    }

    [Fact]
    public async Task Save_StoresLogoBytes()
    {
        await using var db = _fixture.CreateContext();
        var svc = new HotelInfoService(db);
        var logo = new byte[] { 1, 2, 3, 4 };
        await svc.SaveAsync(new HotelInfo { HotelName = "H", Address = "A", ContactNo = "C", Logo = logo });
        var saved = await svc.GetAsync();
        Assert.Equal(logo, saved!.Logo);
    }

    [Theory]
    [InlineData(null, "A", "C", "Please enter hotel name")]
    [InlineData("H", " ", "C", "Please enter address")]
    [InlineData("H", "A", "", "Please enter contact no.")]
    public async Task Save_MissingRequiredField_Throws(string? name, string? address, string? contact, string expected)
    {
        await using var db = _fixture.CreateContext();
        var svc = new HotelInfoService(db);
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            svc.SaveAsync(new HotelInfo { HotelName = name, Address = address, ContactNo = contact }));
        Assert.StartsWith(expected, ex.Message);
    }
}

public class CurrencyServiceTests : IClassFixture<SqliteDbFixture>
{
    private readonly SqliteDbFixture _fixture;

    public CurrencyServiceTests(SqliteDbFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Create_Get_Update_Delete_Roundtrip()
    {
        await using var db = _fixture.CreateContext();
        var svc = new CurrencyService(db);

        var created = await svc.CreateAsync(new CurrencySet { CS_Currency = "USD" });
        Assert.True(created.ID > 0);

        var fetched = await svc.GetAsync(created.ID);
        Assert.Equal("USD", fetched!.CS_Currency);

        await svc.UpdateAsync(new CurrencySet { ID = created.ID, CS_Currency = "EUR" });
        Assert.Equal("EUR", (await svc.GetAsync(created.ID))!.CS_Currency);

        await svc.DeleteAsync(created.ID);
        Assert.Null(await svc.GetAsync(created.ID));
    }

    [Fact]
    public async Task Create_DuplicateName_Throws()
    {
        await using var db = _fixture.CreateContext();
        var svc = new CurrencyService(db);
        await svc.CreateAsync(new CurrencySet { CS_Currency = "GBP" });
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.CreateAsync(new CurrencySet { CS_Currency = "GBP" }));
        Assert.Equal("Currency Name Already Exists", ex.Message);
    }

    [Fact]
    public async Task Create_EmptyName_Throws()
    {
        await using var db = _fixture.CreateContext();
        var svc = new CurrencyService(db);
        await Assert.ThrowsAsync<ArgumentException>(() => svc.CreateAsync(new CurrencySet { CS_Currency = " " }));
    }

    [Fact]
    public async Task GetAll_OrdersByCurrencyName()
    {
        await using var db = _fixture.CreateContext();
        var svc = new CurrencyService(db);
        await svc.CreateAsync(new CurrencySet { CS_Currency = "ZAR" });
        await svc.CreateAsync(new CurrencySet { CS_Currency = "AUD" });
        var all = await svc.GetAllAsync();
        Assert.Equal(all.OrderBy(c => c.CS_Currency).Select(c => c.ID), all.Select(c => c.ID));
    }

    [Fact]
    public async Task Delete_Missing_Throws()
    {
        await using var db = _fixture.CreateContext();
        var svc = new CurrencyService(db);
        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.DeleteAsync(99999));
    }
}

public class RoomServiceTests : IClassFixture<SqliteDbFixture>
{
    private readonly SqliteDbFixture _fixture;

    public RoomServiceTests(SqliteDbFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task CreateRoom_ThenGetAndSearch()
    {
        await using var db = _fixture.CreateContext();
        var svc = new RoomService(db);
        await svc.CreateRoomAsync(new Room { RoomNo = "101", RoomType = "Standard Room", RoomCharges = 100 });
        await svc.CreateRoomAsync(new Room { RoomNo = "201", RoomType = "Delux Room", RoomCharges = 200 });

        var room = await svc.GetRoomAsync("101");
        Assert.Equal("Standard Room", room!.RoomType);

        var found = await svc.GetRoomsAsync("10");
        Assert.Single(found);
        Assert.Equal("101", found[0].RoomNo);

        var all = await svc.GetRoomsAsync();
        Assert.Contains(all, r => r.RoomNo == "101");
        Assert.Contains(all, r => r.RoomNo == "201");
    }

    [Fact]
    public async Task CreateRoom_DuplicateRoomNo_Throws()
    {
        await using var db = _fixture.CreateContext();
        var svc = new RoomService(db);
        await svc.CreateRoomAsync(new Room { RoomNo = "301", RoomType = "Twin Room", RoomCharges = 50 });
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.CreateRoomAsync(new Room { RoomNo = "301", RoomType = "Twin Room", RoomCharges = 60 }));
        Assert.Equal("Room Number Already Exists", ex.Message);
    }

    [Theory]
    [InlineData("", "Standard Room", 10, "Please enter room no.")]
    [InlineData("401", "", 10, "Please select room type")]
    [InlineData("401", "Standard Room", null, "Please enter room charges")]
    public async Task CreateRoom_MissingField_Throws(string roomNo, string roomType, int? charges, string expected)
    {
        await using var db = _fixture.CreateContext();
        var svc = new RoomService(db);
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            svc.CreateRoomAsync(new Room { RoomNo = roomNo, RoomType = roomType, RoomCharges = charges }));
        Assert.StartsWith(expected, ex.Message);
    }

    [Fact]
    public async Task UpdateRoom_ChangesTypeAndCharges()
    {
        await using var db = _fixture.CreateContext();
        var svc = new RoomService(db);
        await svc.CreateRoomAsync(new Room { RoomNo = "501", RoomType = "Standard Room", RoomCharges = 100 });
        await svc.UpdateRoomAsync(new Room { RoomNo = "501", RoomType = "Family Room", RoomCharges = 150 });
        var updated = await svc.GetRoomAsync("501");
        Assert.Equal("Family Room", updated!.RoomType);
        Assert.Equal(150, updated.RoomCharges);
    }

    [Fact]
    public async Task DeleteRoom_RemovesRecord_MissingThrows()
    {
        await using var db = _fixture.CreateContext();
        var svc = new RoomService(db);
        await svc.CreateRoomAsync(new Room { RoomNo = "601", RoomType = "Twin Room", RoomCharges = 80 });
        await svc.DeleteRoomAsync("601");
        Assert.Null(await svc.GetRoomAsync("601"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.DeleteRoomAsync("601"));
    }

    [Fact]
    public async Task Hall_SaveUpdateDelete_Roundtrip()
    {
        await using var db = _fixture.CreateContext();
        var svc = new RoomService(db);

        var hall = await svc.SaveHallAsync(new Hall { Charges = 500, HallName = "Main Hall" });
        Assert.True(hall.ID > 0);

        var updated = await svc.SaveHallAsync(new Hall { ID = hall.ID, Charges = 600, HallName = "Grand Hall" });
        Assert.Equal(hall.ID, updated.ID);
        Assert.Equal(600, updated.Charges);
        Assert.Equal("Grand Hall", updated.HallName);

        await svc.DeleteHallAsync(hall.ID);
        Assert.DoesNotContain(await svc.GetHallsAsync(), h => h.ID == hall.ID);
    }

    [Fact]
    public async Task Hall_MissingCharges_Throws()
    {
        await using var db = _fixture.CreateContext();
        var svc = new RoomService(db);
        await Assert.ThrowsAsync<ArgumentException>(() => svc.SaveHallAsync(new Hall()));
    }

    [Fact]
    public async Task Garden_SaveUpdateDelete_Roundtrip()
    {
        await using var db = _fixture.CreateContext();
        var svc = new RoomService(db);

        var garden = await svc.SaveGardenAsync(new Garden { Charges = 300 });
        Assert.True(garden.ID > 0);

        var updated = await svc.SaveGardenAsync(new Garden { ID = garden.ID, Charges = 350 });
        Assert.Equal(350, updated.Charges);

        await svc.DeleteGardenAsync(garden.ID);
        Assert.DoesNotContain(await svc.GetGardensAsync(), g => g.ID == garden.ID);
        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.DeleteGardenAsync(garden.ID));
    }

    [Fact]
    public async Task ExtraBed_SaveUpdateDelete_Roundtrip()
    {
        await using var db = _fixture.CreateContext();
        var svc = new RoomService(db);

        var bed = await svc.SaveExtraBedAsync(new ExtraBed { Charges = 40 });
        Assert.True(bed.ID > 0);

        var updated = await svc.SaveExtraBedAsync(new ExtraBed { ID = bed.ID, Charges = 45 });
        Assert.Equal(45, updated.Charges);

        await svc.DeleteExtraBedAsync(bed.ID);
        Assert.DoesNotContain(await svc.GetExtraBedsAsync(), b => b.ID == bed.ID);
        await Assert.ThrowsAsync<ArgumentException>(() => svc.SaveExtraBedAsync(new ExtraBed()));
    }
}

public class GuestServiceTests : IClassFixture<SqliteDbFixture>
{
    private readonly SqliteDbFixture _fixture;

    public GuestServiceTests(SqliteDbFixture fixture) => _fixture = fixture;

    private static Guest ValidGuest(string name = "John Doe") => new()
    {
        GuestID = "",
        GuestName = name,
        Address = "1 Elm St",
        City = "Springfield",
        ContactNo = "5551234",
        IDType = "Passport",
        IDNumber = "P123"
    };

    [Fact]
    public async Task Create_GeneratesGuestId_WithLegacyFormat()
    {
        await using var db = _fixture.CreateContext();
        var svc = new GuestService(db);
        var guest = ValidGuest();
        await svc.CreateAsync(guest);
        Assert.Matches(@"^G-[1-9]{6}$", guest.GuestID);
        Assert.NotNull(await svc.GetAsync(guest.GuestID));
    }

    [Fact]
    public async Task GenerateGuestId_MatchesLegacyFormat()
    {
        await using var db = _fixture.CreateContext();
        var svc = new GuestService(db);
        var id = await svc.GenerateGuestIdAsync();
        Assert.Matches(@"^G-[1-9]{6}$", id);
    }

    [Theory]
    [InlineData("GuestName", "Please enter guest name")]
    [InlineData("Address", "Please enter guest address")]
    [InlineData("City", "Please enter guest city")]
    [InlineData("ContactNo", "Please enter guest contact no.")]
    [InlineData("IDType", "Please select id type")]
    [InlineData("IDNumber", "Please enter id number")]
    public async Task Create_MissingRequiredField_Throws(string field, string expected)
    {
        await using var db = _fixture.CreateContext();
        var svc = new GuestService(db);
        var guest = ValidGuest();
        switch (field)
        {
            case "GuestName": guest.GuestName = ""; break;
            case "Address": guest.Address = ""; break;
            case "City": guest.City = ""; break;
            case "ContactNo": guest.ContactNo = ""; break;
            case "IDType": guest.IDType = ""; break;
            case "IDNumber": guest.IDNumber = ""; break;
        }
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => svc.CreateAsync(guest));
        Assert.StartsWith(expected, ex.Message);
    }

    [Fact]
    public async Task Update_ChangesFields()
    {
        await using var db = _fixture.CreateContext();
        var svc = new GuestService(db);
        var guest = ValidGuest("Jane Roe");
        await svc.CreateAsync(guest);

        guest.City = "Shelbyville";
        guest.Notes = "VIP";
        await svc.UpdateAsync(guest);

        var updated = await svc.GetAsync(guest.GuestID);
        Assert.Equal("Shelbyville", updated!.City);
        Assert.Equal("VIP", updated.Notes);
    }

    [Fact]
    public async Task GetAll_SearchByNamePrefix_OrderedByName()
    {
        await using var db = _fixture.CreateContext();
        var svc = new GuestService(db);
        await svc.CreateAsync(ValidGuest("Zed Alpha"));
        await svc.CreateAsync(ValidGuest("Zed Beta"));
        await svc.CreateAsync(ValidGuest("Amy Gamma"));

        var found = await svc.GetAllAsync("Zed");
        Assert.Equal(2, found.Count);
        Assert.Equal(found.OrderBy(g => g.GuestName).Select(g => g.GuestID), found.Select(g => g.GuestID));
    }

    [Fact]
    public async Task Delete_RemovesGuest_MissingThrows()
    {
        await using var db = _fixture.CreateContext();
        var svc = new GuestService(db);
        var guest = ValidGuest("Del Me");
        await svc.CreateAsync(guest);
        await svc.DeleteAsync(guest.GuestID);
        Assert.Null(await svc.GetAsync(guest.GuestID));
        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.DeleteAsync(guest.GuestID));
    }
}
