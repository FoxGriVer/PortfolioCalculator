using FluentAssertions;
using MongoDB.Driver;
using PortfolioCalculator.Infrastructure.MongoDB.Documents;
using PortfolioCalculator.Infrastructure.MongoDB.Init;
using PortfolioCalculator.Infrastructure.MongoDB.Repos.Write;
using Testcontainers.MongoDb;

namespace PortfolioCalculator.Infrastructure.Tests.MongoDB.Repos.Write
{
    public class InvestmentWriteRepositoryTests : IAsyncLifetime
    {
        private readonly MongoDbContainer _mongo = new MongoDbBuilder()
            .WithImage("mongo:7")
            .WithUsername("root")
            .WithPassword("rootpassword")
            .Build();

        private MongoContext _ctx = default!;
        private InvestmentWriteRepository _repo = default!;
        private IMongoDatabase _db = default!;

        public async Task InitializeAsync()
        {
            await _mongo.StartAsync();

            var client = new MongoClient(_mongo.GetConnectionString());
            _db = client.GetDatabase("PortfolioCalculator_test");

            _ctx = new MongoContext(_db);
            _repo = new InvestmentWriteRepository(_ctx);
        }

        public async Task DisposeAsync()
        {
            await _mongo.DisposeAsync();
        }

        [Fact]
        public async Task BulkUpsertAsync_InsertsInvestments()
        {
            // Arrange
            var models = new List<WriteModel<InvestmentDocument>>
            {
                CreateUpsertModel(
                    id: "INV1",
                    type: "Stock",
                    isin: "ISIN1",
                    city: null,
                    fundId: null
                ),
                CreateUpsertModel(
                    id: "INV2",
                    type: "RealEstate",
                    isin: null,
                    city: "Berlin",
                    fundId: null
                ),
            };

            // Act
            await _repo.BulkUpsertAsync(models, CancellationToken.None);

            // Assert
            var count = await _ctx.Investments.CountDocumentsAsync(Builders<InvestmentDocument>.Filter.Empty);
            count.Should().Be(2);

            var inv1 = await _ctx.Investments.Find(x => x.Id == "INV1").FirstOrDefaultAsync();
            inv1.Should().NotBeNull();
            inv1!.Type.Should().Be("Stock");
            inv1.ISIN.Should().Be("ISIN1");
        }

        [Fact]
        public async Task BulkUpsertAsync_WhenCalledTwice_UpdatesExistingDocument_NotDuplicate()
        {
            // Arrange
            var initial = new List<WriteModel<InvestmentDocument>>
            {
                CreateUpsertModel(
                    id: "INV1",
                    type: "Stock",
                    isin: "ISIN_OLD",
                    city: null,
                    fundId: null
                ),
            };

            // Act
            await _repo.BulkUpsertAsync(initial, CancellationToken.None);

            var updated = new List<WriteModel<InvestmentDocument>>
            {
                CreateUpsertModel(
                    id: "INV1",
                    type: "Stock",
                    isin: "ISIN_NEW",
                    city: null,
                    fundId: null
                ),
            };

            await _repo.BulkUpsertAsync(updated, CancellationToken.None);

            var count = await _ctx.Investments.CountDocumentsAsync(Builders<InvestmentDocument>.Filter.Empty);
            count.Should().Be(1);

            var inv1 = await _ctx.Investments.Find(x => x.Id == "INV1").FirstOrDefaultAsync();
            inv1.Should().NotBeNull();
            inv1!.ISIN.Should().Be("ISIN_NEW");
        }

        [Fact]
        public async Task BulkUpsertAsync_WhenEmpty_DoesNothing()
        {
            // Act
            await _repo.BulkUpsertAsync(Array.Empty<WriteModel<InvestmentDocument>>(), CancellationToken.None);

            // Assert
            var count = await _ctx.Investments.CountDocumentsAsync(Builders<InvestmentDocument>.Filter.Empty);
            count.Should().Be(0);
        }

        private static UpdateOneModel<InvestmentDocument> CreateUpsertModel(
            string id,
            string type,
            string? isin,
            string? city,
            string? fundId)
        {
            var filter = Builders<InvestmentDocument>.Filter.Eq(x => x.Id, id);

            var update = Builders<InvestmentDocument>.Update
                .SetOnInsert(x => x.Id, id)
                .Set(x => x.Type, type)
                .Set(x => x.ISIN, isin)
                .Set(x => x.City, city)
                .Set(x => x.FundId, fundId);

            return new UpdateOneModel<InvestmentDocument>(filter, update)
            {
                IsUpsert = true
            };
        }
    }
}
