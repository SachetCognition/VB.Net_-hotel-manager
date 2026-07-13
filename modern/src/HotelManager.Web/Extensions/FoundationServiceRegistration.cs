using HotelManager.Application.Interfaces;
using HotelManager.Infrastructure;
using HotelManager.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.Web.Extensions;

public static class FoundationServiceRegistration
{
    public static IServiceCollection AddFoundationServices(this IServiceCollection services)
    {
        services.AddScoped<HotelDbContext>(sp =>
            sp.GetRequiredService<IDbContextFactory<HotelDbContext>>().CreateDbContext());
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}
