using PortfolioCalculator.Infrastructure.MongoDB.Documents;

namespace PortfolioCalculator.Infrastructure.MongoDB.Repositories.Write.Interfaces
{
    public interface ITransactionWriteRepository
    {
        Task DeleteAllAsync(CancellationToken ct);
        Task InsertManyAsync(IReadOnlyCollection<TransactionDocument> docs, CancellationToken ct);
    }
}
