namespace HotelManager.Web.Extensions;

/// <summary>Owned by Child B (reservations/check-in/check-out). Register IReservationService, ICheckInService, ICheckOutService here.</summary>
public static class ReservationServiceRegistration
{
    public static IServiceCollection AddReservationServices(this IServiceCollection services)
    {
        return services;
    }
}
