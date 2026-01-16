namespace PortfolioCalculator.Application.Abstractions.Repositories.Read
{
    public interface IQuoteReadRepository
    {
        Task<decimal?> GetLatestPriceAsync(string isin, DateTime referenceDate, CancellationToken ct);
    }
}
