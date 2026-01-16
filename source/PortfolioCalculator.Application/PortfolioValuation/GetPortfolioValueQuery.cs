using MediatR;
using PortfolioCalculator.Application.PortfolioValuation.DTOs;

namespace PortfolioCalculator.Application.PortfolioValuation
{
    public sealed class GetPortfolioValueQuery : IRequest<PortfolioValuationResultDto>
    {
        public string InvestorId { get; }
        public DateTime ReferenceDate { get; }

        public GetPortfolioValueQuery(
            string investorId,
            DateTime referenceDate)
        {
            InvestorId = investorId;
            ReferenceDate = referenceDate;
        }
    }
}
