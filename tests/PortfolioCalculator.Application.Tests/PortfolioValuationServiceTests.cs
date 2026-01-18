using Microsoft.Extensions.Logging;
using Moq;
using PortfolioCalculator.Application.Abstractions.PortfolioValuation;
using PortfolioCalculator.Application.Abstractions.Repositories.Models;
using PortfolioCalculator.Application.Abstractions.Repositories.Read;
using PortfolioCalculator.Application.PortfolioValuation;
using PortfolioCalculator.Domain.Enums;

namespace PortfolioCalculator.Application.Tests
{
    public sealed class PortfolioValuationServiceTests
    {
        private readonly Mock<IOwnershipReadRepository> _ownershipReadRepository = new();
        private readonly Mock<IInvestmentReadRepository> _investmentReadRepository = new();
        private readonly Mock<ITransactionReadRepository> _transactionReadRepository = new();
        private readonly Mock<IQuoteReadRepository> _quoteReadRepository = new();
        private readonly Mock<ILogger<PortfolioValuationService>> _logger = new();

        private IPortfolioValuationService CreatePortfolioValuationService()
        => new PortfolioValuationService(
            _ownershipReadRepository.Object,
            _investmentReadRepository.Object,
            _transactionReadRepository.Object,
            _quoteReadRepository.Object,
            _logger.Object);

