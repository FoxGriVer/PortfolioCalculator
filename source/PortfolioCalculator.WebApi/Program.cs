using PortfolioCalculator.Application.DI;
using PortfolioCalculator.Application.PortfolioValuation;
using PortfolioCalculator.Infrastructure.DI;
using System.Text.Json.Serialization;

namespace PortfolioCalculator.WebApi
{
    public static class Program
    {
        private static async Task Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            builder.Services
                .AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(GetPortfolioValueQuery).Assembly));

            builder.Services
                .AddApplication()
                .AddInfrastructure(builder.Configuration);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AngularDev", policy =>
                    policy.WithOrigins("http://localhost:4200")
                          .AllowAnyHeader()
                          .AllowAnyMethod());
            });

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseCors("AngularDev");

            app.MapGet("/", () => Results.Redirect("/swagger"))
                            .ExcludeFromDescription();

            app.MapControllers();

            await app.RunAsync();
        }
    }
}