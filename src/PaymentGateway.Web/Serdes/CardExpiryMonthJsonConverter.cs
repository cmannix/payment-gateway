using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using NodaTime.Text;
using PaymentGateway.Domain;

namespace PaymentGateway.Web.Models
{
    public class CardExpiryMonthJsonConverter : JsonConverter<ExpiryMonth>
    {
        private static readonly CultureInfo _culture = CultureInfo.InvariantCulture;
        private static readonly YearMonthPattern _pattern = YearMonthPattern.Create("yyyy-MM", _culture);
        
        public override ExpiryMonth Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            try
            {
                var value = reader.GetString() ?? throw new JsonException("Expiry month must be provided");
                return new(_pattern.Parse(value).Value);
            }
            catch (Exception ex)
            {
                throw new JsonException(ex.Message);
            }
        }
        public override void Write(
            Utf8JsonWriter writer,
            ExpiryMonth expiryMonth,
            JsonSerializerOptions options) => writer.WriteStringValue(expiryMonth.Value.ToString(_pattern.PatternText, _culture));
    }
}
