using MongoDB.Driver;
using PortfolioCalculator.Application.Abstractions.Repositories.Read;
using PortfolioCalculator.Infrastructure.MongoDB.Init;

namespace PortfolioCalculator.Infrastructure.MongoDB.Repositories.Read
{
    public sealed class OwnershipReadRepository : IOwnershipReadRepository
    {
        private readonly MongoContext _mongoContext;

        public OwnershipReadRepository(MongoContext mongoContext)
        {
            _mongoContext = mongoContext;
        }

        public async Task<IReadOnlyList<string>> GetOwnedInvestmentIdsAsync(string ownerType, string ownerId, CancellationToken ct)
        {
            var ids = await _mongoContext.OwnershipLinks
                .Find(x => x.OwnerType == ownerType && x.OwnerId == ownerId)
                .Project(x => x.InvestmentId)
                .ToListAsync(ct);

            return ids;
        }
    }
}
