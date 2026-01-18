using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PortfolioCalculator.Application.DI;
using PortfolioCalculator.Application.Import;
using PortfolioCalculator.Application.PortfolioValuation;
using PortfolioCalculator.Infrastructure.DI;

namespace PortfolioCalculator.Cli;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        using var host = BuildHost(args);

        var config = host.Services.GetRequiredService<IConfiguration>();

        if (string.IsNullOrWhiteSpace(config["Mongo:ConnectionString"]))
        {
            throw new InvalidOperationException(
                "Mongo configuration not found. " +
                "Provide appsettings.json or Mongo__ConnectionString env var.");
        }

        using var scope = host.Services.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var runner = new CliRunner(mediator);

        await runner.RunAsync(args);
    }

    private static IHost BuildHost(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Debug);
            });

        builder.ConfigureAppConfiguration(ConfigureConfiguration);
        builder.ConfigureServices(ConfigureServices);

        var host = builder.Build();
        return host;
    }

    private static void ConfigureConfiguration(HostBuilderContext context, IConfigurationBuilder config)
    {
        // Standard path (bin / Docker)
        config.AddJsonFile(
            "appsettings.json",
            optional: true,
            reloadOnChange: true);

        // Start from the solution (Visual Studio)
        var localAppSettingsPath = Path.Combine(
            "source",
            "PortfolioCalculator.ConsoleApp",
            "appsettings.json");

        config.AddJsonFile(
            localAppSettingsPath,
            optional: true,
            reloadOnChange: true);

        config.AddEnvironmentVariables();
    }

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ImportAllCsvCommand).Assembly));
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(GetPortfolioValueQuery).Assembly));

        services
            .AddApplication()
            .AddInfrastructure(context.Configuration);
    }

}
