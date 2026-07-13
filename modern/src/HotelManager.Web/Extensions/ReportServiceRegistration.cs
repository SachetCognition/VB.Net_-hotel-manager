using HotelManager.Application.Interfaces;
using HotelManager.Web.Services.Reports;

namespace HotelManager.Web.Extensions;

/// <summary>Owned by Child F (reports). Register IReportService here.</summary>
public static class ReportServiceRegistration
{
    public static IServiceCollection AddReportServices(this IServiceCollection services)
    {
        services.AddScoped<ReportDataService>();
        services.AddScoped<IReportService, ReportService>();
        return services;
    }
}
