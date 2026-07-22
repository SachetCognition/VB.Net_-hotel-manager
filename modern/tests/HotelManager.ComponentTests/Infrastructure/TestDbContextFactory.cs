using HotelManager.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.ComponentTests.Infrastructure;

/// <summary>
/// A simple <see cref="IDbContextFactory{TContext}"/> that hands out fresh
/// <see cref="HotelDbContext"/> instances bound to the shared test options.
/// </summary>
public sealed class TestDbContextFactory : IDbContextFactory<HotelDbContext>
{
    private readonly DbContextOptions<HotelDbContext> _options;

    public TestDbContextFactory(DbContextOptions<HotelDbContext> options) => _options = options;

    public HotelDbContext CreateDbContext() => new(_options);
}
