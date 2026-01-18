using MongoDB.Driver;
using PortfolioCalculator.Application.Abstractions.Repositories.Read;
using PortfolioCalculator.Infrastructure.MongoDB.Documents;
using PortfolioCalculator.Infrastructure.MongoDB.Init;

namespace PortfolioCalculator.Infrastructure.MongoDB.Repositories.Read
{
    public sealed class QuoteReadRepository : IQuoteReadRepository
    {
        private readonly MongoContext _mongoContext;

        public QuoteReadRepository(MongoContext mongoContext)
        {
            _mongoContext = mongoContext;
        }

        public async Task<decimal?> GetLatestPriceAsync(string isin, DateTime referenceDate, CancellationToken ct)
        {
            var quote = await _mongoContext.Quotes
                .Find(q => q.StockId == isin && q.Date <= referenceDate)
                .SortByDescending(q => q.Date)
                .Limit(1)
                .FirstOrDefaultAsync(ct);

            return quote?.Price;
        }

        public async Task<IReadOnlyDictionary<string, decimal?>> GetLatestPricesByIsinsAsync(
            IReadOnlyCollection<string> isins,
            DateTime referenceDate,
            CancellationToken ct)
        {
            if (isins == null || isins.Count == 0)
                return new Dictionary<string, decimal?>(StringComparer.OrdinalIgnoreCase);

            // match: isin in (...) && date <= referenceDate
            var match = Builders<QuoteDocument>.Filter.And(
                Builders<QuoteDocument>.Filter.In(x => x.StockId, isins),
                Builders<QuoteDocument>.Filter.Lte(x => x.Date, referenceDate));

            // aggregate: sort by (ISIN asc, Date desc), group by ISIN take first
            var results = await _mongoContext.Quotes.Aggregate()
                .Match(match)
                .SortByDescending(x => x.Date) // важен desc по Date
                .Group(
                    x => x.StockId,
                    g => new
                    {
                        ISIN = g.Key,
                        LatestPrice = g.First().Price
                    })
                .ToListAsync(ct);

            var dict = new Dictionary<string, decimal?>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in results)
            {
                dict[item.ISIN] = item.LatestPrice;
            }

            // для ISIN-ов, у которых нет quote <= date, ключа просто не будет — это ок
            return dict;
        }
    }
}
