using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NodaTime;
using PaymentGateway.Domain;

namespace PaymentGateway.Web.Models
{
    public record CardParameters(Cardholder Cardholder, Pan Pan, Cvv Cvv, ExpiryMonth ExpiryMonth);

    public record PaymentRequest(Amount Amount, string Description, Instant Timestamp, CardParameters Card) : IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Amount.Value <= 0m)
            {
                yield return new ValidationResult("Payment amount must be positive", new[] { nameof(Amount) });
            }

            if (Amount.CurrencyCode != "GBP")
            {
                yield return new ValidationResult("Payment currency must be GBP", new[] { nameof(Amount) });
            }
        }
    }
}
