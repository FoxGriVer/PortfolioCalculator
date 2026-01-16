using MongoDB.Driver;
using PortfolioCalculator.Application.Abstractions.Repositories.Read;
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
    }
}
