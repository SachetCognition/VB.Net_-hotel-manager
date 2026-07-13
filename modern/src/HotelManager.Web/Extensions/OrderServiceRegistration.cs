namespace HotelManager.Web.Extensions;

/// <summary>Owned by Child C (orders/invoices/transactions). Register IRestaurantOrderService, ITransactionService here.</summary>
public static class OrderServiceRegistration
{
    public static IServiceCollection AddOrderServices(this IServiceCollection services)
    {
        return services;
    }
}
