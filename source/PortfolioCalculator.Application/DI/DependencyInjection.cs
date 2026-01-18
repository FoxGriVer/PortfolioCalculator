using Microsoft.Extensions.DependencyInjection;
using PortfolioCalculator.Application.Abstractions.PortfolioValuation;
using PortfolioCalculator.Application.PortfolioValuation;

namespace PortfolioCalculator.Application.DI
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IPortfolioValuationService, PortfolioBulkValuationService>();

            return services;
        }
    }
}
