using PortfolioCalculator.Domain.Enums;

namespace PortfolioCalculator.Application.Abstractions.Repositories.Read
{
    public interface IOwnershipReadRepository
    {
        Task<IReadOnlyList<string>> GetOwnedInvestmentIdsAsync(OwnerType ownerType, string ownerId, CancellationToken ct);
    }
}
