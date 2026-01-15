namespace PortfolioCalculator.Infrastructure.MongoDB.Configuration
{
    public sealed class MongoDBSettings
    {
        public string ConnectionString { get; set; } = default!;
        public string Database { get; set; } = default!;
    }
}
