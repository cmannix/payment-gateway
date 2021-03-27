using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using PaymentGateway.Domain;

namespace PaymentGateway.Web.Models
{
    public class CardPanJsonConverter : JsonConverter<Pan>
    {

        public override Pan Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) => new(new Sensitive<string>(reader.GetString()));
        public override void Write(
            Utf8JsonWriter writer,
            Pan cardPanValue,
            JsonSerializerOptions options) => writer.WriteStringValue(cardPanValue.Value.Value);
    }
}
