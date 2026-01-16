using PortfolioCalculator.Domain.Enums;

namespace PortfolioCalculator.Application.Abstractions.Repositories.Models
{
    public record TransactionModel
    {
        public string InvestmentId { get; init; }

        public DateTime Date { get; init; }

        public TransactionType Type { get; init; }

        public decimal Value { get; init; }

        public TransactionModel(
            string investmentId,
            DateTime date,
            TransactionType type,
            decimal value)
        {
            InvestmentId = investmentId;
            Date = date;
            Type = type;
            Value = value;
        }
    }
}
