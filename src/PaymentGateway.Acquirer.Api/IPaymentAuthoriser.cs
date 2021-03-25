using System.Threading.Tasks;
using PaymentGateway.Domain;

namespace PaymentGateway.Acquirer.Api
{
    public interface IPaymentAuthoriser
    {
        Task<PaymentResult> Authorise(AuthoriseRequest payment);
    }

    public record AuthoriseRequest(string id);
}
