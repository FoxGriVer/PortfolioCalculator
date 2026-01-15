using MongoDB.Driver;
using PortfolioCalculator.Application.Abstractions.Database;
using PortfolioCalculator.Infrastructure.MongoDB.Documents;

namespace PortfolioCalculator.Infrastructure.MongoDB.Init
{
    public sealed class MongoIndexInitializer : IDatabaseInitializer
    {
        private readonly MongoContext _mongoContext;

        public MongoIndexInitializer(MongoContext mongoContext)
        {
            _mongoContext = mongoContext;
        }

        public async Task EnsureIndexesAsync(CancellationToken ct)
        {
            var txIndex = new CreateIndexModel<TransactionDocument>(
                Builders<TransactionDocument>.IndexKeys
                    .Ascending(x => x.InvestmentId)
                    .Ascending(x => x.Date)
            );
            await _mongoContext.Transactions.Indexes.CreateOneAsync(txIndex, cancellationToken: ct);

            var qIndex = new CreateIndexModel<QuoteDocument>(
                Builders<QuoteDocument>.IndexKeys
                    .Ascending(x => x.StockId)
                    .Ascending(x => x.Date)
            );
            await _mongoContext.Quotes.Indexes.CreateOneAsync(qIndex, cancellationToken: ct);

            var linkIndex = new CreateIndexModel<OwnershipLinkDocument>(
                Builders<OwnershipLinkDocument>.IndexKeys
                    .Ascending(x => x.OwnerType)
                    .Ascending(x => x.OwnerId)
            );
            await _mongoContext.OwnershipLinks.Indexes.CreateOneAsync(linkIndex, cancellationToken: ct);

            var linkUnique = new CreateIndexModel<OwnershipLinkDocument>(
                Builders<OwnershipLinkDocument>.IndexKeys
                    .Ascending(x => x.OwnerType)
                    .Ascending(x => x.OwnerId)
                    .Ascending(x => x.InvestmentId),
                new CreateIndexOptions { Unique = true }
            );
            await _mongoContext.OwnershipLinks.Indexes.CreateOneAsync(linkUnique, cancellationToken: ct);
        }
    }
}
