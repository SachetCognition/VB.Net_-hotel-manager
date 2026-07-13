using HotelManager.Domain.Entities;
using HotelManager.Web.Services;

namespace HotelManager.IntegrationTests;

public class TransactionServiceTests : IClassFixture<SqliteDbFixture>
{
    private readonly TransactionService _service;

    public TransactionServiceTests(SqliteDbFixture fixture) =>
        _service = new TransactionService(new FixtureDbContextFactory(fixture));

    [Fact]
    public async Task Save_ComputesDueAmount_AndMonth()
    {
        var saved = await _service.SaveAsync(new Trans
        {
            Employee_Party_Name = "Vendor A",
            TransactionType = "Credit",
            TransactionDate = new DateTime(2026, 3, 15),
            TransactionAmount = 1000,
            AmountReceived = 400
        });

        Assert.Equal(600, saved.DueAmount);
        Assert.Equal("March", saved.TransactionMonth);
        Assert.True(saved.ID > 0);
    }

    [Fact]
    public async Task Save_Update_ChangesExistingRow()
    {
        var saved = await _service.SaveAsync(new Trans
        {
            Employee_Party_Name = "Vendor B",
            TransactionAmount = 500,
            AmountReceived = 500
        });

        saved.AmountReceived = 200;
        var updated = await _service.SaveAsync(saved);

        Assert.Equal(saved.ID, updated.ID);
        var loaded = await _service.GetAsync(saved.ID);
        Assert.Equal(300, loaded!.DueAmount);
    }

    [Fact]
    public async Task GetAll_SearchFiltersByPartyTypeOrDetails()
    {
        await _service.SaveAsync(new Trans
        {
            Employee_Party_Name = "SearchTarget",
            TransactionType = "Debit",
            TransactionDetails = "Laundry supplies"
        });

        Assert.NotEmpty(await _service.GetAllAsync("SearchTarget"));
        Assert.NotEmpty(await _service.GetAllAsync("Laundry"));
        Assert.Empty(await _service.GetAllAsync("NoSuchParty"));
    }

    [Fact]
    public async Task Delete_RemovesTransaction()
    {
        var saved = await _service.SaveAsync(new Trans { Employee_Party_Name = "ToDelete" });
        await _service.DeleteAsync(saved.ID);
        Assert.Null(await _service.GetAsync(saved.ID));
    }
}
