using HotelManager.Application.Billing;
using HotelManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.IntegrationTests.Reservations;

public class CheckOutTests(ReservationsFixture fixture) : IClassFixture<ReservationsFixture>
{
    private static readonly DateTime D0 = new(2026, 10, 1);

    private async Task<CheckInRoom> CheckInGuestAsync(string roomNo)
    {
        using var db = fixture.CreateContext();
        var svc = fixture.CreateCheckInService(db);
        return await svc.CheckInAsync(new CheckInRoom
        {
            GuestID = "G-1",
            RoomNo = roomNo,
            RoomCharges = 1000,
            DateIN = D0,
            DateOUT = D0.AddDays(2),
            ServiceTaxPer = 12.36,
            LuxuryTaxPer = 5,
        });
    }

    [Fact]
    public async Task Check_out_creates_bill_and_flips_status()
    {
        var checkIn = await CheckInGuestAsync("101");
        using var db = fixture.CreateContext();
        var svc = fixture.CreateCheckOutService(db);
        var checkout = await svc.CheckOutAsync(checkIn.ID, new CheckoutRoom { Notes = "bye" });

        Assert.StartsWith("B", checkout.BillNo);
        Assert.Equal(checkIn.ID, checkout.CheckInID);
        Assert.NotNull(checkout.CheckOutDate);
        var storedCheckIn = await db.CheckIns.AsNoTracking().SingleAsync(c => c.ID == checkIn.ID);
        Assert.Equal("Checked Out", storedCheckIn.Status);
    }

    [Fact]
    public async Task Double_checkout_is_prevented()
    {
        var checkIn = await CheckInGuestAsync("102");
        using var db = fixture.CreateContext();
        var svc = fixture.CreateCheckOutService(db);
        await svc.CheckOutAsync(checkIn.ID, new CheckoutRoom());

        using var db2 = fixture.CreateContext();
        var svc2 = fixture.CreateCheckOutService(db2);
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc2.CheckOutAsync(checkIn.ID, new CheckoutRoom()));
        Assert.Equal("Guest has already been checked out", ex.Message);
    }

    [Fact]
    public async Task Checkout_of_unknown_check_in_throws()
    {
        using var db = fixture.CreateContext();
        var svc = fixture.CreateCheckOutService(db);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.CheckOutAsync(987654, new CheckoutRoom()));
    }

    [Fact]
    public async Task Tax_room_rows_are_persisted_with_the_bill()
    {
        var checkIn = await CheckInGuestAsync("103");
        var bill = BillingCalculator.ComputeStay(
            checkIn.DateIN!.Value, checkIn.DateOUT!.Value, 1000, 0, 0, 12.36, 5, 0);

        using var db = fixture.CreateContext();
        var svc = fixture.CreateCheckOutService(db);
        var checkout = await svc.CheckOutAsync(checkIn.ID, new CheckoutRoom(), new TaxRoom
        {
            EducationalTax = bill.EducessTax,
            EducationalTaxAmount = bill.EducessTaxAmount,
            HEduTax = bill.HEduCessTax,
            HEduTaxAmount = bill.HEduCessTaxAmount,
        });

        var tax = await db.TaxRooms.AsNoTracking().SingleAsync(t => t.BillID == checkout.ID);
        Assert.Equal(bill.EducessTax, tax.EducationalTax);
        Assert.Equal(bill.EducessTaxAmount, tax.EducationalTaxAmount);
        Assert.Equal(bill.HEduCessTax, tax.HEduTax);
        Assert.Equal(bill.HEduCessTaxAmount, tax.HEduTaxAmount);
    }

    [Fact]
    public async Task Bill_numbers_are_sequential()
    {
        using var db = fixture.CreateContext();
        var svc = fixture.CreateCheckOutService(db);
        var maxId = await db.Checkouts.MaxAsync(c => (int?)c.ID) ?? 0;
        Assert.Equal("B" + (maxId + 1), await svc.GenerateBillNoAsync());
    }

    [Fact]
    public async Task Search_finds_checkouts_by_bill_no()
    {
        var checkIn = await CheckInGuestAsync("104");
        using var db = fixture.CreateContext();
        var svc = fixture.CreateCheckOutService(db);
        var checkout = await svc.CheckOutAsync(checkIn.ID, new CheckoutRoom());
        var found = await svc.GetCheckoutsAsync(checkout.BillNo);
        Assert.Contains(found, c => c.ID == checkout.ID);
    }
}
