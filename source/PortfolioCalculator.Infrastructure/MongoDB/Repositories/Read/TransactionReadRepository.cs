using MongoDB.Driver;
using PortfolioCalculator.Application.Abstractions.Repositories.Models;
using PortfolioCalculator.Application.Abstractions.Repositories.Read;
using PortfolioCalculator.Domain.Enums;
using PortfolioCalculator.Infrastructure.MongoDB.Documents;
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

        public async Task<IReadOnlyDictionary<string, IReadOnlyList<TransactionModel>>> GetUpToDateTransactionsByInvestmentIdsAsync(
            IReadOnlyCollection<string> investmentIds,
            DateTime referenceDate,
            CancellationToken ct)
        {
            if (investmentIds == null || investmentIds.Count == 0)
            {
                return new Dictionary<string, IReadOnlyList<TransactionModel>>();
            }

            // 1) Один запрос: investmentId IN (...) AND date <= referenceDate
            var filter = Builders<TransactionDocument>.Filter.And(
                Builders<TransactionDocument>.Filter.In(x => x.InvestmentId, investmentIds),
                Builders<TransactionDocument>.Filter.Lte(x => x.Date, referenceDate));

            var docs = await _mongoContext.Transactions
                .Find(filter)
                .SortBy(x => x.InvestmentId) // удобно для группировки
                .ThenBy(x => x.Date)
                .ToListAsync(ct);

            // 2) Собираем Dictionary<string, List<TransactionModel>>
            var result = new Dictionary<string, List<TransactionModel>>(StringComparer.OrdinalIgnoreCase);

            foreach (var document in docs)
            {
                if (!Enum.TryParse<TransactionType>(document.Type, ignoreCase: true, out var transactionType))
                {
                    throw new InvalidOperationException($"Unknown transaction type '{document.Type}'");
                }

                var model = new TransactionModel(
                    document.InvestmentId,
                    document.Date,
                    transactionType,
                    document.Value);

                if (!result.TryGetValue(document.InvestmentId, out var list))
                {
                    list = new List<TransactionModel>();
                    result[document.InvestmentId] = list;
                }

                list.Add(model);
            }

            // 3) Превращаем в IReadOnlyDictionary<string, IReadOnlyList<TransactionModel>>
            var readOnly = new Dictionary<string, IReadOnlyList<TransactionModel>>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in result)
            {
                readOnly[kv.Key] = kv.Value;
            }

            return readOnly;
        }
    }
}
