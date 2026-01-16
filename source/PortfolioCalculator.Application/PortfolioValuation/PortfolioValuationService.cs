using PortfolioCalculator.Application.Abstractions.PortfolioValuation;
using PortfolioCalculator.Application.PortfolioValuation.DTOs;

namespace PortfolioCalculator.Application.PortfolioValuation
{
    public sealed class PortfolioValuationService : IPortfolioValuationService
    {
        public async Task<PortfolioValuationResultDto> CalculateAsync(string investorId, DateTime referenceDate, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
