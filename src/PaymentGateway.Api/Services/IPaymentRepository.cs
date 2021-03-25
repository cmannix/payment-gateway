using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PaymentGateway.Api.Models;

namespace PaymentGateway.Api.Services
{
    public interface IPaymentRepository
    {
        Task Create(Payment payment);
        Task<Payment> Get(string id);
    }

    public class InMemoryPaymentRepository : IPaymentRepository
    {
        private Dictionary<string, Payment> _payments = new();

        public Task Create(Payment payment)
        {
            _payments[payment.Id] = payment;

            return Task.CompletedTask;
        }

        public Task<Payment> Get(string id)
        {
            if (_payments.TryGetValue(id, out var p))
            {
                return Task.FromResult(p);
            }
            else return Task.FromResult<Payment>(null);
            
        }
    }
}
