using System;

namespace PaymentGateway.Api.Models
{
    public enum PaymentResult
    {
        Succeeded,
        Failed
    }

    public record Amount(decimal Value, string CurrencyCode);
    public record Payment(string Id, Amount Amount, string Description, PaymentResult Result);
}
