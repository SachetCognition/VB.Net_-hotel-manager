using HotelManager.Application.Interfaces;
using HotelManager.Domain.Entities;
using HotelManager.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.Web.Services;

/// <summary>General ledger entries from the legacy Trans table.</summary>
public class TransactionService : ITransactionService
{
    private readonly IDbContextFactory<HotelDbContext> _factory;

    public TransactionService(IDbContextFactory<HotelDbContext> factory) => _factory = factory;

    public async Task<IReadOnlyList<Trans>> GetAllAsync(string? search = null)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var query = db.Transactions.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(t =>
                t.Employee_Party_Name!.Contains(search) ||
                t.TransactionType!.Contains(search) ||
                t.TransactionDetails!.Contains(search));
        return await query.OrderByDescending(t => t.TransactionDate).ToListAsync();
    }

    public async Task<Trans?> GetAsync(int id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Transactions.AsNoTracking().FirstOrDefaultAsync(t => t.ID == id);
    }

    public async Task<Trans> SaveAsync(Trans transaction)
    {
        transaction.DueAmount = (transaction.TransactionAmount ?? 0) - (transaction.AmountReceived ?? 0);
        transaction.TransactionMonth ??= transaction.TransactionDate?.ToString("MMMM");
        await using var db = await _factory.CreateDbContextAsync();
        if (transaction.ID == 0)
            db.Transactions.Add(transaction);
        else
            db.Transactions.Update(transaction);
        await db.SaveChangesAsync();
        return transaction;
    }

    public async Task DeleteAsync(int id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var transaction = await db.Transactions.FindAsync(id);
        if (transaction is not null)
        {
            db.Transactions.Remove(transaction);
            await db.SaveChangesAsync();
        }
    }
}
