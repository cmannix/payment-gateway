using System.Threading.Tasks;
using PaymentGateway.Domain;
using NodaTime;
using System;

namespace PaymentGateway.Persistence.Api
{
    public interface IPaymentRepository
    {
        Task Create(Payment payment, Guid merchantId);
        Task<Payment?> Get(Guid id, Guid merchantId);
        // Can extend to e.g. Task<Payment[]> GetAllBetweenDates(merchantId, from, to)
    }

    public record Payment(Guid Id, Amount Amount, string Description, Instant Timestamp, Card Card, PaymentResult Result, Instant CreatedAt);

    public record Card(string MaskedPan, YearMonth ExpiryMonth);

    public class PaymentAlreadyExistsException : Exception
    {
        public PaymentAlreadyExistsException(Guid id) : base()
        {
            PaymentId = id;
        }

        public override string Message => $"Could not store payment as payment with ID {PaymentId} already exists";

        public Guid PaymentId { get; }

        public override string? StackTrace => "";
    }
}
