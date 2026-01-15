using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PortfolioCalculator.Infrastructure.MongoDB.Documents
{
    public sealed class QuoteDocument
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string StockId { get; set; } = default!;

        public DateTime Date { get; set; }

        public decimal Price { get; set; }
    }
}
