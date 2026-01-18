using PortfolioCalculator.Domain.Enums;

namespace PortfolioCalculator.Application.Abstractions.Repositories.Models
{
    public sealed record InvestmentInfoModel
    {
        public string Id { get; init; }

        public InvestmentType Type { get; init; }

        public string? ISIN { get; init; }

        public string? City { get; init; }

        public string? FundId { get; init; }

        public InvestmentInfoModel(
            string id,
            InvestmentType type,
            string? isin,
            string? city,
            string? fundId)
        {
            Id = id;
            Type = type;
            ISIN = isin;
            City = city;
            FundId = fundId;
        }
    }
}
