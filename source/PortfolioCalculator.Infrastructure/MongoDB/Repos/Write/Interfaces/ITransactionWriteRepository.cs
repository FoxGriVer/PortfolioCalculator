using PortfolioCalculator.Infrastructure.MongoDB.Documents;

namespace PortfolioCalculator.Infrastructure.MongoDB.Repos.Write.Interfaces
{
    public interface ITransactionWriteRepository
    {
        Task DeleteAllAsync(CancellationToken ct);
        Task InsertManyAsync(IReadOnlyCollection<TransactionDocument> docs, CancellationToken ct);
    }
}
