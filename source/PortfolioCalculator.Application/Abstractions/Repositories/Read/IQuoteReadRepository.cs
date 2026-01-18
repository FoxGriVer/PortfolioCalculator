namespace PortfolioCalculator.Application.Abstractions.Repositories.Read
{
    public interface IQuoteReadRepository
    {
        Task<decimal?> GetLatestPriceAsync(
            string isin,
            DateTime referenceDate,
            CancellationToken ct);

        Task<IReadOnlyDictionary<string, decimal?>> GetLatestPricesByIsinsAsync(
            IReadOnlyCollection<string> isins,
            DateTime referenceDate,
            CancellationToken ct);
    }
}
