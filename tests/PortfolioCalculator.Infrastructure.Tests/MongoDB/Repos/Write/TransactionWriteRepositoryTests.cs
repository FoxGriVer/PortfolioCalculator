using FluentAssertions;
using MongoDB.Driver;
using PortfolioCalculator.Infrastructure.MongoDB.Documents;
using PortfolioCalculator.Infrastructure.MongoDB.Init;
using PortfolioCalculator.Infrastructure.MongoDB.Repos.Write;
using Testcontainers.MongoDb;

namespace PortfolioCalculator.Infrastructure.Tests.MongoDB.Repos.Write
{
    public class TransactionWriteRepositoryTests : IAsyncLifetime
    {
        private readonly MongoDbContainer _mongo = new MongoDbBuilder()
            .WithImage("mongo:7")
            .WithUsername("root")
            .WithPassword("rootpassword")
            .Build();

        private MongoContext _ctx = default!;
        private TransactionWriteRepository _repo = default!;
        private IMongoDatabase _db = default!;

        public async Task InitializeAsync()
        {
            await _mongo.StartAsync();

            var client = new MongoClient(_mongo.GetConnectionString());
            _db = client.GetDatabase("PortfolioCalculator_test");

            _ctx = new MongoContext(_db);
            _repo = new TransactionWriteRepository(_ctx);
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
                new TransactionDocument
                {
                    InvestmentId = "INV1",
                    Type = "Shares",
                    Date = new DateTime(2019, 12, 30, 0, 0, 0, DateTimeKind.Utc),
                    Value = 10m
                },
                new TransactionDocument
                {
                    InvestmentId = "INV1",
                    Type = "Shares",
                    Date = new DateTime(2019, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                    Value = -2m
                },
                new TransactionDocument
                {
                    InvestmentId = "INV2",
                    Type = "Estate",
                    Date = new DateTime(2019, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                    Value = 100000m
                },
            };

            // Act
            await _repo.InsertManyAsync(docs, CancellationToken.None);

            // Assert
            var count = await _ctx.Transactions.CountDocumentsAsync(Builders<TransactionDocument>.Filter.Empty);
            count.Should().Be(3);
        }

        [Fact]
        public async Task DeleteAllAsync_DeletesDocs()
        {
            // Arrange
            await _ctx.Transactions.InsertManyAsync(new[]
            {
                new TransactionDocument
                {
                    InvestmentId = "INV1",
                    Type = "Shares",
                    Date = new DateTime(2019, 12, 30, 0, 0, 0, DateTimeKind.Utc),
                    Value = 10m
                }
            });

            // Act
            await _repo.DeleteAllAsync(CancellationToken.None);

            // Assert
            var count = await _ctx.Transactions.CountDocumentsAsync(Builders<TransactionDocument>.Filter.Empty);
            count.Should().Be(0);
        }

        [Fact]
        public async Task InsertManyAsync_WhenEmpty_DoesNothing()
        {
            // Act
            await _repo.InsertManyAsync(Array.Empty<TransactionDocument>(), CancellationToken.None);

            // Assert
            var count = await _ctx.Transactions.CountDocumentsAsync(Builders<TransactionDocument>.Filter.Empty);
            count.Should().Be(0);
        }
    }
}
