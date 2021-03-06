using System;
using System.Globalization;
using System.Linq;
using NodaTime;
using NodaTime.Text;

namespace PaymentGateway.Domain
{
    public record Cardholder(Sensitive<string> Name, Sensitive<string> Address);


    public class Cvv : IEquatable<Cvv>
    {
        public Cvv(string cvv)
        {
            if (string.IsNullOrWhiteSpace(cvv) || cvv.Any(c => !char.IsDigit(c)))
            {
                throw new ArgumentException("Card CVV must be non-empty. Only the characters 0-9 are allowed.");
            }

            Value = new Sensitive<string>(cvv);
        }

        public Sensitive<string> Value { get; }

        public bool Equals(Cvv? other) => other is not null && other.Value == Value;

        public override int GetHashCode() => Value.GetHashCode();
    }

    public record ExpiryMonth(YearMonth Value);

    public class Pan : IEquatable<Pan>
    {
        public Pan(string pan)
        {
            if (string.IsNullOrWhiteSpace(pan) || pan.Length != 16 || pan.Any(c => !char.IsDigit(c)))
            {
                throw new ArgumentException("Card PAN must be exactly 16 characters. Only the characters 0-9 are allowed.");
            }

            Value = new Sensitive<string>(pan);
        }

        public Sensitive<string> Value { get; }

        public string MaskedValue => $"************{Value.Value[^4..]}";

        public bool Equals(Pan? other) => other is not null && other.Value == Value;

        public override int GetHashCode() => Value.GetHashCode();
    }
    public record Card(Cardholder Cardholder, Pan Pan, Cvv Cvv, ExpiryMonth ExpiryMonth);
}
