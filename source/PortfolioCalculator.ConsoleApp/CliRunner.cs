using MediatR;
using PortfolioCalculator.Application.Import;
using PortfolioCalculator.Application.PortfolioValuation;
using PortfolioCalculator.Application.PortfolioValuation.DTOs;

namespace PortfolioCalculator.Cli;

public sealed class CliRunner
{
    private const string DefaultDataFolder = "./data";
    private readonly IMediator _mediator;

    public CliRunner(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task RunAsync(string[] args)
    {
        if (args.Length == 0)
        {
            await RunInteractiveAsync();
            return;
        }

        await HandleCommandAsync(args);
    }

    private async Task RunInteractiveAsync()
    {
        PrintHelp();

        while (true)
        {
            Console.Write("> ");
            var line = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(line))
                break;

            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            await HandleCommandAsync(parts);
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

    private async Task HandleCommandAsync(string[] args)
    {
        if (args.Length == 0)
            return;

        var command = args[0];

        try
        {
            switch (command.ToLowerInvariant())
            {
                case "import":
                    await HandleImportAsync(args);
                    break;

                case "value":
                    await HandleValueAsync(args);
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

    private async Task HandleImportAsync(string[] args)
    {
        var folderArg = args.Length >= 2 ? args[1] : DefaultDataFolder;

        if (!Directory.Exists(folderArg))
        {
            Console.WriteLine($"Error: Folder not found: {folderArg}");
            return;
        }

        Console.WriteLine($"Import started. Wait a few seconds ...");

        var result = await _mediator.Send(new ImportAllCsvCommand(folderArg));

        Console.WriteLine("Import completed:");
        Console.WriteLine($"  Investments rows:  {result.InvestmentsRows}");
        Console.WriteLine($"  Transactions rows: {result.TransactionsRows}");
        Console.WriteLine($"  Quotes rows:       {result.QuotesRows}");
    }

    private async Task HandleValueAsync(string[] args)
    {
        if (args.Length != 3)
        {
            Console.WriteLine("Usage: value <investorId> <yyyy-mm-dd>");
            Console.WriteLine("Example: value Investor0 2019-12-31");
            return;
        }

        var investorId = args[1];

        if (!DateTime.TryParseExact(
                args[2],
                "yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out var referenceDate))
        {
            Console.WriteLine("Invalid date format. Expected yyyy-mm-dd");
            Console.WriteLine("Example: value Investor0 2019-12-31");
            return;
        }

        Console.WriteLine($"Calculating portfolio value for '{investorId}' at {referenceDate:yyyy-MM-dd} ...");

        PortfolioValuationResultDto result = await _mediator.Send(
            new GetPortfolioValueQuery(investorId, referenceDate));

        Console.WriteLine();
        Console.WriteLine($"TOTAL: {result.TotalValue:N2}");
        Console.WriteLine();

        if (result.CompositionByType.Count == 0)
        {
            Console.WriteLine("No investments found.");
            return;
        }

        Console.WriteLine("Breakdown by investment type:");
        foreach (var item in result.CompositionByType.OrderByDescending(x => x.Value))
        {
            Console.WriteLine($"  {item.Type,-12} {item.Value,12:N2}");
        }

        Console.WriteLine();
    }
}
