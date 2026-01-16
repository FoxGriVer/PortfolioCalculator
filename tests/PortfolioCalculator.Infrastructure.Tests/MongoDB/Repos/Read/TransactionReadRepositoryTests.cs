using FluentAssertions;
using MongoDB.Driver;
using PortfolioCalculator.Application.Abstractions.Repositories.Models;
using PortfolioCalculator.Domain.Enums;
using PortfolioCalculator.Infrastructure.MongoDB.Documents;
using PortfolioCalculator.Infrastructure.MongoDB.Init;
using PortfolioCalculator.Infrastructure.MongoDB.Repositories.Read;
using Testcontainers.MongoDb;

namespace PortfolioCalculator.Infrastructure.Tests.MongoDB.Repos.Read
{
    public sealed class TransactionReadRepositoryTests : IAsyncLifetime
    {
        private readonly MongoDbContainer _mongo = new MongoDbBuilder()
            .WithImage("mongo:7")
            .WithUsername("root")
            .WithPassword("rootpassword")
            .Build();

        private MongoContext _mongoContext = default!;
        private TransactionReadRepository _repo = default!;
        private IMongoDatabase _db = default!;

        public async Task InitializeAsync()
        {
            await _mongo.StartAsync();

            var client = new MongoClient(_mongo.GetConnectionString());
            _db = client.GetDatabase("PortfolioCalculator_test");

            _mongoContext = new MongoContext(_db);
            _repo = new TransactionReadRepository(_mongoContext);

            var filter = Builders<TransactionDocument>.Filter.Empty;
            await _mongoContext.Transactions.DeleteManyAsync(filter);
        }

        public async Task DisposeAsync()
        {
            await _mongo.DisposeAsync();
        }

        [Fact]
        public async Task GetUpToDateAsync_WhenNoTransactions_ReturnsEmpty()
        {
            // Arrange
            var investmentId = "INV-1";
            var referenceDate = new DateTime(2019, 12, 31, 0, 0, 0, DateTimeKind.Utc);

            // Act
            var result = await _repo.GetUpToDateTransactionsAsync(
                investmentId,
                referenceDate,
                CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetUpToDateAsync_FiltersByInvestmentId()
        {
            // Arrange
            var referenceDate = new DateTime(2019, 12, 31, 0, 0, 0, DateTimeKind.Utc);

            await _mongoContext.Transactions.InsertManyAsync(new[]
            {
                new TransactionDocument
                {
                    InvestmentId = "INV-1",
                    Date = new DateTime(2019, 12, 30, 0, 0, 0, DateTimeKind.Utc),
                    Type = TransactionType.Shares.ToString(),
                    Value = 100m
                },
                new TransactionDocument
                {
                    InvestmentId = "INV-OTHER",
                    Date = new DateTime(2019, 12, 30, 0, 0, 0, DateTimeKind.Utc),
                    Type = TransactionType.Shares.ToString(),
                    Value = 999m
                }
            });

            // Act
            var result = await _repo.GetUpToDateTransactionsAsync(
                "INV-1",
                referenceDate,
                CancellationToken.None);

            // Assert
            result.Should().HaveCount(1);

            result[0].InvestmentId.Should().Be("INV-1");
            result[0].Type.Should().Be(TransactionType.Shares);
            result[0].Value.Should().Be(100m);
        }

        [Fact]
        public async Task GetUpToDateAsync_FiltersByDateInclusive()
        {
            // Arrange
            var investmentId = "INV-1";
            var referenceDate = new DateTime(2019, 12, 31, 0, 0, 0, DateTimeKind.Utc);

            await _mongoContext.Transactions.InsertManyAsync(new[]
            {
                new TransactionDocument
                {
                    InvestmentId = investmentId,
                    Date = new DateTime(2019, 12, 30, 0, 0, 0, DateTimeKind.Utc),
                    Type = TransactionType.Shares.ToString(),
                    Value = 100m
                },
                new TransactionDocument
                {
                    InvestmentId = investmentId,
                    Date = new DateTime(2019, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                    Type = TransactionType.Estate.ToString(),
                    Value = 50m
                },
                new TransactionDocument
                {
                    InvestmentId = investmentId,
                    Date = new DateTime(2020, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                    Type = TransactionType.Shares.ToString(),
                    Value = 200m
                }
            });

            // Act
            var result = await _repo.GetUpToDateTransactionsAsync(
                investmentId,
                referenceDate,
                CancellationToken.None);

            // Assert
            result.Should().HaveCount(2);

            result[0].Date.Should().Be(new DateTime(2019, 12, 30, 0, 0, 0, DateTimeKind.Utc));
            result[1].Date.Should().Be(new DateTime(2019, 12, 31, 0, 0, 0, DateTimeKind.Utc));
        }

        [Fact]
        public async Task GetUpToDateAsync_ParsesTransactionType_IgnoresCase()
        {
            // Arrange
            var investmentId = "INV-1";
            var referenceDate = new DateTime(2019, 12, 31, 0, 0, 0, DateTimeKind.Utc);

            await _mongoContext.Transactions.InsertManyAsync(new[]
            {
                new TransactionDocument
                {
                    InvestmentId = investmentId,
                    Date = new DateTime(2019, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                    Type = "Shares",
                    Value = 123m
                }
            });

            // Act
            var result = await _repo.GetUpToDateTransactionsAsync(
                investmentId,
                referenceDate,
                CancellationToken.None);

            // Assert
            result.Should().HaveCount(1);
            result[0].Type.Should().Be(TransactionType.Shares);
        }

        [Fact]
        public async Task GetUpToDateAsync_WhenUnknownTransactionType_Throws()
        {
            // Arrange
            var investmentId = "INV-1";
            var referenceDate = new DateTime(2019, 12, 31, 0, 0, 0, DateTimeKind.Utc);

            await _mongoContext.Transactions.InsertManyAsync(new[]
            {
                new TransactionDocument
                {
                    InvestmentId = investmentId,
                    Date = new DateTime(2019, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                    Type = "NotARealType",
                    Value = 10m
                }
            });

            // Act
            var act = async () =>
            {
                await _repo.GetUpToDateTransactionsAsync(
                    investmentId,
                    referenceDate,
                    CancellationToken.None);
            };

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Unknown transaction type*");
        }

        [Fact]
        public async Task GetUpToDateAsync_MapsFieldsCorrectly()
        {
            // Arrange
            var investmentId = "INV-9";
            var referenceDate = new DateTime(2019, 12, 31, 0, 0, 0, DateTimeKind.Utc);

            var txDate = new DateTime(2019, 12, 30, 0, 0, 0, DateTimeKind.Utc);

            await _mongoContext.Transactions.InsertManyAsync(new[]
            {
                new TransactionDocument
                {
                    InvestmentId = investmentId,
                    Date = txDate,
                    Type = TransactionType.Building.ToString(),
                    Value = 777m
                }
            });

            // Act
            var result = await _repo.GetUpToDateTransactionsAsync(
                investmentId,
                referenceDate,
                CancellationToken.None);

            // Assert
            result.Should().HaveCount(1);

            var expected = new TransactionModel(
                investmentId,
                txDate,
                TransactionType.Building,
                777m);

            result[0].Should().Be(expected);
        }
    }
}
