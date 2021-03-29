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
using PaymentGateway.Web.Models;
using PaymentGateway.Persistence.Api;
using NodaTime;

namespace PaymentGateway.Web.Tests
{
    public class PaymentsControllerTests
    {
        [Fact]
        public async Task When_authorise_payment_called_with_invalid_request_then_request_fails_with_bad_request()
        {
            var sut = new PaymentController(new InMemoryPaymentRepository(), new AlwaysApprovesPaymentAuthoriser(), SystemClock.Instance, NullLogger<PaymentController>.Instance);
            var paymentRequest = _generatePaymentRequest();
            sut.ModelState.AddModelError("Amount", "Required");

            var result = await sut.Authorise(paymentRequest);

            var actionResult = Assert.IsType<ActionResult<PaymentResponse>>(result);
            _ = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        }

        [Fact]
        public async Task When_authorise_payment_called_with_valid_request_then_request_succeeds_and_returns_created_payment()
        {
            var sut = new PaymentController(new InMemoryPaymentRepository(), new AlwaysApprovesPaymentAuthoriser(), SystemClock.Instance, NullLogger<PaymentController>.Instance);
            var paymentRequest = _generatePaymentRequest();

            var result = await sut.Authorise(paymentRequest);

            var createdPayment = ExpectSuccessResponseWithCreatedPayment(result);
            Assert.Equal(paymentRequest.Amount, createdPayment.Amount);
            Assert.Equal(paymentRequest.Description, createdPayment.Description);
            Assert.Equal(paymentRequest.Card.ExpiryMonth, createdPayment.Card.ExpiryMonth);
            Assert.Equal(paymentRequest.Card.Pan.MaskedValue, createdPayment.Card.MaskedPan);
            Assert.Equal(paymentRequest.Timestamp, createdPayment.Timestamp);
        }

        [Fact]
        public async Task When_authorise_payment_called_with_valid_request_then_request_succeeds_and_returns_created_payment_with_masked_card_details()
        {
            var sut = new PaymentController(new InMemoryPaymentRepository(), new AlwaysApprovesPaymentAuthoriser(), SystemClock.Instance, NullLogger<PaymentController>.Instance);
            var paymentRequest = _generatePaymentRequest();

            var result = await sut.Authorise(paymentRequest);

            var createdPayment = ExpectSuccessResponseWithCreatedPayment(result);
            Assert.Equal(paymentRequest.Card.Pan.MaskedValue, createdPayment.Card.MaskedPan);
            Assert.Equal(paymentRequest.Card.ExpiryMonth, createdPayment.Card.ExpiryMonth);
        }

        [Fact]
        public async Task When_authorise_payment_called_with_valid_request_then_authorisation_is_requested_with_same_payment_details()
        {
            var stubPaymentAuthoriser = new RecordingPaymentAuthoriserStub();
            var sut = new PaymentController(new InMemoryPaymentRepository(), stubPaymentAuthoriser, SystemClock.Instance, NullLogger<PaymentController>.Instance);
            var paymentRequest = _generatePaymentRequest();

            _ = await sut.Authorise(paymentRequest);

            Assert.Equal(paymentRequest.Amount, stubPaymentAuthoriser.LastRequest.Amount);
        }

        [Fact]
        public async Task When_authorise_payment_called_with_valid_request_then_authorisation_is_requested_with_same_card_details()
        {
            var stubPaymentAuthoriser = new RecordingPaymentAuthoriserStub();
            var sut = new PaymentController(new InMemoryPaymentRepository(), stubPaymentAuthoriser, SystemClock.Instance, NullLogger<PaymentController>.Instance);
            var paymentRequest = _generatePaymentRequest();

            _ = await sut.Authorise(paymentRequest);

            Assert.Equal(paymentRequest.Card.Cardholder, stubPaymentAuthoriser.LastRequest.Card.Cardholder);
            Assert.Equal(paymentRequest.Card.Cvv, stubPaymentAuthoriser.LastRequest.Card.Cvv);
            Assert.Equal(paymentRequest.Card.Pan, stubPaymentAuthoriser.LastRequest.Card.Pan);
        }

        [Fact]
        public async Task When_authorise_payment_called_with_valid_request_then_payment_is_stored_with_creation_time()
        {
            var paymentsRepo = new InMemoryPaymentRepository();
            var expectedCreatedAtTime = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(5));
            var clockStub = new ConstantTimeClockStub(expectedCreatedAtTime);
            var sut = new PaymentController(paymentsRepo, new AlwaysApprovesPaymentAuthoriser(), clockStub, NullLogger<PaymentController>.Instance);
            var paymentRequest = _generatePaymentRequest();

            var result = await sut.Authorise(paymentRequest);
            var createdPayment = ExpectSuccessResponseWithCreatedPayment(result);
            var storedPayment = await paymentsRepo.Get(createdPayment.Id, PaymentController.DefaultMerchant.Id);

