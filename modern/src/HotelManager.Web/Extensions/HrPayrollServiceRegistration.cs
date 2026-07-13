using HotelManager.Application.Interfaces;
using HotelManager.Web.Services.Hr;

namespace HotelManager.Web.Extensions;

/// <summary>Owned by Child E (HR &amp; payroll). Register IHrPayrollService here.</summary>
public static class HrPayrollServiceRegistration
{
    public static IServiceCollection AddHrPayrollServices(this IServiceCollection services)
    {
        services.AddScoped<IHrPayrollService, HrPayrollService>();
        return services;
    }
}
