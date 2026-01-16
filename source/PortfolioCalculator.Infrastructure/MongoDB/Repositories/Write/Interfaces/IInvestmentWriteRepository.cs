using MongoDB.Driver;
using PortfolioCalculator.Infrastructure.MongoDB.Documents;

namespace PortfolioCalculator.Infrastructure.MongoDB.Repositories.Write.Interfaces
{
    public interface IInvestmentWriteRepository
    {
        Task BulkUpsertAsync(IReadOnlyCollection<WriteModel<InvestmentDocument>> models, CancellationToken ct);
    }
}
