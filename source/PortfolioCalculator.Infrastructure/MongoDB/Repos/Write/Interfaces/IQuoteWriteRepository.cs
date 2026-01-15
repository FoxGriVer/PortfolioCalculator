using PortfolioCalculator.Infrastructure.MongoDB.Documents;

namespace PortfolioCalculator.Infrastructure.MongoDB.Repos.Write.Interfaces
{
    public interface IQuoteWriteRepository
    {
        Task DeleteAllAsync(CancellationToken ct);
        Task InsertManyAsync(IReadOnlyCollection<QuoteDocument> docs, CancellationToken ct);
    }
}
