using Moq;
using PortfolioCalculator.Application.Abstractions.Database;
using PortfolioCalculator.Application.Abstractions.Import;
using PortfolioCalculator.Application.Import;

namespace PortfolioCalculator.Application.Tests
{
    public sealed class ImportAllCsvCommandHandlerTests
    {
        [Fact]
        public async Task Handle_WhenAllFilesExist_CallsEnsureIndexesThenImports_ReturnsResult()
        {
            // Arrange
            var tempDir = CreateTempFolder();
            var investmentsPath = Path.Combine(tempDir, "Investments.csv");
            var transactionsPath = Path.Combine(tempDir, "Transactions.csv");
            var quotesPath = Path.Combine(tempDir, "Quotes.csv");

            await File.WriteAllTextAsync(investmentsPath, "dummy");
            await File.WriteAllTextAsync(transactionsPath, "dummy");
            await File.WriteAllTextAsync(quotesPath, "dummy");

            var seq = new MockSequence();

            var dbInit = new Mock<IDatabaseInitializer>(MockBehavior.Strict);
            dbInit.InSequence(seq)
                .Setup(x => x.EnsureIndexesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var csv = new Mock<ICsvImportService>(MockBehavior.Strict);

            string? capturedinvestmentsPath = null;
            string? capturedtransactionsPath = null;
            string? capturedquotesPath = null;

            csv.InSequence(seq)
                .Setup(x => x.ImportInvestmentsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, CancellationToken>((p, _) => capturedinvestmentsPath = p)
                .ReturnsAsync(11);

            csv.InSequence(seq)
                .Setup(x => x.ImportTransactionsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, CancellationToken>((p, _) => capturedtransactionsPath = p)
                .ReturnsAsync(22);

            csv.InSequence(seq)
                .Setup(x => x.ImportQuotesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, CancellationToken>((p, _) => capturedquotesPath = p)
                .ReturnsAsync(33);

            var importAllCsvCommandHandler = new ImportAllCsvCommandHandler(dbInit.Object, csv.Object);
            var command = new ImportAllCsvCommand(tempDir);

            // Act
            var result = await importAllCsvCommandHandler.Handle(command, CancellationToken.None);

            // Assert
            dbInit.Verify(x => x.EnsureIndexesAsync(It.IsAny<CancellationToken>()), Times.Once);

            csv.Verify(x => x.ImportInvestmentsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            csv.Verify(x => x.ImportTransactionsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            csv.Verify(x => x.ImportQuotesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

            Assert.Equal(investmentsPath, capturedinvestmentsPath);
            Assert.Equal(transactionsPath, capturedtransactionsPath);
            Assert.Equal(quotesPath, capturedquotesPath);

            Assert.Equal(11, result.InvestmentsRows);
            Assert.Equal(22, result.TransactionsRows);
            Assert.Equal(33, result.QuotesRows);
        }

        [Fact]
        public async Task Handle_WhenInvestmentsMissing_ThrowsFileNotFound_AndDoesNotImport()
        {
            // Arrange
            var tempDir = CreateTempFolder();
            await File.WriteAllTextAsync(Path.Combine(tempDir, "Transactions.csv"), "dummy");
            await File.WriteAllTextAsync(Path.Combine(tempDir, "Quotes.csv"), "dummy");

            var dbInit = new Mock<IDatabaseInitializer>(MockBehavior.Strict);
            dbInit.Setup(x => x.EnsureIndexesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var csv = new Mock<ICsvImportService>(MockBehavior.Strict);

            var importAllCsvCommandHandler = new ImportAllCsvCommandHandler(dbInit.Object, csv.Object);
            var importAllCsvCommand = new ImportAllCsvCommand(tempDir);

            // Act + Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() => importAllCsvCommandHandler.Handle(importAllCsvCommand, CancellationToken.None));

            dbInit.Verify(x => x.EnsureIndexesAsync(It.IsAny<CancellationToken>()), Times.Once);

            csv.Verify(x => x.ImportInvestmentsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            csv.Verify(x => x.ImportTransactionsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            csv.Verify(x => x.ImportQuotesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        private static string CreateTempFolder()
        {
            var dir = Path.Combine(Path.GetTempPath(), "pc_tests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            return dir;
        }
    }
}
