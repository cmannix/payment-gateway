using System;
using System.Threading.Tasks;
using NodaTime;
using PaymentGateway.Persistence.Api;
using Xunit;

namespace PaymentGateway.Persistence.InMemory.Tests
{
    public class InMemoryPaymentRepositoryTests
    {
        [Fact]
        public async Task Can_store_a_payment_for_a_merchant_id()
        {
            var sut = new InMemoryPaymentRepository();

            await sut.Create(
                payment: GeneratePayment(),
                merchantId: Guid.NewGuid());
        }

        [Fact]
        public async Task Can_retrieve_a_stored_payment_for_a_merchant_id()
        {
            var sut = new InMemoryPaymentRepository();
            var (payment, merchantId) = (GeneratePayment(), Guid.NewGuid());
            await sut.Create(payment: payment, merchantId: merchantId);

            var retrievedPayment = await sut.Get(payment.Id, merchantId);

            Assert.Equal(payment.Amount, retrievedPayment.Amount);
            Assert.Equal(payment.Card, retrievedPayment.Card);
            Assert.Equal(payment.Id, retrievedPayment.Id);
            Assert.Equal(payment.Description, retrievedPayment.Description);
            Assert.Equal(payment.Result, retrievedPayment.Result);
            Assert.Equal(payment.Timestamp, retrievedPayment.Timestamp);
        }

        [Fact]
        public async Task Returns_null_if_no_payment_with_that_id_exists()
        {
            var sut = new InMemoryPaymentRepository();

            var retrievedPayment = await sut.Get(Guid.NewGuid(), Guid.NewGuid());

            Assert.Null(retrievedPayment);
        }

        [Fact]
        public async Task Returns_null_if_no_payment_with_that_id_exists_for_the_provided_merchant_id()
        {
            var sut = new InMemoryPaymentRepository();
            var (payment, merchantId) = (GeneratePayment(), Guid.NewGuid());
            await sut.Create(payment: payment, merchantId: merchantId);

            var retrievedPayment = await sut.Get(payment.Id, Guid.NewGuid());

            Assert.Null(retrievedPayment);
        }

        [Fact]
        public async Task Storing_a_payment_twice_with_the_same_id_throws()
        {
            var sut = new InMemoryPaymentRepository();
            var payment = GeneratePayment();
            async Task CreatePayment() => await sut.Create(payment: payment, merchantId: Guid.NewGuid());

            await CreatePayment();
            await Assert.ThrowsAsync<PaymentAlreadyExistsException>(CreatePayment);
        }

        private static Payment GeneratePayment() => new(
            Id: Guid.NewGuid(),
            Amount: new(1.23m, "GBP"),
            Description: "None",
            Timestamp: SystemClock.Instance.GetCurrentInstant(),
            Card: new("*1234", new(2021, 01)),
            Result: Domain.PaymentResult.Succeeded,
            CreatedAt: SystemClock.Instance.GetCurrentInstant());
    }
}
