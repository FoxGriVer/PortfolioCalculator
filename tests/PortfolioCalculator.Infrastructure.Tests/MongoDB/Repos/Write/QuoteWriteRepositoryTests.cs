using FluentAssertions;
using MongoDB.Driver;
using PortfolioCalculator.Infrastructure.MongoDB.Documents;
using PortfolioCalculator.Infrastructure.MongoDB.Init;
using PortfolioCalculator.Infrastructure.MongoDB.Repositories.Write;
using Testcontainers.MongoDb;

namespace PortfolioCalculator.Infrastructure.Tests.MongoDB.Repos.Write
{
    public class QuoteWriteRepositoryTests : IAsyncLifetime
    {
        private readonly MongoDbContainer _mongo = new MongoDbBuilder()
            .WithImage("mongo:7")
            .WithUsername("root")
            .WithPassword("rootpassword")
            .Build();

        private MongoContext _mongoContext = default!;
        private QuoteWriteRepository _repo = default!;
        private IMongoDatabase _db = default!;

        public async Task InitializeAsync()
        {
            await _mongo.StartAsync();

            var client = new MongoClient(_mongo.GetConnectionString());
            _db = client.GetDatabase("PortfolioCalculator_test");

            _mongoContext = new MongoContext(_db);
            _repo = new QuoteWriteRepository(_mongoContext);
        }

        public async Task DisposeAsync()
        {
            await _mongo.DisposeAsync();
        }

        [Fact]
        public async Task InsertManyAsync_InsertsDocs()
        {
            // Arrange
            var docs = new[]
            {
            new QuoteDocument { StockId = "ISIN1", Date = new DateTime(2019, 12, 30, 0, 0, 0, DateTimeKind.Utc), Price = 100m },
            new QuoteDocument { StockId = "ISIN1", Date = new DateTime(2019, 12, 31, 0, 0, 0, DateTimeKind.Utc), Price = 101m },
        };

            // Act
            await _repo.InsertManyAsync(docs, CancellationToken.None);

            // Assert
            var count = await _mongoContext.Quotes.CountDocumentsAsync(Builders<QuoteDocument>.Filter.Empty);
            count.Should().Be(2);
        }

        [Fact]
        public async Task DeleteAllAsync_DeletesDocs()
        {
            // Arrange
            await _mongoContext.Quotes.InsertManyAsync(new[]
            {
                new QuoteDocument { StockId = "ISIN1", Date = new DateTime(2019, 12, 30, 0, 0, 0, DateTimeKind.Utc), Price = 100m },
            });

            // Act
            await _repo.DeleteAllAsync(CancellationToken.None);

            // Assert
            var count = await _mongoContext.Quotes.CountDocumentsAsync(Builders<QuoteDocument>.Filter.Empty);
            count.Should().Be(0);
        }

        [Fact]
        public async Task InsertManyAsync_WhenEmpty_DoesNothing()
        {
            // Act
            await _repo.InsertManyAsync(Array.Empty<QuoteDocument>(), CancellationToken.None);

            // Assert
            var count = await _mongoContext.Quotes.CountDocumentsAsync(Builders<QuoteDocument>.Filter.Empty);
            count.Should().Be(0);
        }
    }
}
