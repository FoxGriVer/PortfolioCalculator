using FluentAssertions;
using MongoDB.Driver;
using PortfolioCalculator.Application.Abstractions.Repositories.Models;
using PortfolioCalculator.Infrastructure.MongoDB.Documents;
using PortfolioCalculator.Infrastructure.MongoDB.Init;
using PortfolioCalculator.Infrastructure.MongoDB.Repositories.Read;
using Testcontainers.MongoDb;

namespace PortfolioCalculator.Infrastructure.Tests.MongoDB.Repos.Read
{
    public sealed class InvestmentReadRepositoryTests : IAsyncLifetime
    {
        private readonly MongoDbContainer _mongo = new MongoDbBuilder()
            .WithImage("mongo:7")
            .WithUsername("root")
            .WithPassword("rootpassword")
            .Build();

        private MongoContext _mongoContext = default!;
        private InvestmentReadRepository _repo = default!;
        private IMongoDatabase _db = default!;

        public async Task InitializeAsync()
        {
            await _mongo.StartAsync();

            var client = new MongoClient(_mongo.GetConnectionString());
            _db = client.GetDatabase("PortfolioCalculator_test");

            _mongoContext = new MongoContext(_db);
            _repo = new InvestmentReadRepository(_mongoContext);

            var filter = Builders<InvestmentDocument>.Filter.Empty;
            await _mongoContext.Investments.DeleteManyAsync(filter);
        }

        public async Task DisposeAsync()
        {
            await _mongo.DisposeAsync();
        }

        [Fact]
        public async Task GetByIdAsync_WhenFound_ReturnsMappedModel()
        {
            // Arrange
            var doc = new InvestmentDocument
            {
                Id = "INV-1",
                Type = "ETF",
                ISIN = "ISIN123",
                City = "Berlin",
                FundId = "FUND-9"
            };

            await _mongoContext.Investments.InsertOneAsync(doc, cancellationToken: CancellationToken.None);

            // Act
            var result = await _repo.GetByIdAsync("INV-1", CancellationToken.None);

            // Assert
            result.Should().NotBeNull();

            var expected = new InvestmentInfoModel(
                "INV-1",
                "ETF",
                "ISIN123",
                "Berlin",
                "FUND-9");

            result.Should().Be(expected);
        }

        [Fact]
        public async Task GetByIdsAsync_WhenEmptyInput_ReturnsEmptyDictionary()
        {
            // Arrange
            var ids = Array.Empty<string>();

            // Act
            var result = await _repo.GetByIdsAsync(ids, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdsAsync_WhenSomeIdsMissing_ReturnsOnlyFoundOnes()
        {
            // Arrange
            var docs = new[]
            {
                new InvestmentDocument
                {
                    Id = "INV-1",
                    Type = "ETF",
                    ISIN = "ISIN1",
                    City = "Berlin",
                    FundId = "FUND-1"
                },
                new InvestmentDocument
                {
                    Id = "INV-2",
                    Type = "Stock",
                    ISIN = "ISIN2",
                    City = "Munich",
                    FundId = "FUND-2"
                }
            };

            await _mongoContext.Investments.InsertManyAsync(docs, cancellationToken: CancellationToken.None);

            var requestedIds = new[] { "INV-1", "INV-404", "INV-2" };

            // Act
            var result = await _repo.GetByIdsAsync(requestedIds, CancellationToken.None);

            // Assert
            result.Should().HaveCount(2);

            result.ContainsKey("INV-1").Should().BeTrue();
            result.ContainsKey("INV-2").Should().BeTrue();
            result.ContainsKey("INV-404").Should().BeFalse();

            result["INV-1"].Should().Be(new InvestmentInfoModel(
                "INV-1",
                "ETF",
                "ISIN1",
                "Berlin",
                "FUND-1"));

            result["INV-2"].Should().Be(new InvestmentInfoModel(
                "INV-2",
                "Stock",
                "ISIN2",
                "Munich",
                "FUND-2"));
        }

        [Fact]
        public async Task GetByIdsAsync_WhenFound_ReturnsDictionaryKeyedById()
        {
            // Arrange
            var doc = new InvestmentDocument
            {
                Id = "INV-9",
                Type = "Bond",
                ISIN = "ISIN9",
                City = "Hamburg",
                FundId = "FUND-9"
            };

            await _mongoContext.Investments.InsertOneAsync(doc, cancellationToken: CancellationToken.None);

            var requestedIds = new[] { "INV-9" };

            // Act
            var result = await _repo.GetByIdsAsync(requestedIds, CancellationToken.None);

            // Assert
            result.Should().HaveCount(1);
            result.Keys.Single().Should().Be("INV-9");
        }
    }
}
