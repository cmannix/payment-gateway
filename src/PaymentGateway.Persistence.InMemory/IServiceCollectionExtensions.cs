using Microsoft.Extensions.DependencyInjection;
using PaymentGateway.Persistence.Api;

namespace PaymentGateway.Persistence.InMemory
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection UseInMemoryPaymentStore(this IServiceCollection services)
        {
            services.AddSingleton<IPaymentRepository, InMemoryPaymentRepository>();

            return services;
        }
    }
}
