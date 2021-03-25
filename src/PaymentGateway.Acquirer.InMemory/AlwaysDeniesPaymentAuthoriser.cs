using System.Threading.Tasks;
using PaymentGateway.Acquirer.Api;
using PaymentGateway.Domain;

namespace PaymentGateway.Acquirer.InMemory
{
    public class AlwaysDeniesPaymentAuthoriser : IPaymentAuthoriser
    {
        public Task<PaymentResult> Authorise(AuthoriseRequest payment) => Task.FromResult(PaymentResult.Failed);
    }
}
