using System.Collections.Generic;
using System.Threading.Tasks;
using PaymentGateway.Domain;
using PaymentGateway.Persistence.Api;

namespace PaymentGateway.Persistence.InMemory
{
    public class InMemoryPaymentRepository : IPaymentRepository
    {
        private Dictionary<string, Payment> _payments = new();

        public Task Create(Payment payment)
        {
            _payments[payment.Id] = payment;

            return Task.CompletedTask;
        }

        public Task<Payment?> Get(string id)
        {
            if (_payments.TryGetValue(id, out var p))
            {
                return Task.FromResult<Payment?>(p);
            }
            else return Task.FromResult<Payment?>(null);

        }
    }
}
