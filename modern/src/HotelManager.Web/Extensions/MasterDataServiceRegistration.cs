namespace HotelManager.Web.Extensions;

/// <summary>Owned by Child A (master data). Register IHotelInfoService, ICurrencyService, IRoomService, IGuestService here.</summary>
public static class MasterDataServiceRegistration
{
    public static IServiceCollection AddMasterDataServices(this IServiceCollection services)
    {
        return services;
    }
}
