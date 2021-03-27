using System;
using System.Threading.Tasks;
using PaymentGateway.Domain;
using PaymentGateway.Acquirer.InMemory;
using PaymentGateway.Persistence.InMemory;
using PaymentGateway.Web.Controllers;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Acquirer.Api;

namespace PaymentGateway.Web.Tests
{
    public class PaymentsControllerTests
    {
        [Fact]
        public async Task When_authorise_payment_called_with_invalid_request_then_request_fails_with_bad_request()
        {
            var sut = new PaymentController(new InMemoryPaymentRepository(), new AlwaysApprovesPaymentAuthoriser(), NullLogger<PaymentController>.Instance);
            var paymentRequest = _generatePaymentRequest();
            sut.ModelState.AddModelError("Amount", "Required");

            var result = await sut.Authorise(paymentRequest);

            var actionResult = Assert.IsType<ActionResult<Payment>>(result);
            _ = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        }

        [Fact]
        public async Task When_authorise_payment_called_with_valid_request_then_request_succeeds_and_returns_created_payment()
        {
            var sut = new PaymentController(new InMemoryPaymentRepository(), new AlwaysApprovesPaymentAuthoriser(), NullLogger<PaymentController>.Instance);
            var paymentRequest = _generatePaymentRequest();

            var result = await sut.Authorise(paymentRequest);

            var createdPayment = expectSuccessResponseWithCreatedPayment(result);
            Assert.Equal(paymentRequest.Payment.Amount, createdPayment.Amount);
            Assert.Equal(paymentRequest.Payment.Description, createdPayment.Description);
        }

        [Fact]
        public async Task When_authorise_payment_called_with_valid_request_then_authorisation_is_requested_with_same_payment_details()
        {
            var stubPaymentAuthoriser = new RecordingPaymentAuthoriserStub();
            var sut = new PaymentController(new InMemoryPaymentRepository(), stubPaymentAuthoriser, NullLogger<PaymentController>.Instance);
            var paymentRequest = _generatePaymentRequest();

            _ = await sut.Authorise(paymentRequest);

            Assert.Equal(paymentRequest.Payment.Amount, stubPaymentAuthoriser.LastRequest.Amount);
        }

        [Fact]
        public async Task When_authorise_payment_called_with_valid_request_then_authorisation_is_requested_with_same_card_details()
        {
            var stubPaymentAuthoriser = new RecordingPaymentAuthoriserStub();
            var sut = new PaymentController(new InMemoryPaymentRepository(), stubPaymentAuthoriser, NullLogger<PaymentController>.Instance);
            var paymentRequest = _generatePaymentRequest();

            _ = await sut.Authorise(paymentRequest);

            Assert.Equal(paymentRequest.Card.Cardholder, stubPaymentAuthoriser.LastRequest.Card.Cardholder);
            Assert.Equal(paymentRequest.Card.CardCvv, stubPaymentAuthoriser.LastRequest.Card.Cvv);
            Assert.Equal(paymentRequest.Card.CardPan, stubPaymentAuthoriser.LastRequest.Card.Pan);
        }

        [Fact]
        public async Task Given_authorise_payment_called_with_valid_request_when_authorisation_is_approved_then_returns_successful_payment()
        {
            var sut = new PaymentController(new InMemoryPaymentRepository(), new AlwaysApprovesPaymentAuthoriser(), NullLogger<PaymentController>.Instance);
            var paymentRequest = _generatePaymentRequest();

            var result = await sut.Authorise(paymentRequest);

            var createdPayment = expectSuccessResponseWithCreatedPayment(result);
            Assert.Equal(PaymentResult.Succeeded, createdPayment.Result);
        }

        [Fact]
        public async Task Given_authorise_payment_called_with_valid_request_when_authorisation_is_denied_then_returns_failed_payment()
        {
            var sut = new PaymentController(new InMemoryPaymentRepository(), new AlwaysThrowsPaymentAuthoriser(), NullLogger<PaymentController>.Instance);
            var paymentRequest = _generatePaymentRequest();

            var result = await sut.Authorise(paymentRequest);

            var createdPayment = expectSuccessResponseWithCreatedPayment(result);
            Assert.Equal(PaymentResult.Failed, createdPayment.Result);
        }

        [Fact]
        public async Task Given_authorise_payment_called_with_valid_request_when_authorisation_fails_then_returns_denied_payment()
        {
            var sut = new PaymentController(new InMemoryPaymentRepository(), new AlwaysThrowsPaymentAuthoriser(), NullLogger<PaymentController>.Instance);
            var paymentRequest = _generatePaymentRequest();

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
            var existingPayment = new Payment(Guid.NewGuid().ToString(), new Amount(1.23m, "GBP"), "Test Payment", new(
                    Cardholder: new(Name: new("James"), Address: new("UK")),
                    Pan: new(new("4444333322221111")),
                    Cvv: new(new("123"))
                ), PaymentResult.Succeeded);
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

        private Models.PaymentRequest _generatePaymentRequest() => new(
                Payment: new(
                    Amount: new(1.23m, "GBP"),
                    Description: "Test"
                ),
                Card: new(
                    Cardholder: new(Name: new("James"), Address: new("UK")),
                    CardPan: new(new("4444333322221111")),
                    CardCvv: new(new("123"))
                ),
                Timestamp: DateTimeOffset.UtcNow
            );

        private class RecordingPaymentAuthoriserStub : IPaymentAuthoriser
        {
            private readonly AuthoriseResult authResult;

            public RecordingPaymentAuthoriserStub(AuthoriseResult authResult = AuthoriseResult.Approved)
            {
                this.authResult = authResult;
            }
            public AuthoriseRequest LastRequest {get; private set;}

            public Task<AuthoriseResult> Authorise(AuthoriseRequest request)
            {
                LastRequest = request;
                return Task.FromResult(authResult);
            }
        }
    }
}
