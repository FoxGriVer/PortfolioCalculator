using MongoDB.Driver;
using PortfolioCalculator.Application.Abstractions.Repositories.Models;
using PortfolioCalculator.Application.Abstractions.Repositories.Read;
using PortfolioCalculator.Domain.Enums;
using PortfolioCalculator.Infrastructure.MongoDB.Init;

namespace PortfolioCalculator.Infrastructure.MongoDB.Repositories.Read
{
    public sealed class InvestmentReadRepository : IInvestmentReadRepository
    {
        private readonly MongoContext _mongoContext;

        public InvestmentReadRepository(MongoContext mongoContext)
        {
            _mongoContext = mongoContext;
        }

        public async Task<InvestmentInfoModel?> GetByIdAsync(string investmentId, CancellationToken ct)
        {
            var investmentDocument = await _mongoContext.Investments
                .Find(x => x.Id == investmentId)
                .FirstOrDefaultAsync(ct);

            if (investmentDocument is null)
            {
                return null;
            }

            var investmentInfo = new InvestmentInfoModel(
                investmentDocument.Id,
                Enum.Parse<InvestmentType>(investmentDocument.Type, ignoreCase: true),
                investmentDocument.ISIN,
                investmentDocument.City,
                investmentDocument.FundId);

            return investmentInfo;
        }

        public async Task<IReadOnlyDictionary<string, InvestmentInfoModel>> GetByIdsAsync(
            IReadOnlyCollection<string> investmentIds,
            CancellationToken ct)
        {
            if (investmentIds.Count == 0)
            {
                return new Dictionary<string, InvestmentInfoModel>();
            }

            var investmentDocuments = await _mongoContext.Investments
                .Find(x => investmentIds.Contains(x.Id))
                .ToListAsync(ct);

            var result = new Dictionary<string, InvestmentInfoModel>();

            foreach (var investmentDocument in investmentDocuments)
            {
                var investmentInfo = new InvestmentInfoModel(
                    investmentDocument.Id,
                    Enum.Parse<InvestmentType>(investmentDocument.Type, ignoreCase: true),
                    investmentDocument.ISIN,
                    investmentDocument.City,
                    investmentDocument.FundId);

                result.Add(investmentDocument.Id, investmentInfo);
            }

            return result;
        }
    }
}
