using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using PaymentGateway.Acquirer.InMemory;
using PaymentGateway.Domain;
using PaymentGateway.Web.Models;
using PaymentGateway.Web.Serdes;
using Xunit;

namespace PaymentGateway.AcceptanceTests
{
    public class PaymentAcceptanceTests : IClassFixture<WebApplicationFactory<Web.Startup>>
    {
        private readonly WebApplicationFactory<Web.Startup> _factory;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public PaymentAcceptanceTests(WebApplicationFactory<Web.Startup> factory)
        {
            _factory = factory;
            _jsonSerializerOptions = new JsonSerializerOptions().ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
            _jsonSerializerOptions.Converters.Add(new SensitiveJsonConverterFactory());
            _jsonSerializerOptions.Converters.Add(new CardCvvJsonConverter());
            _jsonSerializerOptions.Converters.Add(new CardPanJsonConverter());
            _jsonSerializerOptions.Converters.Add(new CardExpiryMonthJsonConverter());
            _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            _jsonSerializerOptions.PropertyNameCaseInsensitive = true;
        }

        [Fact]
        public async Task Can_create_payment()
        {
            var client = TestClient();
            var paymentRequest = GeneratePaymentRequest();

            var createdPayment = await CreatePayment(client, paymentRequest);

            Assert.Equal(paymentRequest.Amount, createdPayment.Amount);
            Assert.Equal(paymentRequest.Description, createdPayment.Description);
        }

        [Theory]
        [MemberData(nameof(InvalidPaymentRequests))]
        public async Task Cant_create_payment_with_invalid_requests(PaymentRequest request)
        {
            var client = TestClient();

            var createResponse = await client.PostAsJsonAsync(
                            requestUri: "/payment",
                            value: request,
                            _jsonSerializerOptions
                        );

            Assert.Equal(System.Net.HttpStatusCode.BadRequest, createResponse.StatusCode);
        }

        [Fact]
        public async Task Can_retrieve_created_payment()
        {
            var client = TestClient();
            var paymentRequest = GeneratePaymentRequest();

            var createdPayment = await CreatePayment(client, paymentRequest);


            var retrievedPayment = await client.GetFromJsonAsync<PaymentResponse>(
                requestUri: $"/payment/{createdPayment.Id}",
                _jsonSerializerOptions
            );


            Assert.Equal(createdPayment.Id, retrievedPayment.Id);
            Assert.Equal(createdPayment.Result, retrievedPayment.Result);
            Assert.Equal(createdPayment.Timestamp, retrievedPayment.Timestamp);
            Assert.Equal(createdPayment.Card, retrievedPayment.Card);
            Assert.Equal(paymentRequest.Amount, retrievedPayment.Amount);
            Assert.Equal(paymentRequest.Description, retrievedPayment.Description);
        }

        [Fact]
        public async Task Payment_result_is_succeeded_when_authorisation_is_approved()
        {
            var client = TestClient(services => services.UsePaymentAuthoriser<AlwaysApprovesPaymentAuthoriser>());
            var paymentRequest = GeneratePaymentRequest();

            var createdPayment = await CreatePayment(client, paymentRequest);

            Assert.Equal(PaymentResult.Succeeded, createdPayment.Result);
        }

        [Fact]
        public async Task Payment_result_is_failed_when_authorisation_is_denied()
        {
            var client = TestClient(services => services.UsePaymentAuthoriser<AlwaysDeniesPaymentAuthoriser>());
            var paymentRequest = GeneratePaymentRequest();

            var createdPayment = await CreatePayment(client, paymentRequest);

            Assert.Equal(PaymentResult.Failed, createdPayment.Result);
        }

        [Fact]
        public async Task Payment_result_is_failed_when_authorisation_errors()
        {
            var client = TestClient(services => services.UsePaymentAuthoriser<AlwaysThrowsPaymentAuthoriser>());
            var paymentRequest = GeneratePaymentRequest();

            var createdPayment = await CreatePayment(client, paymentRequest);

            Assert.Equal(PaymentResult.Failed, createdPayment.Result);
        }

        private async Task<PaymentResponse> CreatePayment(HttpClient client, PaymentRequest paymentRequest)
        {
            var createResponse = await client.PostAsJsonAsync(
                            requestUri: "/payment",
                            value: paymentRequest,
                            _jsonSerializerOptions
                        );
            Assert.True(createResponse.IsSuccessStatusCode, $"Request did not succeed: {createResponse.ReasonPhrase} - {await createResponse.Content.ReadAsStringAsync()}");

            var createdPayment = await createResponse.Content.ReadFromJsonAsync<PaymentResponse>(_jsonSerializerOptions);

            Assert.True (Guid.Empty != createdPayment.Id, $"Response deserialization failed: {await createResponse.Content.ReadAsStringAsync()}");
            
            return createdPayment;
        }

        private HttpClient TestClient(Action<IServiceCollection> customiseServices = null)
        {
            var clientOptions = new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            };

            if (customiseServices is not null)
            {
                return _factory
                    .WithWebHostBuilder(builder => builder.ConfigureTestServices(srv => {
                        customiseServices(srv);
                        }))
                    .CreateClient(clientOptions);
            }
            else
            {
                return _factory.CreateClient(clientOptions);
            }

        }

        private static PaymentRequest GeneratePaymentRequest() => new(
                Amount: new(1.23m, "GBP"),
                Description: "Test",
                Card: GenerateCardParameters(),
                Timestamp: SystemClock.Instance.GetCurrentInstant()
            );
        private static CardParameters GenerateCardParameters() => new(
                    Cardholder: new(new("Joe"), new("London")),
                    Pan: new(new("1111222233334444")),
                    Cvv: new(new("123")),
                    ExpiryMonth: new(new(2021, 1))
                );

        public static IEnumerable<object[]> InvalidPaymentRequests =>
        new List<object[]>
            {
                new object[] { GeneratePaymentRequest() with { Amount = new(-1.23m, "GBP") } }, // Negative amount
                new object[] { GeneratePaymentRequest() with { Amount = new(1.23m, "USD") } }, // Unsupported currency
            };
    }
}
