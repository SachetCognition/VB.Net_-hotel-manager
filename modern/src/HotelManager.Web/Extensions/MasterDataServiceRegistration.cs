using HotelManager.Application.Interfaces;
using HotelManager.Infrastructure.Services;

namespace HotelManager.Web.Extensions;

/// <summary>Owned by Child A (master data).</summary>
public static class MasterDataServiceRegistration
{
    public static IServiceCollection AddMasterDataServices(this IServiceCollection services)
    {
        services.AddScoped<IHotelInfoService, HotelInfoService>();
        services.AddScoped<ICurrencyService, CurrencyService>();
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<IGuestService, GuestService>();
        return services;
    }
}
