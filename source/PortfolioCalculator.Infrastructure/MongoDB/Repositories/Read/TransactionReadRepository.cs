using MongoDB.Driver;
using PortfolioCalculator.Application.Abstractions.Repositories.Models;
using PortfolioCalculator.Application.Abstractions.Repositories.Read;
using PortfolioCalculator.Domain.Enums;
using PortfolioCalculator.Infrastructure.MongoDB.Init;

namespace PortfolioCalculator.Infrastructure.MongoDB.Repositories.Read
{
    public sealed class TransactionReadRepository : ITransactionReadRepository
    {
        private readonly MongoContext _mongoContext;

        public TransactionReadRepository(MongoContext mongoContext)
        {
            _mongoContext = mongoContext;
        }

        public async Task<IReadOnlyList<TransactionModel>> GetUpToDateTransactionsAsync(string investmentId, DateTime referenceDate, CancellationToken ct)
        {
            var docs = await _mongoContext.Transactions
                .Find(x => x.InvestmentId == investmentId && x.Date <= referenceDate)
                .ToListAsync(ct);

            var transactionModels = new List<TransactionModel>();

            foreach (var document in docs)
            {
                if (!Enum.TryParse<TransactionType>(
                            document.Type,
                            ignoreCase: true,
                            out var transactionType))
                {
                    throw new InvalidOperationException(
                        $"Unknown transaction type '{document.Type}'");
                }

                var transaction = new TransactionModel(
                    document.InvestmentId,
                    document.Date,
                    transactionType,
                    document.Value);

                transactionModels.Add(transaction);
            }

            return transactionModels;
        }
    }
}
