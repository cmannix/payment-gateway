using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using PaymentGateway.Domain;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PaymentGateway.Web.Serdes
{
    public static class SwaggerGenOptionsExtensions
    {
        public static void ConfigureDomainTypes(this SwaggerGenOptions c)
        {
            c.MapType<Sensitive<string>>(() =>
                    new OpenApiSchema
                    {
                        Type = "string",
                    });
            c.MapType<Pan>(() =>
                new OpenApiSchema
                {
                    Type = "string",
                    MinLength = 16,
                    MaxLength = 16,
                    Example = OpenApiAnyFactory.CreateFromJson("1111222233334444")
                });
            c.MapType<Cvv>(() =>
                new OpenApiSchema
                {
                    Type = "string",
                    Example = OpenApiAnyFactory.CreateFromJson("123")
                });
            c.MapType<ExpiryMonth>(() =>
                new OpenApiSchema
                {
                    Type = "string",
                    Pattern= "/([0-9]{4})-(?:[0-9]{2})/",
                    Example = OpenApiAnyFactory.CreateFromJson("2021-01")
                });
        }
    }
}
