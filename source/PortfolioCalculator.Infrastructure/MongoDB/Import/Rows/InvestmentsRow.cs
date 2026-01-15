namespace PortfolioCalculator.Infrastructure.MongoDB.Import.Rows
{
    public sealed class InvestmentsRow
    {
        public string InvestorId { get; set; } = default!;

        public string InvestmentId { get; set; } = default!;

        // Stock | RealEstate | Fonds
        public string InvestmentType { get; set; } = default!;

        public string? ISIN { get; set; }

        public string? City { get; set; }

        // Only if InvestmentType == Fonds
        public string? FondsInvestor { get; set; }
    }
}
