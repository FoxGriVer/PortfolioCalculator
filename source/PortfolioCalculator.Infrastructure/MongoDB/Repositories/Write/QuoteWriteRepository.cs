using MongoDB.Driver;
using PortfolioCalculator.Infrastructure.MongoDB.Documents;
using PortfolioCalculator.Infrastructure.MongoDB.Init;
using PortfolioCalculator.Infrastructure.MongoDB.Repositories.Write.Interfaces;

namespace PortfolioCalculator.Infrastructure.MongoDB.Repositories.Write
{
    public class QuoteWriteRepository : IQuoteWriteRepository
    {
        private readonly MongoContext _mongoContext;

        public QuoteWriteRepository(MongoContext mongoContext)
        {
            _mongoContext = mongoContext;
        }

        public Task DeleteAllAsync(CancellationToken ct)
        {
            return _mongoContext.Quotes.DeleteManyAsync(FilterDefinition<QuoteDocument>.Empty, ct);
        }

        public async Task InsertManyAsync(IReadOnlyCollection<QuoteDocument> docs, CancellationToken ct)
        {
            if (docs.Count == 0)
            {
                return;
            }

            await _mongoContext.Quotes.InsertManyAsync(docs, cancellationToken: ct);
        }
    }
}
