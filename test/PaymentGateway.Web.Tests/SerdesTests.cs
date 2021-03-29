using System;
using System.Text.Json;
using NodaTime;
using PaymentGateway.Domain;
using PaymentGateway.Web.Serdes;
using Xunit;

namespace PaymentGateway.Web.Tests
{
    public class SerdesTests
    {
        [Fact]
        public void Can_deserialize_valid_PAN()
        {
            var jsonSerializerOptions = new JsonSerializerOptions();
            jsonSerializerOptions.Converters.Add(new CardPanJsonConverter());
            var panInput = "1111222233334444";
            var panJson = @$"""{panInput}""";
            var pan = JsonSerializer.Deserialize<Pan>(panJson, jsonSerializerOptions);

            Assert.Equal(panInput, pan.Value.Value);
        }

        [Fact]
        public void Can_serialize_valid_PAN()
        {
            var jsonSerializerOptions = new JsonSerializerOptions();
            jsonSerializerOptions.Converters.Add(new CardPanJsonConverter());
            var panInput = "1111222233334444";
            var pan = new Pan(panInput);
            
            var panJson = JsonSerializer.Serialize(pan, jsonSerializerOptions);

            Assert.Equal(@$"""{panInput}""", panJson);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("1234")]
        public void Deserialize_throws_for_invalid_PAN(string input)
        {
            var jsonSerializerOptions = new JsonSerializerOptions();
            jsonSerializerOptions.Converters.Add(new CardPanJsonConverter());
            var panJson = @$"""{input}""";
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Pan>(panJson, jsonSerializerOptions));
        }

        [Fact]
        public void Can_deserialize_valid_CVV()
        {
            var jsonSerializerOptions = new JsonSerializerOptions();
            jsonSerializerOptions.Converters.Add(new CardCvvJsonConverter());
            var input = "123";
            var json = @$"""{input}""";
            var cvv = JsonSerializer.Deserialize<Cvv>(json, jsonSerializerOptions);

            Assert.Equal(input, cvv.Value.Value);
        }

        [Fact]
        public void Can_serialize_valid_CVV()
        {
            var jsonSerializerOptions = new JsonSerializerOptions();
            jsonSerializerOptions.Converters.Add(new CardCvvJsonConverter());
            var input = "123";
            var cvv = new Cvv(input);

            var json = JsonSerializer.Serialize(cvv, jsonSerializerOptions);

            Assert.Equal(@$"""{input}""", json);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Deserialize_throws_for_invalid_CVV(string input)
        {
            var jsonSerializerOptions = new JsonSerializerOptions();
            jsonSerializerOptions.Converters.Add(new CardCvvJsonConverter());
            var json = @$"""{input}""";
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Cvv>(json, jsonSerializerOptions));
        }

        [Fact]
        public void Can_deserialize_valid_ExpiryMonth()
        {
            var jsonSerializerOptions = new JsonSerializerOptions();
            jsonSerializerOptions.Converters.Add(new CardExpiryMonthJsonConverter());
            var json = @$"""2021-01""";
            var expiryMonth = JsonSerializer.Deserialize<ExpiryMonth>(json, jsonSerializerOptions);

            Assert.Equal(new YearMonth(2021, 01), expiryMonth.Value);
        }

        [Fact]
        public void Can_serialize_valid_ExpiryMonth()
        {
            var jsonSerializerOptions = new JsonSerializerOptions();
            jsonSerializerOptions.Converters.Add(new CardExpiryMonthJsonConverter());
            var expiryMonth = new ExpiryMonth(new(2021, 01));

            var json = JsonSerializer.Serialize(expiryMonth, jsonSerializerOptions);

            Assert.Equal(@$"""2021-01""", json);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("ABC")]
        [InlineData("123")]
        [InlineData("2021-01-01")]
        [InlineData("11:30:12")]
        [InlineData("01-2021")]
        public void Deserialize_throws_for_invalid_ExpiryMonth(string input)
        {
            var jsonSerializerOptions = new JsonSerializerOptions();
            jsonSerializerOptions.Converters.Add(new CardExpiryMonthJsonConverter());
            var json = @$"""{input}""";
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<ExpiryMonth>(json, jsonSerializerOptions));
        }

        [Fact]
        public void Can_deserialize_string_Sensitive()
        {
            var jsonSerializerOptions = new JsonSerializerOptions();
            jsonSerializerOptions.Converters.Add(new SensitiveJsonConverterFactory());
            var input = "stringvalue" + Guid.NewGuid().ToString();
            var json = @$"""{input}""";
            var sensitive = JsonSerializer.Deserialize<Sensitive<string>>(json, jsonSerializerOptions);

            Assert.Equal(input, sensitive.Value);
        }

        [Fact]
        public void Can_serialize_string_Sensitive()
        {
            var jsonSerializerOptions = new JsonSerializerOptions();
            jsonSerializerOptions.Converters.Add(new SensitiveJsonConverterFactory());
            var input = "stringvalue" + Guid.NewGuid().ToString();
            var sensitive = new Sensitive<string>(input);

            var json = JsonSerializer.Serialize(sensitive, jsonSerializerOptions);

            Assert.Equal(@$"""{input}""", json);
        }

        [Fact]
        public void Deserialize_throws_for_invalid_string_sensitive_when_input_is_number()
        {
            var jsonSerializerOptions = new JsonSerializerOptions();
            jsonSerializerOptions.Converters.Add(new SensitiveJsonConverterFactory());
            var numberJson = "123";
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Sensitive<string>>(numberJson, jsonSerializerOptions));
        }
    }
}
