using System;
using System.Threading.Tasks;
using PaymentGateway.Domain;
using PaymentGateway.Acquirer.InMemory;
using PaymentGateway.Persistence.InMemory;
using PaymentGateway.Web.Controllers;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace PaymentGateway.Web.Tests
{
    public class PaymentsControllerTests
    {
        [Fact]
        public async Task When_authorise_payment_called_with_valid_request_then_request_succeeds_and_returns_created_payment()
        {
            var sut = new PaymentController(new InMemoryPaymentRepository(), new AlwaysApprovesPaymentAuthoriser(), NullLogger<PaymentController>.Instance);
            var paymentRequest = new Models.PaymentRequest(new Models.PaymentParameters(new Amount(1.23m, "GBP"), "Test Payment"));

            var result = await sut.Authorise(paymentRequest);

            var createdPayment = expectSuccessResponseWithCreatedPayment(result);
            Assert.Equal(paymentRequest.Payment.Amount, createdPayment.Amount);
            Assert.Equal(paymentRequest.Payment.Description, createdPayment.Description);
        }

        [Fact]
        public async Task Given_authorise_payment_called_with_valid_request_when_authorisation_is_approved_then_returns_successful_payment()
        {
            var sut = new PaymentController(new InMemoryPaymentRepository(), new AlwaysApprovesPaymentAuthoriser(), NullLogger<PaymentController>.Instance);
            var paymentRequest = new Models.PaymentRequest(new Models.PaymentParameters(new Amount(1.23m, "GBP"), "Test Payment"));

            var result = await sut.Authorise(paymentRequest);

            var createdPayment = expectSuccessResponseWithCreatedPayment(result);
            Assert.Equal(PaymentResult.Succeeded, createdPayment.Result);
        }

        [Fact]
        public async Task Given_authorise_payment_called_with_valid_request_when_authorisation_is_denied_then_returns_failed_payment()
        {
            var sut = new PaymentController(new InMemoryPaymentRepository(), new AlwaysThrowsPaymentAuthoriser(), NullLogger<PaymentController>.Instance);
            var paymentRequest = new Models.PaymentRequest(new Models.PaymentParameters(new Amount(1.23m, "GBP"), "Test Payment"));

            var result = await sut.Authorise(paymentRequest);

            var createdPayment = expectSuccessResponseWithCreatedPayment(result);
            Assert.Equal(PaymentResult.Failed, createdPayment.Result);
        }

        [Fact]
        public async Task Given_authorise_payment_called_with_valid_request_when_authorisation_fails_then_returns_denied_payment()
        {
            var sut = new PaymentController(new InMemoryPaymentRepository(), new AlwaysThrowsPaymentAuthoriser(), NullLogger<PaymentController>.Instance);
            var paymentRequest = new Models.PaymentRequest(new Models.PaymentParameters(new Amount(1.23m, "GBP"), "Test Payment"));

            var result = await sut.Authorise(paymentRequest);

            var createdPayment = expectSuccessResponseWithCreatedPayment(result);
            Assert.Equal(PaymentResult.Failed, createdPayment.Result);
        }

        [Fact]
        public async Task Given_non_existent_payment_when_get_payment_called_then_returns_not_found()
        {
            var sut = new PaymentController(new InMemoryPaymentRepository(), new AlwaysThrowsPaymentAuthoriser(), NullLogger<PaymentController>.Instance);

            var result = await sut.Get(Guid.NewGuid().ToString());

            var actionResult = Assert.IsType<ActionResult<Payment>>(result);
            _ = Assert.IsType<NotFoundResult>(actionResult.Result);
        }

        [Fact]
        public async Task Given_payment_exists_when_get_payment_called_with_payment_id_then_returns_payment()
        {
            var paymentsRepo = new InMemoryPaymentRepository();
            var existingPayment = new Payment(Guid.NewGuid().ToString(), new Amount(1.23m, "GBP"), "Test Payment", PaymentResult.Succeeded);
            await paymentsRepo.Create(existingPayment);
            var sut = new PaymentController(paymentsRepo, new AlwaysThrowsPaymentAuthoriser(), NullLogger<PaymentController>.Instance);

            var result = await sut.Get(existingPayment.Id);

            var actionResult = Assert.IsType<ActionResult<Payment>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedPayment = Assert.IsType<Payment>(okResult.Value);
            Assert.Equal(existingPayment.Amount, returnedPayment.Amount);
            Assert.Equal(existingPayment.Description, returnedPayment.Description);
            Assert.Equal(existingPayment.Result, returnedPayment.Result);

        }

        private static Payment expectSuccessResponseWithCreatedPayment(ActionResult<Payment> result)
        {
            var actionResult = Assert.IsType<ActionResult<Payment>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var createdPayment = Assert.IsType<Payment>(createdAtActionResult.Value);
            return createdPayment;
        }
    }
}
