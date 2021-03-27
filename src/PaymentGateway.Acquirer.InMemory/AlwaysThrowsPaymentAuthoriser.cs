using System;
using System.Threading.Tasks;
using PaymentGateway.Acquirer.Api;

namespace PaymentGateway.Acquirer.InMemory
{
    public class AlwaysThrowsPaymentAuthoriser : IPaymentAuthoriser
    {
        public Task<AuthoriseResult> Authorise(AuthoriseRequest request) => Task.FromException<AuthoriseResult>(new Exception("ERROR AUTHORISING PAYMENT"));
    }
}
