using PortfolioCalculator.Infrastructure.MongoDB.Documents;

namespace PortfolioCalculator.Infrastructure.MongoDB.Repositories.Write.Interfaces
{
    public interface IQuoteWriteRepository
    {
        Task DeleteAllAsync(CancellationToken ct);
        Task InsertManyAsync(IReadOnlyCollection<QuoteDocument> docs, CancellationToken ct);
    }
}
