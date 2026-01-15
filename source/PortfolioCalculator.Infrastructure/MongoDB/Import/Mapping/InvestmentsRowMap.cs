using CsvHelper.Configuration;
using PortfolioCalculator.Infrastructure.MongoDB.Import.Rows;

namespace PortfolioCalculator.Infrastructure.MongoDB.Import.Mapping
{
    public sealed class InvestmentsRowMap : ClassMap<InvestmentsRow>
    {
        public InvestmentsRowMap()
        {
            Map(m => m.InvestorId).Name("InvestorId");
            Map(m => m.InvestmentId).Name("InvestmentId");
            Map(m => m.InvestmentType).Name("InvestmentType");
            Map(m => m.ISIN).Name("ISIN");
            Map(m => m.City).Name("City");
            Map(m => m.FondsInvestor).Name("FondsInvestor");
        }
    }
}
