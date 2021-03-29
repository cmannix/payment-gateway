using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PaymentGateway.Persistence.Api;

namespace PaymentGateway.Persistence.InMemory
{
    public class InMemoryPaymentRepository : IPaymentRepository
    {
        private Dictionary<Guid, Payment> _payments = new();

        public Task Create(Payment payment)
        {
            _payments[payment.Id] = payment;

            return Task.CompletedTask;
        }

        public Task<Payment?> Get(Guid id)
        {
            if (_payments.TryGetValue(id, out var p))
            {
                return Task.FromResult<Payment?>(p);
            }
            else return Task.FromResult<Payment?>(null);

        }
    }
}
