using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using PortfolioCalculator.Application.Abstractions.Import;
using PortfolioCalculator.Application.Abstractions.Repositories;
using PortfolioCalculator.Application.Abstractions.Repositories.Read;
using PortfolioCalculator.Infrastructure.MongoDB.Configuration;
using PortfolioCalculator.Infrastructure.MongoDB.Import;
using PortfolioCalculator.Infrastructure.MongoDB.Init;
using PortfolioCalculator.Infrastructure.MongoDB.Repositories.Read;
using PortfolioCalculator.Infrastructure.MongoDB.Repositories.Write;
using PortfolioCalculator.Infrastructure.MongoDB.Repositories.Write.Interfaces;

namespace PortfolioCalculator.Infrastructure.MongoDB.DI
{
    public static partial class DependencyInjection
    {
        public static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration config)
        {
            var settings = new MongoDBSettings();
            config.GetSection("Mongo").Bind(settings);
            services.AddSingleton(settings);

            services.AddSingleton<IMongoClient>(_ => new MongoClient(settings.ConnectionString));

            services.AddSingleton<IMongoDatabase>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase(settings.Database);
            });

            // Context
            services.AddSingleton<MongoContext>();

            // Mongo specific services
            services.AddSingleton<IDatabaseInitializer, MongoIndexInitializer>();
            services.AddSingleton<ICsvImportService, CsvImportService>();

            //// Write repositories
            services.AddSingleton<IQuoteWriteRepository, QuoteWriteRepository>();
            services.AddSingleton<ITransactionWriteRepository, TransactionWriteRepository>();
            services.AddSingleton<IInvestmentWriteRepository, InvestmentWriteRepository>();
            services.AddSingleton<IOwnershipLinkWriteRepository, OwnershipLinkWriteRepository>();

            //// Read repositories
            services.AddSingleton<IInvestmentReadRepository, InvestmentReadRepository>();
            services.AddSingleton<IOwnershipReadRepository, OwnershipReadRepository>();
            services.AddSingleton<ITransactionReadRepository, TransactionReadRepository>();
            services.AddSingleton<IQuoteReadRepository, QuoteReadRepository>();

            return services;
        }
    }
}
