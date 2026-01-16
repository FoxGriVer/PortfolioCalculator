using MediatR;
using PortfolioCalculator.Application.Abstractions.PortfolioValuation;
using PortfolioCalculator.Application.PortfolioValuation.DTOs;

namespace PortfolioCalculator.Application.PortfolioValuation
{
    public sealed class GetPortfolioValueQueryHandler : IRequestHandler<GetPortfolioValueQuery, PortfolioValuationResultDto>
    {
        private readonly IPortfolioValuationService _portfolioValuationService;

        public GetPortfolioValueQueryHandler(IPortfolioValuationService portfolioValuationService)
        {
            _portfolioValuationService = portfolioValuationService;
        }

        public Task<PortfolioValuationResultDto> Handle(GetPortfolioValueQuery request, CancellationToken ct)
        {
            var result = _portfolioValuationService.CalculateAsync(request.InvestorId, request.ReferenceDate, ct);

            return result;
        }
    }
}
