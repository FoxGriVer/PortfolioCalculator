using Microsoft.Extensions.Logging;
using PortfolioCalculator.Application.Abstractions.PortfolioValuation;
using PortfolioCalculator.Application.Abstractions.Repositories.Models;
using PortfolioCalculator.Application.Abstractions.Repositories.Read;
using PortfolioCalculator.Application.PortfolioValuation.DTOs;
using PortfolioCalculator.Domain.Enums;

namespace PortfolioCalculator.Application.PortfolioValuation
{
    public sealed class PortfolioBulkValuationService : IPortfolioValuationService
    {
        private readonly IOwnershipReadRepository _ownershipReadRepository;
        private readonly IInvestmentReadRepository _investmentReadRepository;
        private readonly ITransactionReadRepository _transactionReadRepository;
        private readonly IQuoteReadRepository _quoteReadRepository;
        private readonly ILogger<PortfolioBulkValuationService> _logger;

        public PortfolioBulkValuationService(
            IOwnershipReadRepository ownership,
            IInvestmentReadRepository investments,
            ITransactionReadRepository transactions,
            IQuoteReadRepository quotes,
            ILogger<PortfolioBulkValuationService> logger)
        {
            _ownershipReadRepository = ownership;
            _investmentReadRepository = investments;
            _transactionReadRepository = transactions;
            _quoteReadRepository = quotes;
            _logger = logger;
        }

        public async Task<PortfolioValuationResultDto> CalculateAsync(
            string investorId,
            DateTime referenceDate,
            CancellationToken ct)
        {
            var ownedInvestmentIds = await _ownershipReadRepository.GetOwnedInvestmentIdsAsync(OwnerType.Investor, investorId, ct);
            var directInvestmentsById = await _investmentReadRepository.GetByIdsAsync(ownedInvestmentIds, ct);

            var transactionsById = await _transactionReadRepository
                .GetUpToDateTransactionsByInvestmentIdsAsync(ownedInvestmentIds, referenceDate, ct);

            var directStockIsins = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var inv in directInvestmentsById.Values)
            {
                if (inv.Type == InvestmentType.Stock && !string.IsNullOrWhiteSpace(inv.ISIN))
                {
                    directStockIsins.Add(inv.ISIN);
                }
            }

            var directPricesByIsin = await _quoteReadRepository
                .GetLatestPricesByIsinsAsync(directStockIsins.ToList(), referenceDate, ct);

            var compositionByTypes = new Dictionary<InvestmentType, decimal>();

            decimal total = 0m;

            foreach (var investmentId in ownedInvestmentIds)
            {
                if (!directInvestmentsById.TryGetValue(investmentId, out var investmentInfo))
                {
                    _logger.LogWarning("Investment metadata not found for InvestmentId={InvestmentId}. Skipping.", investmentId);
                    continue;
                }

                var value = await ComputeInvestmentValueAsync(
                    investmentInfo,
                    referenceDate,
                    fundRecursionGuard: new HashSet<string>(StringComparer.OrdinalIgnoreCase),
                    transactionsById,
                    directPricesByIsin,
                    ct);

                if (value == 0m)
                    continue;

                total += value;

                if (compositionByTypes.TryGetValue(investmentInfo.Type, out var currentValue))
                {
                    compositionByTypes[investmentInfo.Type] = currentValue + value;
                }
                else
                {
                    compositionByTypes[investmentInfo.Type] = value;
                }
            }

            var breakdown = compositionByTypes
                .Select(keyValuePair => new PortfolioTypeCompositionItemDto(keyValuePair.Key, keyValuePair.Value))
                .OrderByDescending(x => x.Value)
                .ToList();

            return new PortfolioValuationResultDto(total, breakdown);
        }

        private async Task<decimal> ComputeInvestmentValueAsync(
            InvestmentInfoModel investmentInfo,
            DateTime referenceDate,
            HashSet<string> fundRecursionGuard,
            IReadOnlyDictionary<string, IReadOnlyList<TransactionModel>> transactionsByInvestmentId,
            IReadOnlyDictionary<string, decimal?> pricesByIsin,
            CancellationToken ct)
        {
            decimal value;

            switch (investmentInfo.Type)
            {
                case InvestmentType.Stock:
                    value = ComputeStockValue(investmentInfo,
                        transactionsByInvestmentId,
                        pricesByIsin);
                    break;

                case InvestmentType.RealEstate:
                    value = ComputeRealEstateValue(investmentInfo,
                        transactionsByInvestmentId);
                    break;

                case InvestmentType.Fund:
                    value = await ComputeFundPositionValueAsync(
                        investmentInfo,
                        referenceDate,
                        fundRecursionGuard,
                        ct);
                    break;

                default:
                    value = 0m;
                    break;
            }

            return value;
        }

