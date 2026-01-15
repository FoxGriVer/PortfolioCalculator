using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PortfolioCalculator.Application.Import;
using PortfolioCalculator.Infrastructure.DI;

namespace PortfolioCalculator.Cli;

internal static class Program
{
    private const string DefaultDataFolder = "./data";

    public static async Task Main(string[] args)
    {
        using var host = BuildHost(args);

        using var scope = host.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        if (args.Length == 0)
        {
            await RunInteractiveAsync(mediator);
            return;
        }

        await HandleCommandAsync(args, mediator);
    }

    private static IHost BuildHost(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(config =>
            {
                config.AddJsonFile(
                    Path.Combine("source", "PortfolioCalculator.ConsoleApp", "appsettings.json"),
                    optional: false,
                    reloadOnChange: true);

                config.AddEnvironmentVariables();
            })
            .ConfigureServices((context, services) =>
            {
                services.AddMediatR(cfg =>
                    cfg.RegisterServicesFromAssembly(typeof(ImportAllCsvCommand).Assembly));

                services.AddInfrastructure(context.Configuration);
            })
            .Build();
    }

    private static async Task RunInteractiveAsync(IMediator mediator)
    {
        PrintHelp();

        while (true)
        {
            Console.Write("> ");
            var line = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(line))
                break;

            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            await HandleCommandAsync(parts, mediator);
        }
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Enter commands like:");
        Console.WriteLine("  import <path-to-csv-folder>(optional) - Import CSV files (e.g. import ./data)");
        Console.WriteLine("  value Investor0 2019-12-31 - Run Portfolio Calculation");
        Console.WriteLine("Empty line to exit.");
        Console.WriteLine();
    }

    private static async Task HandleCommandAsync(string[] args, IMediator mediator)
    {
        if (args.Length == 0)
            return;

        var command = args[0];

        try
        {
            switch (command.ToLowerInvariant())
            {
                case "import":
                    await HandleImportAsync(args, mediator);
                    break;

                case "value":
                    HandleValue();
                    break;

                default:
                    Console.WriteLine($"Unknown command: {command}");
                    break;
            }
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"File not found: {ex.Message}");
        }
        catch (DirectoryNotFoundException ex)
        {
            Console.WriteLine($"Directory not found: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unexpected error occurred:");
            Console.WriteLine(ex.Message);
        }
    }

    private static async Task HandleImportAsync(string[] args, IMediator mediator)
    {
        var folderArg = args.Length >= 2 ? args[1] : DefaultDataFolder;

        if (!Directory.Exists(folderArg))
        {
            Console.WriteLine($"Error: Folder not found: {folderArg}");
            return;
        }

        var result = await mediator.Send(new ImportAllCsvCommand(folderArg));

        Console.WriteLine("Import completed:");
        Console.WriteLine($"  Investments rows:  {result.InvestmentsRows}");
        Console.WriteLine($"  Transactions rows: {result.TransactionsRows}");
        Console.WriteLine($"  Quotes rows:       {result.QuotesRows}");
    }

    private static void HandleValue()
    {
        Console.WriteLine("Command 'value' is not wired yet in this CLI build.");
        Console.WriteLine("Usage will be: value <investorId> <yyyy-mm-dd>");
    }
}
