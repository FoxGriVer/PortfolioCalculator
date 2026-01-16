using MongoDB.Driver;
using PortfolioCalculator.Infrastructure.MongoDB.Documents;
using PortfolioCalculator.Infrastructure.MongoDB.Init;
using PortfolioCalculator.Infrastructure.MongoDB.Repositories.Write.Interfaces;

namespace PortfolioCalculator.Infrastructure.MongoDB.Repositories.Write
{
    public class TransactionWriteRepository : ITransactionWriteRepository
    {
        private readonly MongoContext _mongoContext;

        public TransactionWriteRepository(MongoContext mongoContext) => _mongoContext = mongoContext;

        public async Task DeleteAllAsync(CancellationToken ct)
        {
            await _mongoContext.Transactions.DeleteManyAsync(FilterDefinition<TransactionDocument>.Empty, ct);
        }

        public async Task InsertManyAsync(IReadOnlyCollection<TransactionDocument> docs, CancellationToken ct)
        {
            if (docs.Count == 0) return;
            await _mongoContext.Transactions.InsertManyAsync(docs, cancellationToken: ct);
        }
    }
}
