using HotelManager.Domain.Entities;
using HotelManager.Web.Services.Reservations;

namespace HotelManager.IntegrationTests.Reservations;

public class ReservationAvailabilityTests(ReservationsFixture fixture) : IClassFixture<ReservationsFixture>
{
    private static readonly DateTime D0 = new(2026, 8, 1);

    [Fact]
    public async Task Room_with_no_bookings_is_available()
    {
        using var db = fixture.CreateContext();
        var svc = fixture.CreateReservationService(db);
        Assert.True(await svc.IsRoomAvailableAsync("101", D0, D0.AddDays(2)));
    }

    [Fact]
    public async Task Temp_reservation_overlap_blocks_room()
    {
        using var db = fixture.CreateContext();
        var svc = fixture.CreateReservationService(db);
        await svc.SaveTempReservationAsync(new TempReservation
        {
            ReservationID = "R-10001",
            GuestID = "G-1",
            RoomNo = "102",
            DateIN = D0,
            DateOUT = D0.AddDays(3),
        });

        // fully inside
        Assert.False(await svc.IsRoomAvailableAsync("102", D0.AddDays(1), D0.AddDays(2)));
        // straddles start
        Assert.False(await svc.IsRoomAvailableAsync("102", D0.AddDays(-1), D0.AddDays(1)));
        // straddles end
        Assert.False(await svc.IsRoomAvailableAsync("102", D0.AddDays(2), D0.AddDays(5)));
        // fully covering
        Assert.False(await svc.IsRoomAvailableAsync("102", D0.AddDays(-1), D0.AddDays(5)));
        // touching boundaries only (DateIn < out AND DateOut > in is legacy predicate)
        Assert.True(await svc.IsRoomAvailableAsync("102", D0.AddDays(3), D0.AddDays(5)));
        Assert.True(await svc.IsRoomAvailableAsync("102", D0.AddDays(-2), D0));
        // other room unaffected
        Assert.True(await svc.IsRoomAvailableAsync("103", D0, D0.AddDays(3)));
    }

    [Fact]
    public async Task Checked_in_room_is_unavailable_until_checkout()
    {
        using var db = fixture.CreateContext();
        var svc = fixture.CreateReservationService(db);
        var checkInSvc = fixture.CreateCheckInService(db);
        await checkInSvc.CheckInAsync(new CheckInRoom
        {
            GuestID = "G-1",
            RoomNo = "104",
            RoomCharges = 1000,
            DateIN = D0,
            DateOUT = D0.AddDays(2),
        });
        Assert.False(await svc.IsRoomAvailableAsync("104", D0, D0.AddDays(1)));
    }

    [Fact]
    public async Task Cancelled_reservation_frees_the_room()
    {
        using var db = fixture.CreateContext();
        var svc = fixture.CreateReservationService(db);
        await svc.CreateReservationAsync(new Reservation
        {
            ReservationID = "R-20001",
            GuestID = "G-1",
            RoomNo = "105",
            DateIN = D0,
            DateOUT = D0.AddDays(2),
        });
        Assert.False(await svc.IsRoomAvailableAsync("105", D0, D0.AddDays(1)));
        await svc.CancelReservationAsync("R-20001");
        Assert.True(await svc.IsRoomAvailableAsync("105", D0, D0.AddDays(1)));
    }

    [Fact]
    public async Task Create_reservation_on_unavailable_room_throws()
    {
        using var db = fixture.CreateContext();
        var svc = fixture.CreateReservationService(db);
        await svc.CreateReservationAsync(new Reservation
        {
            ReservationID = "R-20002",
            GuestID = "G-1",
            RoomNo = "106",
            DateIN = D0,
            DateOUT = D0.AddDays(2),
        });
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.CreateReservationAsync(new Reservation
            {
                ReservationID = "R-20003",
                GuestID = "G-2",
                RoomNo = "106",
                DateIN = D0.AddDays(1),
                DateOUT = D0.AddDays(3),
            }));
    }

    [Fact]
    public async Task Update_reservation_excludes_itself_from_overlap_check()
    {
        using var db = fixture.CreateContext();
        var svc = fixture.CreateReservationService(db);
        var reservation = new Reservation
        {
            ReservationID = "R-20004",
            GuestID = "G-1",
            RoomNo = "107",
            DateIN = D0,
            DateOUT = D0.AddDays(2),
        };
        await svc.CreateReservationAsync(reservation);
        reservation.DateOUT = D0.AddDays(3);
        await svc.UpdateReservationAsync(reservation);
        var updated = await svc.GetReservationAsync("R-20004");
        Assert.Equal(D0.AddDays(3), updated!.DateOUT);
    }

    [Fact]
    public async Task Availability_grid_reports_all_rooms()
    {
        using var db = fixture.CreateContext();
        var svc = fixture.CreateReservationService(db);
        await svc.SaveTempReservationAsync(new TempReservation
        {
            ReservationID = "R-10002",
            GuestID = "G-2",
            RoomNo = "108",
            DateIN = D0,
            DateOUT = D0.AddDays(2),
        });
        var grid = await svc.GetRoomAvailabilityAsync(D0, D0.AddDays(1));
        Assert.Equal(10, grid.Count);
        Assert.False(grid.Single(r => r.RoomNo == "108").IsAvailable);
        Assert.True(grid.Single(r => r.RoomNo == "110").IsAvailable);
    }

    [Fact]
    public async Task Confirm_temp_reservation_moves_it_to_confirmed()
    {
        using var db = fixture.CreateContext();
        var svc = fixture.CreateReservationService(db);
        await svc.SaveTempReservationAsync(new TempReservation
        {
            ReservationID = "R-10003",
            GuestID = "G-1",
            RoomNo = "109",
            DateIN = D0,
            DateOUT = D0.AddDays(2),
        });
        await svc.ConfirmTempReservationAsync("R-10003");
        Assert.Empty(db.TempReservations.Where(t => t.ReservationID == "R-10003"));
        var confirmed = await svc.GetReservationAsync("R-10003");
        Assert.Equal(ReservationService.StatusConfirmed, confirmed!.Status);
        Assert.Equal("109", confirmed.RoomNo);
    }

    [Fact]
    public async Task Generated_reservation_ids_are_unique_and_well_formed()
    {
        using var db = fixture.CreateContext();
        var svc = fixture.CreateReservationService(db);
        var id1 = await svc.GenerateReservationIdAsync();
        var id2 = await svc.GenerateReservationIdAsync();
        Assert.Matches("^R-[1-9]{5}$", id1);
        Assert.Matches("^R-[1-9]{5}$", id2);
    }
}
