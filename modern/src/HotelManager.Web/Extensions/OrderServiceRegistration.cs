using HotelManager.Application.Interfaces;
using HotelManager.Web.Services;

namespace HotelManager.Web.Extensions;

/// <summary>Owned by Child C (orders/invoices/transactions). Register IRestaurantOrderService, ITransactionService here.</summary>
public static class OrderServiceRegistration
{
    public static IServiceCollection AddOrderServices(this IServiceCollection services)
    {
        services.AddScoped<IRestaurantOrderService, OrderService>();
        services.AddScoped<ITransactionService, TransactionService>();
        return services;
    }
}
