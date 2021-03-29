using System.Threading.Tasks;
using PaymentGateway.Domain;
using NodaTime;
using System;

namespace PaymentGateway.Persistence.Api
{
    public interface IPaymentRepository
    {
        Task Create(Payment payment);
        Task<Payment?> Get(Guid id);
    }

    public record Payment(Guid Id, Amount Amount, string Description, Instant Timestamp, Card Card, PaymentResult Result, Instant CreatedAt);

    public record Card(string MaskedPan, YearMonth ExpiryMonth);
}
