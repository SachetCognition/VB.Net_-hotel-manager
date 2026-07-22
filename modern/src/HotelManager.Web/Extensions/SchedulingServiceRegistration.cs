namespace HotelManager.Web.Extensions;

/// <summary>Owned by Child C (scheduling). Register IScheduleService here.</summary>
public static class SchedulingServiceRegistration
{
    public static IServiceCollection AddSchedulingServices(this IServiceCollection services)
    {
        services.AddScoped<HotelManager.Application.Interfaces.IScheduleService, Services.Scheduling.ScheduleService>();
        return services;
    }
}
