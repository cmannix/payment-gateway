using System;
using NodaTime;
using PaymentGateway.Domain;
using PaymentGateway.Persistence.Api;

namespace PaymentGateway.Web.Models
{
    public record CardDetails(string MaskedPan, ExpiryMonth ExpiryMonth);
    public record PaymentResponse(Guid Id, Amount Amount, string Description, PaymentResult Result, Instant Timestamp, CardDetails Card)
    {
        public static PaymentResponse FromPayment(Payment record) => new(
            Id: record.Id,
            Amount: record.Amount,
            Description: record.Description,
            Result: record.Result,
            Timestamp: record.Timestamp,
            Card: new(record.Card.MaskedPan, new(record.Card.ExpiryMonth)));
    }
}
