namespace PortfolioCalculator.Application.Abstractions.Import
{
    public interface ICsvImportService
    {
        Task<int> ImportInvestmentsAsync(string filePath, CancellationToken ct);
        Task<int> ImportTransactionsAsync(string filePath, CancellationToken ct);
        Task<int> ImportQuotesAsync(string filePath, CancellationToken ct);
    }
}
