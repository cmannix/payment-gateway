using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PaymentGateway.Acquirer.Api;

namespace PaymentGateway.Acquirer.InMemory
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection UsePaymentAuthoriser(this IServiceCollection services, IConfiguration configuration)
        {
            var acquirerConfig = configuration.GetSection(InMemoryAcquirerOptions.Acquirer).Get<InMemoryAcquirerOptions>();
            IPaymentAuthoriser paymentAuthoriser = acquirerConfig.AuthoriseBehaviour switch
            {
                AuthoriseBehaviour.Approve => new AlwaysApprovesPaymentAuthoriser(),
                AuthoriseBehaviour.Deny => new AlwaysDeniesPaymentAuthoriser(),
                AuthoriseBehaviour.Error => new AlwaysThrowsPaymentAuthoriser(),
                _ => throw new OptionsValidationException(InMemoryAcquirerOptions.Acquirer, typeof(InMemoryAcquirerOptions), new[] { $"Unknown InMemoryAcquirer behaviour option '{acquirerConfig.AuthoriseBehaviour}'" })
            };

            services.AddSingleton(paymentAuthoriser);

            return services;
        }
    }
}
