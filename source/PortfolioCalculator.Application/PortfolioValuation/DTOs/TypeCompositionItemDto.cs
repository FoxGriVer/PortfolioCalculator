namespace PortfolioCalculator.Application.PortfolioValuation.DTOs
{
    public sealed record TypeCompositionItemDto
    {
        public string Type { get; init; }

        public decimal Value { get; init; }

        public TypeCompositionItemDto(string type, decimal value)
        {
            Type = type;
            Value = value;
        }
    }
}
