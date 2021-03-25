using System.Threading.Tasks;
using PaymentGateway.Acquirer.Api;
using PaymentGateway.Domain;

namespace PaymentGateway.Acquirer.InMemory
{
    public class AlwaysApprovesPaymentAuthoriser : IPaymentAuthoriser
    {
        public Task<PaymentResult> Authorise(AuthoriseRequest payment) => Task.FromResult(PaymentResult.Succeeded);
    }
}
