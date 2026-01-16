using FluentAssertions;
using MongoDB.Driver;
using PortfolioCalculator.Infrastructure.MongoDB.Documents;
using PortfolioCalculator.Infrastructure.MongoDB.Init;
using PortfolioCalculator.Infrastructure.MongoDB.Repositories.Read;
using Testcontainers.MongoDb;

namespace PortfolioCalculator.Infrastructure.Tests.MongoDB.Repos.Read
{
    public sealed class QuoteReadRepositoryTests : IAsyncLifetime
    {
        private readonly MongoDbContainer _mongo = new MongoDbBuilder()
            .WithImage("mongo:7")
            .WithUsername("root")
            .WithPassword("rootpassword")
            .Build();

        private MongoContext _mongoContext = default!;
        private QuoteReadRepository _repo = default!;
        private IMongoDatabase _db = default!;

        public async Task InitializeAsync()
        {
            await _mongo.StartAsync();

            var client = new MongoClient(_mongo.GetConnectionString());
            _db = client.GetDatabase("PortfolioCalculator_test");

            _mongoContext = new MongoContext(_db);
            _repo = new QuoteReadRepository(_mongoContext);

            var filter = Builders<QuoteDocument>.Filter.Empty;
            await _mongoContext.Quotes.DeleteManyAsync(filter);
        }

        public async Task DisposeAsync()
        {
            await _mongo.DisposeAsync();
        }

        [Fact]
        public async Task GetLatestPriceAsync_WhenNoQuotes_ReturnsNull()
        {
            // Arrange
            var isin = "ISIN1";
            var referenceDate = new DateTime(2019, 12, 31, 0, 0, 0, DateTimeKind.Utc);

            // Act
            var result = await _repo.GetLatestPriceAsync(
                isin,
                referenceDate,
                CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetLatestPriceAsync_WhenOnlyAfterReferenceDate_ReturnsNull()
        {
            // Arrange
            var isin = "ISIN1";
            var referenceDate = new DateTime(2019, 12, 31, 0, 0, 0, DateTimeKind.Utc);

            await _mongoContext.Quotes.InsertManyAsync(new[]
            {
                new QuoteDocument
                {
                    StockId = isin,
                    Date = new DateTime(2020, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                    Price = 200m
                }
            });

            // Act
            var result = await _repo.GetLatestPriceAsync(
                isin,
                referenceDate,
                CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetLatestPriceAsync_FiltersByIsin()
        {
            // Arrange
            var referenceDate = new DateTime(2019, 12, 31, 0, 0, 0, DateTimeKind.Utc);

            await _mongoContext.Quotes.InsertManyAsync(new[]
            {
                new QuoteDocument
                {
                    StockId = "ISIN1",
                    Date = new DateTime(2019, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                    Price = 101m
                },
                new QuoteDocument
                {
                    StockId = "ISIN2",
                    Date = new DateTime(2019, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                    Price = 999m
                }
            });

            // Act
            var result = await _repo.GetLatestPriceAsync(
                "ISIN1",
                referenceDate,
                CancellationToken.None);

            // Assert
            result.Should().Be(101m);
        }

        [Fact]
        public async Task GetLatestPriceAsync_UsesReferenceDateInclusive()
        {
            // Arrange
            var isin = "ISIN1";
            var referenceDate = new DateTime(2019, 12, 31, 0, 0, 0, DateTimeKind.Utc);

            await _mongoContext.Quotes.InsertManyAsync(new[]
            {
                new QuoteDocument
                {
                    StockId = isin,
                    Date = new DateTime(2019, 12, 30, 0, 0, 0, DateTimeKind.Utc),
                    Price = 100m
                },
                new QuoteDocument
                {
                    StockId = isin,
                    Date = new DateTime(2019, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                    Price = 101m
                }
            });

            // Act
            var result = await _repo.GetLatestPriceAsync(
                isin,
                referenceDate,
                CancellationToken.None);

            // Assert
            result.Should().Be(101m);
        }

        [Fact]
        public async Task GetLatestPriceAsync_ReturnsLatestPriceBeforeOrOnReferenceDate()
        {
            // Arrange
            var isin = "ISIN1";
            var referenceDate = new DateTime(2019, 12, 31, 0, 0, 0, DateTimeKind.Utc);

            await _mongoContext.Quotes.InsertManyAsync(new[]
            {
                new QuoteDocument
                {
                    StockId = isin,
                    Date = new DateTime(2019, 12, 29, 0, 0, 0, DateTimeKind.Utc),
                    Price = 99m
                },
                new QuoteDocument
                {
                    StockId = isin,
                    Date = new DateTime(2019, 12, 30, 0, 0, 0, DateTimeKind.Utc),
                    Price = 100m
                },
                new QuoteDocument
                {
                    StockId = isin,
                    Date = new DateTime(2019, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                    Price = 101m
                },
                new QuoteDocument
                {
                    StockId = isin,
                    Date = new DateTime(2020, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                    Price = 102m
                }
            });

            // Act
            var result = await _repo.GetLatestPriceAsync(
                isin,
                referenceDate,
                CancellationToken.None);

            // Assert
            result.Should().Be(101m);
        }
    }
}
