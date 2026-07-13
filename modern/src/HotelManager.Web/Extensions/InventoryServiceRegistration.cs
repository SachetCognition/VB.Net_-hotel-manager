namespace HotelManager.Web.Extensions;

/// <summary>Owned by Child D (inventory &amp; stock). Register IInventoryService here.</summary>
public static class InventoryServiceRegistration
{
    public static IServiceCollection AddInventoryServices(this IServiceCollection services)
    {
        services.AddScoped<HotelManager.Application.Interfaces.IInventoryService, Services.Inventory.InventoryService>();
        return services;
    }
}
