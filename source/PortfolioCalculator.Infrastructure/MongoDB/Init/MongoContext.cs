using MongoDB.Driver;
using PortfolioCalculator.Infrastructure.MongoDB.Documents;

namespace PortfolioCalculator.Infrastructure.MongoDB.Init
{
    public sealed class MongoContext
    {
        public IMongoDatabase Db { get; }

        public IMongoCollection<InvestmentDocument> Investments => Db.GetCollection<InvestmentDocument>("investments");
        public IMongoCollection<OwnershipLinkDocument> OwnershipLinks => Db.GetCollection<OwnershipLinkDocument>("ownership_links");
        public IMongoCollection<TransactionDocument> Transactions => Db.GetCollection<TransactionDocument>("transactions");
        public IMongoCollection<QuoteDocument> Quotes => Db.GetCollection<QuoteDocument>("quotes");

        public MongoContext(IMongoDatabase mongoDatabase)
        {
            Db = mongoDatabase;
        }
    }
}
