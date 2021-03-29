using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PaymentGateway.Domain;
using PaymentGateway.Acquirer.Api;
using PaymentGateway.Persistence.Api;
using PaymentGateway.Web.Models;
using NodaTime;

namespace PaymentGateway.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentRepository _paymentStore;
        private readonly IPaymentAuthoriser _paymentAuthoriser;
        private readonly IClock _clock;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IPaymentRepository paymentStore, IPaymentAuthoriser paymentAuthoriser, IClock clock, ILogger<PaymentController> logger)
        {
            _paymentStore = paymentStore;
            _paymentAuthoriser = paymentAuthoriser;
            _clock = clock;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<PaymentResponse>> Authorise(PaymentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var paymentId = Guid.NewGuid();

            var paymentResult = await AuthorisePayment(request, paymentId);
            var payment = new Payment(
                Id: paymentId,
                Amount: request.Amount,
                Description: request.Description,
                Card: new(request.Card.Pan.MaskedValue, request.Card.ExpiryMonth.Value),
                Timestamp: request.Timestamp,
                Result: paymentResult,
                CreatedAt: _clock.GetCurrentInstant());
            await _paymentStore.Create(payment, DefaultMerchant.Id);

            return CreatedAtAction(nameof(Get), new { id = paymentId }, PaymentResponse.FromPayment(payment));

        }

        private async Task<PaymentResult> AuthorisePayment(PaymentRequest request, Guid paymentId)
        {
            PaymentResult paymentResult;
            try
            {
                var authRequest = new AuthoriseRequest(
                    Amount: request.Amount,
                    Card: new(
                        request.Card.Cardholder,
                        request.Card.Pan,
                        request.Card.Cvv,
                        request.Card.ExpiryMonth
                    ),
                    DefaultMerchant,
                    new Metadata(request.Timestamp, paymentId.ToString())
                );
                var authResult = await _paymentAuthoriser.Authorise(authRequest);
                paymentResult = authResult switch
                {
                    AuthoriseResult.Approved => PaymentResult.Succeeded,
                    AuthoriseResult.Denied => PaymentResult.Failed,
                    _ => throw new Exception($"Received unknown result from payment authoriser: '{authResult}'")
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error authorising payment {paymentId}, marking as failed", ex);
                paymentResult = PaymentResult.Failed;
            }

            return paymentResult;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<PaymentResponse>> Get(Guid id)
        {
            return (await _paymentStore.Get(id, DefaultMerchant.Id)) switch
            {
                Payment p => base.Ok(PaymentResponse.FromPayment(p)),
                _ => base.NotFound()
            };
        }

        // TODO: In reality you might imagine this coming from either some store of merchant details, or perhaps from
        // claims in the token that the merchant would use to interact with the gateway
        public static readonly Merchant DefaultMerchant = new(Guid.NewGuid(), "John Lewis", "");
    }
}
