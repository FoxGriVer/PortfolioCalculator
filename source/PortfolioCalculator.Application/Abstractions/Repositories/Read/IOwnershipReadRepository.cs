namespace PortfolioCalculator.Application.Abstractions.Repositories.Read
{
    public interface IOwnershipReadRepository
    {
        Task<IReadOnlyList<string>> GetOwnedInvestmentIdsAsync(string ownerType, string ownerId, CancellationToken ct);
    }
}
