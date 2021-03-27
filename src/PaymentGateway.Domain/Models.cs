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


    public record CardCvv(Sensitive<string> Value);

    public record Cardholder(Sensitive<string> Name, Sensitive<string> Address);

    public record CardPan
    {
        public CardPan(Sensitive<string> pan)
        {
            if (pan.Value.Length != 16)
            {
                throw new System.ArgumentException("Card PAN must be 16 digits");
            }
            _pan = pan;
        }

        private Sensitive<string> _pan;
        public Sensitive<string> Pan
        {
            get => _pan;
        }
    }
    public record Card(Cardholder Cardholder, CardPan Pan, CardCvv Cvv);

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
