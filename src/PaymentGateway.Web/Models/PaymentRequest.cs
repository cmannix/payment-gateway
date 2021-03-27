using System;
using System.ComponentModel.DataAnnotations;
using PaymentGateway.Domain;

namespace PaymentGateway.Web.Models
{
    public record PaymentParameters(Amount Amount, string Description);

    public record CardParameters(Cardholder Cardholder, Pan CardPan, Cvv CardCvv);

    public record PaymentRequest(PaymentParameters Payment, CardParameters Card, DateTimeOffset Timestamp);
}
