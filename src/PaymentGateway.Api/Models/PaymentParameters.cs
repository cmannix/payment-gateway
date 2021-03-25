using System;
namespace PaymentGateway.Api.Models
{
    public record PaymentParameters(Amount Amount, string Description);
}
