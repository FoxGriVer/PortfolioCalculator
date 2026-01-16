using FluentAssertions;
using MongoDB.Driver;
using PortfolioCalculator.Infrastructure.MongoDB.Documents;
using PortfolioCalculator.Infrastructure.MongoDB.Init;
using PortfolioCalculator.Infrastructure.MongoDB.Repositories.Write;
using Testcontainers.MongoDb;

namespace PortfolioCalculator.Infrastructure.Tests.MongoDB.Repos.Write
{
    public class OwnershipLinkWriteRepositoryTests : IAsyncLifetime
    {
        private readonly MongoDbContainer _mongo = new MongoDbBuilder()
            .WithImage("mongo:7")
            .WithUsername("root")
            .WithPassword("rootpassword")
            .Build();

        private MongoContext _mongoContext = default!;
        private OwnershipLinkWriteRepository _repo = default!;
        private IMongoDatabase _db = default!;

        public async Task InitializeAsync()
        {
            await _mongo.StartAsync();

            var client = new MongoClient(_mongo.GetConnectionString());
            _db = client.GetDatabase("PortfolioCalculator_test");

            _mongoContext = new MongoContext(_db);
            _repo = new OwnershipLinkWriteRepository(_mongoContext);
        }

        public async Task DisposeAsync()
        {
            await _mongo.DisposeAsync();
        }

        [Fact]
        public async Task BulkUpsertAsync_InsertsLinks()
        {
            // Arrange
            var models = new List<WriteModel<OwnershipLinkDocument>>
            {
                CreateUpsertModel("Investor", "Investor1", "INV1"),
                CreateUpsertModel("Fund", "Fonds1", "INV2"),
            };

            // Act
            await _repo.BulkUpsertAsync(models, CancellationToken.None);

            // Assert
            var count = await _mongoContext.OwnershipLinks.CountDocumentsAsync(Builders<OwnershipLinkDocument>.Filter.Empty);
            count.Should().Be(2);

            var inv1 = await _mongoContext.OwnershipLinks.Find(x =>
                x.OwnerType == "Investor" && x.OwnerId == "Investor1" && x.InvestmentId == "INV1"
            ).FirstOrDefaultAsync();

            inv1.Should().NotBeNull();
        }

        [Fact]
        public async Task BulkUpsertAsync_WhenCalledTwice_DoesNotCreateDuplicates()
        {
            // Arrange
            var models = new List<WriteModel<OwnershipLinkDocument>>
            {
                CreateUpsertModel("Investor", "Investor1", "INV1"),
            };

            // Act
            await _repo.BulkUpsertAsync(models, CancellationToken.None);
            await _repo.BulkUpsertAsync(models, CancellationToken.None);

            // Assert
            var count = await _mongoContext.OwnershipLinks.CountDocumentsAsync(Builders<OwnershipLinkDocument>.Filter.Empty);
            count.Should().Be(1);
        }

        [Fact]
        public async Task BulkUpsertAsync_WhenEmpty_DoesNothing()
        {
            // Act
            await _repo.BulkUpsertAsync(Array.Empty<WriteModel<OwnershipLinkDocument>>(), CancellationToken.None);

            // Assert
            var count = await _mongoContext.OwnershipLinks.CountDocumentsAsync(Builders<OwnershipLinkDocument>.Filter.Empty);
            count.Should().Be(0);
        }

        private static UpdateOneModel<OwnershipLinkDocument> CreateUpsertModel(string ownerType, string ownerId, string investmentId)
        {
            var filter = Builders<OwnershipLinkDocument>.Filter.And(
                Builders<OwnershipLinkDocument>.Filter.Eq(x => x.OwnerType, ownerType),
                Builders<OwnershipLinkDocument>.Filter.Eq(x => x.OwnerId, ownerId),
                Builders<OwnershipLinkDocument>.Filter.Eq(x => x.InvestmentId, investmentId)
            );

            var update = Builders<OwnershipLinkDocument>.Update
                .SetOnInsert(x => x.OwnerType, ownerType)
                .SetOnInsert(x => x.OwnerId, ownerId)
                .SetOnInsert(x => x.InvestmentId, investmentId);

            return new UpdateOneModel<OwnershipLinkDocument>(filter, update)
            {
                IsUpsert = true
            };
        }
    }
}