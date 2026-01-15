using CsvHelper.Configuration;
using PortfolioCalculator.Infrastructure.MongoDB.Import.Rows;

namespace PortfolioCalculator.Infrastructure.MongoDB.Import.Mapping
{
    public sealed class QuotesRowMap : ClassMap<QuotesRow>
    {
        public QuotesRowMap()
        {
            Map(m => m.ISIN).Name("ISIN");
            Map(m => m.Date).Name("Date").TypeConverterOption.Format("yyyy-MM-dd");
            Map(m => m.PricePerShare).Name("PricePerShare");
        }
    }
}