        private static decimal ComputeStockValue(
            InvestmentInfoModel investmentInfo,
            IReadOnlyDictionary<string, IReadOnlyList<TransactionModel>> transactionsByInvestmentId,
            IReadOnlyDictionary<string, decimal?> pricesByIsin)
        {
            if (string.IsNullOrWhiteSpace(investmentInfo.ISIN))
                return 0m;

            transactionsByInvestmentId.TryGetValue(investmentInfo.Id, out var transactionsForId);
            if (transactionsForId == null)
            {
                transactionsForId = Array.Empty<TransactionModel>();
            }

            var shares = transactionsForId.Where(t => t.Type == TransactionType.Shares)
                .Sum(t => t.Value);

            if (shares == 0m)
                return 0m;

            pricesByIsin.TryGetValue(investmentInfo.ISIN, out var price);
            if (price is null)
                return 0m;

            return shares * price.Value;
        }

        private static decimal ComputeRealEstateValue(
            InvestmentInfoModel investmentInfo,
            IReadOnlyDictionary<string, IReadOnlyList<TransactionModel>> transactionsByInvestmentId)
        {
            transactionsByInvestmentId.TryGetValue(investmentInfo.Id, out var transactionsForId);
            if (transactionsForId == null)
            {
                transactionsForId = Array.Empty<TransactionModel>();
            }

            var estate = transactionsForId.Where(t => t.Type == TransactionType.Estate)
                .Sum(t => t.Value);
            var building = transactionsForId.Where(t => t.Type == TransactionType.Building)
                .Sum(t => t.Value);

            var totalRalEstateValue = estate + building;

            return totalRalEstateValue;
        }


        private async Task<decimal> ComputeFundPositionValueAsync(
            InvestmentInfoModel fundPositionInvestment,
            DateTime referenceDate,
            HashSet<string> fundRecursionGuard,
            CancellationToken ct)
        {
            var positionTransactionsByInvestmentId = await _transactionReadRepository
                .GetUpToDateTransactionsByInvestmentIdsAsync(
                    new[] { fundPositionInvestment.Id },
                    referenceDate,
                    ct);

            positionTransactionsByInvestmentId.TryGetValue(fundPositionInvestment.Id, out var positionTransactionsForId);
            if (positionTransactionsForId == null)
            {
                positionTransactionsForId = Array.Empty<TransactionModel>();
            }

            var percentRaw = positionTransactionsForId
                .Where(t => t.Type == TransactionType.Percentage)
                .Sum(t => t.Value);

            if (percentRaw == 0m)
                return 0m;

            var percent = NormalizePercent(percentRaw);

            if (string.IsNullOrWhiteSpace(fundPositionInvestment.FundId))
            {
                _logger.LogWarning("Fund position InvestmentId={InvestmentId} has no FundId. Skipping.", fundPositionInvestment.Id);
                return 0m;
            }

            var fundId = fundPositionInvestment.FundId;

            if (!fundRecursionGuard.Add(fundId))
            {
                _logger.LogWarning(
                    "Cyclic fund dependency detected at FundId={FundId}. Skipping this branch (value=0).",
                    fundId);

                return 0m;
            }

            try
            {
                var fundInvestmentIds = await _ownershipReadRepository.GetOwnedInvestmentIdsAsync(OwnerType.Fund, fundId, ct);
                if (fundInvestmentIds.Count == 0)
                    return 0m;

                var fundInvestmentsById = await _investmentReadRepository.GetByIdsAsync(fundInvestmentIds, ct);

                var fundTransactionsByInvestmentId = await _transactionReadRepository
                    .GetUpToDateTransactionsByInvestmentIdsAsync(fundInvestmentIds, referenceDate, ct);

                var fundStockIsins = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var inv in fundInvestmentsById.Values)
                {
                    if (inv.Type == InvestmentType.Stock && !string.IsNullOrWhiteSpace(inv.ISIN))
                    {
                        fundStockIsins.Add(inv.ISIN);
                    }
                }

                var fundStockPricesByIsin = await _quoteReadRepository
                    .GetLatestPricesByIsinsAsync(fundStockIsins.ToList(), referenceDate, ct);

                decimal fundTotal = 0m;

                foreach (var fundInvestmentId in fundInvestmentIds)
                {
                    if (!fundInvestmentsById.TryGetValue(fundInvestmentId, out var investmentInfo))
                    {
                        _logger.LogWarning("Fund holding metadata not found for InvestmentId={InvestmentId} in FundId={FundId}. Skipping.",
                            fundInvestmentId, fundId);
                        continue;
                    }

                    var holdingValue = await ComputeInvestmentValueAsync(
                        investmentInfo,
                        referenceDate,
                        fundRecursionGuard,
                        fundTransactionsByInvestmentId,
                        fundStockPricesByIsin,
                        ct);

                    fundTotal += holdingValue;
                }

                return percent * fundTotal;
            }
            finally
            {
                fundRecursionGuard.Remove(fundId);
            }
        }

        private static decimal NormalizePercent(decimal percentRaw)
        {
            var abs = Math.Abs(percentRaw);
            if (abs > 1.0000m)
                return percentRaw / 100m;

            return percentRaw;
        }
    }
}
