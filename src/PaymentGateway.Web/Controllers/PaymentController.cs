using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PaymentGateway.Domain;
using PaymentGateway.Acquirer.Api;
using PaymentGateway.Persistence.Api;
using PaymentGateway.Web.Models;

namespace PaymentGateway.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentRepository _paymentStore;
        private readonly IPaymentAuthoriser _paymentAuthoriser;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IPaymentRepository paymentStore, IPaymentAuthoriser paymentAuthoriser, ILogger<PaymentController> logger)
        {
            _paymentStore = paymentStore;
            _paymentAuthoriser = paymentAuthoriser;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<Payment>> Authorise(PaymentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var payment = new Payment(
                Id: Guid.NewGuid().ToString(),
                Amount: request.Payment.Amount,
                Description: request.Payment.Description,
                Card: new(
                    request.Card.Cardholder,
                    request.Card.CardPan,
                    request.Card.CardCvv
                ),
                Result: PaymentResult.Failed);
            try
            {
                var authRequest = new AuthoriseRequest(
                    payment.Amount,
                    payment.Card,
                    new Merchant("Argos", "7995"),
                    new Metadata(DateTimeOffset.UtcNow, payment.Id)
                );
                var authResult = await _paymentAuthoriser.Authorise(authRequest);
                payment = payment with {
                    Result = authResult switch
                        {
                            AuthoriseResult.Approved => PaymentResult.Succeeded,
                            AuthoriseResult.Denied => PaymentResult.Failed,
                            _ => throw new Exception($"Received unknown result from payment authoriser: '{authResult}'")
                        }
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error authorising payment {payment.Id}, marking as failed", ex);
                payment = payment with { Result = PaymentResult.Failed };
            }
            
            await _paymentStore.Create(payment);

            return CreatedAtAction(nameof(Get), new { id = payment.Id }, payment);

        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<Payment>> Get(string id)
        {
            return (await _paymentStore.Get(id)) switch
            {
                Payment p => Ok(p),
                _ => NotFound()
            };
        }
    }
}
