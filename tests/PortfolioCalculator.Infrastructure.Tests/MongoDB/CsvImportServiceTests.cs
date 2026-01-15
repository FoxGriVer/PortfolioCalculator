using FluentAssertions;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Moq;
using PortfolioCalculator.Infrastructure.MongoDB.Documents;
using PortfolioCalculator.Infrastructure.MongoDB.Import;
using PortfolioCalculator.Infrastructure.MongoDB.Repos.Write.Interfaces;
using System.Globalization;

namespace PortfolioCalculator.Infrastructure.Tests.MongoDB
{
    public sealed class CsvImportServiceTests
    {
        private static string CreateTempCsv(string content)
        {
            var path = Path.Combine(Path.GetTempPath(), $"quotes_{Guid.NewGuid():N}.csv");
            File.WriteAllText(path, content);
            return path;
        }

        [Fact]
        public async Task ImportQuotesAsync_ValidCsv_DeletesThenInsertsAndReturnsRowCount()
        {
            // Arrange
            var csvContent =
                "ISIN;Date;PricePerShare\n" +
                "DE000BASF111;2019-01-01;10.50\n" +
                "US0378331005;2019-01-02;155.12\n";

            var path = CreateTempCsv(csvContent);

            var quoteWriteRepository = new Mock<IQuoteWriteRepository>(MockBehavior.Strict);
            var transactionWriteRepository = new Mock<ITransactionWriteRepository>(MockBehavior.Strict);
            var investmentWriteRepository = new Mock<IInvestmentWriteRepository>(MockBehavior.Strict);
            var ownershipLinkWriteRepository = new Mock<IOwnershipLinkWriteRepository>(MockBehavior.Strict);

            var seq = new MockSequence();
            quoteWriteRepository.InSequence(seq)
                .Setup(r => r.DeleteAllAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            IReadOnlyCollection<QuoteDocument>? insertedDocs = null;

            quoteWriteRepository.InSequence(seq)
                .Setup(r => r.InsertManyAsync(It.IsAny<IReadOnlyCollection<QuoteDocument>>(), It.IsAny<CancellationToken>()))
                .Callback<IReadOnlyCollection<QuoteDocument>, CancellationToken>((docs, _) => insertedDocs = docs)
                .Returns(Task.CompletedTask);

            var сsvImportService = new CsvImportService(quoteWriteRepository.Object,
                transactionWriteRepository.Object,
                investmentWriteRepository.Object,
                ownershipLinkWriteRepository.Object);

            try
            {
                // Act
                var count = await сsvImportService.ImportQuotesAsync(path, CancellationToken.None);

                // Assert
                Assert.Equal(2, count);

                Assert.NotNull(insertedDocs);
                Assert.Equal(2, insertedDocs!.Count);

                var list = insertedDocs!.ToList();

                Assert.Equal("DE000BASF111", list[0].StockId);
                Assert.Equal(DateTime.ParseExact("2019-01-01", "yyyy-MM-dd", CultureInfo.InvariantCulture), list[0].Date);
                Assert.Equal(10.50m, list[0].Price);

                Assert.Equal("US0378331005", list[1].StockId);
                Assert.Equal(DateTime.ParseExact("2019-01-02", "yyyy-MM-dd", CultureInfo.InvariantCulture), list[1].Date);
                Assert.Equal(155.12m, list[1].Price);

                quoteWriteRepository.Verify(r => r.DeleteAllAsync(It.IsAny<CancellationToken>()), Times.Once);
                quoteWriteRepository.Verify(r => r.InsertManyAsync(It.IsAny<IReadOnlyCollection<QuoteDocument>>(), It.IsAny<CancellationToken>()), Times.Once);
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        [Fact]
        public async Task ImportQuotesAsync_UsesSemicolonDelimiter_ParsesCorrectly()
        {
            // Arrange
            var csvContent =
                "ISIN;Date;PricePerShare\n" +
                "AAA;2020-12-31;1.00\n";

            var path = CreateTempCsv(csvContent);

            var quoteWriteRepository = new Mock<IQuoteWriteRepository>(MockBehavior.Strict);
            var transactionWriteRepository = new Mock<ITransactionWriteRepository>(MockBehavior.Strict);
            var investmentWriteRepository = new Mock<IInvestmentWriteRepository>(MockBehavior.Strict);
            var ownershipLinkWriteRepository = new Mock<IOwnershipLinkWriteRepository>(MockBehavior.Strict);

            quoteWriteRepository.Setup(r => r.DeleteAllAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            IReadOnlyCollection<QuoteDocument>? insertedDocs = null;

            quoteWriteRepository.Setup(r => r.InsertManyAsync(It.IsAny<IReadOnlyCollection<QuoteDocument>>(), It.IsAny<CancellationToken>()))
                .Callback<IReadOnlyCollection<QuoteDocument>, CancellationToken>((docs, _) => insertedDocs = docs)
                .Returns(Task.CompletedTask);

            var сsvImportService = new CsvImportService(quoteWriteRepository.Object,
                transactionWriteRepository.Object,
                investmentWriteRepository.Object,
                ownershipLinkWriteRepository.Object);

            try
            {
                // Act
                var count = await сsvImportService.ImportQuotesAsync(path, CancellationToken.None);

                // Assert
                Assert.Equal(1, count);
                Assert.NotNull(insertedDocs);
                Assert.Single(insertedDocs!);

                var doc = insertedDocs!.Single();
                Assert.Equal("AAA", doc.StockId);
                Assert.Equal(1.00m, doc.Price);
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        [Fact]
        public async Task ImportQuotesAsync_EmptyFileWithHeader_ReturnsZeroAndStillCallsDeleteAndInsertEmpty()
        {
            // Arrange
            var csvContent = "ISIN;Date;PricePerShare\n";
            var path = CreateTempCsv(csvContent);

            var quoteWriteRepository = new Mock<IQuoteWriteRepository>(MockBehavior.Strict);
            var transactionWriteRepository = new Mock<ITransactionWriteRepository>(MockBehavior.Strict);
            var investmentWriteRepository = new Mock<IInvestmentWriteRepository>(MockBehavior.Strict);
            var ownershipLinkWriteRepository = new Mock<IOwnershipLinkWriteRepository>(MockBehavior.Strict);

            quoteWriteRepository.Setup(r => r.DeleteAllAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            IReadOnlyCollection<QuoteDocument>? insertedDocs = null;

            quoteWriteRepository.Setup(r => r.InsertManyAsync(It.IsAny<IReadOnlyCollection<QuoteDocument>>(), It.IsAny<CancellationToken>()))
                .Callback<IReadOnlyCollection<QuoteDocument>, CancellationToken>((docs, _) => insertedDocs = docs)
                .Returns(Task.CompletedTask);

            var сsvImportService = new CsvImportService(quoteWriteRepository.Object,
                transactionWriteRepository.Object,
                investmentWriteRepository.Object,
                ownershipLinkWriteRepository.Object);

            try
            {
                // Act
                var count = await сsvImportService.ImportQuotesAsync(path, CancellationToken.None);

                // Assert
                Assert.Equal(0, count);

                Assert.NotNull(insertedDocs);
                Assert.Empty(insertedDocs!);

                quoteWriteRepository.Verify(r => r.DeleteAllAsync(It.IsAny<CancellationToken>()), Times.Once);
                quoteWriteRepository.Verify(r => r.InsertManyAsync(It.IsAny<IReadOnlyCollection<QuoteDocument>>(), It.IsAny<CancellationToken>()), Times.Once);
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        [Fact]
        public async Task ImportQuotesAsync_FileDoesNotExist_Throws()
        {
            // Arrange
            var quoteWriteRepository = new Mock<IQuoteWriteRepository>(MockBehavior.Strict);
            var transactionWriteRepository = new Mock<ITransactionWriteRepository>(MockBehavior.Strict);
            var investmentWriteRepository = new Mock<IInvestmentWriteRepository>(MockBehavior.Strict);
            var ownershipLinkWriteRepository = new Mock<IOwnershipLinkWriteRepository>(MockBehavior.Strict);

            var сsvImportService = new CsvImportService(quoteWriteRepository.Object,
                transactionWriteRepository.Object,
                investmentWriteRepository.Object,
                ownershipLinkWriteRepository.Object);

            // Act + Assert
            await Assert.ThrowsAsync<FileNotFoundException>(async () =>
            {
                await сsvImportService.ImportQuotesAsync("this_file_does_not_exist.csv", CancellationToken.None);
            });

            quoteWriteRepository.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ImportTransactionsAsync_ValidCsv_DeletesThenInserts_ReturnsRowCount_AndMapsCorrectly()
        {
            // Arrange
            var csvContent =
                "InvestmentId;Type;Date;Value\n" +
                "INV1;Shares;2020-01-01;10\n" +
                "INV1;Shares;2020-01-02;-2\n" +
                "INV2;Estate;2020-01-01;100000\n";

            var path = CreateTempCsv(csvContent);

            var quoteWriteRepository = new Mock<IQuoteWriteRepository>(MockBehavior.Strict);
            var transactionWriteRepository = new Mock<ITransactionWriteRepository>(MockBehavior.Strict);
            var investmentWriteRepository = new Mock<IInvestmentWriteRepository>(MockBehavior.Strict);
            var ownershipLinkWriteRepository = new Mock<IOwnershipLinkWriteRepository>(MockBehavior.Strict);

            var seq = new MockSequence();
            transactionWriteRepository.InSequence(seq)
                .Setup(r => r.DeleteAllAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            IReadOnlyCollection<TransactionDocument>? inserted = null;

            transactionWriteRepository.InSequence(seq)
                .Setup(r => r.InsertManyAsync(It.IsAny<IReadOnlyCollection<TransactionDocument>>(), It.IsAny<CancellationToken>()))
                .Callback<IReadOnlyCollection<TransactionDocument>, CancellationToken>((docs, _) => inserted = docs)
                .Returns(Task.CompletedTask);

            var сsvImportService = new CsvImportService(quoteWriteRepository.Object,
                transactionWriteRepository.Object,
                investmentWriteRepository.Object,
                ownershipLinkWriteRepository.Object);

            try
            {
                // Act
                var count = await сsvImportService.ImportTransactionsAsync(path, CancellationToken.None);

                // Assert
                Assert.Equal(3, count);

                Assert.NotNull(inserted);
                Assert.Equal(3, inserted!.Count);

                var list = inserted!.ToList();

                Assert.Equal("INV1", list[0].InvestmentId);
                Assert.Equal("Shares", list[0].Type);
                Assert.Equal(new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc), list[0].Date);
                Assert.Equal(10m, list[0].Value);

                Assert.Equal("INV1", list[1].InvestmentId);
                Assert.Equal(-2m, list[1].Value);

                Assert.Equal("INV2", list[2].InvestmentId);
                Assert.Equal("Estate", list[2].Type);
                Assert.Equal(100000m, list[2].Value);

                transactionWriteRepository.Verify(r => r.DeleteAllAsync(It.IsAny<CancellationToken>()), Times.Once);
                transactionWriteRepository.Verify(r => r.InsertManyAsync(It.IsAny<IReadOnlyCollection<TransactionDocument>>(), It.IsAny<CancellationToken>()), Times.Once);
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        [Fact]
        public async Task ImportTransactionsAsync_HeaderOnly_DeletesAndDoesNotInsert_ReturnsZero()
        {
            // Arrange
            var csvContent = "InvestmentId;Type;Date;Value\n";
            var path = CreateTempCsv(csvContent);

            var quoteWriteRepository = new Mock<IQuoteWriteRepository>(MockBehavior.Strict);
            var transactionWriteRepository = new Mock<ITransactionWriteRepository>(MockBehavior.Strict);
            var investmentWriteRepository = new Mock<IInvestmentWriteRepository>(MockBehavior.Strict);
            var ownershipLinkWriteRepository = new Mock<IOwnershipLinkWriteRepository>(MockBehavior.Strict);

            transactionWriteRepository.Setup(r => r.DeleteAllAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var сsvImportService = new CsvImportService(quoteWriteRepository.Object,
                transactionWriteRepository.Object,
                investmentWriteRepository.Object,
                ownershipLinkWriteRepository.Object);

            try
            {
                // Act
                var count = await сsvImportService.ImportTransactionsAsync(path, CancellationToken.None);

                // Assert
                Assert.Equal(0, count);

                transactionWriteRepository.Verify(r => r.DeleteAllAsync(It.IsAny<CancellationToken>()), Times.Once);
                transactionWriteRepository.Verify(r => r.InsertManyAsync(It.IsAny<IReadOnlyCollection<TransactionDocument>>(), It.IsAny<CancellationToken>()), Times.Never);
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        [Fact]
        public async Task ImportTransactionsAsync_FileNotFound_ThrowsAndDoesNotTouchRepo()
        {
            // Arrange
            var quoteWriteRepository = new Mock<IQuoteWriteRepository>(MockBehavior.Strict);
            var transactionWriteRepository = new Mock<ITransactionWriteRepository>(MockBehavior.Strict);
            var investmentWriteRepository = new Mock<IInvestmentWriteRepository>(MockBehavior.Strict);
            var ownershipLinkWriteRepository = new Mock<IOwnershipLinkWriteRepository>(MockBehavior.Strict);

            var сsvImportService = new CsvImportService(quoteWriteRepository.Object,
                            transactionWriteRepository.Object,
                            investmentWriteRepository.Object,
                            ownershipLinkWriteRepository.Object);

            // Act + Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() =>
                сsvImportService.ImportTransactionsAsync("no_such_file.csv", CancellationToken.None));

            transactionWriteRepository.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ImportInvestmentsAsync_ValidCsv_CallsBulkUpsertForInvestmentsAndLinks_ReturnsRowCount()
        {
            // Arrange
            var csv =
                "InvestorId;InvestmentId;InvestmentType;ISIN;City;FondsInvestor\n" +
                "Investor1;INV_STOCK;Stock;ISIN_STOCK;;\n" +
                "Investor1;INV_RE;RealEstate;;Berlin;\n" +
                "Investor1;INV_FUND;Fonds;;;Fonds1\n" +
                "Fonds1;INV_IN_FUND;Stock;ISIN_IN_FUND;;\n";

            var filePath = CreateTempCsv(csv);

            var quoteWriteRepository = new Mock<IQuoteWriteRepository>(MockBehavior.Loose);
            var transactionWriteRepository = new Mock<ITransactionWriteRepository>(MockBehavior.Loose);
            var investmentWriteRepository = new Mock<IInvestmentWriteRepository>(MockBehavior.Strict);
            var ownershipLinkWriteRepository = new Mock<IOwnershipLinkWriteRepository>(MockBehavior.Strict);

            IReadOnlyCollection<WriteModel<InvestmentDocument>>? invModels = null;
            IReadOnlyCollection<WriteModel<OwnershipLinkDocument>>? linkModels = null;

            investmentWriteRepository.Setup(r => r.BulkUpsertAsync(It.IsAny<IReadOnlyCollection<WriteModel<InvestmentDocument>>>(), It.IsAny<CancellationToken>()))
                .Callback<IReadOnlyCollection<WriteModel<InvestmentDocument>>, CancellationToken>((m, _) => invModels = m)
                .Returns(Task.CompletedTask);

            ownershipLinkWriteRepository.Setup(r => r.BulkUpsertAsync(It.IsAny<IReadOnlyCollection<WriteModel<OwnershipLinkDocument>>>(), It.IsAny<CancellationToken>()))
                .Callback<IReadOnlyCollection<WriteModel<OwnershipLinkDocument>>, CancellationToken>((m, _) => linkModels = m)
                .Returns(Task.CompletedTask);

            var сsvImportService = new CsvImportService(quoteWriteRepository.Object,
                            transactionWriteRepository.Object,
                            investmentWriteRepository.Object,
                            ownershipLinkWriteRepository.Object);

            try
            {
                // Act
                var count = await сsvImportService.ImportInvestmentsAsync(filePath, CancellationToken.None);

                // Assert
                count.Should().Be(4);

                investmentWriteRepository.Verify(r => r.BulkUpsertAsync(It.IsAny<IReadOnlyCollection<WriteModel<InvestmentDocument>>>(), It.IsAny<CancellationToken>()), Times.Once);
                ownershipLinkWriteRepository.Verify(r => r.BulkUpsertAsync(It.IsAny<IReadOnlyCollection<WriteModel<OwnershipLinkDocument>>>(), It.IsAny<CancellationToken>()), Times.Once);

                invModels.Should().NotBeNull();
                invModels!.Should().HaveCount(4);

                linkModels.Should().NotBeNull();
                linkModels!.Should().HaveCount(4);

                invModels!.All(m => m is UpdateOneModel<InvestmentDocument>).Should().BeTrue();
                invModels!.Cast<UpdateOneModel<InvestmentDocument>>().All(m => m.IsUpsert).Should().BeTrue();

                linkModels!.All(m => m is UpdateOneModel<OwnershipLinkDocument>).Should().BeTrue();
                linkModels!.Cast<UpdateOneModel<OwnershipLinkDocument>>().All(m => m.IsUpsert).Should().BeTrue();

                var invList = invModels!.Cast<UpdateOneModel<InvestmentDocument>>().ToList();

                var stockModel = FindInvestmentModel(invList, "INV_STOCK");
                stockModel.Should().NotBeNull();
                var stockUpdate = RenderUpdate(stockModel!.Update);
                stockUpdate.Should().Contain("Type").And.Contain("Stock");
                stockUpdate.Should().Contain("ISIN").And.Contain("ISIN_STOCK");
                stockUpdate.Should().Contain("FundId").And.Contain("null");

                var fundModel = FindInvestmentModel(invList, "INV_FUND");
                var fundUpdate = RenderUpdate(fundModel!.Update);
                fundUpdate.Should().Contain("Type").And.Contain("Fund");
                fundUpdate.Should().Contain("FundId").And.Contain("Fonds1");

                var linkList = linkModels!.Cast<UpdateOneModel<OwnershipLinkDocument>>().ToList();

                var linkInvestor = FindLinkModel(linkList, "Investor", "Investor1", "INV_STOCK");
                linkInvestor.Should().NotBeNull();

                var linkFundOwner = FindLinkModel(linkList, "Fund", "Fonds1", "INV_IN_FUND");
                linkFundOwner.Should().NotBeNull();
            }
            finally
            {
                if (File.Exists(filePath)) File.Delete(filePath);
            }
        }

        private static UpdateOneModel<InvestmentDocument>? FindInvestmentModel(
            IReadOnlyList<UpdateOneModel<InvestmentDocument>> models,
            string investmentId)
        {
            return models.FirstOrDefault(m => RenderFilter(m.Filter).Contains(investmentId));
        }

        private static UpdateOneModel<OwnershipLinkDocument>? FindLinkModel(
            IReadOnlyList<UpdateOneModel<OwnershipLinkDocument>> models,
            string ownerType,
            string ownerId,
            string investmentId)
        {
            var rendered = models.Select(m => new { Model = m, Filter = RenderFilter(m.Filter) }).ToList();
            return rendered.FirstOrDefault(x =>
                x.Filter.Contains(ownerType) &&
                x.Filter.Contains(ownerId) &&
                x.Filter.Contains(investmentId)
            )?.Model;
        }

        private static string RenderFilter<T>(FilterDefinition<T> filter)
        {
            var serializerRegistry = BsonSerializer.SerializerRegistry;
            var serializer = serializerRegistry.GetSerializer<T>();

            var doc = filter.Render(new RenderArgs<T>(serializer, serializerRegistry));
            return doc.ToString();
        }

        private static string RenderUpdate<T>(UpdateDefinition<T> update)
        {
            var serializerRegistry = BsonSerializer.SerializerRegistry;
            var serializer = serializerRegistry.GetSerializer<T>();

            var doc = update.Render(new RenderArgs<T>(serializer, serializerRegistry));
            return doc?.ToString() ?? string.Empty;
        }
    }
}
