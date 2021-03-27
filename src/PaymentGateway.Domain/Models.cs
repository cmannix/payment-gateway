using System.Diagnostics;

namespace PaymentGateway.Domain
{
    public enum PaymentResult
    {
        Succeeded,
        Failed
    }

    public record Amount(decimal Value, string CurrencyCode);

    public record Payment(string Id, Amount Amount, string Description, Card Card, PaymentResult Result);


    public record Cvv(Sensitive<string> Value);

    public record Cardholder(Sensitive<string> Name, Sensitive<string> Address);

    public record Pan(Sensitive<string> Value);
    public record Card(Cardholder Cardholder, Pan Pan, Cvv Cvv);

    public record Merchant(string Name, string merchantCategory);

    [DebuggerDisplay("{Value}")]
    public record Sensitive<T>(T Value)
    {
        public override string ToString()
        {
            return "*****";
        }
    }
}
