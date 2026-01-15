using MongoDB.Driver;
using PortfolioCalculator.Infrastructure.MongoDB.Documents;
using PortfolioCalculator.Infrastructure.MongoDB.Init;
using PortfolioCalculator.Infrastructure.MongoDB.Repos.Write.Interfaces;

namespace PortfolioCalculator.Infrastructure.MongoDB.Repos.Write
{
    public sealed class InvestmentWriteRepository : IInvestmentWriteRepository
    {
        private readonly MongoContext _mongoContext;

        public InvestmentWriteRepository(MongoContext mongoContext)
        {
            _mongoContext = mongoContext;
        }

        public async Task BulkUpsertAsync(IReadOnlyCollection<WriteModel<InvestmentDocument>> models, CancellationToken ct)
        {
            if (models.Count == 0) return;

            await _mongoContext.Investments.BulkWriteAsync(
                models,
                new BulkWriteOptions { IsOrdered = false },
                ct
            );
        }
    }
}
