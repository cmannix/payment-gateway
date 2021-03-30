using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using PaymentGateway.Acquirer.InMemory;
using PaymentGateway.Persistence.InMemory;
using NodaTime;
using MicroElements.Swashbuckle.NodaTime;
using NodaTime.Serialization.SystemTextJson;
using PaymentGateway.Web.Serdes;
using Serilog;
using Prometheus;

namespace PaymentGateway.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.UseInMemoryPaymentStore();
            services.UsePaymentAuthoriser(Configuration);
            services.AddSingleton<IClock>(services => SystemClock.Instance);
            services.AddControllers().AddJsonOptions(opt => {
                opt.JsonSerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
                opt.JsonSerializerOptions.Converters.Add(new SensitiveJsonConverterFactory());
                opt.JsonSerializerOptions.Converters.Add(new CardCvvJsonConverter());
                opt.JsonSerializerOptions.Converters.Add(new CardPanJsonConverter());
                opt.JsonSerializerOptions.Converters.Add(new CardExpiryMonthJsonConverter());
                opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "PaymentGateway.Web", Version = "v1" });
                c.ConfigureForNodaTimeWithSystemTextJson();

                c.ConfigureDomainTypes();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PaymentGateway.Web v1"));

            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseHttpMetrics();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapMetrics();
            });
        }
    }
}
