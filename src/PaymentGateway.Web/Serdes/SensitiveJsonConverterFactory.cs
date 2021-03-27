using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using PaymentGateway.Domain;

namespace PaymentGateway.Web.Models
{
    public class SensitiveJsonConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeToConvert.IsGenericType)
                return false;

            var type = typeToConvert;

            if (!type.IsGenericTypeDefinition)
                type = type.GetGenericTypeDefinition();

            return type == typeof(Sensitive<>);
        }

        public override JsonConverter CreateConverter(Type typeToConvert,
                                                      JsonSerializerOptions options)
        {
            var valueType = typeToConvert.GenericTypeArguments[0];
            var converterType = typeof(SensitiveJsonConverter<>).MakeGenericType(valueType);

            return (JsonConverter)Activator.CreateInstance(converterType,
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: new object[] { options },
                culture: null);
        }
    }
}
