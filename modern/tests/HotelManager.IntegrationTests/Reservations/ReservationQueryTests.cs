using HotelManager.Domain.Entities;

namespace HotelManager.IntegrationTests.Reservations;

public class ReservationQueryTests(ReservationsFixture fixture) : IClassFixture<ReservationsFixture>
{
    private static readonly DateTime D0 = new(2026, 12, 1);

    [Fact]
    public async Task Get_reservations_filters_by_search()
    {
        using var db = fixture.CreateContext();
        var svc = fixture.CreateReservationService(db);
        await svc.CreateReservationAsync(new Reservation
        {
            ReservationID = "R-40001",
            GuestID = "G-1",
            RoomNo = "101",
            DateIN = D0,
            DateOUT = D0.AddDays(1),
        });
        var all = await svc.GetReservationsAsync();
        Assert.Contains(all, r => r.ReservationID == "R-40001");
        var byGuest = await svc.GetReservationsAsync("Alice");
        Assert.Contains(byGuest, r => r.ReservationID == "R-40001");
        var none = await svc.GetReservationsAsync("no-such-guest");
        Assert.DoesNotContain(none, r => r.ReservationID == "R-40001");
    }

    [Fact]
    public async Task Temp_reservations_can_be_listed_and_deleted()
    {
        using var db = fixture.CreateContext();
        var svc = fixture.CreateReservationService(db);
        await svc.SaveTempReservationAsync(new TempReservation
        {
            ReservationID = "R-40002",
            GuestID = "G-2",
            RoomNo = "102",
            DateIN = D0,
            DateOUT = D0.AddDays(1),
        });
        var temps = await svc.GetTempReservationsAsync();
        Assert.Contains(temps, t => t.ReservationID == "R-40002");
        await svc.DeleteTempReservationAsync("R-40002");
        temps = await svc.GetTempReservationsAsync();
        Assert.DoesNotContain(temps, t => t.ReservationID == "R-40002");
    }

    [Fact]
    public async Task Hall_and_garden_reservations_can_be_listed()
    {
        using var db = fixture.CreateContext();
        var svc = fixture.CreateReservationService(db);
        await svc.SaveHallAndGardenReservationAsync(new ReservationHallAndGarden
        {
            ID = "R-40003",
            GuestID = "G-1",
            DateFrom_Hall = D0,
            DateTo_Hall = D0.AddDays(1),
            Rate_Hall = 5000,
            DateFrom_Garden = D0,
            DateTo_Garden = D0.AddDays(1),
            Rate_Garden = 3000,
        });
        var list = await svc.GetHallAndGardenReservationsAsync();
        Assert.Contains(list, r => r.ID == "R-40003");
    }

    [Fact]
    public async Task Hall_or_garden_reservations_can_be_listed_and_deleted()
    {
        using var db = fixture.CreateContext();
        var svc = fixture.CreateReservationService(db);
        await svc.SaveHallOrGardenReservationAsync(new ReservationHallOrGarden
        {
            ID = "R-40004",
            GuestID = "G-1",
            Type = "Hall",
            DateFrom = D0,
            DateTo = D0.AddDays(1),
            Rate = 5000,
        });
        var list = await svc.GetHallOrGardenReservationsAsync();
        Assert.Contains(list, r => r.ID == "R-40004");
        await svc.DeleteHallOrGardenReservationAsync("R-40004");
        list = await svc.GetHallOrGardenReservationsAsync();
        Assert.DoesNotContain(list, r => r.ID == "R-40004");
    }

    [Fact]
    public async Task Check_ins_can_be_filtered_searched_and_deleted()
    {
        using var db = fixture.CreateContext();
        var svc = fixture.CreateCheckInService(db);
        var saved = await svc.CheckInAsync(new CheckInRoom
        {
            GuestID = "G-1",
            RoomNo = "103",
            RoomCharges = 1000,
            DateIN = D0,
            DateOUT = D0.AddDays(1),
        });
        var byStatus = await svc.GetCheckInsAsync("Checked In");
        Assert.Contains(byStatus, c => c.ID == saved.ID);
        var bySearch = await svc.GetCheckInsAsync(null, "103");
        Assert.Contains(bySearch, c => c.ID == saved.ID);
        var none = await svc.GetCheckInsAsync("Checked Out", "103");
        Assert.DoesNotContain(none, c => c.ID == saved.ID);

        await svc.DeleteCheckInAsync(saved.ID);
        Assert.Null(await svc.GetCheckInAsync(saved.ID));
    }

    [Fact]
    public async Task Checkout_can_be_fetched_by_id()
    {
        using var db = fixture.CreateContext();
        var checkInSvc = fixture.CreateCheckInService(db);
        var checkIn = await checkInSvc.CheckInAsync(new CheckInRoom
        {
            GuestID = "G-2",
            RoomNo = "104",
            RoomCharges = 1000,
            DateIN = D0,
            DateOUT = D0.AddDays(1),
        });
        var svc = fixture.CreateCheckOutService(db);
        var checkout = await svc.CheckOutAsync(checkIn.ID, new CheckoutRoom());
        var fetched = await svc.GetCheckoutAsync(checkout.ID);
        Assert.NotNull(fetched);
        Assert.Equal(checkout.BillNo, fetched!.BillNo);
        Assert.Null(await svc.GetCheckoutAsync(987654));
    }
}
