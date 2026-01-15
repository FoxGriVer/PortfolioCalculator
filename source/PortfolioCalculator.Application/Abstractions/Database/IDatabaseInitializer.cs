namespace PortfolioCalculator.Application.Abstractions.Database
{
    public interface IDatabaseInitializer
    {
        Task EnsureIndexesAsync(CancellationToken ct);
    }
}
