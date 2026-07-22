using HotelManager.Web.Services;

namespace HotelManager.Web.Extensions;

/// <summary>Owned by Workstream B (UI). Registers the Web-layer dashboard aggregation service.</summary>
public static class DashboardServiceRegistration
{
    public static IServiceCollection AddDashboardServices(this IServiceCollection services)
    {
        services.AddScoped<IDashboardService, DashboardService>();
        return services;
    }
}
