using MongoDB.Bson.Serialization.Attributes;

namespace PortfolioCalculator.Infrastructure.MongoDB.Documents
{
    public sealed class InvestmentDocument
    {
        [BsonId]
        public string Id { get; set; } = default!;

        // "Stock" | "RealEstate" | "Fund"
        public string Type { get; set; } = default!;

        // Used for Stock
        public string? ISIN { get; set; }

        // Used for RealEstate
        public string? City { get; set; }

        // Fund reference
        public string? FundId { get; set; }
    }
}
