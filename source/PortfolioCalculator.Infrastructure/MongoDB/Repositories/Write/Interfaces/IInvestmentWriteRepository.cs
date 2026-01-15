using MongoDB.Driver;
using PortfolioCalculator.Infrastructure.MongoDB.Documents;

namespace PortfolioCalculator.Infrastructure.MongoDB.Repos.Write.Interfaces
{
    public interface IInvestmentWriteRepository
    {
        Task BulkUpsertAsync(IReadOnlyCollection<WriteModel<InvestmentDocument>> models, CancellationToken ct);
    }
}
