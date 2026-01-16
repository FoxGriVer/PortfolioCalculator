using FluentAssertions;
using MongoDB.Driver;
using PortfolioCalculator.Infrastructure.MongoDB.Documents;
using PortfolioCalculator.Infrastructure.MongoDB.Init;
using PortfolioCalculator.Infrastructure.MongoDB.Repositories.Read;
using Testcontainers.MongoDb;

namespace PortfolioCalculator.Infrastructure.Tests.MongoDB.Repos.Read
{
    public sealed class OwnershipReadRepositoryTests : IAsyncLifetime
    {
        private readonly MongoDbContainer _mongo = new MongoDbBuilder()
            .WithImage("mongo:7")
            .WithUsername("root")
            .WithPassword("rootpassword")
            .Build();

        private MongoContext _mongoContext = default!;
        private OwnershipReadRepository _repo = default!;
        private IMongoDatabase _db = default!;

        public async Task InitializeAsync()
        {
            await _mongo.StartAsync();

            var client = new MongoClient(_mongo.GetConnectionString());
            _db = client.GetDatabase("PortfolioCalculator_test");

            _mongoContext = new MongoContext(_db);
            _repo = new OwnershipReadRepository(_mongoContext);

            var filter = Builders<OwnershipLinkDocument>.Filter.Empty;
            await _mongoContext.OwnershipLinks.DeleteManyAsync(filter);
        }

        public async Task DisposeAsync()
        {
            await _mongo.DisposeAsync();
        }

        [Fact]
        public async Task GetOwnedInvestmentIdsAsync_WhenNoLinks_ReturnsEmpty()
        {
            // Arrange
            var ownerType = "Investor";
            var ownerId = "INVESTOR-1";

            // Act
            var result = await _repo.GetOwnedInvestmentIdsAsync(
                ownerType,
                ownerId,
                CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetOwnedInvestmentIdsAsync_FiltersByOwnerTypeAndOwnerId()
        {
            // Arrange
            await _mongoContext.OwnershipLinks.InsertManyAsync(new[]
            {
                new OwnershipLinkDocument
                {
                    OwnerType = "Investor",
                    OwnerId = "INVESTOR-1",
                    InvestmentId = "INV-1"
                },
                new OwnershipLinkDocument
                {
                    OwnerType = "Investor",
                    OwnerId = "INVESTOR-2",
                    InvestmentId = "INV-2"
                },
                new OwnershipLinkDocument
                {
                    OwnerType = "Fund",
                    OwnerId = "INVESTOR-1",
                    InvestmentId = "INV-3"
                }
            });

            // Act
            var result = await _repo.GetOwnedInvestmentIdsAsync(
                "Investor",
                "INVESTOR-1",
                CancellationToken.None);

            // Assert
            result.Should().HaveCount(1);
            result[0].Should().Be("INV-1");
        }

        [Fact]
        public async Task GetOwnedInvestmentIdsAsync_WhenMultipleLinks_ReturnsAllInvestmentIds()
        {
            // Arrange
            await _mongoContext.OwnershipLinks.InsertManyAsync(new[]
            {
                new OwnershipLinkDocument
                {
                    OwnerType = "Investor",
                    OwnerId = "INVESTOR-1",
                    InvestmentId = "INV-1"
                },
                new OwnershipLinkDocument
                {
                    OwnerType = "Investor",
                    OwnerId = "INVESTOR-1",
                    InvestmentId = "INV-2"
                }
            });

            // Act
            var result = await _repo.GetOwnedInvestmentIdsAsync(
                "Investor",
                "INVESTOR-1",
                CancellationToken.None);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain("INV-1");
            result.Should().Contain("INV-2");
        }
    }
}
