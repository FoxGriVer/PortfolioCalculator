using MediatR;
using PortfolioCalculator.Application.Abstractions.Import;
using PortfolioCalculator.Application.Abstractions.Repositories;

namespace PortfolioCalculator.Application.Import
{
    public sealed class ImportAllCsvCommandHandler : IRequestHandler<ImportAllCsvCommand, ImportAllCsvResult>
    {
        private readonly IDatabaseInitializer _databaseInitializer;
        private readonly ICsvImportService _csvImportService;

        public ImportAllCsvCommandHandler(IDatabaseInitializer databaseInitializer, ICsvImportService csvImportService)
        {
            _databaseInitializer = databaseInitializer;
            _csvImportService = csvImportService;
        }

        public async Task<ImportAllCsvResult> Handle(ImportAllCsvCommand request, CancellationToken cancellationToken)
        {
            await _databaseInitializer.EnsureIndexesAsync(cancellationToken);

            var investmentsPath = Path.Combine(request.FolderPath, "Investments.csv");
            var transactionsPath = Path.Combine(request.FolderPath, "Transactions.csv");
            var quotesPath = Path.Combine(request.FolderPath, "Quotes.csv");

            if (!File.Exists(investmentsPath))
                throw new FileNotFoundException($"File not found: {investmentsPath}");
            if (!File.Exists(transactionsPath))
                throw new FileNotFoundException($"File not found: {transactionsPath}");
            if (!File.Exists(quotesPath))
                throw new FileNotFoundException($"File not found: {quotesPath}");

            var investments = await _csvImportService.ImportInvestmentsAsync(investmentsPath, cancellationToken);
            var transactions = await _csvImportService.ImportTransactionsAsync(transactionsPath, cancellationToken);
            var quotes = await _csvImportService.ImportQuotesAsync(quotesPath, cancellationToken);

            return new ImportAllCsvResult(investments, transactions, quotes);
        }
    }
}