        [Fact]
        public async Task CalculateAsync_Stock_ComputesSharesTimesLatestPrice()
        {
            var investorId = "Investor1";
            var date = new DateTime(2019, 12, 31);

            _ownershipReadRepository
                .Setup(r => r.GetOwnedInvestmentIdsAsync(
                    OwnerType.Investor,
                    investorId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string> { "S1" });

            _investmentReadRepository
                .Setup(r => r.GetByIdsAsync(
                    It.IsAny<IReadOnlyCollection<string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, InvestmentInfoModel>
                {
                    ["S1"] = new InvestmentInfoModel(
                        id: "S1",
                        type: InvestmentType.Stock,
                        isin: "US123",
                        city: null,
                        fundId: null)
                });

            _transactionReadRepository
                .Setup(r => r.GetUpToDateTransactionsAsync(
                    "S1",
                    date,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TransactionModel>
                {
                    new("S1", new DateTime(2019,1,1), TransactionType.Shares, 10m),
                    new("S1", new DateTime(2019,6,1), TransactionType.Shares, 5m)
                });

            _quoteReadRepository
                .Setup(r => r.GetLatestPriceAsync(
                    "US123",
                    date,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(100m);

            var portfolioValuationService = CreatePortfolioValuationService();

            var result = await portfolioValuationService.CalculateAsync(investorId, date, CancellationToken.None);

            Assert.Equal(1500m, result.TotalValue);
            Assert.Single(result.CompositionByType);
            Assert.Equal(InvestmentType.Stock, result.CompositionByType[0].Type);
            Assert.Equal(1500m, result.CompositionByType[0].Value);
        }

        [Fact]
        public async Task CalculateAsync_Stock_NoQuote_ReturnsZero()
        {
            // Arrange
            var investorId = "Investor1";
            var date = new DateTime(2019, 12, 31);

            _ownershipReadRepository
                .Setup(r => r.GetOwnedInvestmentIdsAsync(OwnerType.Investor, investorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string> { "S1" });

            _investmentReadRepository
                .Setup(r => r.GetByIdsAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, InvestmentInfoModel>
                {
                    ["S1"] = new InvestmentInfoModel(
                        id: "S1",
                        type: InvestmentType.Stock,
                        isin: "US123",
                        city: null,
                        fundId: null)
                });

            _transactionReadRepository
                .Setup(r => r.GetUpToDateTransactionsAsync("S1", date, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TransactionModel>
                {
                new("S1", date.AddDays(-1), TransactionType.Shares, 10m)
                });

            _quoteReadRepository
                .Setup(r => r.GetLatestPriceAsync("US123", date, It.IsAny<CancellationToken>()))
                .ReturnsAsync((decimal?)null);

            var portfolioValuationService = CreatePortfolioValuationService();

            // Act
            var result = await portfolioValuationService.CalculateAsync(investorId, date, CancellationToken.None);

            // Assert
            Assert.Equal(0m, result.TotalValue);
            Assert.Single(result.CompositionByType);
            Assert.Equal(InvestmentType.Stock, result.CompositionByType[0].Type);
            Assert.Equal(0m, result.CompositionByType[0].Value);
        }

        [Fact]
        public async Task CalculateAsync_RealEstate_SumsEstateAndBuilding()
        {
            // Arrange
            var investorId = "Investor1";
            var date = new DateTime(2019, 12, 31);

            _ownershipReadRepository
                .Setup(r => r.GetOwnedInvestmentIdsAsync(OwnerType.Investor, investorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string> { "R1" });

            _investmentReadRepository
                .Setup(r => r.GetByIdsAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, InvestmentInfoModel>
                {
                    ["R1"] = new InvestmentInfoModel(
                        id: "R1",
                        type: InvestmentType.RealEstate,
                        isin: null,
                        city: "Berlin",
                        fundId: null)
                });

            _transactionReadRepository
                .Setup(r => r.GetUpToDateTransactionsAsync("R1", date, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TransactionModel>
                {
                    new("R1", date.AddYears(-1), TransactionType.Estate, 200_000m),
                    new("R1", date.AddYears(-1), TransactionType.Building, 50_000m),
                    new("R1", date.AddMonths(-6), TransactionType.Building, -5_000m)
                });

            var portfolioValuationService = CreatePortfolioValuationService();

            // Act
            var result = await portfolioValuationService.CalculateAsync(investorId, date, CancellationToken.None);

            // Assert
            Assert.Equal(245_000m, result.TotalValue);
            Assert.Single(result.CompositionByType);
            Assert.Equal(InvestmentType.RealEstate, result.CompositionByType[0].Type);
            Assert.Equal(245_000m, result.CompositionByType[0].Value);
        }

        [Fact]
        public async Task CalculateAsync_Fund_ComputesPercentTimesFundTotal_WithNormalization()
        {
            // Arrange
            var investorId = "Investor1";
            var date = new DateTime(2019, 12, 31);

            // Investor owns FundPosition FP1
            _ownershipReadRepository
                .Setup(r => r.GetOwnedInvestmentIdsAsync(OwnerType.Investor, investorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string> { "FP1" });

            _investmentReadRepository
                .Setup(r => r.GetByIdsAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, InvestmentInfoModel>
                {
                    ["FP1"] = new InvestmentInfoModel(
                        id: "FP1",
                        type: InvestmentType.Fund,
                        isin: null,
                        city: null,
                        fundId: "FondsA")
                });

            // Percentage on FP1: 20 => 0.2
            _transactionReadRepository
                .Setup(r => r.GetUpToDateTransactionsAsync("FP1", date, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TransactionModel>
                {
                    new("FP1", date.AddDays(-10), TransactionType.Percentage, 20m)
                });

            // FondsA holds one stock S2
            _ownershipReadRepository
                .Setup(r => r.GetOwnedInvestmentIdsAsync(OwnerType.Fund, "FondsA", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string> { "S2" });

            _investmentReadRepository
                .Setup(r => r.GetByIdsAsync(
                    It.Is<IReadOnlyCollection<string>>(ids => ids.Count == 1 && ids.Contains("S2")),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, InvestmentInfoModel>
                {
                    ["S2"] = new InvestmentInfoModel(
                        id: "S2",
                        type: InvestmentType.Stock,
                        isin: "US999",
                        city: null,
                        fundId: null)
                });

            // S2 value: 100 shares * 50 price = 5000
            _transactionReadRepository
                .Setup(r => r.GetUpToDateTransactionsAsync("S2", date, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TransactionModel>
                {
                    new("S2", date.AddDays(-100), TransactionType.Shares, 100m)
                });

            _quoteReadRepository
                .Setup(r => r.GetLatestPriceAsync("US999", date, It.IsAny<CancellationToken>()))
                .ReturnsAsync(50m);

            var portfolioValuationService = CreatePortfolioValuationService();

            // Act
            var result = await portfolioValuationService.CalculateAsync(investorId, date, CancellationToken.None);

            // Assert
            Assert.Equal(1000m, result.TotalValue);
            Assert.Single(result.CompositionByType);
            Assert.Equal(InvestmentType.Fund, result.CompositionByType[0].Type);
            Assert.Equal(1000m, result.CompositionByType[0].Value);
        }

        [Fact]
        public async Task CalculateAsync_FundCycle_DoesNotThrow_SkipsBranchAndLogsWarning()
        {
            // Arrange
            var investorId = "Investor1";
            var date = new DateTime(2019, 12, 31);

            // Investor owns FP1 -> FondsA (100%)
            _ownershipReadRepository
                .Setup(r => r.GetOwnedInvestmentIdsAsync(OwnerType.Investor, investorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string> { "FP1" });

            _investmentReadRepository
                .Setup(r => r.GetByIdsAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, InvestmentInfoModel>
                {
                    ["FP1"] = new InvestmentInfoModel("FP1", InvestmentType.Fund, null, null, "FondsA")
                });

            _transactionReadRepository
                .Setup(r => r.GetUpToDateTransactionsAsync("FP1", date, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TransactionModel>
                {
                new("FP1", date.AddDays(-1), TransactionType.Percentage, 100m)
                });

            // FondsA holds FP2 -> FondsB
            _ownershipReadRepository
                .Setup(r => r.GetOwnedInvestmentIdsAsync(OwnerType.Fund, "FondsA", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string> { "FP2" });

            _investmentReadRepository
                .Setup(r => r.GetByIdsAsync(
                    It.Is<IReadOnlyCollection<string>>(ids => ids.Contains("FP2")),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, InvestmentInfoModel>
                {
                    ["FP2"] = new InvestmentInfoModel("FP2", InvestmentType.Fund, null, null, "FondsB")
                });

            _transactionReadRepository
                .Setup(r => r.GetUpToDateTransactionsAsync("FP2", date, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TransactionModel>
                {
                new("FP2", date.AddDays(-1), TransactionType.Percentage, 100m)
                });

            // FondsB holds FP3 -> FondsA (cycle)
            _ownershipReadRepository
                .Setup(r => r.GetOwnedInvestmentIdsAsync(OwnerType.Fund, "FondsB", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string> { "FP3" });

            _investmentReadRepository
                .Setup(r => r.GetByIdsAsync(
                    It.Is<IReadOnlyCollection<string>>(ids => ids.Contains("FP3")),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, InvestmentInfoModel>
                {
                    ["FP3"] = new InvestmentInfoModel("FP3", InvestmentType.Fund, null, null, "FondsA")
                });

            _transactionReadRepository
                .Setup(r => r.GetUpToDateTransactionsAsync("FP3", date, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TransactionModel>
                {
                new("FP3", date.AddDays(-1), TransactionType.Percentage, 100m)
                });

            var portfolioValuationService = CreatePortfolioValuationService();

            // Act
            var ex = await Record.ExceptionAsync(() => portfolioValuationService.CalculateAsync(investorId, date, CancellationToken.None));

            Assert.Null(ex);

            var result = await portfolioValuationService.CalculateAsync(investorId, date, CancellationToken.None);
            Assert.Equal(0m, result.TotalValue);

            // Assert: warning logged at least once
            _logger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Cyclic fund dependency detected")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task CalculateAsync_Fund_FundInsideFund_ComputesRecursively()
        {
            var investorId = "Investor1";
            var date = new DateTime(2019, 12, 31);

            _ownershipReadRepository
                .Setup(r => r.GetOwnedInvestmentIdsAsync(OwnerType.Investor, investorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string> { "FP1" });

            _investmentReadRepository
                .Setup(r => r.GetByIdsAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, InvestmentInfoModel>
                {
                    ["FP1"] = new InvestmentInfoModel("FP1", InvestmentType.Fund, null, null, "FondsA")
                });

            _transactionReadRepository
                .Setup(r => r.GetUpToDateTransactionsAsync("FP1", date, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TransactionModel> { new("FP1", date.AddDays(-1), TransactionType.Percentage, 100m) });

            // FondsA holdings: Stock S1 + FundPosition FP2 (-> FondsB)
            _ownershipReadRepository
                .Setup(r => r.GetOwnedInvestmentIdsAsync(OwnerType.Fund, "FondsA", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string> { "S1", "FP2" });

            _investmentReadRepository
                .Setup(r => r.GetByIdsAsync(
                    It.Is<IReadOnlyCollection<string>>(ids => ids.Contains("S1") && ids.Contains("FP2")),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, InvestmentInfoModel>
                {
                    ["S1"] = new InvestmentInfoModel("S1", InvestmentType.Stock, "US111", null, null),
                    ["FP2"] = new InvestmentInfoModel("FP2", InvestmentType.Fund, null, null, "FondsB")
                });

            // S1 = 100 shares * 10 = 1000
            _transactionReadRepository
                .Setup(r => r.GetUpToDateTransactionsAsync("S1", date, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TransactionModel> { new("S1", date.AddDays(-10), TransactionType.Shares, 100m) });

            _quoteReadRepository
                .Setup(r => r.GetLatestPriceAsync("US111", date, It.IsAny<CancellationToken>()))
                .ReturnsAsync(10m);

            // FP2 percentage = 50% in FondsB
            _transactionReadRepository
                .Setup(r => r.GetUpToDateTransactionsAsync("FP2", date, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TransactionModel> { new("FP2", date.AddDays(-10), TransactionType.Percentage, 50m) });

            // FondsB holdings: Stock S2
            _ownershipReadRepository
                .Setup(r => r.GetOwnedInvestmentIdsAsync(OwnerType.Fund, "FondsB", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string> { "S2" });

            _investmentReadRepository
                .Setup(r => r.GetByIdsAsync(
                    It.Is<IReadOnlyCollection<string>>(ids => ids.Count == 1 && ids.Contains("S2")),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, InvestmentInfoModel>
                {
                    ["S2"] = new InvestmentInfoModel("S2", InvestmentType.Stock, "US222", null, null)
                });

            // S2 = 20 shares * 100 = 2000
            _transactionReadRepository
                .Setup(r => r.GetUpToDateTransactionsAsync("S2", date, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TransactionModel> { new("S2", date.AddDays(-10), TransactionType.Shares, 20m) });

            _quoteReadRepository
                .Setup(r => r.GetLatestPriceAsync("US222", date, It.IsAny<CancellationToken>()))
                .ReturnsAsync(100m);

            var portfolioValuationService = CreatePortfolioValuationService();

            var result = await portfolioValuationService.CalculateAsync(investorId, date, CancellationToken.None);

            Assert.Equal(2000m, result.TotalValue);
            Assert.Single(result.CompositionByType);
            Assert.Equal(InvestmentType.Fund, result.CompositionByType[0].Type);
            Assert.Equal(2000m, result.CompositionByType[0].Value);
        }

        [Fact]
        public async Task CalculateAsync_Fund_PercentageZero_ReturnsZero()
        {
            var investorId = "Investor1";
            var date = new DateTime(2019, 12, 31);

            _ownershipReadRepository
                .Setup(r => r.GetOwnedInvestmentIdsAsync(OwnerType.Investor, investorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string> { "FP1" });

            _investmentReadRepository
                .Setup(r => r.GetByIdsAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, InvestmentInfoModel>
                {
                    ["FP1"] = new InvestmentInfoModel("FP1", InvestmentType.Fund, null, null, "FondsA")
                });

            _transactionReadRepository
                .Setup(r => r.GetUpToDateTransactionsAsync("FP1", date, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TransactionModel>
                {
            new("FP1", date.AddDays(-1), TransactionType.Percentage, 0m)
                });

            var portfolioValuationService = CreatePortfolioValuationService();

            var result = await portfolioValuationService.CalculateAsync(investorId, date, CancellationToken.None);

            Assert.Equal(0m, result.TotalValue);
            Assert.Single(result.CompositionByType);
            Assert.Equal(InvestmentType.Fund, result.CompositionByType[0].Type);
            Assert.Equal(0m, result.CompositionByType[0].Value);
        }

        [Fact]
        public async Task CalculateAsync_Fund_EmptyHoldings_ReturnsZero()
        {
            var investorId = "Investor1";
            var date = new DateTime(2019, 12, 31);

            _ownershipReadRepository
                .Setup(r => r.GetOwnedInvestmentIdsAsync(OwnerType.Investor, investorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string> { "FP1" });

            _investmentReadRepository
                .Setup(r => r.GetByIdsAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, InvestmentInfoModel>
                {
                    ["FP1"] = new InvestmentInfoModel("FP1", InvestmentType.Fund, null, null, "FondsA")
                });

            _transactionReadRepository
                .Setup(r => r.GetUpToDateTransactionsAsync("FP1", date, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TransactionModel>
                {
            new("FP1", date.AddDays(-10), TransactionType.Percentage, 100m)
                });

            // Фонд существует, но активов нет
            _ownershipReadRepository
                .Setup(r => r.GetOwnedInvestmentIdsAsync(OwnerType.Fund, "FondsA", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string>());

            var portfolioValuationService = CreatePortfolioValuationService();

            var result = await portfolioValuationService.CalculateAsync(investorId, date, CancellationToken.None);

            Assert.Equal(0m, result.TotalValue);
        }
    }
}
