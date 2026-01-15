namespace PortfolioCalculator.Infrastructure.MongoDB.Import.Rows
{
    public sealed class TransactionsRow
    {
        public string InvestmentId { get; set; } = default!;

        // Shares | Estate | Building | Percentage
        public string Type { get; set; } = default!;

        public DateTime Date { get; set; }

        public decimal Value { get; set; }
    }
}
