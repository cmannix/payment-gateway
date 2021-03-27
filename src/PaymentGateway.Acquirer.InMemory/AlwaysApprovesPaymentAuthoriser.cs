using System.Threading.Tasks;
using PaymentGateway.Acquirer.Api;

namespace PaymentGateway.Acquirer.InMemory
{
    public class AlwaysApprovesPaymentAuthoriser : IPaymentAuthoriser
    {
        public Task<AuthoriseResult> Authorise(AuthoriseRequest request) => Task.FromResult(AuthoriseResult.Approved);
    }
}