            Assert.Equal(expectedCreatedAtTime, storedPayment.CreatedAt);
        }

        [Fact]
        public async Task Given_authorise_payment_called_with_valid_request_when_authorisation_is_approved_then_returns_successful_payment()
        {
            var sut = new PaymentController(new InMemoryPaymentRepository(), new AlwaysApprovesPaymentAuthoriser(), SystemClock.Instance, NullLogger<PaymentController>.Instance);
            var paymentRequest = _generatePaymentRequest();

            var result = await sut.Authorise(paymentRequest);

            var createdPayment = ExpectSuccessResponseWithCreatedPayment(result);
            Assert.Equal(PaymentResult.Succeeded, createdPayment.Result);
        }

        [Fact]
        public async Task Given_authorise_payment_called_with_valid_request_when_authorisation_is_denied_then_returns_failed_payment()
        {
            var sut = new PaymentController(new InMemoryPaymentRepository(), new AlwaysThrowsPaymentAuthoriser(), SystemClock.Instance, NullLogger<PaymentController>.Instance);
            var paymentRequest = _generatePaymentRequest();

            var result = await sut.Authorise(paymentRequest);

            var createdPayment = ExpectSuccessResponseWithCreatedPayment(result);
            Assert.Equal(PaymentResult.Failed, createdPayment.Result);
        }

        [Fact]
        public async Task Given_authorise_payment_called_with_valid_request_when_authorisation_fails_then_returns_denied_payment()
        {
            var sut = new PaymentController(new InMemoryPaymentRepository(), new AlwaysThrowsPaymentAuthoriser(), SystemClock.Instance, NullLogger<PaymentController>.Instance);
            var paymentRequest = _generatePaymentRequest();

            var result = await sut.Authorise(paymentRequest);

            var createdPayment = ExpectSuccessResponseWithCreatedPayment(result);
            Assert.Equal(PaymentResult.Failed, createdPayment.Result);
        }

        [Fact]
        public async Task Given_non_existent_payment_when_get_payment_called_then_returns_not_found()
        {
            var sut = new PaymentController(new InMemoryPaymentRepository(), new AlwaysThrowsPaymentAuthoriser(), SystemClock.Instance, NullLogger<PaymentController>.Instance);

            var result = await sut.Get(Guid.NewGuid());

            var actionResult = Assert.IsType<ActionResult<PaymentResponse>>(result);
            _ = Assert.IsType<NotFoundResult>(actionResult.Result);
        }

        [Fact]
        public async Task Given_payment_exists_when_get_payment_called_with_payment_id_then_returns_payment()
        {
            var paymentsRepo = new InMemoryPaymentRepository();
            var existingPayment = new Payment(
                Id: Guid.NewGuid(),
                Amount: new(1.23m, "GBP"),
                Description: "Test Payment",
                Card: new(
                    MaskedPan: "1234",
                    ExpiryMonth: new(2021, 01)
                ),
                Result: PaymentResult.Succeeded,
                Timestamp: SystemClock.Instance.GetCurrentInstant(),
                CreatedAt: SystemClock.Instance.GetCurrentInstant());
            await paymentsRepo.Create(existingPayment, PaymentController.DefaultMerchant.Id);
            var sut = new PaymentController(paymentsRepo, new AlwaysThrowsPaymentAuthoriser(), SystemClock.Instance, NullLogger<PaymentController>.Instance);

            var result = await sut.Get(existingPayment.Id);

            var actionResult = Assert.IsType<ActionResult<PaymentResponse>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedPayment = Assert.IsType<PaymentResponse>(okResult.Value);
            Assert.Equal(existingPayment.Amount, returnedPayment.Amount);
            Assert.Equal(existingPayment.Description, returnedPayment.Description);
            Assert.Equal(existingPayment.Result, returnedPayment.Result);

        }

        private static PaymentResponse ExpectSuccessResponseWithCreatedPayment(ActionResult<PaymentResponse> result)
        {
            var actionResult = Assert.IsType<ActionResult<PaymentResponse>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var createdPayment = Assert.IsType<PaymentResponse>(createdAtActionResult.Value);
            return createdPayment;
        }

        private PaymentRequest _generatePaymentRequest() => new(
                    Amount: new(1.23m, "GBP"),
                    Description: "Test",
                Card: new(
                    Cardholder: new(Name: new("James"), Address: new("UK")),
                    Pan: new(new("4444333322221111")),
                    Cvv: new(new("123")),
                    ExpiryMonth: new(new(2021,1))
                ),
                Timestamp: SystemClock.Instance.GetCurrentInstant()
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

        private class ConstantTimeClockStub : IClock
        {
            private readonly Instant instant;

            public ConstantTimeClockStub(Instant instant)
            {
                this.instant = instant;
            }
            public Instant GetCurrentInstant() => instant;
        }
    }
}
