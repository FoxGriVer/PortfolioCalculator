using CsvHelper.Configuration;
using PortfolioCalculator.Infrastructure.MongoDB.Import.Rows;

namespace PortfolioCalculator.Infrastructure.MongoDB.Import.Mapping
{
    public sealed class TransactionsRowMap : ClassMap<TransactionsRow>
    {
        public TransactionsRowMap()
        {
            Map(m => m.InvestmentId).Name("InvestmentId");
            Map(m => m.Type).Name("Type");
            Map(m => m.Date).Name("Date").TypeConverterOption.Format("yyyy-MM-dd");
            Map(m => m.Value).Name("Value");
        }
    }
}
