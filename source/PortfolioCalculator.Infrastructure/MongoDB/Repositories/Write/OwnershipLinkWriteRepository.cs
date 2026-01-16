using MongoDB.Driver;
using PortfolioCalculator.Infrastructure.MongoDB.Documents;
using PortfolioCalculator.Infrastructure.MongoDB.Init;
using PortfolioCalculator.Infrastructure.MongoDB.Repositories.Write.Interfaces;

namespace PortfolioCalculator.Infrastructure.MongoDB.Repositories.Write
{
    public sealed class OwnershipLinkWriteRepository : IOwnershipLinkWriteRepository
    {
        private readonly MongoContext _mongoContext;

        public OwnershipLinkWriteRepository(MongoContext mongoContext)
        {
            _mongoContext = mongoContext;
        }

        public async Task BulkUpsertAsync(IReadOnlyCollection<WriteModel<OwnershipLinkDocument>> models, CancellationToken ct)
        {
            if (models.Count == 0) return;

            await _mongoContext.OwnershipLinks.BulkWriteAsync(
                models,
                new BulkWriteOptions { IsOrdered = false },
                ct
            );
        }
    }
}
