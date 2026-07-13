using HotelManager.Application.Billing;
using HotelManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.IntegrationTests.Reservations;

public class HallGardenReservationTests(ReservationsFixture fixture) : IClassFixture<ReservationsFixture>
{
    private static readonly DateTime D0 = new(2026, 11, 1);

    [Fact]
    public async Task Hall_and_garden_totals_match_billing_calculator()
    {
        using var db = fixture.CreateContext();
        var svc = fixture.CreateReservationService(db);
        var reservation = new ReservationHallAndGarden
        {
            ID = "R-30001",
            GuestID = "G-1",
            Hall = "Hall",
            DateFrom_Hall = D0,
            DateTo_Hall = D0.AddDays(2),
            Rate_Hall = 5000,
            Garden = "Garden",
            DateFrom_Garden = D0,
            DateTo_Garden = D0.AddDays(3),
            Rate_Garden = 3000,
            OtherCharges = 500,
            DiscountPer = 5,
            ServiceTaxPer = 12.36,
            LuxuryTaxPer = 5,
            TotalPaid = 2000,
        };
        await svc.SaveHallAndGardenReservationAsync(reservation);

        var expected = BillingCalculator.ComputeHallAndGarden(
            D0, D0.AddDays(2), 5000, D0, D0.AddDays(3), 3000, 500, 5, 12.36, 5, 2000);
        Assert.Equal(expected.DaysHall, reservation.Days_Hall);
        Assert.Equal(expected.TotalChargesHall, reservation.TotalCharges_Hall);
        Assert.Equal(expected.DaysGarden, reservation.Days_Garden);
        Assert.Equal(expected.TotalChargesGarden, reservation.TotalCharges_Garden);
        Assert.Equal(expected.Bill.SubTotal, reservation.SubTotal);
        Assert.Equal(expected.Bill.GrandTotal, reservation.GrandTotal);
        Assert.Equal(expected.Bill.Balance, reservation.Balance);

        var tax = await db.TaxReservationsHallAndGarden.AsNoTracking()
            .SingleAsync(t => t.ReservationID == "R-30001");
        Assert.Equal(expected.Bill.EducessTax, tax.EducationalTax);
        Assert.Equal(expected.Bill.EducessTaxAmount, tax.EducationalTaxAmount);
        Assert.Equal(expected.Bill.HEduCessTax, tax.HEduTax);
        Assert.Equal(expected.Bill.HEduCessTaxAmount, tax.HEduTaxAmount);
    }

    [Fact]
    public async Task Hall_or_garden_totals_match_billing_calculator()
    {
        using var db = fixture.CreateContext();
        var svc = fixture.CreateReservationService(db);
        var reservation = new ReservationHallOrGarden
        {
            ID = "R-30002",
            GuestID = "G-2",
            Type = "Hall",
            DateFrom = D0,
            DateTo = D0.AddDays(2),
            Rate = 5000,
            OtherCharges = 300,
            DiscountPer = 10,
            ServiceTaxPer = 12.36,
            LuxuryTaxPer = 5,
            TotalPaid = 1000,
        };
        await svc.SaveHallOrGardenReservationAsync(reservation);

        var expected = BillingCalculator.ComputeHallOrGarden(
            D0, D0.AddDays(2), 5000, 300, 10, 12.36, 5, 1000);
        Assert.Equal(expected.NoOfDays, reservation.Days);
        Assert.Equal(expected.TotalCharges, reservation.TotalCharges);
        Assert.Equal(expected.SubTotal, reservation.SubTotal);
        Assert.Equal(expected.GrandTotal, reservation.GrandTotal);
        Assert.Equal(expected.Balance, reservation.Balance);

        var tax = await db.TaxReservationsHallOrGarden.AsNoTracking()
            .SingleAsync(t => t.ReservationID == "R-30002");
        Assert.Equal(expected.EducessTaxAmount, tax.EducationalTaxAmount);
        Assert.Equal(expected.HEduCessTaxAmount, tax.HEduTaxAmount);
    }

    [Fact]
    public async Task Saving_existing_hall_or_garden_reservation_updates_in_place()
    {
        using var db = fixture.CreateContext();
        var svc = fixture.CreateReservationService(db);
        var reservation = new ReservationHallOrGarden
        {
            ID = "R-30003",
            GuestID = "G-1",
            Type = "Garden",
            DateFrom = D0,
            DateTo = D0.AddDays(1),
            Rate = 3000,
        };
        await svc.SaveHallOrGardenReservationAsync(reservation);

        using var db2 = fixture.CreateContext();
        var svc2 = fixture.CreateReservationService(db2);
        await svc2.SaveHallOrGardenReservationAsync(new ReservationHallOrGarden
        {
            ID = "R-30003",
            GuestID = "G-1",
            Type = "Garden",
            DateFrom = D0,
            DateTo = D0.AddDays(3),
            Rate = 3000,
        });

        Assert.Equal(1, await db2.ReservationsHallOrGarden.CountAsync(r => r.ID == "R-30003"));
        Assert.Equal(1, await db2.TaxReservationsHallOrGarden.CountAsync(t => t.ReservationID == "R-30003"));
        var updated = await db2.ReservationsHallOrGarden.AsNoTracking().SingleAsync(r => r.ID == "R-30003");
        Assert.Equal(3, updated.Days);
        Assert.Equal(9000, updated.TotalCharges);
    }

    [Fact]
    public async Task Delete_removes_reservation_and_tax_rows()
    {
        using var db = fixture.CreateContext();
        var svc = fixture.CreateReservationService(db);
        await svc.SaveHallAndGardenReservationAsync(new ReservationHallAndGarden
        {
            ID = "R-30004",
            GuestID = "G-1",
            DateFrom_Hall = D0,
            DateTo_Hall = D0.AddDays(1),
            Rate_Hall = 5000,
            DateFrom_Garden = D0,
            DateTo_Garden = D0.AddDays(1),
            Rate_Garden = 3000,
        });
        await svc.DeleteHallAndGardenReservationAsync("R-30004");
        Assert.Equal(0, await db.ReservationsHallAndGarden.CountAsync(r => r.ID == "R-30004"));
        Assert.Equal(0, await db.TaxReservationsHallAndGarden.CountAsync(t => t.ReservationID == "R-30004"));
    }
}
