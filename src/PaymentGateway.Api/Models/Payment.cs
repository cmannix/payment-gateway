using System;

namespace PaymentGateway.Api.Models
{
    public record Amount(decimal Value, string CurrencyCode);
    public record Payment(string Id, Amount Amount, string Description);
}
