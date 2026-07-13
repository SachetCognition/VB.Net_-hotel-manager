using HotelManager.Application.Interfaces;
using HotelManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.Infrastructure.Services;

public class CurrencyService : ICurrencyService
{
    private readonly HotelDbContext _db;

    public CurrencyService(HotelDbContext db) => _db = db;

    public async Task<IReadOnlyList<CurrencySet>> GetAllAsync() =>
        await _db.Currencies.AsNoTracking().OrderBy(c => c.CS_Currency).ToListAsync();

    public async Task<CurrencySet?> GetAsync(int id) =>
        await _db.Currencies.AsNoTracking().FirstOrDefaultAsync(c => c.ID == id);

    public async Task<CurrencySet> CreateAsync(CurrencySet currency)
    {
        if (string.IsNullOrWhiteSpace(currency.CS_Currency))
            throw new ArgumentException("Please enter currency", nameof(currency));
        if (await _db.Currencies.AnyAsync(c => c.CS_Currency == currency.CS_Currency))
            throw new InvalidOperationException("Currency Name Already Exists");
        _db.Currencies.Add(currency);
        await _db.SaveChangesAsync();
        return currency;
    }

    public async Task UpdateAsync(CurrencySet currency)
    {
        if (string.IsNullOrWhiteSpace(currency.CS_Currency))
            throw new ArgumentException("Please enter currency", nameof(currency));
        var existing = await _db.Currencies.FirstAsync(c => c.ID == currency.ID);
        existing.CS_Currency = currency.CS_Currency;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var existing = await _db.Currencies.FirstOrDefaultAsync(c => c.ID == id)
            ?? throw new InvalidOperationException("No record found");
        _db.Currencies.Remove(existing);
        await _db.SaveChangesAsync();
    }
}
