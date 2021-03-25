using System;
using System.Threading.Tasks;
using PaymentGateway.Acquirer.Api;
using PaymentGateway.Domain;

namespace PaymentGateway.Acquirer.InMemory
{
    public class AlwaysThrowsPaymentAuthoriser : IPaymentAuthoriser
    {
        public Task<PaymentResult> Authorise(AuthoriseRequest payment) => Task.FromException<PaymentResult>(new Exception("ERROR AUTHORISING PAYMENT"));
    }
}
