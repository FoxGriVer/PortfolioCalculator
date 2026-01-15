using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PortfolioCalculator.Infrastructure.MongoDB.DI;

namespace PortfolioCalculator.Infrastructure.DI
{
    public static partial class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.AddMongoDb(config);

            return services;
        }
    }
}
