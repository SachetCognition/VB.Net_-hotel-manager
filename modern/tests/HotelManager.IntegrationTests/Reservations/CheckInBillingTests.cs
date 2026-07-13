using HotelManager.Application.Billing;
using HotelManager.Domain.Entities;

namespace HotelManager.IntegrationTests.Reservations;

public class CheckInBillingTests(ReservationsFixture fixture) : IClassFixture<ReservationsFixture>
{
    private static readonly DateTime D0 = new(2026, 9, 1);

    [Fact]
    public async Task Check_in_totals_match_billing_calculator()
    {
        using var db = fixture.CreateContext();
        var svc = fixture.CreateCheckInService(db);
        var saved = await svc.CheckInAsync(new CheckInRoom
        {
            GuestID = "G-1",
            RoomNo = "101",
            RoomCharges = 1000,
            DateIN = D0,
            DateOUT = D0.AddDays(3),
            OtherCharges = 200,
            DiscountPer = 10,
            ServiceTaxPer = 12.36,
            LuxuryTaxPer = 5,
            TotalPaid = 1000,
        });

        var expected = BillingCalculator.ComputeStay(D0, D0.AddDays(3), 1000, 200, 10, 12.36, 5, 1000);
        Assert.Equal(expected.NoOfDays, saved.NoOfDays);
        Assert.Equal(expected.TotalCharges, saved.TotalRoomCharges);
        Assert.Equal(expected.Discount, saved.Discount);
        Assert.Equal(expected.SubTotal, saved.SubTotal);
        Assert.Equal(expected.ServiceTaxAmount, saved.ServiceTaxAmount);
        Assert.Equal(expected.LuxuryTaxAmount, saved.LuxuryTaxAmount);
        Assert.Equal(expected.GrandTotal, saved.GrandTotal);
        Assert.Equal(expected.Balance, saved.Balance);
        Assert.Equal("Checked In", saved.Status);
    }

    [Fact]
    public async Task Same_day_stay_counts_as_one_day()
    {
        using var db = fixture.CreateContext();
        var svc = fixture.CreateCheckInService(db);
        var saved = await svc.CheckInAsync(new CheckInRoom
        {
            GuestID = "G-1",
            RoomNo = "102",
            RoomCharges = 1000,
            DateIN = D0,
            DateOUT = D0,
        });
        Assert.Equal(1, saved.NoOfDays);
        Assert.Equal(1000, saved.TotalRoomCharges);
    }

    [Fact]
    public async Task Extra_bed_adds_charges_to_other_charges()
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
            OtherCharges = 100,
            ExtraBed = "Yes",
        });
        // Seeded extra bed charge is 250.
        Assert.Equal(350, saved.OtherCharges);
        var expected = BillingCalculator.ComputeStay(D0, D0.AddDays(1), 1000, 350, 0, 0, 0, 0);
        Assert.Equal(expected.GrandTotal, saved.GrandTotal);
    }

    [Fact]
    public async Task Total_paid_above_grand_total_is_rejected()
    {
        using var db = fixture.CreateContext();
        var svc = fixture.CreateCheckInService(db);
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.CheckInAsync(new CheckInRoom
            {
                GuestID = "G-1",
                RoomNo = "104",
                RoomCharges = 1000,
                DateIN = D0,
                DateOUT = D0.AddDays(1),
                TotalPaid = 99999,
            }));
        Assert.Equal("Total paid can not be more than grand total", ex.Message);
    }

    [Fact]
    public async Task Check_in_requires_guest_room_and_valid_dates()
    {
        using var db = fixture.CreateContext();
        var svc = fixture.CreateCheckInService(db);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.CheckInAsync(new CheckInRoom { RoomNo = "105", DateIN = D0, DateOUT = D0.AddDays(1) }));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.CheckInAsync(new CheckInRoom { GuestID = "G-1", DateIN = D0, DateOUT = D0.AddDays(1) }));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.CheckInAsync(new CheckInRoom { GuestID = "G-1", RoomNo = "105", DateIN = D0.AddDays(2), DateOUT = D0 }));
    }

    [Fact]
    public async Task Update_check_in_recomputes_totals()
    {
        using var db = fixture.CreateContext();
        var svc = fixture.CreateCheckInService(db);
        var saved = await svc.CheckInAsync(new CheckInRoom
        {
            GuestID = "G-2",
            RoomNo = "106",
            RoomCharges = 1000,
            DateIN = D0,
            DateOUT = D0.AddDays(1),
        });
        saved.DateOUT = D0.AddDays(4);
        await svc.UpdateCheckInAsync(saved);
        var updated = await svc.GetCheckInAsync(saved.ID);
        Assert.Equal(4, updated!.NoOfDays);
        Assert.Equal(4000, updated.TotalRoomCharges);
    }
}
