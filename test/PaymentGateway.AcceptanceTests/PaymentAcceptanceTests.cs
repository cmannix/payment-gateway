using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Services;
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
            var client = _testClient();
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
            var client = _testClient();
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

        [Fact]
        public async Task Payment_result_is_succeeded_when_authorisation_is_approved()
        {
            var client = _testClient(services => services.AddSingleton<IPaymentAuthoriser, AlwaysApprovesPaymentAuthoriser>());
            var paymentRequest = _generatePaymentRequest();

            var createResponse = await client.PostAsJsonAsync(
                requestUri: "/payment",
                value: paymentRequest
            );
            var createdPayment = await createResponse.Content.ReadFromJsonAsync<Payment>();

            Assert.Equal(PaymentResult.Succeeded, createdPayment.Result);
        }

        [Fact]
        public async Task Payment_result_is_failed_when_authorisation_is_denied()
        {
            var client = _testClient(services => services.AddSingleton<IPaymentAuthoriser, AlwaysDeniesPaymentAuthoriser>());
            var paymentRequest = _generatePaymentRequest();

            var createResponse = await client.PostAsJsonAsync(
                requestUri: "/payment",
                value: paymentRequest
            );
            var createdPayment = await createResponse.Content.ReadFromJsonAsync<Payment>();

            Assert.Equal(PaymentResult.Failed, createdPayment.Result);
        }

        [Fact]
        public async Task Payment_result_is_failed_when_authorisation_errors()
        {
            var client = _testClient(services => services.AddSingleton<IPaymentAuthoriser, AlwaysThrowsPaymentAuthoriser>());
            var paymentRequest = _generatePaymentRequest();

            var createResponse = await client.PostAsJsonAsync(
                requestUri: "/payment",
                value: paymentRequest
            );
            var createdPayment = await createResponse.Content.ReadFromJsonAsync<Payment>();

            Assert.Equal(PaymentResult.Failed, createdPayment.Result);
        }

        private HttpClient _testClient(Action<IServiceCollection> customiseServices = null)
        {
            var clientOptions = new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            };

            if (customiseServices is not null)
            {
                return _factory
                    .WithWebHostBuilder(builder => builder.ConfigureTestServices(customiseServices))
                    .CreateClient(clientOptions);
            }
            else
            {
                return _factory.CreateClient(clientOptions);
            }

        }

        private PaymentRequest _generatePaymentRequest() => new(
                Payment: new(
                    Amount: new(1.23m, "GBP"),
                    Description: "Test"
                )
            );
    }
}
