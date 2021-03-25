using System;
using Microsoft.Extensions.DependencyInjection;
using PaymentGateway.Acquirer.Api;

namespace PaymentGateway.Acquirer.InMemory
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection UsePaymentAuthoriser<TAuthoriser>(this IServiceCollection services) where TAuthoriser: class, IPaymentAuthoriser
        {
            services.AddSingleton<IPaymentAuthoriser, TAuthoriser>();

            return services;
        }
    }
}
