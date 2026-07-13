using HotelManager.Application.Interfaces;
using HotelManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.Infrastructure.Services;

public class HotelInfoService : IHotelInfoService
{
    private readonly HotelDbContext _db;

    public HotelInfoService(HotelDbContext db) => _db = db;

    public Task<HotelInfo?> GetAsync() =>
        _db.HotelInfos.AsNoTracking().FirstOrDefaultAsync();

    public async Task SaveAsync(HotelInfo info)
    {
        if (string.IsNullOrWhiteSpace(info.HotelName))
            throw new ArgumentException("Please enter hotel name", nameof(info));
        if (string.IsNullOrWhiteSpace(info.Address))
            throw new ArgumentException("Please enter address", nameof(info));
        if (string.IsNullOrWhiteSpace(info.ContactNo))
            throw new ArgumentException("Please enter contact no.", nameof(info));

        var existing = await _db.HotelInfos.FirstOrDefaultAsync();
        if (existing is null)
        {
            _db.HotelInfos.Add(info);
        }
        else
        {
            existing.HotelName = info.HotelName;
            existing.Address = info.Address;
            existing.ContactNo = info.ContactNo;
            existing.ContactNo1 = info.ContactNo1;
            existing.Email = info.Email;
            existing.TIN = info.TIN;
            existing.STNo = info.STNo;
            existing.Logo = info.Logo;
        }
        await _db.SaveChangesAsync();
    }
}
