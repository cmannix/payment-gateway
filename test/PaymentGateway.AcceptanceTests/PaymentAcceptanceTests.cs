using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using PaymentGateway.Api.Models;
using Xunit;

namespace PaymentGateway.AcceptanceTests
{
    public class PaymentAcceptanceTests : IClassFixture<WebApplicationFactory<Api.Startup>>
    {
        private readonly WebApplicationFactory<Api.Startup> _factory;

        public PaymentAcceptanceTests(WebApplicationFactory<Api.Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Can_create_payment()
        {
            var client = _factory.CreateClient();
            var paymentRequest = _generatePaymentRequest();


            var createResponse = await client.PostAsJsonAsync(
                requestUri: "/payment",
                value: paymentRequest
            );
            var createdPayment = await createResponse.Content.ReadFromJsonAsync<Payment>();


            Assert.Equal(paymentRequest.Payment.Amount, createdPayment.Amount);
            Assert.Equal(paymentRequest.Payment.Description, createdPayment.Description);
        }

        [Fact]
        public async Task Can_retrieve_created_payment()
        {
            var client = _factory.CreateClient();
            var paymentRequest = _generatePaymentRequest();
            var createResponse = await client.PostAsJsonAsync(
                requestUri: "/payment",
                value: paymentRequest
            );
            var createdPaymentLocation = createResponse.Headers.Location;


            var retrievedPayment = await client.GetFromJsonAsync<Payment>(
                requestUri: createdPaymentLocation
            );


            Assert.Contains(retrievedPayment.Id, createdPaymentLocation.ToString());
            Assert.Equal(paymentRequest.Payment.Amount, retrievedPayment.Amount);
            Assert.Equal(paymentRequest.Payment.Description, retrievedPayment.Description);
        }

        private PaymentRequest _generatePaymentRequest() => new (
                Payment: new(
                    Amount: new(1.23m, "GBP"),
                    Description: "Test"
                )
            );
    }
}
