using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PortfolioCalculator.Infrastructure.MongoDB.Documents
{
    public sealed class TransactionDocument
    {
        [BsonId]
        public ObjectId Id { get; set; }

        // Investment reference
        public string InvestmentId { get; set; } = default!;

        public DateTime Date { get; set; }

        // "Shares" | "Estate" | "Building" | "Percentage"
        public string Type { get; set; } = default!;

        public decimal Value { get; set; }
    }
}
