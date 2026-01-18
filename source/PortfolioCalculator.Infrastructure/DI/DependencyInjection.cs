using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PortfolioCalculator.Application.Abstractions.Import;
using PortfolioCalculator.Infrastructure.MongoDB.DI;
using PortfolioCalculator.Infrastructure.MongoDB.Import;

namespace PortfolioCalculator.Infrastructure.DI
{
    public static partial class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<ICsvImportService, CsvImportService>();

            services.AddMongoDb(config);

            return services;
        }
    }
}
