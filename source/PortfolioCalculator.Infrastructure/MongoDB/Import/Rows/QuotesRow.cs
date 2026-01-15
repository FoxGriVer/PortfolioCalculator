namespace PortfolioCalculator.Infrastructure.MongoDB.Import.Rows
{
    public sealed class QuotesRow
    {
        public string ISIN { get; set; } = default!;

        public DateTime Date { get; set; }

        public decimal PricePerShare { get; set; }
    }
}
