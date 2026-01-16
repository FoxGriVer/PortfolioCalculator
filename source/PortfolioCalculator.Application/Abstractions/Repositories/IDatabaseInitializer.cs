namespace PortfolioCalculator.Application.Abstractions.Repositories
{
    public interface IDatabaseInitializer
    {
        Task EnsureIndexesAsync(CancellationToken ct);
    }
}
