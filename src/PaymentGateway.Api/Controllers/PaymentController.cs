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
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IPaymentRepository paymentStore, ILogger<PaymentController> logger)
        {
            _paymentStore = paymentStore;
            _logger = logger;
        }

        [HttpPost]
        public ActionResult<Payment> Authorise(PaymentRequest request)
        {
            var payment = new Payment(Guid.NewGuid().ToString(), request.Payment.Amount, request.Payment.Description);
            _paymentStore.Create(payment);

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
