using MediatR;
using PortfolioCalculator.Application.Import;

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

    private static void HandleValue()
    {
        Console.WriteLine("Command 'value' is not wired yet in this CLI build.");
        Console.WriteLine("Usage will be: value <investorId> <yyyy-mm-dd>");
    }
}
