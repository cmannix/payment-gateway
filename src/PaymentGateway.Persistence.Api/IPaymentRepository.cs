using System.Threading.Tasks;
using PaymentGateway.Domain;

namespace PaymentGateway.Persistence.Api
{
    public interface IPaymentRepository
    {
        Task Create(Payment payment);
        Task<Payment> Get(string id);
    }
}
