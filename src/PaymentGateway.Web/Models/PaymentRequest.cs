using System;
using PaymentGateway.Domain;

namespace PaymentGateway.Web.Models
{
    public record PaymentParameters(Amount Amount, string Description);

    public record CardParameters(Cardholder Cardholder, CardPan CardPan, CardCvv CardCvv);

    public record PaymentRequest(PaymentParameters Payment, CardParameters Card, DateTimeOffset Timestamp);
}
