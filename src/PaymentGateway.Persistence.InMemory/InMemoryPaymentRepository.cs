using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PaymentGateway.Persistence.Api;

namespace PaymentGateway.Persistence.InMemory
{
    public class InMemoryPaymentRepository : IPaymentRepository
    {
        private Dictionary<Guid, (Payment, Guid)> _payments = new();

        public Task Create(Payment payment, Guid merchantId)
        {
            if (_payments.ContainsKey(payment.Id)) {
                throw new PaymentAlreadyExistsException(payment.Id);
            } else
            {
                _payments[payment.Id] = (payment, merchantId);
            }
            return Task.CompletedTask;
        }

        public Task<Payment?> Get(Guid id, Guid merchantId)
        {
            if (_payments.TryGetValue(id, out var pm))
            {
                var (payment, mId) = pm;
                return Task.FromResult(merchantId == mId ? payment : null);
            }
            else return Task.FromResult<Payment?>(null);

        }
    }
}
