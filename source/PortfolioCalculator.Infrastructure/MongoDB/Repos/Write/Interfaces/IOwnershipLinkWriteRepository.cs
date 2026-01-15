using MongoDB.Driver;
using PortfolioCalculator.Infrastructure.MongoDB.Documents;

namespace PortfolioCalculator.Infrastructure.MongoDB.Repos.Write.Interfaces
{
    public interface IOwnershipLinkWriteRepository
    {
        Task BulkUpsertAsync(IReadOnlyCollection<WriteModel<OwnershipLinkDocument>> models, CancellationToken ct);
    }
}
