namespace PortfolioCalculator.Application.PortfolioValuation.DTOs
{
    public sealed record PortfolioValuationResultDto
    {
        public decimal TotalValue { get; init; }

        public IReadOnlyList<TypeCompositionItemDto> CompositionByType { get; init; }

        public PortfolioValuationResultDto(
            decimal totalValue,
            IReadOnlyList<TypeCompositionItemDto> compositionByType)
        {
            TotalValue = totalValue;
            CompositionByType = compositionByType;
        }
    }
}
