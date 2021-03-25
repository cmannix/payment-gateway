using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Controllers
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
            var payment = new Payment(Guid.NewGuid().ToString(), request.Payment.Amount, request.Payment.Description, PaymentResult.Failed);
            try
            {
                payment = payment with { Result = await _paymentAuthoriser.Authorise(request) };
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
