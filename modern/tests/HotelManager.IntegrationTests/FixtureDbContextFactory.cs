using HotelManager.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.IntegrationTests;

/// <summary>IDbContextFactory adapter over SqliteDbFixture for services that create their own contexts.</summary>
public class FixtureDbContextFactory : IDbContextFactory<HotelDbContext>
{
    private readonly SqliteDbFixture _fixture;

    public FixtureDbContextFactory(SqliteDbFixture fixture) => _fixture = fixture;

    public HotelDbContext CreateDbContext() => _fixture.CreateContext();
}
