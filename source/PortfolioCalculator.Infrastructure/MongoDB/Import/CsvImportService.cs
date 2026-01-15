using CsvHelper;
using CsvHelper.Configuration;
using MongoDB.Driver;
using PortfolioCalculator.Application.Abstractions.Import;
using PortfolioCalculator.Infrastructure.MongoDB.Documents;
using PortfolioCalculator.Infrastructure.MongoDB.Import.Mapping;
using PortfolioCalculator.Infrastructure.MongoDB.Import.Rows;
using PortfolioCalculator.Infrastructure.MongoDB.Repos.Write.Interfaces;
using System.Globalization;

namespace PortfolioCalculator.Infrastructure.MongoDB.Import
{
    public sealed class CsvImportService : ICsvImportService
    {
        private readonly IQuoteWriteRepository _quoteWriteRepository;
        private readonly ITransactionWriteRepository _transactionWriteRepository;
        private readonly IInvestmentWriteRepository _investmentWriteRepository;
        private readonly IOwnershipLinkWriteRepository _ownershipLinkWriteRepository;

        public CsvImportService(IQuoteWriteRepository quoteWriteRepository,
            ITransactionWriteRepository transactionWriteRepository,
            IInvestmentWriteRepository investmentWriteRepository,
            IOwnershipLinkWriteRepository ownershipLinkWriteRepository)
        {
            _quoteWriteRepository = quoteWriteRepository;
            _transactionWriteRepository = transactionWriteRepository;
            _investmentWriteRepository = investmentWriteRepository;
            _ownershipLinkWriteRepository = ownershipLinkWriteRepository;
        }

        private static CsvConfiguration SemicolonConfig() => new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ";",
            MissingFieldFound = null,
            BadDataFound = null,
            TrimOptions = TrimOptions.Trim
        };

        public async Task<int> ImportInvestmentsAsync(string filePath, CancellationToken ct)
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, SemicolonConfig());
            csv.Context.RegisterClassMap<InvestmentsRowMap>();

            var rows = csv.GetRecords<InvestmentsRow>().ToList();

            var investmentUpserts = new List<WriteModel<InvestmentDocument>>(rows.Count);
            var linkUpserts = new List<WriteModel<OwnershipLinkDocument>>(rows.Count);

            foreach (var r in rows)
            {
                var normalizedType = NormalizeInvestmentType(r.InvestmentType);

                // Upsert Investment metadata
                var invFilter = Builders<InvestmentDocument>.Filter.Eq(x => x.Id, r.InvestmentId);

                var invUpdate = Builders<InvestmentDocument>.Update
                    .SetOnInsert(x => x.Id, r.InvestmentId)
                    .Set(x => x.Type, normalizedType)
                    .Set(x => x.ISIN, string.IsNullOrWhiteSpace(r.ISIN) ? null : r.ISIN)
                    .Set(x => x.City, string.IsNullOrWhiteSpace(r.City) ? null : r.City)
                    .Set(x => x.FundId, normalizedType == "Fund" ? r.FondsInvestor : null);

                investmentUpserts.Add(new UpdateOneModel<InvestmentDocument>(invFilter, invUpdate) { IsUpsert = true });

                // Upsert Ownership link
                var ownerType = r.InvestorId.StartsWith("Fonds", StringComparison.OrdinalIgnoreCase)
                    ? "Fund"
                    : "Investor";

                var linkFilter = Builders<OwnershipLinkDocument>.Filter.And(
                    Builders<OwnershipLinkDocument>.Filter.Eq(x => x.OwnerType, ownerType),
                    Builders<OwnershipLinkDocument>.Filter.Eq(x => x.OwnerId, r.InvestorId),
                    Builders<OwnershipLinkDocument>.Filter.Eq(x => x.InvestmentId, r.InvestmentId)
                );

                var linkUpdate = Builders<OwnershipLinkDocument>.Update
                    .SetOnInsert(x => x.OwnerType, ownerType)
                    .SetOnInsert(x => x.OwnerId, r.InvestorId)
                    .SetOnInsert(x => x.InvestmentId, r.InvestmentId);

                linkUpserts.Add(new UpdateOneModel<OwnershipLinkDocument>(linkFilter, linkUpdate) { IsUpsert = true });
            }

            await _investmentWriteRepository.BulkUpsertAsync(investmentUpserts, ct);
            await _ownershipLinkWriteRepository.BulkUpsertAsync(linkUpserts, ct);

            return rows.Count;
        }

        private static string NormalizeInvestmentType(string raw)
        {
            if (raw == "Fonds")
            {
                return "Fund";
            }

            return raw;
        }

        public async Task<int> ImportTransactionsAsync(string filePath, CancellationToken ct)
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, SemicolonConfig());
            csv.Context.RegisterClassMap<TransactionsRowMap>();

            var rows = csv.GetRecords<TransactionsRow>().ToList();

            var docs = rows.Select(r => new TransactionDocument
            {
                InvestmentId = r.InvestmentId,
                Date = r.Date,
                Type = r.Type,
                Value = r.Value
            }).ToList();

            await _transactionWriteRepository.DeleteAllAsync(ct);

            if (docs.Count > 0)
                await _transactionWriteRepository.InsertManyAsync(docs, ct);

            return rows.Count;
        }

        public async Task<int> ImportQuotesAsync(string filePath, CancellationToken ct)
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, SemicolonConfig());
            csv.Context.RegisterClassMap<QuotesRowMap>();

            var rows = csv.GetRecords<QuotesRow>().ToList();

            var docs = rows.Select(r => new QuoteDocument
            {
                StockId = r.ISIN,
                Date = r.Date,
                Price = r.PricePerShare
            }).ToList();

            await _quoteWriteRepository.DeleteAllAsync(ct);
            await _quoteWriteRepository.InsertManyAsync(docs, ct);

            return rows.Count;
        }
    }
}
