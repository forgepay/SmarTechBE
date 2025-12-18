using CryptoOnRamp.BLL.Services.ExchangeRate.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoOnRamp.BLL.Services.ExchangeRate;

public static class Configure
{
    public static IServiceCollection AddExchangeRateService(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<ExchangeRateServiceOptions>(
            configuration.GetSection(ExchangeRateServiceOptions.Position));

        services.AddScoped<IExchangeRateService, ExchangeRateService>();
        services.AddScoped<IExchangeRateDataProvider, ExchangeRateDataProvider>();

        return services;
    }
}