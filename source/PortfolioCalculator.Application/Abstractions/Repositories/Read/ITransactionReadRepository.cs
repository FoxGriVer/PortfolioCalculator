using PortfolioCalculator.Application.Abstractions.Repositories.Models;

namespace PortfolioCalculator.Application.Abstractions.Repositories.Read
{
    public interface ITransactionReadRepository
    {
        Task<IReadOnlyList<TransactionModel>> GetUpToDateTransactionsAsync(string investmentId, DateTime referenceDate, CancellationToken ct);
    }
}
