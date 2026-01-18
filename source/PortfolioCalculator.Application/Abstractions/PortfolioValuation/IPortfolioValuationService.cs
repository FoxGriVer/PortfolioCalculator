using PortfolioCalculator.Application.PortfolioValuation.DTOs;

namespace PortfolioCalculator.Application.Abstractions.PortfolioValuation
{
    public interface IPortfolioValuationService
    {
        Task<PortfolioValuationResultDto> CalculateAsync(string investorId, DateTime referenceDate, CancellationToken ct);
    }
}
