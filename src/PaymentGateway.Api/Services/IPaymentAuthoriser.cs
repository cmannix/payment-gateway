using System;
using System.Threading.Tasks;
using PaymentGateway.Api.Models;

namespace PaymentGateway.Api.Services
{
    public interface IPaymentAuthoriser
    {
        Task<PaymentResult> Authorise(PaymentRequest payment);
    }

    public class AlwaysApprovesPaymentAuthoriser : IPaymentAuthoriser
    {
        public Task<PaymentResult> Authorise(PaymentRequest payment) => Task.FromResult(PaymentResult.Succeeded);
    }

    public class AlwaysDeniesPaymentAuthoriser : IPaymentAuthoriser
    {
        public Task<PaymentResult> Authorise(PaymentRequest payment) => Task.FromResult(PaymentResult.Failed);
    }

    public class AlwaysThrowsPaymentAuthoriser : IPaymentAuthoriser
    {
        public Task<PaymentResult> Authorise(PaymentRequest payment) => Task.FromException<PaymentResult>(new Exception("ERROR AUTHORISING PAYMENT"));
    }
}
