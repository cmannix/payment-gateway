using System.Diagnostics;

namespace PaymentGateway.Domain
{
    public enum PaymentResult
    {
        Succeeded,
        Failed
    }

    public record Amount(decimal Value, string CurrencyCode);

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
