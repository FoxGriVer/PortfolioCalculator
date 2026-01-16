using PortfolioCalculator.Application.Abstractions.Repositories.Models;

namespace PortfolioCalculator.Application.Abstractions.Repositories.Read
{
    public interface IInvestmentReadRepository
    {
        Task<InvestmentInfoModel?> GetByIdAsync(string investmentId, CancellationToken ct);

        Task<IReadOnlyDictionary<string, InvestmentInfoModel>> GetByIdsAsync(
            IReadOnlyCollection<string> investmentIds,
            CancellationToken ct);
    }
}
