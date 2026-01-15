using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PortfolioCalculator.Infrastructure.MongoDB.Documents
{
    public sealed class OwnershipLinkDocument
    {
        [BsonId]
        public ObjectId Id { get; set; }

        // "Investor" | "Fund"
        public string OwnerType { get; set; } = default!;

        public string OwnerId { get; set; } = default!;

        public string InvestmentId { get; set; } = default!;
    }
}
