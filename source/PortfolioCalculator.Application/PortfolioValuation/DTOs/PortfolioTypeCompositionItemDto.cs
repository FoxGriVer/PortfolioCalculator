using PortfolioCalculator.Domain.Enums;

namespace PortfolioCalculator.Application.PortfolioValuation.DTOs
{
    public sealed record PortfolioTypeCompositionItemDto
    {
        public InvestmentType Type { get; init; }

        public decimal Value { get; init; }

        public PortfolioTypeCompositionItemDto(InvestmentType type, decimal value)
        {
            Type = type;
            Value = value;
        }
    }
}
