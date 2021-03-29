using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using PaymentGateway.Domain;

namespace PaymentGateway.Web.Serdes
{
    public class SensitiveJsonConverter<TValue> : JsonConverter<Sensitive<TValue>>
    {
        private readonly JsonConverter<TValue>? _valueConverter;
        private readonly Type _valueType;

        public SensitiveJsonConverter(JsonSerializerOptions options)
        {
            _valueConverter = options.GetConverter(typeof(TValue)) as JsonConverter<TValue>;
            _valueType = typeof(TValue);
        }

        public override Sensitive<TValue> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            TValue? value;
            if (_valueConverter != null)
            {
                value = _valueConverter.Read(ref reader, _valueType, options);
            }
            else
            {
                value = JsonSerializer.Deserialize<TValue>(ref reader, options);
            }

            if (value is null)
            {
                throw new JsonException("Value must be provided");
            }

            return new Sensitive<TValue>(value);
        }

        public override void Write(
            Utf8JsonWriter writer,
            Sensitive<TValue> sensitiveValue,
            JsonSerializerOptions options)
        {
            if (_valueConverter != null)
            {
                _valueConverter.Write(writer, sensitiveValue.Value, options);
            }
            else
            {
                JsonSerializer.Serialize(writer, sensitiveValue.Value, options);
            }
        }
    }
}
