namespace PortfolioCalculator.Application.Import
{
    public sealed class ImportAllCsvResult
    {
        public int InvestmentsRows { get; }
        public int TransactionsRows { get; }
        public int QuotesRows { get; }

        public ImportAllCsvResult(
            int investmentsRows,
            int transactionsRows,
            int quotesRows)
        {
            InvestmentsRows = investmentsRows;
            TransactionsRows = transactionsRows;
            QuotesRows = quotesRows;
        }
    }
}
