namespace PortfolioCalculator.Application.PortfolioValuation.DTOs
{
    public sealed record PortfolioValuationResultDto
    {
        public decimal TotalValue { get; init; }

        public IReadOnlyList<PortfolioTypeCompositionItemDto> CompositionByType { get; init; }

        public PortfolioValuationResultDto(
            decimal totalValue,
            IReadOnlyList<PortfolioTypeCompositionItemDto> compositionByType)
        {
            TotalValue = totalValue;
            CompositionByType = compositionByType;
        }
    }
}
