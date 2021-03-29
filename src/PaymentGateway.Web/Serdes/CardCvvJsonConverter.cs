using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using PaymentGateway.Domain;

namespace PaymentGateway.Web.Serdes
{
    public class CardCvvJsonConverter : JsonConverter<Cvv>
    {

        public override Cvv Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            try
            {
                var value = reader.GetString() ?? throw new JsonException("CVV must be provided");
                return new(value);
            } catch (ArgumentException ex)
            {
                throw new JsonException(ex.Message);
            }
            
        }
        public override void Write(
            Utf8JsonWriter writer,
            Cvv cardPanValue,
            JsonSerializerOptions options) => writer.WriteStringValue(cardPanValue.Value.Value);
    }
}
