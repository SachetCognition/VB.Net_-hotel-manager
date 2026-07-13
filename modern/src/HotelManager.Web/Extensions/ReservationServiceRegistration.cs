using HotelManager.Application.Interfaces;
using HotelManager.Web.Services.Reservations;

namespace HotelManager.Web.Extensions;

/// <summary>Owned by Child B (reservations/check-in/check-out). Register IReservationService, ICheckInService, ICheckOutService here.</summary>
public static class ReservationServiceRegistration
{
    public static IServiceCollection AddReservationServices(this IServiceCollection services)
    {
        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<ICheckInService, CheckInService>();
        services.AddScoped<ICheckOutService, CheckOutService>();
        return services;
    }
}
