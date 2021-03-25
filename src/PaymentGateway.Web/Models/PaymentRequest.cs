using PaymentGateway.Domain;

namespace PaymentGateway.Web.Models
{
    public record PaymentParameters(Amount Amount, string Description);

    public record PaymentRequest(PaymentParameters Payment);
}
